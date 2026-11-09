using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Lays out the existing Stairs UI when the scene loads: it scales with the screen size,
// keeps the texts inside their boxes and stops the start / game over elements from overlapping.
// It also puts the dimmed stairs photo behind the board and draws a frame around the playing field.
// StairsGameManager adds this component to the Canvas automatically.
public class StairsUiLayout : MonoBehaviour
{
    // size of the frame around the board in world units (the board itself is 3 x 5)
    public const float FrameWidth = 3.28f;
    public const float FrameHeight = 5.28f;

    // zoom: smaller camera size = bigger board on screen (it was 5 with the camera at y = 1)
    const float CameraSize = 3.4f;
    const float CameraCenterY = 0.35f;

    private Camera mainCamera;
    private SpriteRenderer backgroundRenderer;
    private int lastScreenWidth;
    private int lastScreenHeight;

    void Awake()
    {
        SetupScaler();
        LayoutStartScreen();
        LayoutGameOverScreen();
        CreateBackdrop();
    }

    void Update()
    {
        // keep the photo covering the screen when the window is resized
        if (Screen.width != lastScreenWidth || Screen.height != lastScreenHeight)
        {
            FitBackground();
        }
    }

    // The photo and the frame are normal objects in the game world (behind the board),
    // because a UI Canvas would be drawn on top of the board.
    void CreateBackdrop()
    {
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            return;
        }

        // zoom in on the board (same cubes, just bigger); the camera is centered on board + score
        if (mainCamera.orthographic)
        {
            mainCamera.orthographicSize = CameraSize;
            Vector3 cameraPosition = mainCamera.transform.position;
            mainCamera.transform.position = new Vector3(cameraPosition.x, CameraCenterY, cameraPosition.z);
        }

        Sprite photo = null;
        Transform startBackground = transform.Find("StartScreen/Background");
        if (startBackground != null)
        {
            Image photoImage = startBackground.GetComponent<Image>();
            if (photoImage != null)
            {
                photo = photoImage.sprite;
            }
        }

        if (photo != null)
        {
            GameObject photoObject = new GameObject("GameBackground");
            backgroundRenderer = photoObject.AddComponent<SpriteRenderer>();
            backgroundRenderer.sprite = photo;
            backgroundRenderer.color = new Color(0.4f, 0.4f, 0.4f, 1f); // dimmed
            backgroundRenderer.sortingOrder = -20;
            FitBackground();
        }

        Texture2D texture = new Texture2D(1, 1);
        texture.SetPixel(0, 0, Color.white);
        texture.Apply();
        Sprite white = Sprite.Create(texture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);

