using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Replaces the plain "Time is up! / Game Complete!" panels with cards in the same style as the HUD and
// the pause menu. Created together with GameHud (see GameHud.Ensure); it watches GameManager and shows
//   - "TIME'S UP!"     when a level attempt runs out of time  (Try Again / Main Menu)
//   - "GAME COMPLETE!" after the last level is cleared        (Play Again / Main Menu)
// While it exists, GameManager no longer switches on the old ResultPanel / GameCompletePanel.
public class EndScreen : MonoBehaviour
{
    public static EndScreen Instance;

    private GameObject overlay;
    private CanvasGroup group;
    private RectTransform failCard, winCard;
    private TextMeshProUGUI failSub;
    private RectTransform failBarFill;
    private TextMeshProUGUI winSub;
    private RectTransform medalsRow;

    private bool shown;
    private float fade;

    private void Awake()
    {
        Instance = this;
        UiKit.EnsureFont();
        Build();
        overlay.SetActive(false);
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    // ------------------------------------------------------------------ build
    private void Build()
    {
        Canvas canvas = UiKit.MakeCanvas("EndCanvas", 150, transform);

        overlay = UiKit.Rt("Overlay", canvas.transform).gameObject;
        UiKit.Fill(overlay.GetComponent<RectTransform>());
        Image dim = overlay.AddComponent<Image>();
        dim.color = new Color(0.04f, 0.04f, 0.14f, 0.72f);
        dim.raycastTarget = true;
        group = overlay.AddComponent<CanvasGroup>();

        BuildFailCard();
        BuildWinCard();
    }

    private UiKit.PanelParts NewCard(string name, Vector2 size)
    {
        UiKit.PanelParts p = UiKit.Panel(overlay.transform, name, size, 44,
            UiKit.Navy.WithA(0.98f), UiKit.BerryLight.WithA(0.5f), 3f);
        UiKit.Center(p.root, size);
        return p;
    }

    private static void Top(RectTransform rt, float y, Vector2 size)
    {
        UiKit.Place(rt, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, y), size);
    }

    private TextMeshProUGUI Title(Transform parent, string text, Color color, float y)
    {
        TextMeshProUGUI t = UiKit.Txt("Title", parent, text, 58f, color, FontStyles.Bold);
        t.characterSpacing = 5f;
        t.enableAutoSizing = true;
        t.fontSizeMin = 34f;
        t.fontSizeMax = 58f;
        Top(t.rectTransform, y, new Vector2(580f, 76f));
        return t;
    }

    private void BuildFailCard()
    {
        UiKit.PanelParts card = NewCard("FailCard", new Vector2(640f, 590f));
        failCard = card.root;
        Transform c = card.content;

        Image berry = UiKit.Img("Berry", c, UiKit.BerryIcon(), Color.white);
        Top(berry.rectTransform, -34f, new Vector2(96f, 96f));

        Title(c, "TIME'S UP!", UiKit.Gold, -144f);

        failSub = UiKit.Txt("Sub", c, "", 30f, UiKit.BerryLight);
        failSub.richText = true;
        Top(failSub.rectTransform, -232f, new Vector2(560f, 42f));

        Image track = UiKit.Img("BarTrack", c, UiKit.Rounded(9), Color.white.WithA(0.14f), true);
        Top(track.rectTransform, -292f, new Vector2(460f, 18f));
        Image fill = UiKit.Img("BarFill", track.transform, UiKit.Rounded(9), UiKit.Berry, true);
        UiKit.Fill(fill.rectTransform);
        fill.rectTransform.anchorMax = new Vector2(0f, 1f);
        failBarFill = fill.rectTransform;

        Button again = UiKit.MakeButton(c, "TryAgainButton", "TRY AGAIN", new Vector2(420f, 96f),
            UiKit.Mint, UiKit.MintDark, Hex("#0B2E22"), 38f, OnTryAgain);
        Top((RectTransform)again.transform, -356f, new Vector2(420f, 96f));

        Button menu = UiKit.MakeButton(c, "MainMenuButton", "MAIN MENU", new Vector2(420f, 76f),
            UiKit.NavyLight, UiKit.NavyMid, UiKit.Cream, 32f, OnMainMenu);
        Top((RectTransform)menu.transform, -470f, new Vector2(420f, 76f));
    }

