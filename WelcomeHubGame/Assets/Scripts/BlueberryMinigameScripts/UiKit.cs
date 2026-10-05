using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Shared helpers for the code-built HUD (GameHud.cs) and pause menu (PauseMenu.cs):
// the colour palette, procedurally drawn sprites (no image files needed) and small widget builders.
public static class UiKit
{
    // ---- Palette (blueberry theme) ----
    public static readonly Color Navy       = Hex("#17163A");
    public static readonly Color NavyMid    = Hex("#201F52");
    public static readonly Color NavyLight  = Hex("#34327A");
    public static readonly Color Berry      = Hex("#5B7CFA");
    public static readonly Color BerryLight = Hex("#A9BBFF");
    public static readonly Color Cream      = Hex("#F4F1FF");
    public static readonly Color Gold       = Hex("#FFC857");
    public static readonly Color Danger     = Hex("#FF5C7A");
    public static readonly Color Mint       = Hex("#38D39F");
    public static readonly Color MintDark   = Hex("#1E9E74");

    public static Color Hex(string h)
    {
        ColorUtility.TryParseHtmlString(h, out Color c);
        return c;
    }

    public static Color WithA(this Color c, float a)
    {
        c.a = a;
        return c;
    }

    // ---- Font ----
    public static TMP_FontAsset Font;

    public static void EnsureFont()
    {
        if (Font != null) return;
        TMP_Text any = Object.FindFirstObjectByType<TMP_Text>(FindObjectsInactive.Include);
        Font = any != null ? any.font : TMP_Settings.defaultFontAsset;
    }

    // ---- Procedural sprites ----
    private static readonly Dictionary<int, Sprite> roundedCache = new Dictionary<int, Sprite>();
    private static Sprite circleSprite, ringSprite, glowSprite, berrySprite, checkSprite;

    private static Texture2D NewTex(int size)
    {
        Texture2D t = new Texture2D(size, size, TextureFormat.RGBA32, false);
        t.wrapMode = TextureWrapMode.Clamp;
        t.filterMode = FilterMode.Bilinear;
        return t;
    }

    private static Sprite ToSprite(Texture2D t, Color32[] px, Vector4 border)
    {
        t.SetPixels32(px);
        t.Apply();
        return Sprite.Create(t, new Rect(0, 0, t.width, t.height), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect, border);
    }

