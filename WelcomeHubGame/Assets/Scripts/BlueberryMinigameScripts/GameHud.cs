using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Replaces the old text-only HUD (Time / Berries / Level pills) with a drawn one:
//   top-left   : round countdown timer (seconds only, ring drains, turns gold then red)
//   top-center : level medallions (done = check mark, current = glowing, upcoming = dim)
//   top-right  : berry counter with progress bar, plus the pause button
// Everything is built in code at scene start (see Bootstrap) and reads its numbers from BlueberryGameManager,
// so no scene wiring is needed. The old HUD objects are simply switched off.
public class GameHud : MonoBehaviour
{
    public static GameHud Instance;

    // ---- Auto-creation: puts one "GameUI" object (HUD + pause menu) into any scene that has a BlueberryGameManager ----
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Bootstrap()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
        Ensure();
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Ensure();
    }

    private static void Ensure()
    {
        if (BlueberryGameManager.Instance == null) return;
        if (FindFirstObjectByType<GameHud>() != null) return;

        GameObject go = new GameObject("GameUI");
        go.AddComponent<SceneryBuilder>(); // sky, hills, trees, underground (world art)
        go.AddComponent<GameHud>();
        go.AddComponent<PauseMenu>();
        go.AddComponent<EndScreen>();
        go.AddComponent<ScreenBounds>(); // invisible walls at the screen edges
    }

    // ---- Widgets ----
    private class Medal
    {
        public RectTransform root;
        public Image glow, border, disc, check;
        public TextMeshProUGUI num;
        public int state = -1; // 0 done, 1 current, 2 upcoming
    }

    private static readonly string[] LegacyNames = { "TimePill", "TimeText", "ScorePill", "ScoreText", "LevelPill", "LevelText" };

    private Canvas canvas;

    // timer
    private RectTransform timerRoot;
    private Image ringFill;
    private TextMeshProUGUI timeTxt;
    private Color ringColor, numColor;
    private int shownSecs = int.MinValue;

    // level
    private RectTransform levelPill;
    private readonly List<Medal> medals = new List<Medal>();
    private readonly List<Image> connectors = new List<Image>();
    private int builtCount = -1;
    private int shownLevel = -1;

    // berries
    private RectTransform berryIcon;
    private TextMeshProUGUI berryTxt;
    private RectTransform barFill;
    private float barValue;
    private int shownScore = int.MinValue, shownTarget = int.MinValue;
    private float popTimer, flashTimer;
    private int shownPenalties;
    private float shakeTimer, minusTimer;
    private TextMeshProUGUI minusTxt;
    private Vector2 berryTxtBase;

    // pause button
    private Button pauseBtn;

    private void Awake()
    {
        Instance = this;
        UiKit.EnsureFont();
        canvas = UiKit.MakeCanvas("HudCanvas", -1, transform); // under the old canvas, so the bush / result panels cover it
        HideLegacyHud();
        BuildTimer();
        BuildBerries();
        BuildPauseButton();
        ringColor = UiKit.Berry;
        numColor = UiKit.Cream;
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    private void HideLegacyHud()
    {
        Transform[] all = FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (Transform t in all)
        {
            if (System.Array.IndexOf(LegacyNames, t.name) < 0) continue;
            if (t.IsChildOf(transform)) continue; // never touch our own widgets
            if (t.GetComponentInParent<Canvas>() == null) continue;
            t.gameObject.SetActive(false);
        }
    }

    // ------------------------------------------------------------------ timer
    private void BuildTimer()
    {
        UiKit.PanelParts p = UiKit.Panel(canvas.transform, "Timer", new Vector2(128f, 128f), 0,
            UiKit.Navy.WithA(0.94f), UiKit.BerryLight.WithA(0.55f), 4f);
        UiKit.Place(p.root, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(44f, -40f), new Vector2(128f, 128f));
        timerRoot = p.root;

        Image track = UiKit.Img("Track", p.content, UiKit.Ring(), Color.white.WithA(0.13f));
        UiKit.Fill(track.rectTransform, 7f, 7f, 7f, 7f);

        ringFill = UiKit.Img("Ring", p.content, UiKit.Ring(), UiKit.Berry);
        UiKit.Fill(ringFill.rectTransform, 7f, 7f, 7f, 7f);
        ringFill.type = Image.Type.Filled;
        ringFill.fillMethod = Image.FillMethod.Radial360;
        ringFill.fillOrigin = (int)Image.Origin360.Top;
        ringFill.fillClockwise = false;
        ringFill.fillAmount = 1f;

        timeTxt = UiKit.Txt("Seconds", p.content, "0", 54f, UiKit.Cream, FontStyles.Bold);
        UiKit.Fill(timeTxt.rectTransform, 20f, 14f, 20f, 14f);
        timeTxt.enableAutoSizing = true;
        timeTxt.fontSizeMin = 24f;
        timeTxt.fontSizeMax = 54f;
    }

    // ------------------------------------------------------------------ level medallions
    private void BuildLevels(int count)
    {
        if (levelPill != null) Destroy(levelPill.gameObject);
        medals.Clear();
        connectors.Clear();
        builtCount = count;
        shownLevel = -1;
        if (count <= 0) return;

        const float slot = 76f, gap = 30f, pad = 26f, h = 92f;
        float width = pad * 2f + count * slot + (count - 1) * gap;

        UiKit.PanelParts p = UiKit.Panel(canvas.transform, "Levels", new Vector2(width, h), 46,
            UiKit.Navy.WithA(0.94f), UiKit.BerryLight.WithA(0.5f), 3f);
        UiKit.Place(p.root, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -40f), new Vector2(width, h));
        levelPill = p.root;

        float x0 = pad - 3f + slot * 0.5f; // content is inset by the 3px border

        // connectors first so the medallions sit on top of them
        for (int i = 0; i < count - 1; i++)
        {
            float cx = x0 + i * (slot + gap) + (slot + gap) * 0.5f;
            Image c = UiKit.Img("Link" + i, p.content, UiKit.Rounded(4), UiKit.NavyLight, true);
            UiKit.Place(c.rectTransform, new Vector2(0f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(cx, 0f), new Vector2(gap + 24f, 8f));
            connectors.Add(c);
        }

        for (int i = 0; i < count; i++)
        {
            Medal m = new Medal();
            m.root = UiKit.Rt("Level" + (i + 1), p.content);
            UiKit.Place(m.root, new Vector2(0f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(x0 + i * (slot + gap), 0f), new Vector2(slot, slot));

            m.glow = UiKit.Img("Glow", m.root, UiKit.Glow(), UiKit.Berry.WithA(0.55f));
            UiKit.Center(m.glow.rectTransform, new Vector2(132f, 132f));
            m.border = UiKit.Img("Ring", m.root, UiKit.Circle(), UiKit.Gold);
            UiKit.Center(m.border.rectTransform, new Vector2(74f, 74f));
            m.disc = UiKit.Img("Disc", m.root, UiKit.Circle(), UiKit.Berry);
            m.check = UiKit.Img("Check", m.root, UiKit.Check(), Color.white);
            UiKit.Center(m.check.rectTransform, new Vector2(30f, 30f));
            m.num = UiKit.Txt("Number", m.root, (i + 1).ToString(), 32f, UiKit.Cream, FontStyles.Bold);
            UiKit.Center(m.num.rectTransform, new Vector2(60f, 60f));
            medals.Add(m);
        }
    }

    private void ApplyMedal(Medal m, int state)
    {
        m.state = state;
        m.glow.gameObject.SetActive(state == 1);
        m.border.gameObject.SetActive(state == 1);
        m.check.gameObject.SetActive(state == 0);
        m.num.gameObject.SetActive(state != 0);

        switch (state)
        {
            case 0: // done
                UiKit.Center(m.disc.rectTransform, new Vector2(54f, 54f));
                m.disc.color = UiKit.Mint;
                break;
            case 1: // current
                UiKit.Center(m.disc.rectTransform, new Vector2(64f, 64f));
                m.disc.color = UiKit.Berry;
                m.num.fontSize = 36f;
                m.num.color = Color.white;
                break;
            default: // upcoming
                UiKit.Center(m.disc.rectTransform, new Vector2(54f, 54f));
                m.disc.color = UiKit.NavyLight;
                m.num.fontSize = 28f;
                m.num.color = UiKit.BerryLight.WithA(0.7f);
                break;
        }
    }

    private void ApplyLevels(int index)
    {
        shownLevel = index;
        for (int i = 0; i < medals.Count; i++)
            ApplyMedal(medals[i], i < index ? 0 : (i == index ? 1 : 2));
        for (int i = 0; i < connectors.Count; i++)
            connectors[i].color = i < index ? UiKit.Mint : UiKit.NavyLight;
    }

    // ------------------------------------------------------------------ berries
    private void BuildBerries()
    {
        UiKit.PanelParts p = UiKit.Panel(canvas.transform, "Berries", new Vector2(330f, 88f), 44,
            UiKit.Navy.WithA(0.94f), UiKit.BerryLight.WithA(0.5f), 3f);
        UiKit.Place(p.root, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-156f, -40f), new Vector2(330f, 88f));

        Image icon = UiKit.Img("Icon", p.content, UiKit.BerryIcon(), Color.white);
        UiKit.Place(icon.rectTransform, new Vector2(0f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(46f, 0f), new Vector2(62f, 62f));
        berryIcon = icon.rectTransform;

        berryTxt = UiKit.Txt("Count", p.content, "0", 42f, UiKit.Cream, FontStyles.Bold, TextAlignmentOptions.MidlineLeft);
        UiKit.Fill(berryTxt.rectTransform, 88f, 30f, 18f, 6f);
        berryTxt.richText = true;
        berryTxtBase = berryTxt.rectTransform.anchoredPosition;

        // Floating "-5" that drops out of the counter when a bad berry takes berries away
        minusTxt = UiKit.Txt("Minus", p.root, "-5", 44f, UiKit.Danger, FontStyles.Bold);
        UiKit.Place(minusTxt.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 1f), new Vector2(0f, -4f), new Vector2(140f, 56f));
        minusTxt.color = UiKit.Danger.WithA(0f);

        Image track = UiKit.Img("BarTrack", p.content, UiKit.Rounded(5), Color.white.WithA(0.14f), true);
        track.rectTransform.anchorMin = new Vector2(0f, 0f);
        track.rectTransform.anchorMax = new Vector2(1f, 0f);
        track.rectTransform.pivot = new Vector2(0.5f, 0f);
        track.rectTransform.offsetMin = new Vector2(88f, 14f);
        track.rectTransform.offsetMax = new Vector2(-22f, 24f);

        Image fill = UiKit.Img("BarFill", track.transform, UiKit.Rounded(5), UiKit.Berry, true);
        UiKit.Fill(fill.rectTransform);
        fill.rectTransform.anchorMax = new Vector2(0f, 1f);
        barFill = fill.rectTransform;
    }

    // ------------------------------------------------------------------ pause button
    private void BuildPauseButton()
    {
        UiKit.PanelParts p = UiKit.Panel(canvas.transform, "PauseButton", new Vector2(88f, 88f), 0,
            UiKit.Navy.WithA(0.94f), UiKit.BerryLight.WithA(0.55f), 3f);
        UiKit.Place(p.root, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-44f, -40f), new Vector2(88f, 88f));

        Image barL = UiKit.Img("BarL", p.content, UiKit.Rounded(5), UiKit.Cream, true);
        UiKit.Place(barL.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-10f, 0f), new Vector2(12f, 34f));
        Image barR = UiKit.Img("BarR", p.content, UiKit.Rounded(5), UiKit.Cream, true);
        UiKit.Place(barR.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(10f, 0f), new Vector2(12f, 34f));

        p.fillImg.raycastTarget = true;
        pauseBtn = p.root.gameObject.AddComponent<Button>();
        UiKit.StyleButton(pauseBtn, p.fillImg);
        pauseBtn.onClick.AddListener(() =>
        {
            if (PauseMenu.Instance != null) PauseMenu.Instance.Open();
        });
        p.root.gameObject.AddComponent<UiHoverPop>();
    }

    // ------------------------------------------------------------------ per-frame update
    private void Update()
    {
        BlueberryGameManager gm = BlueberryGameManager.Instance;
        if (gm == null) return;

        float dt = Time.unscaledDeltaTime;
        bool paused = PauseMenu.IsPaused;

        // --- timer ---
        float t = Mathf.Max(0f, gm.TimeRemaining);
        int secs = Mathf.CeilToInt(t);
        if (secs != shownSecs)
        {
            shownSecs = secs;
            timeTxt.text = secs.ToString();
        }
        float total = gm.TimeTotal;
        float frac = total > 0f ? Mathf.Clamp01(t / total) : 0f;
        ringFill.fillAmount = frac;

        bool urgent = secs <= 10 && !gm.IsGameOver;
        Color ringTarget = urgent ? UiKit.Danger : (frac < 0.3f ? UiKit.Gold : UiKit.Berry);
        Color numTarget = urgent ? UiKit.Danger : (frac < 0.3f ? UiKit.Gold : UiKit.Cream);
        float k = 1f - Mathf.Exp(-8f * dt);
        ringColor = Color.Lerp(ringColor, ringTarget, k);
        numColor = Color.Lerp(numColor, numTarget, k);
        ringFill.color = ringColor;
        timeTxt.color = numColor;

        float pulse = (urgent && !paused) ? 1f + 0.06f * (0.5f + 0.5f * Mathf.Sin(Time.unscaledTime * 9f)) : 1f;
        timerRoot.localScale = new Vector3(pulse, pulse, 1f);

        // --- levels ---
        int count = gm.LevelCount;
        if (count != builtCount) BuildLevels(count);
        if (count > 0 && gm.LevelIndex != shownLevel) ApplyLevels(gm.LevelIndex);
        if (medals.Count > 0 && shownLevel >= 0 && shownLevel < medals.Count)
        {
            Medal cur = medals[shownLevel];
            float g = 0.45f + 0.2f * Mathf.Sin(Time.unscaledTime * 3f);
            cur.glow.color = UiKit.Berry.WithA(g);
        }

        // --- berries ---
        int score = gm.Score, target = gm.TargetScore;
        if (score != shownScore || target != shownTarget)
        {
            if (shownScore != int.MinValue)
            {
                if (score > shownScore) popTimer = 0.3f;
            }
            shownScore = score;
            shownTarget = target;
            berryTxt.text = score + "<size=62%><color=#" + ColorUtility.ToHtmlStringRGB(UiKit.BerryLight) + "> / " + target + "</color></size>";
        }

        // Bad berry eaten: the counter flashes red, shakes, and a red "-5" drops out of it
        if (gm.PenaltyCount != shownPenalties)
        {
            if (gm.PenaltyCount > shownPenalties)
            {
                flashTimer = 0.7f;
                shakeTimer = 0.45f;
                minusTimer = 0.9f;
            }
            shownPenalties = gm.PenaltyCount;
        }
        float goal = target > 0 ? Mathf.Clamp01((float)score / target) : 0f;
        barValue = Mathf.MoveTowards(barValue, goal, dt * 2.5f);
        barFill.anchorMax = new Vector2(barValue, 1f);

        if (popTimer > 0f) popTimer = Mathf.Max(0f, popTimer - dt);
        float popScale = 1f + 0.3f * Mathf.Sin((1f - popTimer / 0.3f) * Mathf.PI) * (popTimer > 0f ? 1f : 0f);
        berryIcon.localScale = new Vector3(popScale, popScale, 1f);

        if (flashTimer > 0f) flashTimer = Mathf.Max(0f, flashTimer - dt);
        berryTxt.color = Color.Lerp(UiKit.Cream, UiKit.Danger, Mathf.Clamp01(flashTimer / 0.5f));

        // shake the counter text sideways while shakeTimer runs
        if (shakeTimer > 0f) shakeTimer = Mathf.Max(0f, shakeTimer - dt);
        float shake = Mathf.Sin(Time.unscaledTime * 80f) * 8f * (shakeTimer / 0.45f);
        berryTxt.rectTransform.anchoredPosition = berryTxtBase + new Vector2(shake, 0f);

        // red "-5" drops out of the counter and fades
        if (minusTimer > 0f) minusTimer = Mathf.Max(0f, minusTimer - dt);
        float mk = 1f - minusTimer / 0.9f; // 0 -> 1 over the effect
        float mAlpha = minusTimer > 0f ? Mathf.Clamp01(Mathf.Min(mk * 8f, (1f - mk) * 2.5f)) : 0f;
        minusTxt.color = UiKit.Danger.WithA(mAlpha);
        minusTxt.rectTransform.anchoredPosition = new Vector2(0f, -4f - 46f * mk);

        // --- pause button ---
        pauseBtn.interactable = !gm.IsGameOver && !paused;
    }
}
