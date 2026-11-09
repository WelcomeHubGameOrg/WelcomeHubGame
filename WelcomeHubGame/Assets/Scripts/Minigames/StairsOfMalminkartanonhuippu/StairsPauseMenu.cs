using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

// Builds the pause button, the pause menu and the info page in code, so nothing has to be wired up in the editor.
// StairsGameManager adds this component to the Canvas automatically.
public class StairsPauseMenu : MonoBehaviour
{
    [Header("Info page")]
    [SerializeField] private string infoTitle = "Malminkartano Stairs";

    [Tooltip("{max} is replaced with the number of steps needed to win.")]
    [TextArea(6, 16)]
    [SerializeField]
    private string infoBody =
        "Malminkartanonhuippu is the highest point in Helsinki, about 90 m above sea level. " +
        "The hill grew over the years as a landfill site (1976-96) and is now a popular place to exercise.\n\n" +
        "The long stairs to the top have {max} steps, and the view from the summit covers the whole city. " +
        "In winter the hill is a favourite spot for sledding.\n\n" +
        "In the game: climb all {max} steps and avoid the cats.\n" +
        "Auto Climb: you only change lane. Step Mode: every move is one step.\n" +
        "P or Esc: pause";

    private StairsGameManager manager;
    private GameObject pauseButton;
    private GameObject pausePanel;
    private GameObject infoPanel;
    private TMP_Text soundLabel;
    private TMP_Text infoBodyText;
    private Image autoModeImage;
    private Image stepModeImage;
    private GameObject countdownBox;
    private TMP_Text countdownText;
    private int shownCountdown;
    private GameObject winPanel;
    private TMP_Text winStatsText;

    private static readonly Color ButtonColor = new Color(0.18f, 0.55f, 0.30f, 1f);
    private static readonly Color InactiveButtonColor = new Color(0.28f, 0.30f, 0.34f, 1f);
    private static readonly Color CardColor = new Color(0.12f, 0.14f, 0.17f, 0.97f);
    private static readonly Color DimColor = new Color(0f, 0f, 0f, 0.7f);

    void Awake()
    {
        BuildPauseButton();
        BuildPausePanel();
        BuildInfoPanel();
        BuildCountdown();
        BuildWinPanel();
        BuildStartScreenButtons();
    }

    void Start()
    {
        manager = FindAnyObjectByType<StairsGameManager>();
        RefreshSoundLabel();
        RefreshModeButtons();
    }

    void Update()
    {
        if (manager == null)
        {
            return;
        }

        // win screen
        if (manager.HasWon && !winPanel.activeSelf)
        {
            winStatsText.text = manager.WinMessage;
            winPanel.transform.SetAsLastSibling();
            winPanel.SetActive(true);
        }

        bool canPause = manager.GameStarted && !manager.GameEnded;

        // the pause button is only visible while playing
        if (pauseButton.activeSelf != canPause)
        {
            pauseButton.SetActive(canPause);
        }

        // keeps the menu in sync (also when the game paused itself, the browser tab lost focus)
        // (the menu is hidden during the 3-2-1 after Resume)
        bool showPanel = manager.IsPaused && !manager.IsCountingDown;
        if (pausePanel.activeSelf != showPanel)
        {
            pausePanel.SetActive(showPanel);
        }

        int countdown = manager.CountdownValue;
        if (countdown != shownCountdown)
        {
            shownCountdown = countdown;
            countdownBox.SetActive(countdown > 0);

            if (countdown > 0)
            {
                countdownText.text = countdown.ToString();
            }
        }

        Keyboard keyboard = Keyboard.current;
        if (keyboard != null && (keyboard.escapeKey.wasPressedThisFrame || keyboard.pKey.wasPressedThisFrame))
        {
            if (infoPanel.activeSelf)
            {
                CloseInfo();
            }
            else if (manager.IsPaused)
            {
                manager.Resume();
            }
            else if (canPause)
            {
                manager.Pause();
            }
        }
    }

    // ---- button actions ----

    void OnResume()
    {
        manager.Resume();
    }

    void OnRestart()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    void OnToggleSound()
    {
        manager.SetMuted(!manager.IsMuted);
        RefreshSoundLabel();
    }

