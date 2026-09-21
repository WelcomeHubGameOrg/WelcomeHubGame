using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class MapDirector : MonoBehaviour
{
    [Header("Spawn Settings")]
    public int initialBtnCount = 3;
    public Vector2 minBounds = new Vector2(-400, -200);
    public Vector2 maxBounds = new Vector2(400, 200);

    [Header("Zones")]
    [Tooltip("One entry per region (A, B, C, D) each with its polygon outline")]
    public List<MapZone> zones;

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
        List<MapZone> allowedZones = GetZonesForFlags(randomGame.allowedZones);

        if (allowedZones.Count == 0)
        {
            Debug.LogWarning(
                $"No zones matched {randomGame.allowedZones} for {randomGame.displayName}. Falling back to full map bounds");
        }
        
        Vector2 cleanPos = Vector2.zero;
        bool foundValidPos = false;

        for (int attempt = 0; attempt < maxSpawnAttempts; attempt++)
        {
            Vector2 candidatePos = GetRandomCandidate(allowedZones);

            if (IsInAllowedZones(candidatePos, allowedZones) && IsPositionValid(candidatePos))
            {
                cleanPos = candidatePos;
                foundValidPos = true;
                break;
            }
        }

        if (!foundValidPos)
        {
            Debug.LogWarning("Map is too crowded, relaxing distance requirement");
            foundValidPos = TryFindAnyPointInZones(allowedZones, out cleanPos);
        }

        if (!foundValidPos)
        {
            // if allowedZones is empty AND the full fallback also fails
            Debug.LogError("Failed to find valid spawnpoint, skipping spawn");
            return;
        }
        
        MapButtonData newButtonData = new MapButtonData(randomGame, cleanPos);
        GameManager.Instance.mapState.Add(newButtonData);
        BuildButtonUI(newButtonData);
        // set button icon or text using randomGame.icon or randomGame.displayName
    }

    private List<MapZone> GetZonesForFlags(MapZoneType allowedFlags)
    {
        if (allowedFlags == MapZoneType.None || zones == null)
            return new List<MapZone>();

        // some bitwise AND magic here : true if zone's flag is one of the allowed flags
        return zones.Where(z => (allowedFlags & z.zoneId) != 0).ToList();
    }

    private Vector2 GetRandomCandidate(List<MapZone> allowedZones)
    {
        if (allowedZones == null || allowedZones.Count == 0)
            return new Vector2(Random.Range(minBounds.x, maxBounds.x), Random.Range(minBounds.y, maxBounds.y));

        Vector2 min = allowedZones[0].Min;
        Vector2 max = allowedZones[0].Max;
        foreach (var zone in allowedZones)
        {
            min = Vector2.Min(min, zone.Min);
            max = Vector2.Max(max, zone.Max);
        }
        
        return new Vector2(Random.Range(min.x, max.x), Random.Range(min.y, max.y));
    }

    private bool IsInAllowedZones(Vector2 pos, List<MapZone> allowedZones)
    {
        if (allowedZones == null || allowedZones.Count == 0) return true; // no restriction specified
        return allowedZones.Any(z => z.Contains(pos));
    }

    private bool TryFindAnyPointInZones(List<MapZone> allowedZones, out Vector2 pos)
    {
        int attempts = maxSpawnAttempts * 4;
        for (int i = 0; i < attempts; i++)
        {
            Vector2 candidate = GetRandomCandidate(allowedZones);
            if (IsInAllowedZones(candidate, allowedZones))
            {
                pos = candidate;
                return true;
            }
        }

        pos = Vector2.zero;
        return false;
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
