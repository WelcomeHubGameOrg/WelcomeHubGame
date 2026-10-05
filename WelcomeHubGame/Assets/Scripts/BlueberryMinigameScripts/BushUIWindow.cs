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
    [SerializeField] private float berryScale = 1.25f;     // berry buttons are drawn this much bigger than the prefab
    [SerializeField] private float berryGap = 16f;         // empty space kept between neighbouring berries (never touching)
    private const int CandidatesPerBerry = 80;
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

    private void Awake()
    {
        Instance = this;
        if (panelRoot) panelRoot.SetActive(false);

        if (closeButton)
            closeButton.onClick.AddListener(CloseBushUI);
        else
            Debug.LogWarning($"[{nameof(BushUIWindow)}] Close Button is not assigned in the Inspector — the close button won't work (press E to close as a fallback).", this);

        RestyleCloseButton();

        // These two are required to show any berries at all — log loudly if either is missing
        if (berryContainer == null)
            Debug.LogError($"[{nameof(BushUIWindow)}] Berry Container is not assigned in the Inspector — berry buttons have nowhere to be placed and will never appear.", this);
        if (berryButtonPrefab == null)
            Debug.LogError($"[{nameof(BushUIWindow)}] Berry Button Prefab is not assigned in the Inspector — there is nothing to instantiate for each berry.", this);
    }

    // Swaps the plain red Close button for one in the shared HUD / pause-menu style (chunky rounded,
    // blueberry colours). The old button is kept in the scene but switched off.
    private void RestyleCloseButton()
    {
        if (closeButton == null) return;
        UiKit.EnsureFont();

        RectTransform old = (RectTransform)closeButton.transform;
        Button styled = UiKit.MakeButton(old.parent, "CloseButton_Styled", "CLOSE", new Vector2(250f, 78f),
            UiKit.NavyLight, UiKit.NavyMid, UiKit.Cream, 32f, CloseBushUI);

        RectTransform r = (RectTransform)styled.transform;
        r.anchorMin = old.anchorMin;
        r.anchorMax = old.anchorMax;
        r.pivot = old.pivot;
        r.anchoredPosition = old.anchoredPosition;
        r.sizeDelta = new Vector2(250f, 78f);
        r.SetAsLastSibling(); // above the bush art and the berries

        closeButton.gameObject.SetActive(false);
    }

    // True while the berry panel is showing (read by PauseMenu so Esc closes the bush panel first)
    public bool IsOpen => panelRoot != null && panelRoot.activeSelf;

    // Frame in which Esc was consumed by closing this panel. PauseMenu ignores Esc in that same
    // frame, otherwise one key press would close the bush AND open the pause menu.
    public static int EscConsumedFrame = -1;

    private void Update()
    {
        // Esc closes the bush panel too, in addition to the Close button and pressing E again.
        if (IsOpen && Input.GetKeyDown(KeyCode.Escape))
        {
            EscConsumedFrame = Time.frameCount;
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
        RectTransform containerRect = berryContainer as RectTransform;
        Vector2 baseSize = ((RectTransform)berryButtonPrefab.transform).sizeDelta * berryScale;

        // Work out every berry's position first, so they can all be shrunk together if the bush is
        // crowded -- berries must never overlap or touch.
        List<Vector2> positions = PickScatterPositions(containerRect, berriesData.Count, baseSize, out float sizeFactor);
        Vector2 btnSize = baseSize * sizeFactor;

        for (int i = 0; i < berriesData.Count; i++)
        {
            bool isBad = berriesData[i];
            GameObject btnObj = Instantiate(berryButtonPrefab, berryContainer);
            // The template (Berry Button) lives inactive in the scene, and Instantiate()
            // copies that inactive state onto every clone -- without this, every berry
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
            if (btnRect)
            {
                btnRect.sizeDelta = btnSize;
                btnRect.anchoredPosition = positions[i];
            }

            if (btn)
            {
                btn.onClick.AddListener(() => OnBerryClicked(isBad, btnObj));
            }
            else
            {
                Debug.LogError($"[{nameof(BushUIWindow)}] Berry Button Prefab has no Button component on its root -- can't wire up clicks.", btnObj);
            }
        }
    }

    // Picks a position for every berry inside an ELLIPSE (the bush art is a rounded cloud that doesn't
    // fill its bounding box). Each berry takes the candidate spot farthest from the berries already
    // placed; afterwards, if the closest pair is still nearer than size+gap, ALL berries are scaled
    // down (sizeFactor < 1) just enough that nothing touches.
    private List<Vector2> PickScatterPositions(RectTransform containerRect, int count, Vector2 btnSize, out float sizeFactor)
    {
        List<Vector2> result = new List<Vector2>(count);
        sizeFactor = 1f;
        if (count <= 0) return result;

        Vector2 halfSize = containerRect != null ? containerRect.rect.size * 0.5f : new Vector2(300f, 150f);
        halfSize -= btnSize * 0.5f;
        halfSize.x = Mathf.Max(halfSize.x, 1f);
        halfSize.y = Mathf.Max(halfSize.y, 1f);

        Vector2 ellipseHalf = new Vector2(halfSize.x * scatterEllipseFitX, halfSize.y * scatterEllipseFitY);
        Vector2 center = new Vector2(0f, halfSize.y * scatterVerticalBias);

        float wanted = btnSize.x + berryGap; // centre-to-centre distance that keeps a clear gap
        for (int i = 0; i < count; i++)
        {
            Vector2 best = center;
            float bestNearest = -1f;
            for (int a = 0; a < CandidatesPerBerry; a++)
            {
                Vector2 candidate = center + RandomPointInEllipse(ellipseHalf);
                float nearest = float.MaxValue;
                for (int j = 0; j < result.Count; j++)
                    nearest = Mathf.Min(nearest, Vector2.Distance(candidate, result[j]));

                if (nearest > bestNearest) { bestNearest = nearest; best = candidate; }
                if (nearest >= wanted) break; // clear spot found
            }
            result.Add(best);
        }

        float minPair = float.MaxValue;
        for (int i = 0; i < result.Count; i++)
            for (int j = i + 1; j < result.Count; j++)
                minPair = Mathf.Min(minPair, Vector2.Distance(result[i], result[j]));

        if (minPair < wanted)
            sizeFactor = Mathf.Clamp(minPair / wanted, 0.45f, 1f);

        return result;
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

        // ConsumeBerryUI can finish the level: the old level's bushes get deactivated, the player leaves
        // their trigger, and the panel is closed (currentBushRef becomes null) before we get here.
        if (currentBushRef != null && currentBushRef.BerryCount <= 0)
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
