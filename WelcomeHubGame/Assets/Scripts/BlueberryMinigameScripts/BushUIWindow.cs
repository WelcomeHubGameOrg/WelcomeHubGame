using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BushUIWindow : MonoBehaviour
{
    public static BushUIWindow Instance;

    [Header("UI")]
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private Transform berryContainer;
    [SerializeField] private GameObject berryButtonPrefab;
    [SerializeField] private Button closeButton;
    [SerializeField] private Image bushBackground; // the big bush artwork the berries are scattered over

    [Header("Colors (fallback if no sprite is set)")]
    [SerializeField] private Color goodColor = new Color(0.2f, 0.4f, 1f); // ripe (safe to collect)
    [SerializeField] private Color badColor = new Color(0.8f, 0.2f, 0.2f);   // unripe/bad (penalty)

    [Header("Sprites")]
    [SerializeField] private Sprite goodBerrySprite;
    [SerializeField] private Sprite badBerrySprite;

    [Header("Labels")]
    [SerializeField] private string goodLabel = "Good berry";
    [SerializeField] private string badLabel = "Bad berry";

    [Header("Scatter")]
    [SerializeField] private float minBerryDistance = 46f; // keep scattered berries from piling up on each other
    [SerializeField] private int scatterAttempts = 25;
    // The bush art is a flat, wide cloud shape that tapers in sharply at its bottom-left/right
    // corners — a single round-ish ellipse fit still let berries land past the edge there, so
    // X and Y are shrunk independently (Y much tighter) and the sampling area is nudged upward,
    // away from those tapering lower corners, rather than trying to match the silhouette exactly.
    [SerializeField] [Range(0.3f, 1f)] private float scatterEllipseFitX = 0.62f;
    [SerializeField] [Range(0.2f, 1f)] private float scatterEllipseFitY = 0.42f;
    [SerializeField] [Range(-0.3f, 0.3f)] private float scatterVerticalBias = 0.1f;

    [Header("Bad Berry Feedback")]
    [SerializeField] private Color badPenaltyTextColor = new Color(0.85f, 0.15f, 0.15f);
    [SerializeField] private float badPenaltyPopupDuration = 0.8f;
    [SerializeField] private float badPenaltyPopupRise = 45f;

    private Bush currentBushRef;
    private readonly List<Vector2> placedPositions = new List<Vector2>();

    private void Awake()
    {
        Instance = this;
        if (panelRoot) panelRoot.SetActive(false);

        if (closeButton)
            closeButton.onClick.AddListener(CloseBushUI);
        else
            Debug.LogWarning($"[{nameof(BushUIWindow)}] Close Button is not assigned in the Inspector — the close button won't work (press E to close as a fallback).", this);

        // These two are required to show any berries at all — log loudly if either is missing
        if (berryContainer == null)
            Debug.LogError($"[{nameof(BushUIWindow)}] Berry Container is not assigned in the Inspector — berry buttons have nowhere to be placed and will never appear.", this);
        if (berryButtonPrefab == null)
            Debug.LogError($"[{nameof(BushUIWindow)}] Berry Button Prefab is not assigned in the Inspector — there is nothing to instantiate for each berry.", this);
    }

    private void Update()
    {
        // Esc closes the bush panel too, in addition to the Close button and pressing E again.
        if (panelRoot != null && panelRoot.activeSelf && Input.GetKeyDown(KeyCode.Escape))
        {
            CloseBushUI();
        }
    }

    public void OpenBushUI(Bush bush, List<bool> berriesData)
    {
        currentBushRef = bush;
        currentBushRef.SetUIOpen(true);
        if (panelRoot) panelRoot.SetActive(true);

        // Lock player movement while the berry panel is open
        PlayerMovement.InputEnabled = false;

        if (berryContainer == null || berryButtonPrefab == null)
        {
            // Can't build the berry list without these — bail out instead of crashing,
            // so the panel still opens/closes cleanly (see the two errors logged in Awake).
            return;
        }

        foreach (Transform child in berryContainer)
        {
            Destroy(child.gameObject);
        }
        placedPositions.Clear();

        RectTransform containerRect = berryContainer as RectTransform;

        foreach (bool isBad in berriesData)
        {
            GameObject btnObj = Instantiate(berryButtonPrefab, berryContainer);
            // The template (Berry Button) lives inactive in the scene, and Instantiate()
            // copies that inactive state onto every clone — without this, every berry
            // button is created but invisible/non-interactive.
            btnObj.SetActive(true);

            Button btn = btnObj.GetComponent<Button>();
            Image img = btnObj.GetComponent<Image>();
            TMP_Text label = btnObj.GetComponentInChildren<TMP_Text>();

            Sprite sprite = isBad ? badBerrySprite : goodBerrySprite;
            if (img)
            {
                if (sprite) img.sprite = sprite;
                img.color = sprite ? Color.white : (isBad ? badColor : goodColor);
            }

            // The berries are visually distinct (blue & shiny vs. brown & mottled) now,
            // so a text label on top of a tiny scattered icon would just be clutter.
            if (label) label.gameObject.SetActive(false);

            RectTransform btnRect = btnObj.GetComponent<RectTransform>();
            if (btnRect && containerRect)
                btnRect.anchoredPosition = PickScatterPosition(containerRect, btnRect);

            if (btn)
            {
                btn.onClick.AddListener(() => OnBerryClicked(isBad, btnObj));
            }
            else
            {
                Debug.LogError($"[{nameof(BushUIWindow)}] Berry Button Prefab has no Button component on its root — can't wire up clicks.", btnObj);
            }
        }
    }

    // Random position inside the container, biased away from berries already placed
    // this time round so they don't stack directly on top of each other. Points are
    // sampled inside an ELLIPSE (not the full rectangle) — the bush art is a rounded
    // cloud shape that doesn't fill its own bounding box, so rectangle sampling was
    // dropping berries outside the bush entirely, in the corners.
    private Vector2 PickScatterPosition(RectTransform containerRect, RectTransform btnRect)
    {
        Vector2 halfSize = containerRect.rect.size * 0.5f;
        Vector2 margin = btnRect.rect.size * 0.5f;
        halfSize -= margin;
        halfSize.x = Mathf.Max(halfSize.x, 1f);
        halfSize.y = Mathf.Max(halfSize.y, 1f);

        Vector2 ellipseHalf = new Vector2(halfSize.x * scatterEllipseFitX, halfSize.y * scatterEllipseFitY);
        Vector2 center = new Vector2(0f, halfSize.y * scatterVerticalBias);

        Vector2 best = Vector2.zero;
        float bestScore = -1f;
        for (int attempt = 0; attempt < scatterAttempts; attempt++)
        {
            Vector2 candidate = center + RandomPointInEllipse(ellipseHalf);

            float nearest = float.MaxValue;
            foreach (Vector2 placed in placedPositions)
                nearest = Mathf.Min(nearest, Vector2.Distance(candidate, placed));
            if (placedPositions.Count == 0) nearest = float.MaxValue;

            if (nearest >= minBerryDistance)
            {
                placedPositions.Add(candidate);
                return candidate;
            }
            if (nearest > bestScore)
            {
                bestScore = nearest;
                best = candidate;
            }
        }

        // Couldn't find a fully clear spot within the attempt budget — use the best one found.
        placedPositions.Add(best);
        return best;
    }

    private static Vector2 RandomPointInEllipse(Vector2 halfSize)
    {
        float angle = Random.Range(0f, Mathf.PI * 2f);
        float r = Mathf.Sqrt(Random.value); // sqrt keeps the distribution uniform over the area, not bunched at the center
        return new Vector2(Mathf.Cos(angle) * r * halfSize.x, Mathf.Sin(angle) * r * halfSize.y);
    }

    private void OnBerryClicked(bool isBad, GameObject btnObj)
    {
        if (currentBushRef == null) return;
        currentBushRef.ConsumeBerryUI(isBad);

        if (isBad)
            StartCoroutine(PlayBadBerryPopup(btnObj));
        else
            Destroy(btnObj);

        if (currentBushRef.BerryCount <= 0)
        {
            CloseBushUI();
        }
    }

    // Bad berry: show a floating "+5" (penalty) popup that rises and fades out,
    // reusing the berry button's own (normally hidden) label so no extra prefab is needed.
    private IEnumerator PlayBadBerryPopup(GameObject btnObj)
    {
        Button btn = btnObj.GetComponent<Button>();
        if (btn) btn.interactable = false;

        Image img = btnObj.GetComponent<Image>();
        if (img) img.enabled = false;

        TMP_Text label = btnObj.GetComponentInChildren<TMP_Text>(true);
        RectTransform textRect = label ? label.rectTransform : null;
        Vector2 startPos = textRect ? textRect.anchoredPosition : Vector2.zero;

        if (label)
        {
            label.gameObject.SetActive(true);
            label.text = "+5";
            label.color = badPenaltyTextColor;
        }

        float elapsed = 0f;
        while (elapsed < badPenaltyPopupDuration)
        {
            if (btnObj == null) yield break; // panel closed/reopened mid-animation and cleared this button already
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / badPenaltyPopupDuration);

            if (textRect)
                textRect.anchoredPosition = startPos + Vector2.up * (badPenaltyPopupRise * t);
            if (label)
            {
                Color c = label.color;
                c.a = 1f - t;
                label.color = c;
            }

            yield return null;
        }

        Destroy(btnObj);
    }

    public void CloseBushUI()
    {
        if (panelRoot) panelRoot.SetActive(false);
        if (currentBushRef != null) currentBushRef.SetUIOpen(false);
        currentBushRef = null;

        // Re-enable player movement once the berry panel closes
        PlayerMovement.InputEnabled = true;
    }
}