        // thin light border, then a thin dark margin between the border and the board
        CreateFrameRect("FrameBorder", white, new Vector2(FrameWidth, FrameHeight), new Color(1f, 1f, 1f, 0.9f), -10, 2f);
        CreateFrameRect("FramePanel", white, new Vector2(FrameWidth - 0.12f, FrameHeight - 0.12f), new Color(0.05f, 0.07f, 0.08f, 1f), -9, 1.5f);
    }

    void FitBackground()
    {
        if (backgroundRenderer == null || mainCamera == null)
        {
            return;
        }

        float viewHeight = mainCamera.orthographicSize * 2f;
        float viewWidth = viewHeight * mainCamera.aspect;
        Vector2 spriteSize = backgroundRenderer.sprite.bounds.size;
        float scale = Mathf.Max(viewWidth / spriteSize.x, viewHeight / spriteSize.y);

        Transform t = backgroundRenderer.transform;
        t.localScale = new Vector3(scale, scale, 1f);
        t.position = new Vector3(mainCamera.transform.position.x, mainCamera.transform.position.y, 5f);

        lastScreenWidth = Screen.width;
        lastScreenHeight = Screen.height;
    }

    static void CreateFrameRect(string rectName, Sprite white, Vector2 size, Color color, int sortingOrder, float z)
    {
        GameObject rect = new GameObject(rectName);
        rect.transform.position = new Vector3(0f, 0f, z);
        rect.transform.localScale = new Vector3(size.x, size.y, 1f);
        SpriteRenderer frameRenderer = rect.AddComponent<SpriteRenderer>();
        frameRenderer.sprite = white;
        frameRenderer.color = color;
        frameRenderer.sortingOrder = sortingOrder;
    }

    void SetupScaler()
    {
        CanvasScaler scaler = GetComponent<CanvasScaler>();
        if (scaler == null)
        {
            return;
        }

        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1280f, 720f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;
    }

    void LayoutStartScreen()
    {
        Transform start = transform.Find("StartScreen");
        if (start == null)
        {
            return;
        }

        Stretch(start);

        // photo fills the whole screen, cropped instead of stretched
        Transform background = start.Find("Background");
        if (background != null)
        {
            Stretch(background);
            FillParentKeepingAspect(background);
            background.SetAsFirstSibling();
        }

        // dark panel behind the texts so they are readable on top of the photo
        RectTransform panel = GetOrCreatePanel(start, "TextPanel", new Color(0f, 0f, 0f, 0.6f));
        Place(panel, new Vector2(0.08f, 0.55f), new Vector2(0.92f, 0.95f));
        panel.SetSiblingIndex(1);

        SetupText(start.Find("Title"), new Vector2(0.12f, 0.80f), new Vector2(0.88f, 0.93f), 60f, 24f);
        SetupText(start.Find("Instructions"), new Vector2(0.12f, 0.58f), new Vector2(0.88f, 0.78f), 44f, 20f);

        Transform button = start.Find("StartButton");
        if (button != null)
        {
            PlaceFixed(button, new Vector2(0.5f, 0.30f), Vector2.zero, new Vector2(320f, 80f));
            SetButtonLabelSize(button, 36f);
            button.SetAsLastSibling();
        }
    }

    void LayoutGameOverScreen()
    {
        Transform over = transform.Find("GameOverScreen");
        if (over == null)
        {
            return;
        }

        Stretch(over);

        // dims the whole game behind the card
        Transform dim = over.Find("Background");
        if (dim != null)
        {
            Stretch(dim);
            Image dimImage = dim.GetComponent<Image>();
            if (dimImage != null)
            {
                dimImage.color = new Color(0f, 0f, 0f, 0.7f);
            }
            dim.SetAsFirstSibling();
        }

        RectTransform card = GetOrCreatePanel(over, "Card", new Color(0.12f, 0.14f, 0.17f, 0.95f));
        PlaceFixed(card, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(560f, 340f));
        card.SetSiblingIndex(1);

        Transform score = over.Find("ScoreText");
        if (score != null)
        {
            PlaceFixed(score, new Vector2(0.5f, 0.5f), new Vector2(0f, 50f), new Vector2(480f, 160f));
            SetupTextStyle(score, 56f, 24f);
            score.SetAsLastSibling();
        }

        Transform retry = over.Find("TryAgainButton");
        if (retry != null)
        {
            PlaceFixed(retry, new Vector2(0.5f, 0.5f), new Vector2(0f, -100f), new Vector2(300f, 70f));
            SetButtonLabelSize(retry, 32f);
            retry.SetAsLastSibling();
        }
    }

    // ---- helpers ----

    static RectTransform Place(Transform t, Vector2 anchorMin, Vector2 anchorMax)
    {
        RectTransform rect = (RectTransform)t;
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        rect.localScale = Vector3.one;
        return rect;
    }

    static RectTransform Stretch(Transform t)
    {
        return Place(t, Vector2.zero, Vector2.one);
    }

    static RectTransform PlaceFixed(Transform t, Vector2 anchor, Vector2 anchoredPosition, Vector2 size)
    {
        RectTransform rect = (RectTransform)t;
        rect.anchorMin = anchor;
        rect.anchorMax = anchor;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = size;
        rect.localScale = Vector3.one;
        return rect;
    }

    static void SetupText(Transform t, Vector2 anchorMin, Vector2 anchorMax, float maxSize, float minSize)
    {
        if (t == null)
        {
            return;
        }

        Place(t, anchorMin, anchorMax);
        SetupTextStyle(t, maxSize, minSize);
        t.SetAsLastSibling();
    }

    static void SetupTextStyle(Transform t, float maxSize, float minSize)
    {
        TMP_Text text = t.GetComponent<TMP_Text>();
        if (text == null)
        {
            return;
        }

        text.enableAutoSizing = true;
        text.fontSizeMin = minSize;
        text.fontSizeMax = maxSize;
        text.textWrappingMode = TextWrappingModes.Normal;
        text.alignment = TextAlignmentOptions.Center;
    }

    static void SetButtonLabelSize(Transform button, float size)
    {
        TMP_Text label = button.GetComponentInChildren<TMP_Text>(true);
        if (label != null)
        {
            label.fontSize = size;
        }
    }

    static void FillParentKeepingAspect(Transform t)
    {
        Image image = t.GetComponent<Image>();
        if (image == null || image.sprite == null)
        {
            return;
        }

        AspectRatioFitter fitter = t.GetComponent<AspectRatioFitter>();
        if (fitter == null)
        {
            fitter = t.gameObject.AddComponent<AspectRatioFitter>();
        }

        fitter.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;
        fitter.aspectRatio = image.sprite.rect.width / image.sprite.rect.height;
    }

    static RectTransform GetOrCreatePanel(Transform parent, string panelName, Color color)
    {
        Transform existing = parent.Find(panelName);
        GameObject panel;

        if (existing != null)
        {
            panel = existing.gameObject;
        }
        else
        {
            panel = new GameObject(panelName, typeof(RectTransform), typeof(Image));
            panel.transform.SetParent(parent, false);
        }

        Image image = panel.GetComponent<Image>();
        image.color = color;
        image.raycastTarget = false;
        return (RectTransform)panel.transform;
    }
}