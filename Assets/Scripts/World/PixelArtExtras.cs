using System.Collections.Generic;
using UnityEngine;

// More procedural pixel art: a 5x7 pixel font, the horror logo, glows, vignette, fog, lamps, clock, signs, rails.
public static class PixelArtExtras
{
    static Color32 C(int r, int g, int b, int a = 255) { return new Color32((byte)r, (byte)g, (byte)b, (byte)a); }

    // ---------------- 5x7 pixel font ----------------

    static readonly Dictionary<char, string[]> font = new Dictionary<char, string[]>();
    static bool fontBuilt;

    static void G(char c, params string[] rows) { font[c] = rows; }

    static void BuildFont()
    {
        if (fontBuilt) return;
        fontBuilt = true;
        G('A', "01110", "10001", "10001", "11111", "10001", "10001", "10001");
        G('B', "11110", "10001", "10001", "11110", "10001", "10001", "11110");
        G('C', "01110", "10001", "10000", "10000", "10000", "10001", "01110");
        G('D', "11110", "10001", "10001", "10001", "10001", "10001", "11110");
        G('E', "11111", "10000", "10000", "11110", "10000", "10000", "11111");
        G('F', "11111", "10000", "10000", "11110", "10000", "10000", "10000");
        G('G', "01110", "10001", "10000", "10111", "10001", "10001", "01111");
        G('H', "10001", "10001", "10001", "11111", "10001", "10001", "10001");
        G('I', "01110", "00100", "00100", "00100", "00100", "00100", "01110");
        G('J', "00111", "00010", "00010", "00010", "00010", "10010", "01100");
        G('K', "10001", "10010", "10100", "11000", "10100", "10010", "10001");
        G('L', "10000", "10000", "10000", "10000", "10000", "10000", "11111");
        G('M', "10001", "11011", "10101", "10101", "10001", "10001", "10001");
        G('N', "10001", "11001", "10101", "10011", "10001", "10001", "10001");
        G('O', "01110", "10001", "10001", "10001", "10001", "10001", "01110");
        G('P', "11110", "10001", "10001", "11110", "10000", "10000", "10000");
        G('Q', "01110", "10001", "10001", "10001", "10101", "10010", "01101");
        G('R', "11110", "10001", "10001", "11110", "10100", "10010", "10001");
        G('S', "01111", "10000", "10000", "01110", "00001", "00001", "11110");
        G('T', "11111", "00100", "00100", "00100", "00100", "00100", "00100");
        G('U', "10001", "10001", "10001", "10001", "10001", "10001", "01110");
        G('V', "10001", "10001", "10001", "10001", "10001", "01010", "00100");
        G('W', "10001", "10001", "10001", "10101", "10101", "11011", "10001");
        G('X', "10001", "10001", "01010", "00100", "01010", "10001", "10001");
        G('Y', "10001", "10001", "01010", "00100", "00100", "00100", "00100");
        G('Z', "11111", "00001", "00010", "00100", "01000", "10000", "11111");
        G('0', "01110", "10001", "10011", "10101", "11001", "10001", "01110");
        G('1', "00100", "01100", "00100", "00100", "00100", "00100", "01110");
        G('2', "01110", "10001", "00001", "00010", "00100", "01000", "11111");
        G('3', "11110", "00001", "00001", "01110", "00001", "00001", "11110");
        G('4', "00010", "00110", "01010", "10010", "11111", "00010", "00010");
        G('5', "11111", "10000", "11110", "00001", "00001", "10001", "01110");
        G('6', "00110", "01000", "10000", "11110", "10001", "10001", "01110");
        G('7', "11111", "00001", "00010", "00100", "01000", "01000", "01000");
        G('8', "01110", "10001", "10001", "01110", "10001", "10001", "01110");
        G('9', "01110", "10001", "10001", "01111", "00001", "00010", "01100");
        G(':', "00000", "00100", "00000", "00000", "00100", "00000", "00000");
        G('.', "00000", "00000", "00000", "00000", "00000", "00110", "00110");
        G('-', "00000", "00000", "00000", "11111", "00000", "00000", "00000");
        G('?', "01110", "10001", "00001", "00010", "00100", "00000", "00100");
        G('!', "00100", "00100", "00100", "00100", "00100", "00000", "00100");
        G('/', "00001", "00001", "00010", "00100", "01000", "10000", "10000");
        G(' ', "00000", "00000", "00000", "00000", "00000", "00000", "00000");
    }

