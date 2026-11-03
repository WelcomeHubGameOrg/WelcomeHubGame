using System.Collections.Generic;
using UnityEngine;

public class Bush : MonoBehaviour
{
    [Header("Capacity")]
    public int minCapacity = 6;
    public int maxCapacity = 12;
    [Range(0f, 1f)] public float badChance = 0.2f; // ~20% bad/inedible berries
    [Range(0f, 1f)] public float startFillFraction = 0.5f; // how full the bush is when a level (re)starts / after a visit

    [Header("Bad Berry Cap")]
    // Without a cap, bad berries can pile up forever: the player rationally avoids
    // clicking them (they're a penalty, not score), so they never get removed, while
    // regrowth stops once the bush hits capacity -- a bush could end up ALL bad and
    // stay stuck that way. This caps how many bad berries can exist in one bush at a
    // time (roughly a third of its capacity), guaranteeing there's always room left for
    // good ones to spawn.
    [Range(0.1f, 0.6f)] public float maxBadFraction = 0.34f;

    [Header("Regrowth")]
    public float regenInterval = 4f; // seconds between new berries appearing (while the bush's UI window is closed)
    public float unavailableAfterLeaving = 10f; // seconds the bush can't be opened again after you leave it

    // Hard upper limit on berries per bush. More than this can't be laid out in the berry panel without the
    // berries having to shrink to avoid touching, so capacity is clamped to it whatever the Inspector says.
    public const int MaxBerriesPerBush = 8;

    private int capacity;
    private int maxBad; // cap on concurrent bad berries, recomputed per capacity (see Bad Berry Cap above)
    private float regenTimer;
    private bool uiOpen;
    private float unavailableUntil = 0f;

    private List<bool> berries = new List<bool>(); // true = bad
    private bool playerNearby = false;

    void Start()
    {
        // Start roughly half-full so there's already something to collect
        RegrowFromScratch();
    }

    void Update()
    {
        // (E does nothing while the pause menu is open -- Update still runs when Time.timeScale is 0)
        bool ended = BlueberryGameManager.Instance != null && BlueberryGameManager.Instance.IsGameOver; // end screen is up
        if (playerNearby && !PauseMenu.IsPaused && !ended && Input.GetKeyDown(KeyCode.E))
        {
            if (uiOpen)
            {
                // E toggles the window closed again (in addition to the Close button and walking away)
                if (BushUIWindow.Instance != null) BushUIWindow.Instance.CloseBushUI();
            }
            else if (Time.time >= unavailableUntil && berries.Count > 0 && BushUIWindow.Instance != null)
            {
                // Ignored entirely while the bush is still on cooldown after being left
                BushUIWindow.Instance.OpenBushUI(this, new List<bool>(berries));
            }
        }

        // Dots reappear when the cooldown ends
        if (dotsRenderer != null && dotsRenderer.enabled != IsReady) UpdateVisual();

        // Gradual regrowth — paused while this bush's UI window is open
        if (!uiOpen && berries.Count < capacity)
        {
            regenTimer += Time.deltaTime;
            if (regenTimer >= regenInterval)
            {
                regenTimer = 0f;
                berries.Add(RollNewBerry());
                UpdateVisual();
            }
        }
    }

    public void SetUIOpen(bool open)
    {
        uiOpen = open;
        if (open) return;

        // Whenever the player leaves — whether some berries are left (usually the bad ones)
        // or the bush was fully cleared out — all of its berries get completely replaced,
        // and the bush can't be opened again for unavailableAfterLeaving seconds.
        RegrowFromScratch();
        unavailableUntil = Time.time + unavailableAfterLeaving;
        UpdateVisual(); // bush looks empty during the cooldown
    }

    // Public entry point for LevelManager/BlueberryGameManager to force a completely fresh bush
    // (new berries, no cooldown) whenever a level (re)starts. SetActive(false)/(true) does
    // NOT re-run Start(), so without this a retried or revisited level would keep whatever
    // berries/cooldown the bush had from the previous attempt.
    public void ResetBush()
    {
        uiOpen = false;
        unavailableUntil = 0f;
        RegrowFromScratch();
    }

    private void RegrowFromScratch()
    {
        berries.Clear();

        capacity = Mathf.Min(Random.Range(minCapacity, maxCapacity + 1), MaxBerriesPerBush);
        maxBad = Mathf.Max(1, Mathf.CeilToInt(capacity * maxBadFraction));

        int startCount = Mathf.CeilToInt(capacity * startFillFraction);
        for (int i = 0; i < startCount; i++)
            berries.Add(RollNewBerry());

        regenTimer = 0f;
        UpdateVisual();
    }

    // Rolls whether a newly-added berry is bad, respecting the maxBad cap so a bush can
    // never accumulate more bad berries than that regardless of luck or how long it sits.
    private bool RollNewBerry()
    {
        if (CountBad() >= maxBad) return false; // cap reached -- force this one good
        return Random.value < badChance;
    }

    private int CountBad()
    {
        int count = 0;
        for (int i = 0; i < berries.Count; i++)
            if (berries[i]) count++;
        return count;
    }

    public void ConsumeBerryUI(bool isBad)
    {
        if (isBad)
        {
            BlueberryGameManager.Instance.AddBadBerryPenalty(); // -5 from the collected berries
        }
        else
        {
            BlueberryGameManager.Instance.AddGoodBerry(); // +1 to the score
        }

        // Remove one berry of the matching type — which exact index doesn't matter,
        // this avoids index desync if the list changed while the window was open.
        berries.Remove(isBad);

        UpdateVisual();
    }