    void OpenInfo()
    {
        int max = manager != null ? manager.MaxScoreValue : 0;
        infoBodyText.text = infoBody.Replace("{max}", max.ToString());
        infoPanel.transform.SetAsLastSibling();
        infoPanel.SetActive(true);
    }

    void CloseInfo()
    {
        infoPanel.SetActive(false);
    }

    void RefreshSoundLabel()
    {
        if (soundLabel != null && manager != null)
        {
            soundLabel.text = manager.IsMuted ? "Sound: Off" : "Sound: On";
        }
    }

    // ---- UI building ----

    void BuildPauseButton()
    {
        GameObject button = CreateButton(transform, "II", new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(72f, 72f), () => manager.Pause());
        pauseButton = button;

        // top-right corner
        RectTransform rect = (RectTransform)button.transform;
        rect.anchorMin = Vector2.one;
        rect.anchorMax = Vector2.one;
        rect.pivot = Vector2.one;
        rect.anchoredPosition = new Vector2(-24f, -24f);

        button.SetActive(false);
    }

    void BuildPausePanel()
    {
        pausePanel = CreateOverlay("PausePanel");
        CreateCard(pausePanel.transform, new Vector2(520f, 480f));

        CreateText(pausePanel.transform, "Paused", 56f, 24f, new Vector2(0.5f, 0.5f), new Vector2(0f, 165f), new Vector2(420f, 80f));

        Vector2 size = new Vector2(320f, 60f);
        Vector2 center = new Vector2(0.5f, 0.5f);

        CreateButton(pausePanel.transform, "Resume", center, new Vector2(0f, 70f), size, OnResume);
        CreateButton(pausePanel.transform, "Info", center, new Vector2(0f, -10f), size, OpenInfo);

        GameObject soundButton = CreateButton(pausePanel.transform, "Sound: On", center, new Vector2(0f, -90f), size, OnToggleSound);
        soundLabel = soundButton.GetComponentInChildren<TMP_Text>();

        CreateButton(pausePanel.transform, "Restart", center, new Vector2(0f, -170f), size, OnRestart);

        pausePanel.SetActive(false);
    }

    void BuildInfoPanel()
    {
        infoPanel = CreateOverlay("InfoPanel");
        CreateCard(infoPanel.transform, new Vector2(800f, 520f));

        CreateText(infoPanel.transform, infoTitle, 48f, 24f, new Vector2(0.5f, 0.5f), new Vector2(0f, 210f), new Vector2(720f, 70f));

        infoBodyText = CreateText(infoPanel.transform, infoBody, 30f, 16f, new Vector2(0.5f, 0.5f), new Vector2(0f, 0f), new Vector2(720f, 330f));
        infoBodyText.alignment = TextAlignmentOptions.TopLeft;

        CreateButton(infoPanel.transform, "Close", new Vector2(0.5f, 0.5f), new Vector2(0f, -205f), new Vector2(240f, 60f), CloseInfo);

        infoPanel.SetActive(false);
    }

    // mode selection above the Start button and an Info button under it
    void BuildStartScreenButtons()
    {
        Transform start = transform.Find("StartScreen");
        if (start == null)
        {
            return;
        }

        Vector2 anchor = new Vector2(0.5f, 0.30f);
        Vector2 modeSize = new Vector2(240f, 56f);

        GameObject autoButton = CreateButton(start, "Auto Climb", anchor, new Vector2(-130f, 85f), modeSize, () => SelectMode(true));
        GameObject stepButton = CreateButton(start, "Step Mode", anchor, new Vector2(130f, 85f), modeSize, () => SelectMode(false));
        autoModeImage = autoButton.GetComponent<Image>();
        stepModeImage = stepButton.GetComponent<Image>();

        CreateButton(start, "Info", anchor, new Vector2(0f, -85f), new Vector2(220f, 56f), OpenInfo);
    }

    void SelectMode(bool auto)
    {
        manager.SetAutoAdvance(auto);
        RefreshModeButtons();
    }

    // the selected mode is green, the other one is gray
    void RefreshModeButtons()
    {
        if (manager == null || autoModeImage == null || stepModeImage == null)
        {
            return;
        }

        autoModeImage.color = manager.AutoAdvance ? ButtonColor : InactiveButtonColor;
        stepModeImage.color = manager.AutoAdvance ? InactiveButtonColor : ButtonColor;
    }