    // Removes Polish letters so the pixel font can draw them.
    public static string Ascii(string s)
    {
        var sb = new System.Text.StringBuilder();
        foreach (char ch in s.ToUpperInvariant())
        {
            switch (ch)
            {
                case 'Ą': sb.Append('A'); break;
                case 'Ć': sb.Append('C'); break;
                case 'Ę': sb.Append('E'); break;
                case 'Ł': sb.Append('L'); break;
                case 'Ń': sb.Append('N'); break;
                case 'Ó': sb.Append('O'); break;
                case 'Ś': sb.Append('S'); break;
                case 'Ź': case 'Ż': sb.Append('Z'); break;
                default: sb.Append(ch); break;
            }
        }
        return sb.ToString();
    }

    public static int TextWidth(string s) { return s.Length * 6 - 1; }

    static void Blit(PixelArt.Pix p, string s, int x0, int y0, Color32 col)
    {
        BuildFont();
        int x = x0;
        foreach (char ch in s)
        {
            string[] rows;
            if (!font.TryGetValue(ch, out rows)) rows = font[' '];
            for (int ry = 0; ry < 7; ry++)
                for (int rx = 0; rx < 5; rx++)
                    if (rows[ry][rx] == '1') p.Set(x + rx, y0 + (6 - ry), col);
            x += 6;
        }
    }

    // Plain pixel text on a transparent background.
    public static Sprite Text(string text, Color32 col, int pad = 1)
    {
        text = Ascii(text);
        var p = new PixelArt.Pix(TextWidth(text) + pad * 2, 7 + pad * 2);
        Blit(p, text, pad, pad, col);
        return p.ToSprite();
    }

    // The horror logo: pixel letters, a dark outline, a dull red glow under them and blood drips.
    public static Sprite Logo(string text, Color32 fill, Color32 blood, int seed)
    {
        text = Ascii(text);
        int pad = 6;
        int w = TextWidth(text) + pad * 2;
        int h = 7 + pad * 2 + 6;
        var mask = new PixelArt.Pix(w, h);
        Blit(mask, text, pad, pad + 6, C(255, 255, 255));

        var p = new PixelArt.Pix(w, h);
        var rng = new System.Random(seed);

        // glow
        for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
            {
                float best = 99f;
                for (int dy = -3; dy <= 3; dy++)
                    for (int dx = -3; dx <= 3; dx++)
                    {
                        int xx = x + dx, yy = y + dy;
                        if (xx < 0 || yy < 0 || xx >= w || yy >= h) continue;
                        if (mask.Get(xx, yy).a > 0) best = Mathf.Min(best, Mathf.Sqrt(dx * dx + dy * dy));
                    }
                if (best < 4f) p.Set(x, y, C(blood.r, blood.g / 2, blood.b / 2, (int)(90 * (1f - best / 4f))));
            }
        // outline
        for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
                if (mask.Get(x, y).a > 0)
                    for (int dy = -1; dy <= 1; dy++)
                        for (int dx = -1; dx <= 1; dx++)
                            if (p.Get(x + dx, y + dy).a < 250 && mask.Get(x + dx, y + dy).a == 0) p.Set(x + dx, y + dy, C(8, 6, 10));
        // shadow (down-right)
        for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
                if (mask.Get(x, y).a > 0 && p.Get(x + 1, y - 1).a < 250 && mask.Get(x + 1, y - 1).a == 0) p.Set(x + 1, y - 1, C(8, 6, 10));
        // fill: pale letters with a slight vertical gradient
        for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
                if (mask.Get(x, y).a > 0)
                {
                    float k = 0.75f + 0.25f * ((y - pad - 6) / 7f);
                    p.Set(x, y, C((int)(fill.r * k), (int)(fill.g * k), (int)(fill.b * k)));
                }
        // drips: from some letter pixels on the lowest row of a letter
        for (int x = pad; x < w - pad; x++)
        {
            if (mask.Get(x, pad + 6).a > 0 && rng.NextDouble() < 0.28)
            {
                int len = 2 + rng.Next(0, 5);
                for (int i = 1; i <= len; i++) p.Set(x, pad + 6 - i, blood);
                p.Set(x, pad + 6 - len - 1, C(blood.r * 3 / 4, blood.g * 3 / 4, blood.b * 3 / 4));
            }
        }
        return p.ToSprite();
    }

