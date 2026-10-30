using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Pause menu, built in code (created together with GameHud, see GameHud.Bootstrap).
//   Resume (highlighted) / Restart (current level) / Settings (volume) / Main Menu (MainMap)
// Opened with the pause button or Esc. Esc first closes the berry panel if it is open.
public class PauseMenu : MonoBehaviour
{
    public static PauseMenu Instance;
    public static bool IsPaused { get; private set; }

    private const string MainMenuScene = "MainMap"; // the hub map scene (Assets/Scenes/MainMap.unity)
    private const string VolumePref = "MasterVolume";

    private GameObject overlay;
    private CanvasGroup overlayGroup;
    private RectTransform mainCard, settingsCard;
    private Slider volumeSlider;
    private TextMeshProUGUI volumeValue;

    private bool prevInputEnabled = true;
    private float prevTimeScale = 1f;
    private float fade;

    private void Awake()
    {
        Instance = this;
        IsPaused = false;
        UiKit.EnsureFont();
        AudioListener.volume = Mathf.Clamp01(PlayerPrefs.GetFloat(VolumePref, 1f));
        Build();
        overlay.SetActive(false);
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
        if (IsPaused)
        {
            // scene is going away while paused -- never leave the game frozen
            IsPaused = false;
            Time.timeScale = 1f;
            PlayerMovement.InputEnabled = true;
        }
    }

    // ------------------------------------------------------------------ build
    private void Build()
    {
        Canvas canvas = UiKit.MakeCanvas("PauseCanvas", 200, transform);

        overlay = UiKit.Rt("Overlay", canvas.transform).gameObject;
        UiKit.Fill(overlay.GetComponent<RectTransform>());
        Image dim = overlay.AddComponent<Image>();
        dim.color = new Color(0.04f, 0.04f, 0.14f, 0.7f);
        dim.raycastTarget = true; // blocks clicks on the game behind
        overlayGroup = overlay.AddComponent<CanvasGroup>();

        BuildMainCard();
        BuildSettingsCard();
    }

    private UiKit.PanelParts NewCard(string name, Vector2 size)
    {
        UiKit.PanelParts p = UiKit.Panel(overlay.transform, name, size, 44,
            UiKit.Navy.WithA(0.98f), UiKit.BerryLight.WithA(0.5f), 3f);
        UiKit.Center(p.root, size);
        return p;
    }

    private void BuildMainCard()
    {
        Vector2 size = new Vector2(520f, 650f);
        UiKit.PanelParts card = NewCard("PauseCard", size);
        mainCard = card.root;
        Transform c = card.content;

        Image berry = UiKit.Img("Berry", c, UiKit.BerryIcon(), Color.white);
        UiKit.Place(berry.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -34f), new Vector2(84f, 84f));