    // White rounded rectangle, 9-sliced: the corners keep a constant radius at any size.
    public static Sprite Rounded(int radius)
    {
        if (roundedCache.TryGetValue(radius, out Sprite cached) && cached != null) return cached;

        int size = radius * 2 + 2;
        Texture2D tex = NewTex(size);
        Color32[] px = new Color32[size * size];
        float half = size * 0.5f;
        float inner = half - radius;
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float cx = Mathf.Abs(x + 0.5f - half) - inner;
                float cy = Mathf.Abs(y + 0.5f - half) - inner;
                float outside = Mathf.Sqrt(Mathf.Max(cx, 0f) * Mathf.Max(cx, 0f) + Mathf.Max(cy, 0f) * Mathf.Max(cy, 0f));
                float d = outside + Mathf.Min(Mathf.Max(cx, cy), 0f) - radius;
                float a = Mathf.Clamp01(0.5f - d);
                px[y * size + x] = new Color(1f, 1f, 1f, a);
            }
        }
        Sprite s = ToSprite(tex, px, new Vector4(radius, radius, radius, radius));
        roundedCache[radius] = s;
        return s;
    }

    public static Sprite Circle()
    {
        if (circleSprite != null) return circleSprite;
        const int size = 128;
        Texture2D tex = NewTex(size);
        Color32[] px = new Color32[size * size];
        float r = size * 0.5f - 0.5f;
        for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float dx = x + 0.5f - size * 0.5f, dy = y + 0.5f - size * 0.5f;
                float a = Mathf.Clamp01(r - Mathf.Sqrt(dx * dx + dy * dy) + 0.5f);
                px[y * size + x] = new Color(1f, 1f, 1f, a);
            }
        circleSprite = ToSprite(tex, px, Vector4.zero);
        return circleSprite;
    }

    // Thin ring, used with Image.Type.Filled (radial) for the countdown.
    public static Sprite Ring()
    {
        if (ringSprite != null) return ringSprite;
        const int size = 256;
        Texture2D tex = NewTex(size);
        Color32[] px = new Color32[size * size];
        float outer = size * 0.5f - 1f;
        float innerR = outer * 0.84f;
        for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float dx = x + 0.5f - size * 0.5f, dy = y + 0.5f - size * 0.5f;
                float dist = Mathf.Sqrt(dx * dx + dy * dy);
                float a = Mathf.Clamp01(outer - dist + 0.5f) * Mathf.Clamp01(dist - innerR + 0.5f);
                px[y * size + x] = new Color(1f, 1f, 1f, a);
            }
        ringSprite = ToSprite(tex, px, Vector4.zero);
        return ringSprite;
    }

    // Soft round glow (white, alpha falls off towards the edge).
    public static Sprite Glow()
    {
        if (glowSprite != null) return glowSprite;
        const int size = 128;
        Texture2D tex = NewTex(size);
        Color32[] px = new Color32[size * size];
        float r = size * 0.5f;
        for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float dx = x + 0.5f - r, dy = y + 0.5f - r;
                float t = Mathf.Clamp01(1f - Mathf.Sqrt(dx * dx + dy * dy) / r);
                px[y * size + x] = new Color(1f, 1f, 1f, t * t);
            }
        glowSprite = ToSprite(tex, px, Vector4.zero);
        return glowSprite;
    }

    // Shaded blueberry icon with a highlight and the little dimple on top.
    public static Sprite BerryIcon()
    {
        if (berrySprite != null) return berrySprite;
        const int size = 128;
        Texture2D tex = NewTex(size);
        Color32[] px = new Color32[size * size];
        Color dark = Hex("#2B3BB8");
        Color lightCol = Hex("#7E9BFF");
        Color rim = new Color(dark.r * 0.75f, dark.g * 0.75f, dark.b * 0.75f, 1f);
        Color deep = Hex("#141C6B");
        Vector3 light = new Vector3(-0.45f, 0.6f, 0.66f).normalized;
        const float R = 0.88f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float nx = (x + 0.5f) / size * 2f - 1f;
                float ny = (y + 0.5f) / size * 2f - 1f;
                float bx = nx, by = ny + 0.04f;
                float dist = Mathf.Sqrt(bx * bx + by * by);
                float a = Mathf.Clamp01((R - dist) * size * 0.5f + 0.5f);
                if (a <= 0f) { px[y * size + x] = new Color32(0, 0, 0, 0); continue; }

                float rr = Mathf.Min(dist / R, 1f);
                float nz = Mathf.Sqrt(Mathf.Max(0f, 1f - rr * rr));
                Vector3 n = new Vector3(bx / R, by / R, nz);
                float shade = Mathf.Clamp01(Vector3.Dot(n, light));
                Color c = Color.Lerp(dark, lightCol, shade);
                c = Color.Lerp(c, rim, Mathf.SmoothStep(0.7f, 1f, rr) * 0.55f);

                // specular highlight (upper left)
                float hx = bx + 0.32f, hy = by - 0.34f;
                float hd = Mathf.Sqrt(hx * hx + hy * hy * 1.6f);
                float h = Mathf.Clamp01(1f - hd / 0.16f);
                c = Color.Lerp(c, Color.white, h * h * 0.9f);

                // dimple at the top
                float dxp = bx - 0.02f, dyp = by - 0.46f;
                float dd = Mathf.Clamp01(1f - Mathf.Sqrt(dxp * dxp + dyp * dyp) / 0.13f);
                c = Color.Lerp(c, deep, dd * 0.85f);

                c.a = a;
                px[y * size + x] = c;
            }
        }
        berrySprite = ToSprite(tex, px, Vector4.zero);
        return berrySprite;
    }

    public static Sprite Check()
    {
        if (checkSprite != null) return checkSprite;
        const int size = 128;
        Texture2D tex = NewTex(size);
        Color32[] px = new Color32[size * size];
        Vector2 p1 = new Vector2(-0.48f, 0.02f), p2 = new Vector2(-0.12f, -0.36f), p3 = new Vector2(0.52f, 0.42f);
        const float thick = 0.13f;
        for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                Vector2 p = new Vector2((x + 0.5f) / size * 2f - 1f, (y + 0.5f) / size * 2f - 1f);
                float d = Mathf.Min(DistSeg(p, p1, p2), DistSeg(p, p2, p3));
                float a = Mathf.Clamp01((thick - d) * size * 0.5f + 0.5f);
                px[y * size + x] = new Color(1f, 1f, 1f, a);
            }
        checkSprite = ToSprite(tex, px, Vector4.zero);
        return checkSprite;
    }

    private static float DistSeg(Vector2 p, Vector2 a, Vector2 b)
    {
        Vector2 ab = b - a;
        float t = Mathf.Clamp01(Vector2.Dot(p - a, ab) / ab.sqrMagnitude);
        return (p - (a + ab * t)).magnitude;
    }

    // ---- Widget builders ----
    public static RectTransform Rt(string name, Transform parent)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.layer = 5; // UI
        go.transform.SetParent(parent, false);
        return go.GetComponent<RectTransform>();
    }

    public static Image Img(string name, Transform parent, Sprite sprite, Color color, bool sliced = false, bool raycast = false)
    {
        RectTransform rt = Rt(name, parent);
        Image im = rt.gameObject.AddComponent<Image>();
        im.sprite = sprite;
        im.color = color;
        im.type = sliced ? Image.Type.Sliced : Image.Type.Simple;
        im.raycastTarget = raycast;
        return im;
    }

    public static void Place(RectTransform rt, Vector2 anchor, Vector2 pivot, Vector2 pos, Vector2 size)
    {
        rt.anchorMin = anchor;
        rt.anchorMax = anchor;
        rt.pivot = pivot;
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
    }

    public static void Center(RectTransform rt, Vector2 size)
    {
        Place(rt, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, size);
    }

    // Stretch to the parent, with insets (left, bottom, right, top).
    public static void Fill(RectTransform rt, float l = 0f, float b = 0f, float r = 0f, float t = 0f)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.offsetMin = new Vector2(l, b);
        rt.offsetMax = new Vector2(-r, -t);
    }

    public static TextMeshProUGUI Txt(string name, Transform parent, string text, float size, Color color,
        FontStyles style = FontStyles.Normal, TextAlignmentOptions align = TextAlignmentOptions.Center)
    {
        RectTransform rt = Rt(name, parent);
        TextMeshProUGUI t = rt.gameObject.AddComponent<TextMeshProUGUI>();
        if (Font != null) t.font = Font;
        t.text = text;
        t.fontSize = size;
        t.color = color;
        t.fontStyle = style;
        t.alignment = align;
        t.raycastTarget = false;
        t.textWrappingMode = TextWrappingModes.NoWrap;
        t.overflowMode = TextOverflowModes.Overflow;
        return t;
    }

    public class PanelParts
    {
        public RectTransform root;     // position/size this one
        public RectTransform content;  // put children in here (inset by the border width)
        public Image borderImg;
        public Image fillImg;
    }

    // Rounded panel (or circle when radius <= 0) with a thin light border and a soft drop shadow.
    public static PanelParts Panel(Transform parent, string name, Vector2 size, int radius, Color fill, Color border, float borderWidth, bool shadow = true)
    {
        bool circle = radius <= 0;
        Image outer = Img(name, parent, circle ? Circle() : Rounded(radius), border, !circle);
        outer.rectTransform.sizeDelta = size;
        if (shadow)
        {
            Shadow sh = outer.gameObject.AddComponent<Shadow>();
            sh.effectColor = new Color(0f, 0f, 0f, 0.38f);
            sh.effectDistance = new Vector2(0f, -7f);
        }
        int innerRadius = Mathf.Max(2, radius - Mathf.RoundToInt(borderWidth));
        Image inner = Img("Fill", outer.transform, circle ? Circle() : Rounded(innerRadius), fill, !circle);
        Fill(inner.rectTransform, borderWidth, borderWidth, borderWidth, borderWidth);

        return new PanelParts { root = outer.rectTransform, content = inner.rectTransform, borderImg = outer, fillImg = inner };
    }

    // Chunky button: coloured face on top of a darker "lip" so it reads as raised; pops on hover/press.
    public static Button MakeButton(Transform parent, string name, string label, Vector2 size,
        Color face, Color lip, Color textColor, float fontSize, UnityAction onClick)
    {
        RectTransform root = Rt(name, parent);
        root.sizeDelta = size;
        int r = Mathf.RoundToInt(size.y * 0.32f);

        Image lipImg = Img("Lip", root, Rounded(r), lip, true);
        Fill(lipImg.rectTransform);
        Image faceImg = Img("Face", root, Rounded(r), face, true, true);
        Fill(faceImg.rectTransform, 0f, 8f, 0f, 0f); // the 8px left at the bottom shows the lip

        TextMeshProUGUI txt = Txt("Label", faceImg.transform, label, fontSize, textColor, FontStyles.Bold);
        Fill(txt.rectTransform);

        Button btn = root.gameObject.AddComponent<Button>();
        StyleButton(btn, faceImg);
        if (onClick != null) btn.onClick.AddListener(onClick);
        root.gameObject.AddComponent<UiHoverPop>();
        return btn;
    }

    // Tint states for a Button whose target graphic is already coloured (the tint multiplies).
    public static void StyleButton(Button btn, Graphic target)
    {
        btn.targetGraphic = target;
        btn.transition = Selectable.Transition.ColorTint;
        Navigation nav = btn.navigation;
        nav.mode = Navigation.Mode.None; // no sticky keyboard focus highlight
        btn.navigation = nav;
        ColorBlock cb = btn.colors;
        cb.normalColor = new Color(0.93f, 0.93f, 0.93f, 1f);
        cb.highlightedColor = Color.white;
        cb.selectedColor = cb.normalColor;
        cb.pressedColor = new Color(0.76f, 0.76f, 0.76f, 1f);
        cb.disabledColor = new Color(0.55f, 0.55f, 0.55f, 0.6f);
        cb.colorMultiplier = 1f;
        cb.fadeDuration = 0.08f;
        btn.colors = cb;
    }

    public static Canvas MakeCanvas(string name, int sortingOrder, Transform parent)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        go.layer = 5;
        go.transform.SetParent(parent, false);

        Canvas c = go.GetComponent<Canvas>();
        c.renderMode = RenderMode.ScreenSpaceOverlay;
        c.sortingOrder = sortingOrder;

        CanvasScaler cs = go.GetComponent<CanvasScaler>();
        cs.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        cs.referenceResolution = new Vector2(1920f, 1080f);
        cs.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        cs.matchWidthOrHeight = 0.5f;

        EnsureEventSystem();
        return c;
    }

    private static void EnsureEventSystem()
    {
        if (Object.FindFirstObjectByType<EventSystem>() != null) return;
        new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
    }
}

// Small scale "pop" on hover / press for buttons. Uses unscaled time so it also works while the game is paused.
public class UiHoverPop : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    public float hoverScale = 1.05f;
    public float pressScale = 0.96f;

    private bool over, down;
    private float current = 1f;

    private void OnEnable()
    {
        over = down = false;
        current = 1f;
        transform.localScale = Vector3.one;
    }

    private void Update()
    {
        float target = down ? pressScale : (over ? hoverScale : 1f);
        current = Mathf.Lerp(current, target, 1f - Mathf.Exp(-20f * Time.unscaledDeltaTime));
        transform.localScale = new Vector3(current, current, 1f);
    }

    public void OnPointerEnter(PointerEventData e) { over = true; }
    public void OnPointerExit(PointerEventData e) { over = false; down = false; }
    public void OnPointerDown(PointerEventData e) { down = true; }
    public void OnPointerUp(PointerEventData e) { down = false; }
}
