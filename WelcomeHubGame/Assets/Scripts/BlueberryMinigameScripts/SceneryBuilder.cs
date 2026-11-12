using System.Collections.Generic;
using UnityEngine;

// Dresses up the world behind and below the gameplay, all drawn in code (no image files needed):
//   sky gradient, sun, drifting clouds, two layers of hills, grass tufts above ground;
//   plain dirt underground, fading to dark with depth.
// Everything sits on negative sorting orders, so platforms, bushes and the player stay in front.
// Created together with GameHud (see GameHud.Ensure) -- nothing to set up in the scene.
public class SceneryBuilder : MonoBehaviour
{
    private class Cloud { public Transform t; public float speed; }
    private class Twinkle { public SpriteRenderer sr; public float phase; public float baseAlpha; }

    private readonly List<Cloud> clouds = new List<Cloud>();
    private readonly List<Twinkle> twinkles = new List<Twinkle>();
    private System.Random rng;

    private const float GroundTop = 0f;      // top surface of the "Ground" object
    private const float GroundBottom = -1f;  // bottom of the "Ground" object

    private void Awake()
    {
        rng = new System.Random(20260905);

        BuildSky();
        BuildSun();
        BuildClouds();
        BuildHills();
        BuildGrass();
        BuildUnderground();
    }

    private void Update()
    {
        float dt = Time.deltaTime;
        foreach (Cloud c in clouds)
        {
            Vector3 p = c.t.position;
            p.x += c.speed * dt;
            if (p.x > 22f) p.x = -22f;
            c.t.position = p;
        }

        float time = Time.time;
        foreach (Twinkle t in twinkles)
        {
            Color col = t.sr.color;
            col.a = t.baseAlpha * (0.75f + 0.25f * Mathf.Sin(time * 1.6f + t.phase));
            t.sr.color = col;
        }
    }

    // ------------------------------------------------------------------ small helpers
    private float Rand(float a, float b) { return a + (float)rng.NextDouble() * (b - a); }