        TextMeshProUGUI title = UiKit.Txt("Title", c, "PAUSED", 52f, UiKit.Cream, FontStyles.Bold);
        title.characterSpacing = 10f;
        UiKit.Place(title.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -130f), new Vector2(440f, 64f));

        // Resume: bigger and in a different colour so it stands out from the rest
        Button resume = UiKit.MakeButton(c, "ResumeButton", "RESUME", new Vector2(400f, 96f),
            UiKit.Mint, UiKit.MintDark, Hex("#0B2E22"), 40f, Resume);
        Top(resume, -228f);

        Color face = UiKit.NavyLight, lip = UiKit.NavyMid;
        Button restart = UiKit.MakeButton(c, "RestartButton", "RESTART", new Vector2(400f, 78f), face, lip, UiKit.Cream, 32f, Restart);
        Top(restart, -352f);
        Button settings = UiKit.MakeButton(c, "SettingsButton", "SETTINGS", new Vector2(400f, 78f), face, lip, UiKit.Cream, 32f, ShowSettings);
        Top(settings, -448f);
        Button menu = UiKit.MakeButton(c, "MainMenuButton", "MAIN MENU", new Vector2(400f, 78f), face, lip, UiKit.Cream, 32f, MainMenu);
        Top(menu, -544f);

        TextMeshProUGUI hint = UiKit.Txt("Hint", mainCard, "Press Esc to resume", 24f, UiKit.BerryLight.WithA(0.75f));
        UiKit.Place(hint.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 1f), new Vector2(0f, -22f), new Vector2(420f, 34f));
    }

    private void BuildSettingsCard()
    {
        Vector2 size = new Vector2(620f, 470f);
        UiKit.PanelParts card = NewCard("SettingsCard", size);
        settingsCard = card.root;
        Transform c = card.content;

        TextMeshProUGUI title = UiKit.Txt("Title", c, "SETTINGS", 50f, UiKit.Cream, FontStyles.Bold);
        title.characterSpacing = 10f;
        UiKit.Place(title.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -40f), new Vector2(500f, 64f));

        TextMeshProUGUI label = UiKit.Txt("VolumeLabel", c, "Volume", 32f, UiKit.Cream, FontStyles.Bold, TextAlignmentOptions.MidlineLeft);
        UiKit.Place(label.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(-120f, -160f), new Vector2(260f, 44f));
        volumeValue = UiKit.Txt("VolumeValue", c, "100%", 32f, UiKit.BerryLight, FontStyles.Bold, TextAlignmentOptions.MidlineRight);
        UiKit.Place(volumeValue.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(180f, -160f), new Vector2(160f, 44f));

        volumeSlider = BuildSlider(c, new Vector2(0f, -240f), new Vector2(500f, 44f));
        volumeSlider.onValueChanged.AddListener(OnVolumeChanged);

        Button back = UiKit.MakeButton(c, "BackButton", "BACK", new Vector2(320f, 78f), UiKit.NavyLight, UiKit.NavyMid, UiKit.Cream, 32f, ShowMain);
        Top(back, -332f);
    }

    private Slider BuildSlider(Transform parent, Vector2 pos, Vector2 size)
    {
        RectTransform root = UiKit.Rt("VolumeSlider", parent);
        UiKit.Place(root, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), pos, size);

        Image bg = UiKit.Img("Background", root, UiKit.Rounded(9), Hex("#0E0D2A"), true, true);
        bg.rectTransform.anchorMin = new Vector2(0f, 0.5f);
        bg.rectTransform.anchorMax = new Vector2(1f, 0.5f);
        bg.rectTransform.pivot = new Vector2(0.5f, 0.5f);
        bg.rectTransform.anchoredPosition = Vector2.zero;
        bg.rectTransform.sizeDelta = new Vector2(0f, 18f);

        RectTransform fillArea = UiKit.Rt("Fill Area", root);
        fillArea.anchorMin = new Vector2(0f, 0.5f);
        fillArea.anchorMax = new Vector2(1f, 0.5f);
        fillArea.pivot = new Vector2(0.5f, 0.5f);
        fillArea.anchoredPosition = Vector2.zero;
        fillArea.sizeDelta = new Vector2(-28f, 18f);

        Image fill = UiKit.Img("Fill", fillArea, UiKit.Rounded(9), UiKit.Berry, true);
        fill.rectTransform.sizeDelta = new Vector2(14f, 0f);

        RectTransform slideArea = UiKit.Rt("Handle Slide Area", root);
        UiKit.Fill(slideArea, 14f, 0f, 14f, 0f);

        Image handle = UiKit.Img("Handle", slideArea, UiKit.Circle(), UiKit.Cream, false, true);
        handle.rectTransform.sizeDelta = new Vector2(38f, 0f);

        Slider s = root.gameObject.AddComponent<Slider>();
        s.fillRect = fill.rectTransform;
        s.handleRect = handle.rectTransform;
        s.targetGraphic = handle;
        s.direction = Slider.Direction.LeftToRight;
        s.minValue = 0f;
        s.maxValue = 1f;
        s.wholeNumbers = false;
        Navigation nav = s.navigation;
        nav.mode = Navigation.Mode.None;
        s.navigation = nav;
        s.SetValueWithoutNotify(AudioListener.volume);
        return s;
    }

    private static void Top(Button b, float y)
    {
        RectTransform rt = (RectTransform)b.transform;
        UiKit.Place(rt, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, y), rt.sizeDelta);
    }

    private static Color Hex(string h) { return UiKit.Hex(h); }

    // ------------------------------------------------------------------ state
    private bool CanPause()
    {
        GameManager gm = GameManager.Instance;
        if (gm == null || gm.IsGameOver) return false;
        BushUIWindow bush = BushUIWindow.Instance;
        if (bush != null && bush.IsOpen) return false;
        return true;
    }

    public void Open()
    {
        if (IsPaused || !CanPause()) return;
        IsPaused = true;
        prevInputEnabled = PlayerMovement.InputEnabled;
        PlayerMovement.InputEnabled = false;
        prevTimeScale = Time.timeScale;
        Time.timeScale = 0f;

        overlay.SetActive(true);
        ShowMain();
        fade = 0f;
        ApplyFade();
    }

    public void Resume()
    {
        if (!IsPaused) return;
        IsPaused = false;
        Time.timeScale = prevTimeScale > 0f ? prevTimeScale : 1f;
        PlayerMovement.InputEnabled = prevInputEnabled;
        overlay.SetActive(false);
    }

    // Restarts the CURRENT level from scratch (timer, berries, bushes, player position).
    public void Restart()
    {
        Resume();
        PlayerMovement.InputEnabled = true;
        if (BushUIWindow.Instance != null && BushUIWindow.Instance.IsOpen) BushUIWindow.Instance.CloseBushUI();
        if (GameManager.Instance != null) GameManager.Instance.RestartGame();
    }

    public void MainMenu()
    {
        if (!Application.CanStreamedLevelBeLoaded(MainMenuScene))
        {
            Debug.LogError($"[{nameof(PauseMenu)}] Scene '{MainMenuScene}' can't be loaded -- add it to File > Build Profiles (Scene List) / Build Settings.", this);
            return;
        }
        IsPaused = false;
        Time.timeScale = 1f;
        PlayerMovement.InputEnabled = true;
        SceneManager.LoadScene(MainMenuScene);
    }

    private void ShowMain()
    {
        mainCard.gameObject.SetActive(true);
        settingsCard.gameObject.SetActive(false);
    }

    private void ShowSettings()
    {
        mainCard.gameObject.SetActive(false);
        settingsCard.gameObject.SetActive(true);
        volumeSlider.SetValueWithoutNotify(AudioListener.volume);
        UpdateVolumeLabel(AudioListener.volume);
    }

    private void OnVolumeChanged(float v)
    {
        AudioListener.volume = v;
        PlayerPrefs.SetFloat(VolumePref, v);
        UpdateVolumeLabel(v);
    }

    private void UpdateVolumeLabel(float v)
    {
        volumeValue.text = Mathf.RoundToInt(v * 100f) + "%";
    }

    private void ApplyFade()
    {
        float e = 1f - (1f - fade) * (1f - fade); // ease-out
        overlayGroup.alpha = e;
        float s = Mathf.Lerp(0.92f, 1f, e);
        mainCard.localScale = new Vector3(s, s, 1f);
        settingsCard.localScale = new Vector3(s, s, 1f);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // Esc was already used this frame to close the berry panel -- don't also open the pause menu
            if (Time.frameCount != BushUIWindow.EscConsumedFrame)
            {
                if (IsPaused)
                {
                    if (settingsCard.gameObject.activeSelf) ShowMain();
                    else Resume();
                }
                else
                {
                    Open();
                }
            }
        }

        if (IsPaused && fade < 1f)
        {
            fade = Mathf.MoveTowards(fade, 1f, Time.unscaledDeltaTime * 6f);
            ApplyFade();
        }
    }
}
