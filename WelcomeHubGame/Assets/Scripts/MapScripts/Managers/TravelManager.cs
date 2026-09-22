using System.Collections;
using Unity.VectorGraphics;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TravelManager : MonoBehaviour
{
    public static TravelManager instance;

    public RectTransform playerRect;
    public Vector2 playerPosition;
    [SerializeField] private float moveSpeed = 500f; // pixels per second
    private Coroutine moveRoutine;

    public Image player;
    public Sprite playerIcon;
    [SerializeField] private Sprite bus;


    void Start()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void MoveTo(Vector2 targetAnchoredPosition, string sceneName)
    {
        if (moveRoutine != null)
            StopCoroutine(moveRoutine);

        moveRoutine = StartCoroutine(MoveRoutine(targetAnchoredPosition, sceneName));
    }

    private IEnumerator MoveRoutine(Vector2 target, string sceneName)
    {
        player.sprite = bus;

        Vector2 start = playerRect.anchoredPosition;
        float distance = Vector2.Distance(start, target);
        float duration = distance / moveSpeed;
        float elapsed = 0f;
        Debug.DrawLine(start, target);
        Debug.Log("start: " + start);
        Debug.Log("target: " + target);

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            playerRect.anchoredPosition = Vector2.Lerp(start, target, t);
            yield return null;
        }

        playerRect.anchoredPosition = target;
        moveRoutine = null;
        playerPosition = playerRect.anchoredPosition;

        player.sprite = playerIcon;
        SceneManager.LoadScene(sceneName);
    }

}
