using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MapDirector : MonoBehaviour
{
    [Header("Spawn Settings")]
    public int initialBtnCount = 3;
    public Vector2 minBounds = new Vector2(-400, -200);
    public Vector2 maxBounds = new Vector2(400, 200);

    [Header("Proximity Settings")]
    [Tooltip("Minimum distance in pixels required between buttons")]
    public float minBtnDistance = 120f;

    [Tooltip("Max attempts to find a suitable spot before giving up (prevents infinite loops)")]
    public int maxSpawnAttempts = 15;
    
    
    [Header("UI References")]
    public GameObject buttonPrefab;
    public Transform mapCanvasTransform;

    private void Start()
    {
        // process results from last minigame
        // ProcessLastGameResults();
        
        if (GameManager.Instance.mapState.Count > 0)
        {
            var savedButtons = new List<MapButtonData>(GameManager.Instance.mapState);
            foreach (var savedButton in savedButtons)
            {
                BuildButtonUI(savedButton);
            }
        }
        else
        {
            SpawnBatch(initialBtnCount);
        }
    }

    private void SpawnBatch(int count)
    {
        for (int i = 0; i < count; i++)
            SpawnRandomButton();
    }
    
    public void SpawnRandomButton()
    {
        var pool = GameManager.Instance.AvailableMinigames;
        if (pool == null || pool.Count == 0)
        {
            Debug.Log("No Minigames Available");
            return;
        }
        
        MinigameData randomGame = pool[Random.Range(0, pool.Count)];
        Vector2 cleanPos = Vector2.zero;
        bool foundValidPos = false;

        for (int attempt = 0; attempt < maxSpawnAttempts; attempt++)
        {
            Vector2 candidatePos = new Vector2(Random.Range(minBounds.x, maxBounds.x), Random.Range(minBounds.y, maxBounds.y));

            if (IsPositionValid(candidatePos))
            {
                cleanPos = candidatePos;
                foundValidPos = true;
                break;
            }
        }

        if (!foundValidPos)
        {
            Debug.LogWarning("Map is too crowded, spawning button without proper clearance");
            cleanPos = new Vector2(Random.Range(minBounds.x, maxBounds.x), Random.Range(minBounds.y, maxBounds.y));
        }
        
        MapButtonData newButtonData = new MapButtonData(randomGame, cleanPos);
        GameManager.Instance.mapState.Add(newButtonData);
        BuildButtonUI(newButtonData);
        // set button icon or text using randomGame.icon or randomGame.displayName
    }

    private void BuildButtonUI(MapButtonData data)
    {
        if (buttonPrefab == null || mapCanvasTransform == null) return;
        
        GameObject btnObj = Instantiate(buttonPrefab, mapCanvasTransform);
        btnObj.GetComponent<RectTransform>().anchoredPosition = data.screenPosition;
        
        Button btn = btnObj.GetComponent<Button>();
        btn.onClick.AddListener(() =>
        {
            GameManager.Instance.mapState.Remove(data);
            GameManager.Instance.LaunchMinigame(data.minigameData);
            Destroy(btnObj);
        });
        
        //update button text or icons here with data.minigameData.displayName
    }

    private bool IsPositionValid(Vector2 pos)
    {
        foreach (var existingButton in GameManager.Instance.mapState)
        {
            float distance = Vector2.Distance(pos, existingButton.screenPosition);
            if (distance < minBtnDistance) return false;
        }
        
        return true;
    }

    private void ProcessLastGameResults()
    {
        // check if a game was just evaluated by the gamemanager
        // use this to trigger floating text, particle effects, whatever
    }
}
