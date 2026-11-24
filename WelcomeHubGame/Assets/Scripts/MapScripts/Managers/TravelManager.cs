using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class TravelManager : MonoBehaviour
{
    public static TravelManager instance;

    [Header("Player visuals")]
    [SerializeField] private RectTransform playerRect;
    [SerializeField] private Image player;
    [SerializeField] private Sprite playerIcon;
    [SerializeField] private Sprite bus;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 500f; // pixels per second

    // Static so the position survives the map scene being reloaded
    private static Vector2 savedPosition;
    private static bool hasSavedPosition;

    public bool isTraveling;

    private void Awake()
    {
        // Lives in the map scene only. A new one is created each time the map loads.
        instance = this;
    }

    private void Start()
    {
        if (hasSavedPosition)
            playerRect.anchoredPosition = savedPosition;
    }

    private void OnDestroy()
    {
        if (instance == this)
            instance = null;
    }

    /// <summary>
    /// Moves the player to the destination UI element, then launches the minigame.
    /// </summary>
    public void MoveTo(RectTransform destination, MinigameData minigame)
    {
        if (isTraveling) return; // ignore clicks while already travelling

        if (minigame == null)
        {
            Debug.LogError("TravelManager.MoveTo called with no MinigameData!");
            return;
        }

        StartCoroutine(MoveRoutine(destination.position, minigame));
    }

    private IEnumerator MoveRoutine(Vector3 targetWorldPos, MinigameData minigame)
    {
        isTraveling = true;
        player.sprite = bus;

        // Using world positions means it doesn't matter which parent/anchors
        // the spawned buttons use, as long as they're on the same canvas.
        Vector3 start = playerRect.position;
        float distance = Vector3.Distance(start, targetWorldPos);
        float duration = distance / moveSpeed;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            playerRect.position = Vector3.Lerp(start, targetWorldPos, t);
            yield return null;
        }

        playerRect.position = targetWorldPos;

        savedPosition = playerRect.anchoredPosition;
        hasSavedPosition = true;

        player.sprite = playerIcon;
        isTraveling = false;

        GameManager.Instance.LaunchMinigame(minigame);
    }
    public static bool TryGetSavedPosition(out Vector2 pos)
    {
        pos = savedPosition;
        return hasSavedPosition;
    }
}