    // ---------------- lighting sprites ----------------

    static Sprite glow;
    // Soft white radial gradient (use with a colour tint).
    public static Sprite Glow()
    {
        if (glow == null)
        {
            const int n = 64;
            var t = new Texture2D(n, n, TextureFormat.RGBA32, false);
            t.filterMode = FilterMode.Bilinear;
            t.wrapMode = TextureWrapMode.Clamp;
            var d = new Color32[n * n];
            for (int y = 0; y < n; y++)
                for (int x = 0; x < n; x++)
                {
                    float dx = (x - n / 2f + 0.5f) / (n / 2f), dy = (y - n / 2f + 0.5f) / (n / 2f);
                    float r = Mathf.Sqrt(dx * dx + dy * dy);
                    float a = Mathf.Clamp01(1f - r);
                    a = a * a;
                    d[y * n + x] = new Color32(255, 255, 255, (byte)(a * 255f));
                }
            t.SetPixels32(d);
            t.Apply();
            glow = Sprite.Create(t, new Rect(0, 0, n, n), new Vector2(0.5f, 0.5f), 100f);
        }
        return glow;
    }

    static Sprite vignette;
    // Full-screen darkness with a transparent middle (the conductor's lantern lights it from within).
    public static Sprite Vignette()
    {
        if (vignette == null)
        {
            const int w = 96, h = 54;
            var t = new Texture2D(w, h, TextureFormat.RGBA32, false);
            t.filterMode = FilterMode.Bilinear;
            t.wrapMode = TextureWrapMode.Clamp;
            var d = new Color32[w * h];
            for (int y = 0; y < h; y++)
                for (int x = 0; x < w; x++)
                {
                    float dx = (x - w / 2f) / (w / 2f), dy = (y - h / 2f) / (h / 2f);
                    float r = Mathf.Sqrt(dx * dx * 0.85f + dy * dy * 1.1f);
                    float a = Mathf.SmoothStep(0.35f, 1.25f, r);
                    d[y * w + x] = new Color32(2, 3, 8, (byte)(Mathf.Clamp01(a) * 235f));
                }
            t.SetPixels32(d);
            t.Apply();
            vignette = Sprite.Create(t, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), 100f);
        }
        return vignette;
    }

    static Sprite fog;
    public static Sprite Fog()
    {
        if (fog == null)
        {
            const int w = 128, h = 32;
            var t = new Texture2D(w, h, TextureFormat.RGBA32, false);
            t.filterMode = FilterMode.Bilinear;
            t.wrapMode = TextureWrapMode.Repeat;
            var d = new Color32[w * h];
            for (int y = 0; y < h; y++)
                for (int x = 0; x < w; x++)
                {
                    // tileable noise: sample on a torus
                    float ax = x / (float)w * Mathf.PI * 2f;
                    float n = Mathf.PerlinNoise(Mathf.Cos(ax) * 1.6f + 4f, y * 0.11f + Mathf.Sin(ax) * 1.6f);
                    n = n * 0.7f + Mathf.PerlinNoise(Mathf.Cos(ax) * 3.4f + 9f, y * 0.25f + Mathf.Sin(ax) * 3.4f) * 0.3f;
                    float edge = Mathf.Sin(Mathf.PI * (y / (float)(h - 1)));
                    float a = Mathf.Clamp01((n - 0.35f) * 1.6f) * edge;
                    d[y * w + x] = new Color32(200, 210, 225, (byte)(a * 120f));
                }
            t.SetPixels32(d);
            t.Apply();
            fog = Sprite.Create(t, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), 100f);
        }
        return fog;
    }

    // ---------------- world props ----------------

    static Sprite lamp;
    // A platform lamp post, 12x56.
    public static Sprite Lamp()
    {
        if (lamp == null)
        {
            var p = new PixelArt.Pix(12, 56);
            Color32 iron = C(28, 30, 38), light = C(46, 50, 62);
            p.Box(5, 0, 6, 46, iron);
            p.Box(5, 0, 5, 46, light);
            p.Box(3, 0, 8, 3, iron);
            p.Box(2, 46, 9, 47, iron);
            p.Box(3, 48, 8, 52, C(255, 226, 150));
            p.Box(2, 53, 9, 54, iron);
            p.Box(4, 55, 7, 55, iron);
            // outline
            var src = (Color32[])p.d.Clone();
            for (int y = 0; y < 56; y++)
                for (int x = 0; x < 12; x++)
                    if (src[y * 12 + x].a == 0 &&
                        ((x + 1 < 12 && src[y * 12 + x + 1].a != 0) || (x > 0 && src[y * 12 + x - 1].a != 0) || (y + 1 < 56 && src[(y + 1) * 12 + x].a != 0) || (y > 0 && src[(y - 1) * 12 + x].a != 0)))
                        p.Set(x, y, C(10, 10, 16));
            lamp = p.ToSprite();
        }
        return lamp;
    }

    static Sprite clockFace;
    // An analogue station clock face, 32x32. The hands are separate images.
    public static Sprite ClockFace()
    {
        if (clockFace == null)
        {
            var p = new PixelArt.Pix(32, 32);
            p.Circle(16, 16, 15, C(20, 20, 26));
            p.Circle(16, 16, 13, C(226, 218, 190));
            p.Circle(16, 16, 12, C(238, 232, 208));
            for (int i = 0; i < 12; i++)
            {
                float a = i * Mathf.PI / 6f;
                int x = 16 + Mathf.RoundToInt(Mathf.Sin(a) * 10f);
                int y = 16 + Mathf.RoundToInt(Mathf.Cos(a) * 10f);
                p.Set(x, y, C(30, 24, 20));
                if (i % 3 == 0) { p.Set(x + 1, y, C(30, 24, 20)); p.Set(x, y + 1, C(30, 24, 20)); }
            }
            p.Circle(16, 16, 1, C(30, 24, 20));
            clockFace = p.ToSprite();
        }
        return clockFace;
    }

    // A station sign board with text ("PERON 13").
    public static Sprite Sign(string text)
    {
        text = Ascii(text);
        int w = TextWidth(text) + 10, h = 15;
        var p = new PixelArt.Pix(w, h);
        p.Fill(C(16, 34, 66));
        p.Box(0, 0, w - 1, 0, C(230, 230, 230)); p.Box(0, h - 1, w - 1, h - 1, C(230, 230, 230));
        p.Box(0, 0, 0, h - 1, C(230, 230, 230)); p.Box(w - 1, 0, w - 1, h - 1, C(230, 230, 230));
        Blit(p, text, 5, 4, C(236, 236, 226));
        return p.ToSprite();
    }

    static Sprite rails;
    // Two rails with sleepers, 64x14 (tile horizontally).
    public static Sprite Rails()
    {
        if (rails == null)
        {
            var p = new PixelArt.Pix(64, 14);
            for (int x = 0; x < 64; x += 8) p.Box(x, 1, x + 3, 12, C(54, 38, 28));
            p.Box(0, 9, 63, 10, C(96, 100, 112));
            p.Box(0, 10, 63, 10, C(150, 156, 170));
            p.Box(0, 3, 63, 4, C(96, 100, 112));
            p.Box(0, 4, 63, 4, C(150, 156, 170));
            rails = p.ToSprite();
        }
        return rails;
    }
}
