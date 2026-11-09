using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

// Builds the pause button, the pause menu and the info page in code, so nothing has to be wired up in the editor.
// StairsGameManager adds this component to the Canvas automatically.
// To change the info text from the Inspector, add this component to the Canvas yourself first.
public class StairsPauseMenu : MonoBehaviour
{
    [Header("Info page")]
    [SerializeField] private string infoTitle = "About the game";

    [Tooltip("{max} is replaced with the number of steps needed to win.")]
    [TextArea(6, 16)]
    [SerializeField]
    private string infoBody =
        "Climb the stairs all the way to the top!\n\n" +
        "A / D or the arrow keys: change lane\n" +
        "Click the left or right half of the screen: change lane\n" +
        "Avoid the cats. Reach {max} steps to win.\n\n" +
        "P or Esc: pause";

    private StairsGameManager manager;
    private GameObject pauseButton;
    private GameObject pausePanel;
    private GameObject infoPanel;
    private TMP_Text soundLabel;
    private TMP_Text infoBodyText;

    private static readonly Color ButtonColor = new Color(0.18f, 0.55f, 0.30f, 1f);
    private static readonly Color CardColor = new Color(0.12f, 0.14f, 0.17f, 0.97f);
    private static readonly Color DimColor = new Color(0f, 0f, 0f, 0.7f);

    void Awake()
    {
        BuildPauseButton();
        BuildPausePanel();
        BuildInfoPanel();
        BuildStartScreenInfoButton();
    }

    void Start()
    {
        manager = FindAnyObjectByType<StairsGameManager>();
        RefreshSoundLabel();
    }

    void Update()
    {
        if (manager == null)
        {
            return;
        }

        bool canPause = manager.GameStarted && !manager.GameEnded;

        // the pause button is only visible while playing
        if (pauseButton.activeSelf != canPause)
        {
            pauseButton.SetActive(canPause);
        }

        // keeps the menu in sync (also when the game paused itself, e.g. the browser tab lost focus)
        if (pausePanel.activeSelf != manager.IsPaused)
        {
            pausePanel.SetActive(manager.IsPaused);
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

    // an Info button under the Start button, so the info page can be read before playing
    void BuildStartScreenInfoButton()
    {
        Transform start = transform.Find("StartScreen");
        if (start == null)
        {
            return;
        }

        CreateButton(start, "Info", new Vector2(0.5f, 0.30f), new Vector2(0f, -85f), new Vector2(220f, 56f), OpenInfo);
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
        image.color = DimColor; // raycast target stays ON, so clicks do not reach the game behind it
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