    private SpriteRenderer Spawn(string name, Sprite sprite, Vector2 pos, int order, Color color)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(transform, false);
        go.transform.position = new Vector3(pos.x, pos.y, 0f);
        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.sortingOrder = order;
        sr.color = color;
        return sr;
    }

    private static Color C(int r, int g, int b, float a = 1f) { return new Color(r / 255f, g / 255f, b / 255f, a); }

    // Tiny software canvas: soft ellipses composited "over", optionally wrapping around the edges (for tiling).
    private class Pix
    {
        public readonly int w, h;
        public readonly Color[] px;
        private readonly bool wrap;

        public Pix(int w, int h, bool wrap = false)
        {
            this.w = w; this.h = h; this.wrap = wrap;
            px = new Color[w * h];
        }

        public void Blend(int x, int y, Color c, float a)
        {
            if (wrap) { x = ((x % w) + w) % w; y = ((y % h) + h) % h; }
            else if (x < 0 || y < 0 || x >= w || y >= h) return;

            a = Mathf.Clamp01(a) * c.a;
            if (a <= 0f) return;
            int i = y * w + x;
            Color d = px[i];
            float outA = a + d.a * (1f - a);
            if (outA <= 0f) return;
            float k = d.a * (1f - a);
            px[i] = new Color((c.r * a + d.r * k) / outA, (c.g * a + d.g * k) / outA, (c.b * a + d.b * k) / outA, outA);
        }

        public void Ellipse(float cx, float cy, float rx, float ry, Color c, float alpha = 1f, float edge = 1.5f)
        {
            int x0 = Mathf.FloorToInt(cx - rx - 2f), x1 = Mathf.CeilToInt(cx + rx + 2f);
            int y0 = Mathf.FloorToInt(cy - ry - 2f), y1 = Mathf.CeilToInt(cy + ry + 2f);
            float soft = edge / Mathf.Min(rx, ry);
            for (int y = y0; y <= y1; y++)
                for (int x = x0; x <= x1; x++)
                {
                    float nx = (x + 0.5f - cx) / rx, ny = (y + 0.5f - cy) / ry;
                    float nd = Mathf.Sqrt(nx * nx + ny * ny);
                    float a = Mathf.Clamp01((1f - nd) / soft + 0.5f);
                    if (a > 0f) Blend(x, y, c, a * alpha);
                }
        }

        public void Disc(float cx, float cy, float r, Color c, float alpha = 1f)
        {
            Ellipse(cx, cy, r, r, c, alpha, 1f);
        }

        public Sprite ToSprite(float ppu, Vector2 pivot)
        {
            Texture2D t = new Texture2D(w, h, TextureFormat.RGBA32, false);
            t.wrapMode = TextureWrapMode.Clamp;
            t.filterMode = FilterMode.Bilinear;
            t.SetPixels(px);
            t.Apply();
            return Sprite.Create(t, new Rect(0, 0, w, h), pivot, ppu, 0, SpriteMeshType.FullRect);
        }
    }

    // Periodic value noise (repeats every `period` lattice cells) so dirt tiles seamlessly.
    private static float Hash(int x, int y, int seed)
    {
        unchecked
        {
            int h = x * 374761393 + y * 668265263 + seed * 1442695041;
            h = (h ^ (h >> 13)) * 1274126177;
            h ^= h >> 16;
            return (h & 0xFFFFFF) / (float)0x1000000;
        }
    }

    private static int Mod(int a, int m) { return ((a % m) + m) % m; }

    private static float Noise(float x, float y, int period, int seed)
    {
        int x0 = Mathf.FloorToInt(x), y0 = Mathf.FloorToInt(y);
        float fx = x - x0, fy = y - y0;
        fx = fx * fx * (3f - 2f * fx);
        fy = fy * fy * (3f - 2f * fy);
        float a = Hash(Mod(x0, period), Mod(y0, period), seed);
        float b = Hash(Mod(x0 + 1, period), Mod(y0, period), seed);
        float c = Hash(Mod(x0, period), Mod(y0 + 1, period), seed);
        float d = Hash(Mod(x0 + 1, period), Mod(y0 + 1, period), seed);
        return Mathf.Lerp(Mathf.Lerp(a, b, fx), Mathf.Lerp(c, d, fx), fy);
    }

    // ------------------------------------------------------------------ sky + sun + clouds
    private void BuildSky()
    {
        // 4x256 vertical gradient stretched over the whole background (pale at the horizon, blue above)
        Pix p = new Pix(4, 256);
        Color horizon = C(214, 240, 255), top = C(95, 174, 240);
        for (int y = 0; y < 256; y++)
        {
            float t = Mathf.Pow(y / 255f, 0.85f);
            Color col = Color.Lerp(horizon, top, t);
            for (int x = 0; x < 4; x++) p.px[y * 4 + x] = col;
        }
        Sprite s = p.ToSprite(1f, new Vector2(0.5f, 0.5f)); // 4 x 256 units natural size
        SpriteRenderer sr = Spawn("Sky", s, new Vector2(0f, 3.5f), -100, Color.white);
        sr.transform.localScale = new Vector3(120f / 4f, 9f / 256f, 1f); // 120 wide, spanning y = -1 .. 8
    }

    private void BuildSun()
    {
        Vector2 pos = new Vector2(4.6f, 4.5f);
        SpriteRenderer glow = Spawn("SunGlow", UiKit.Glow(), pos, -99, new Color(1f, 0.93f, 0.62f, 0.55f));
        glow.transform.localScale = Vector3.one * 7f;
        SpriteRenderer disc = Spawn("Sun", UiKit.Circle(), pos, -98, new Color(1f, 0.96f, 0.72f, 1f));
        disc.transform.localScale = Vector3.one * 1.05f;
        twinkles.Add(new Twinkle { sr = glow, phase = 0f, baseAlpha = 0.55f });
    }

    private Sprite MakeCloud(int variant)
    {
        const int W = 256, H = 128;
        Pix p = new Pix(W, H);
        System.Random r = new System.Random(100 + variant);
        List<Vector3> blobs = new List<Vector3>(); // x, y, radius
        int baseCount = 4 + variant % 2;
        for (int i = 0; i < baseCount; i++)
        {
            float x = 52f + i * (152f / (baseCount - 1)) + (float)(r.NextDouble() - 0.5) * 10f;
            blobs.Add(new Vector3(x, 44f + (float)r.NextDouble() * 10f, 24f + (float)r.NextDouble() * 12f));
        }
        blobs.Add(new Vector3(90f + (float)r.NextDouble() * 20f, 74f, 30f + (float)r.NextDouble() * 8f));
        blobs.Add(new Vector3(140f + (float)r.NextDouble() * 24f, 66f, 24f + (float)r.NextDouble() * 8f));

        Color shade = C(214, 228, 246), light = Color.white;
        for (int y = 0; y < H; y++)
            for (int x = 0; x < W; x++)
            {
                float a = 0f;
                foreach (Vector3 b in blobs)
                {
                    float d = Mathf.Sqrt((x + 0.5f - b.x) * (x + 0.5f - b.x) + (y + 0.5f - b.y) * (y + 0.5f - b.y));
                    a = Mathf.Max(a, Mathf.Clamp01((b.z - d) / 2.5f + 0.5f));
                }
                a *= Mathf.Clamp01((y - 18f) / 6f); // flat-ish bottom
                if (a <= 0f) continue;
                Color c = Color.Lerp(shade, light, Mathf.Clamp01((y - 24f) / 55f));
                c.a = a * 0.96f;
                p.px[y * W + x] = c;
            }
        return p.ToSprite(64f, new Vector2(0.5f, 0.5f)); // 4 x 2 units
    }

    private void BuildClouds()
    {
        Sprite[] variants = { MakeCloud(0), MakeCloud(1), MakeCloud(2) };
        for (int i = 0; i < 7; i++)
        {
            SpriteRenderer sr = Spawn("Cloud" + i, variants[i % 3], new Vector2(Rand(-18f, 18f), Rand(3.0f, 6.6f)), -95, Color.white);
            float s = Rand(0.8f, 1.8f);
            sr.transform.localScale = new Vector3(s, s, 1f);
            clouds.Add(new Cloud { t = sr.transform, speed = Rand(0.12f, 0.45f) });
        }
    }

    // ------------------------------------------------------------------ hills
    private Sprite MakeHills(Color col, Color topCol, float baseH, float[] amps, int[] harm, float[] phase)
    {
        const int W = 2048, H = 128; // 64 x 4 units at 32 px per unit, periodic so copies join seamlessly
        const float PPU = 32f;
        Pix p = new Pix(W, H);
        for (int x = 0; x < W; x++)
        {
            float u = (x + 0.5f) / W;
            float hUnits = baseH;
            for (int k = 0; k < amps.Length; k++) hUnits += amps[k] * Mathf.Sin(2f * Mathf.PI * harm[k] * u + phase[k]);
            float hp = hUnits * PPU;
            for (int y = 0; y < H; y++)
            {
                float a = Mathf.Clamp01(hp - y);
                if (a <= 0f) break;
                Color c = Color.Lerp(col, topCol, Mathf.Clamp01(y / Mathf.Max(hp, 1f)));
                c.a = a;
                p.px[y * W + x] = c;
            }
        }
        return p.ToSprite(PPU, new Vector2(0.5f, 0f));
    }

    private void BuildHills()
    {
        Sprite far = MakeHills(C(150, 208, 190), C(176, 224, 204), 1.7f,
            new[] { 0.7f, 0.4f, 0.2f }, new[] { 3, 7, 13 }, new[] { 0.4f, 2.1f, 4.0f });
        Sprite near = MakeHills(C(108, 186, 122), C(128, 200, 134), 0.9f,
            new[] { 0.45f, 0.3f, 0.15f }, new[] { 5, 9, 17 }, new[] { 1.3f, 3.2f, 0.7f });

        for (int i = 0; i < 2; i++)
        {
            float x = i == 0 ? -32f : 32f;
            Spawn("HillsFar" + i, far, new Vector2(x, GroundTop - 0.1f), -90, Color.white);
            Spawn("HillsNear" + i, near, new Vector2(x, GroundTop - 0.1f), -80, Color.white);
        }
    }

    // ------------------------------------------------------------------ grass tufts
    private Sprite MakeTuft()
    {
        Pix p = new Pix(64, 48);
        Color[] greens = { C(74, 168, 63), C(57, 136, 47), C(104, 196, 84) };
        for (int b = 0; b < 5; b++)
        {
            float bx = 12f + b * 10f;
            float h = 20f + (b % 2 == 0 ? 14f : 4f) + (float)rng.NextDouble() * 6f;
            float lean = (b - 2) * 5f;
            Color col = greens[b % 3];
            for (float t = 0f; t <= 1f; t += 0.02f)
                p.Disc(bx + lean * t * t, 1f + t * h, Mathf.Lerp(2.6f, 0.6f, t), col);
        }
        return p.ToSprite(128f, new Vector2(0.5f, 0f)); // 0.5 x 0.375 units
    }

    private void BuildGrass()
    {
        Sprite tuft = MakeTuft();

        for (int i = 0; i < 46; i++)
        {
            SpriteRenderer sr = Spawn("Tuft" + i, tuft, new Vector2(Rand(-19f, 19f), GroundTop - 0.04f), -3, Color.white);
            float s = Rand(0.8f, 1.6f);
            sr.transform.localScale = new Vector3(rng.NextDouble() < 0.5 ? s : -s, s, 1f);
        }
    }

    // ------------------------------------------------------------------ underground
    private Sprite MakeDirtTile()
    {
        const int N = 512;
        Pix p = new Pix(N, N, true);
        Color baseCol = C(150, 108, 66); // same brown as the ground tile, so the two join seamlessly

        for (int y = 0; y < N; y++)
            for (int x = 0; x < N; x++)
            {
                float n1 = Noise(x / 64f, y / 64f, 8, 11);
                float n2 = Noise(x / 16f, y / 16f, 32, 23);
                float n3 = Noise(x / 4f, y / 4f, 128, 37);
                float v = 1f + (n1 - 0.5f) * 0.18f + (n2 - 0.5f) * 0.10f + (n3 - 0.5f) * 0.06f;
                p.px[y * N + x] = new Color(baseCol.r * v, baseCol.g * v, baseCol.b * v, 1f);
            }

        System.Random r = new System.Random(7);
        float R(float a, float b) { return a + (float)r.NextDouble() * (b - a); }

        // a few faint dark specks, like the ones on the ground tile
        for (int i = 0; i < 22; i++)
            p.Ellipse(R(0, N), R(0, N), R(4, 8), R(2.5f, 5f), C(124, 88, 54), 0.6f);


        return p.ToSprite(64f, new Vector2(0.5f, 0.5f)); // 8 x 8 units per tile
    }

    private void BuildUnderground()
    {
        float depth = 13f; // dirt goes from just under the ground tile down to about y = -14

        // dirt (tiled)
        Sprite dirt = MakeDirtTile();
        SpriteRenderer d = Spawn("Dirt", dirt, new Vector2(0f, GroundBottom + 0.1f - depth * 0.5f), -10, Color.white);
        d.drawMode = SpriteDrawMode.Tiled;
        d.size = new Vector2(120f, depth);

        // darkening with depth: a gradient strip, then a solid dark block below it
        Pix g = new Pix(4, 256);
        Color deep = C(66, 44, 32); // dark brown, not purple
        for (int y = 0; y < 256; y++)
        {
            float t = 1f - y / 255f; // row 255 is the top (no tint), row 0 the bottom (full tint)
            Color c = deep;
            c.a = Mathf.Pow(t, 0.9f) * 0.6f;
            for (int x = 0; x < 4; x++) g.px[y * 4 + x] = c;
        }
        Sprite grad = g.ToSprite(1f, new Vector2(0.5f, 0.5f));
        float stripH = 7f;
        SpriteRenderer strip = Spawn("DepthFade", grad, new Vector2(0f, GroundBottom + 0.1f - stripH * 0.5f), -8, Color.white);
        strip.transform.localScale = new Vector3(120f / 4f, stripH / 256f, 1f);

        Pix solid = new Pix(2, 2);
        for (int i = 0; i < 4; i++) solid.px[i] = new Color(deep.r, deep.g, deep.b, 0.6f);
        SpriteRenderer block = Spawn("DepthBlock", solid.ToSprite(1f, new Vector2(0.5f, 0.5f)), new Vector2(0f, GroundBottom + 0.1f - stripH - 4f), -8, Color.white);
        block.transform.localScale = new Vector3(60f, 4f, 1f); // 120 x 8 units
    }
}
