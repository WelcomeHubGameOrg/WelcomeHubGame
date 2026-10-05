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
    private const int CandidatesPerBerry = 300;
    private const int RelaxIterations = 60; // push-apart passes after the initial scatter
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

        // Relaxation: push any two berries that are closer than `wanted` apart (and keep them inside the ellipse),
        // so crowded bushes still get a clean layout instead of having to shrink the berries.
        for (int it = 0; it < RelaxIterations; it++)
        {
            bool moved = false;
            for (int i = 0; i < result.Count; i++)
                for (int j = i + 1; j < result.Count; j++)
                {
                    Vector2 delta = result[j] - result[i];
                    float dist = delta.magnitude;
                    if (dist >= wanted) continue;
                    Vector2 dir = dist > 0.001f ? delta / dist : Vector2.right;
                    float push = (wanted - dist) * 0.5f * 1.05f;
                    result[i] -= dir * push;
                    result[j] += dir * push;
                    moved = true;
                }
            for (int i = 0; i < result.Count; i++)
            {
                Vector2 p = result[i] - center;
                float k = (p.x * p.x) / (ellipseHalf.x * ellipseHalf.x) + (p.y * p.y) / (ellipseHalf.y * ellipseHalf.y);
                if (k > 1f) result[i] = center + p / Mathf.Sqrt(k);
            }
            if (!moved) break;
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

        if (!gameObject.activeInHierarchy) return; // level just finished and the panel was closed -- nothing to animate

        StartCoroutine(PlayBerryPopup(btnObj, isBad));

        // ConsumeBerryUI can finish the level: the old level's bushes get deactivated, the player leaves
        // their trigger, and the panel is closed (currentBushRef becomes null) before we get here.
        if (currentBushRef != null && currentBushRef.BerryCount <= 0)
        {
            CloseBushUI();
        }
    }

    // Pick feedback: the berry flashes (red for bad, green for good) and swells, a ring bursts out from it,
    // and a "+5" / "+1" badge pops in (with a little overshoot), floats up and fades out.
    // Bad berries also shake.
    private IEnumerator PlayBerryPopup(GameObject btnObj, bool isBad)
    {
        Color accent = isBad ? UiKit.Danger : UiKit.Mint;
        string label = isBad ? "+5" : "+1";
        Button btn = btnObj.GetComponent<Button>();
        if (btn) btn.interactable = false;

        Image img = btnObj.GetComponent<Image>();
        RectTransform berryRt = btnObj.GetComponent<RectTransform>();
        TMP_Text oldLabel = btnObj.GetComponentInChildren<TMP_Text>(true);
        if (oldLabel) oldLabel.gameObject.SetActive(false);
        if (btn) btn.onClick.RemoveAllListeners();

        UiKit.EnsureFont();
        Vector2 basePos = berryRt.anchoredPosition;
        Vector3 baseScale = berryRt.localScale;
        float berryW = berryRt.sizeDelta.x;

        // Everything below is parented to the berry container (cleared the next time the panel opens)
        RectTransform ring = UiKit.Img("PopRing", berryContainer, UiKit.Ring(), accent).rectTransform;
        RectTransform badge = UiKit.Rt("PopBadge", berryContainer);
        Image pill = UiKit.Img("Pill", badge, UiKit.Rounded(26), UiKit.Navy.WithA(0.95f), true);
        UiKit.Fill(pill.rectTransform);
        TextMeshProUGUI txt = UiKit.Txt("PopText", badge, label, 54f, accent, FontStyles.Bold);
        UiKit.Fill(txt.rectTransform);

        foreach (RectTransform r in new[] { ring, badge })
        {
            r.anchorMin = berryRt.anchorMin; r.anchorMax = berryRt.anchorMax; r.pivot = new Vector2(0.5f, 0.5f);
            r.anchoredPosition = basePos;
        }
        ring.sizeDelta = new Vector2(berryW, berryW);
        badge.sizeDelta = new Vector2(132f, 72f);
        ring.SetAsLastSibling();
        badge.SetAsLastSibling();

        Color ringBase = accent;
        const float duration = 1.1f;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            if (btnObj == null) yield break; // panel reopened and cleared everything mid-animation
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            // Berry: red flash + shake + swell in the first ~35%, then shrinks away
            if (img)
            {
                float flash = Mathf.Clamp01(t / 0.12f);
                img.color = Color.Lerp(Color.white, accent, flash);
                float fade = Mathf.Clamp01((t - 0.35f) / 0.35f);
                img.color = img.color.WithA(1f - fade);
            }
            float shake = isBad && t < 0.4f ? Mathf.Sin(elapsed * 70f) * 9f * (1f - t / 0.4f) : 0f;
            berryRt.anchoredPosition = basePos + new Vector2(shake, 0f);
            float swell = t < 0.2f ? 1f + 0.35f * (t / 0.2f) : Mathf.Lerp(1.35f, 0.3f, Mathf.Clamp01((t - 0.2f) / 0.5f));
            berryRt.localScale = baseScale * swell;

            // Ring: expands and fades
            float rt01 = Mathf.Clamp01(t / 0.55f);
            float ringScale = Mathf.Lerp(0.7f, 1.8f, 1f - (1f - rt01) * (1f - rt01));
            ring.localScale = new Vector3(ringScale, ringScale, 1f);
            ring.GetComponent<Image>().color = ringBase.WithA(0.9f * (1f - rt01));

            // Badge: pops in with overshoot, floats up, fades out over the last 40%
            float pop = Mathf.Clamp01(t / 0.18f);
            float overshoot = 1f + 2.70158f * Mathf.Pow(pop - 1f, 3f) + 1.70158f * Mathf.Pow(pop - 1f, 2f); // easeOutBack
            badge.localScale = Vector3.one * Mathf.Max(0.01f, overshoot);
            badge.anchoredPosition = basePos + Vector2.up * (30f + 80f * (1f - (1f - t) * (1f - t)));
            float a = 1f - Mathf.Clamp01((t - 0.6f) / 0.4f);
            pill.color = UiKit.Navy.WithA(0.95f * a);
            txt.color = accent.WithA(a);

            yield return null;
        }

        if (btnObj) Destroy(btnObj);
        if (ring) Destroy(ring.gameObject);
        if (badge) Destroy(badge.gameObject);
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