    void BuildWinPanel()
    {
        winPanel = CreateOverlay("WinPanel");
        CreateCard(winPanel.transform, new Vector2(560f, 380f));

        Vector2 center = new Vector2(0.5f, 0.5f);
        CreateText(winPanel.transform, "You reached the top!", 56f, 28f, center, new Vector2(0f, 110f), new Vector2(500f, 90f));
        winStatsText = CreateText(winPanel.transform, "", 40f, 22f, center, new Vector2(0f, 10f), new Vector2(500f, 110f));

        // reloads the scene, so the start screen (mode selection) comes back
        CreateButton(winPanel.transform, "Menu", center, new Vector2(0f, -110f), new Vector2(320f, 60f), OnRestart);

        winPanel.SetActive(false);
    }

    // big 3-2-1 in the middle of the screen
    void BuildCountdown()
    {
        countdownBox = new GameObject("Countdown", typeof(RectTransform), typeof(Image));
        countdownBox.transform.SetParent(transform, false);
        Place(countdownBox, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(240f, 240f));

        Image image = countdownBox.GetComponent<Image>();
        image.color = new Color(0f, 0f, 0f, 0.6f);
        image.raycastTarget = false;

        countdownText = CreateText(countdownBox.transform, "3", 160f, 60f, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(240f, 240f));
        countdownText.fontStyle = FontStyles.Bold;

        countdownBox.SetActive(false);
    }

    GameObject CreateOverlay(string overlayName)
    {
        GameObject overlay = new GameObject(overlayName, typeof(RectTransform), typeof(Image));
        overlay.transform.SetParent(transform, false);

        RectTransform rect = (RectTransform)overlay.transform;
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        rect.localScale = Vector3.one;

        Image image = overlay.GetComponent<Image>();
        image.color = DimColor; 
        return overlay;
    }

    static void CreateCard(Transform parent, Vector2 size)
    {
        GameObject card = new GameObject("Card", typeof(RectTransform), typeof(Image));
        card.transform.SetParent(parent, false);
        Place(card, new Vector2(0.5f, 0.5f), Vector2.zero, size);

        Image image = card.GetComponent<Image>();
        image.color = CardColor;
        image.raycastTarget = false;
    }

    static TMP_Text CreateText(Transform parent, string content, float maxSize, float minSize, Vector2 anchor, Vector2 position, Vector2 size)
    {
        GameObject textObject = new GameObject("Text", typeof(RectTransform));
        textObject.transform.SetParent(parent, false);
        Place(textObject, anchor, position, size);

        TextMeshProUGUI text = textObject.AddComponent<TextMeshProUGUI>();
        text.text = content;
        text.color = Color.white;
        text.alignment = TextAlignmentOptions.Center;
        text.enableAutoSizing = true;
        text.fontSizeMin = minSize;
        text.fontSizeMax = maxSize;
        text.textWrappingMode = TextWrappingModes.Normal;
        text.raycastTarget = false;
        return text;
    }

    GameObject CreateButton(Transform parent, string label, Vector2 anchor, Vector2 position, Vector2 size, System.Action onClick)
    {
        GameObject buttonObject = new GameObject(label + "Button", typeof(RectTransform), typeof(Image), typeof(Button));
        buttonObject.transform.SetParent(parent, false);
        Place(buttonObject, anchor, position, size);

        Image image = buttonObject.GetComponent<Image>();
        image.color = ButtonColor;

        Button button = buttonObject.GetComponent<Button>();
        button.targetGraphic = image;
        button.onClick.AddListener(() =>
        {
            if (manager != null)
            {
                manager.PlayClick();
            }

            onClick();
        });

        CreateText(buttonObject.transform, label, 32f, 16f, new Vector2(0.5f, 0.5f), Vector2.zero, size);
        return buttonObject;
    }

    static void Place(GameObject go, Vector2 anchor, Vector2 position, Vector2 size)
    {
        RectTransform rect = (RectTransform)go.transform;
        rect.anchorMin = anchor;
        rect.anchorMax = anchor;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
        rect.localScale = Vector3.one;
    }
}