    public int BerryCount => berries.Count;

    // ---- Visual state: a bush that still has berries shows small blue berry dots on top of the plain
    // bush art; once every berry is picked the dots disappear (plain bush), and they come back as
    // soon as berries regrow. The dots are a child SpriteRenderer built at runtime, so the existing
    // bush sprite/prefab/scene objects don't need to change.
    private SpriteRenderer bushRenderer;
    private SpriteRenderer dotsRenderer;

    // Dot positions in the bush texture's pixel coordinates (180x180, origin bottom-left), picked inside the
    // bush silhouette. Three layouts so neighbouring bushes don't look identical.
    private static readonly Vector2[][] DotLayouts = new Vector2[][]
    {
        new Vector2[] { new Vector2(79f, 38f), new Vector2(87f, 92f), new Vector2(120f, 56f), new Vector2(86f, 133f), new Vector2(34f, 64f), new Vector2(154f, 73f), new Vector2(46f, 101f), new Vector2(122f, 112f) },
        new Vector2[] { new Vector2(124f, 64f), new Vector2(54f, 90f), new Vector2(140f, 108f), new Vector2(96f, 139f), new Vector2(81f, 43f), new Vector2(97f, 98f), new Vector2(44f, 53f) },
        new Vector2[] { new Vector2(93f, 129f), new Vector2(70f, 57f), new Vector2(143f, 59f), new Vector2(44f, 99f), new Vector2(134f, 107f), new Vector2(103f, 85f), new Vector2(105f, 39f), new Vector2(32f, 62f) }
    };
    private static readonly Sprite[] dotSprites = new Sprite[3];

    // Ready = has berries AND is not on its post-visit cooldown. Only a ready bush shows berry dots.
    private bool IsReady => berries.Count > 0 && Time.time >= unavailableUntil;

    private void UpdateVisual()
    {
        if (bushRenderer == null) bushRenderer = GetComponent<SpriteRenderer>();
        if (bushRenderer == null) return;
        bushRenderer.color = Color.white;

        if (dotsRenderer == null)
        {
            Sprite dots = GetDotsSprite(bushRenderer.sprite);
            if (dots == null) return;
            GameObject go = new GameObject("BerryDots");
            go.transform.SetParent(transform, false);
            dotsRenderer = go.AddComponent<SpriteRenderer>();
            dotsRenderer.sprite = dots;
            dotsRenderer.flipX = bushRenderer.flipX;
            dotsRenderer.flipY = bushRenderer.flipY;
            dotsRenderer.sortingLayerID = bushRenderer.sortingLayerID;
            dotsRenderer.sortingOrder = bushRenderer.sortingOrder + 1;
        }
        dotsRenderer.enabled = IsReady;
    }

    private Sprite GetDotsSprite(Sprite bushSprite)
    {
        if (bushSprite == null) return null;
        int variant = Mathf.Abs(Mathf.RoundToInt(transform.position.x * 31f + transform.position.y * 17f)) % DotLayouts.Length;
        if (dotSprites[variant] != null) return dotSprites[variant];

        int w = Mathf.RoundToInt(bushSprite.rect.width), h = Mathf.RoundToInt(bushSprite.rect.height);
        float sx = w / 180f, sy = h / 180f; // layouts are authored for a 180x180 bush texture
        Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;
        tex.wrapMode = TextureWrapMode.Clamp;
        Color[] px = new Color[w * h];

        Color outline = new Color(0.08f, 0.10f, 0.35f, 1f);
        Color fill = new Color(0.31f, 0.42f, 1f, 1f);
        float r = 11f * sx;
        foreach (Vector2 d in DotLayouts[variant])
        {
            float cx = d.x * sx, cy = d.y * sy;
            int x0 = Mathf.Max(0, Mathf.FloorToInt(cx - r - 2)), x1 = Mathf.Min(w - 1, Mathf.CeilToInt(cx + r + 2));
            int y0 = Mathf.Max(0, Mathf.FloorToInt(cy - r - 2)), y1 = Mathf.Min(h - 1, Mathf.CeilToInt(cy + r + 2));
            for (int y = y0; y <= y1; y++)
                for (int x = x0; x <= x1; x++)
                {
                    float dist = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), new Vector2(cx, cy));
                    float cover = Mathf.Clamp01(r + 0.5f - dist);
                    if (cover <= 0f) continue;
                    Color c = dist > r - 2.2f ? outline : fill;
                    float hl = Mathf.Clamp01(3.2f * sx + 0.5f - Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), new Vector2(cx - r * 0.35f, cy + r * 0.35f)));
                    c = Color.Lerp(c, new Color(0.85f, 0.9f, 1f, 1f), hl * (dist <= r - 2.2f ? 0.9f : 0f));
                    c.a = cover;
                    px[y * w + x] = c;
                }
        }
        tex.SetPixels(px);
        tex.Apply();

        Vector2 pivot = new Vector2(bushSprite.pivot.x / bushSprite.rect.width, bushSprite.pivot.y / bushSprite.rect.height);
        dotSprites[variant] = Sprite.Create(tex, new Rect(0, 0, w, h), pivot, bushSprite.pixelsPerUnit);
        return dotSprites[variant];
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player")) playerNearby = true;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerNearby = false;
            if (BushUIWindow.Instance != null) BushUIWindow.Instance.CloseBushUI();
        }
    }
}