    private void BuildWinCard()
    {
        UiKit.PanelParts card = NewCard("WinCard", new Vector2(640f, 570f));
        winCard = card.root;
        Transform c = card.content;

        medalsRow = UiKit.Rt("Medals", c);
        Top(medalsRow, -34f, new Vector2(560f, 96f));

        Title(c, "GAME COMPLETE!", UiKit.Mint, -148f);

        winSub = UiKit.Txt("Sub", c, "", 30f, UiKit.BerryLight);
        Top(winSub.rectTransform, -236f, new Vector2(560f, 42f));

        Button again = UiKit.MakeButton(c, "PlayAgainButton", "PLAY AGAIN", new Vector2(420f, 96f),
            UiKit.Mint, UiKit.MintDark, Hex("#0B2E22"), 38f, OnPlayAgain);
        Top((RectTransform)again.transform, -318f, new Vector2(420f, 96f));

        Button menu = UiKit.MakeButton(c, "MainMenuButton", "MAIN MENU", new Vector2(420f, 76f),
            UiKit.NavyLight, UiKit.NavyMid, UiKit.Cream, 32f, OnMainMenu);
        Top((RectTransform)menu.transform, -432f, new Vector2(420f, 76f));
    }

    private static Color Hex(string h) { return UiKit.Hex(h); }

    // ------------------------------------------------------------------ show / hide
    private void Show(bool won)
    {
        GameManager gm = GameManager.Instance;
        shown = true;

        // The berry panel may still be open (time ran out while picking) -- close it first,
        // it re-enables player input, which we then switch off again below.
        if (BushUIWindow.Instance != null && BushUIWindow.Instance.IsOpen) BushUIWindow.Instance.CloseBushUI();
        PlayerMovement.InputEnabled = false;

        failCard.gameObject.SetActive(!won);
        winCard.gameObject.SetActive(won);

        if (won)
        {
            int count = gm != null ? gm.LevelCount : 0;
            BuildMedals(count);
            winSub.text = count > 0 ? "All " + count + " levels cleared. Great picking!" : "Great picking!";
        }
        else if (gm != null)
        {
            failSub.text = gm.Score + " / " + gm.TargetScore + " berries collected";
            float f = gm.TargetScore > 0 ? Mathf.Clamp01((float)gm.Score / gm.TargetScore) : 0f;
            failBarFill.anchorMax = new Vector2(f, 1f);
        }

        overlay.SetActive(true);
        fade = 0f;
        ApplyFade();
    }

    private void Hide()
    {
        shown = false;
        overlay.SetActive(false);
        PlayerMovement.InputEnabled = true;
    }

    private void BuildMedals(int count)
    {
        for (int i = medalsRow.childCount - 1; i >= 0; i--)
            Destroy(medalsRow.GetChild(i).gameObject);

        const float step = 96f;
        for (int i = 0; i < count; i++)
        {
            float x = (i - (count - 1) * 0.5f) * step;
            Image glow = UiKit.Img("Glow", medalsRow, UiKit.Glow(), UiKit.Mint.WithA(0.35f));
            UiKit.Place(glow.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(x, 0f), new Vector2(120f, 120f));
            Image disc = UiKit.Img("Disc", medalsRow, UiKit.Circle(), UiKit.Mint);
            UiKit.Place(disc.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(x, 0f), new Vector2(72f, 72f));
            Image check = UiKit.Img("Check", medalsRow, UiKit.Check(), Color.white);
            UiKit.Place(check.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(x, 0f), new Vector2(36f, 36f));
        }
    }

    private void ApplyFade()
    {
        float e = 1f - (1f - fade) * (1f - fade);
        group.alpha = e;
        float s = Mathf.Lerp(0.92f, 1f, e);
        failCard.localScale = new Vector3(s, s, 1f);
        winCard.localScale = new Vector3(s, s, 1f);
    }

    private void Update()
    {
        GameManager gm = GameManager.Instance;
        if (gm == null) return;

        if (gm.IsGameOver && !shown) Show(gm.GameWon);
        else if (!gm.IsGameOver && shown) Hide();

        if (shown && fade < 1f)
        {
            fade = Mathf.MoveTowards(fade, 1f, Time.unscaledDeltaTime * 6f);
            ApplyFade();
        }
    }

    // ------------------------------------------------------------------ buttons
    private void OnTryAgain()
    {
        if (GameManager.Instance != null) GameManager.Instance.RestartGame(); // Update() hides the card next frame
    }

    private void OnPlayAgain()
    {
        if (GameManager.Instance != null) GameManager.Instance.RestartFullGame();
    }

    private void OnMainMenu()
    {
        if (PauseMenu.Instance != null) PauseMenu.Instance.MainMenu();
    }
}
