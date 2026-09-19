using UnityEngine;
using UnityEngine.UI;

// Procedural pixel art: all sprites are drawn in code (no image files needed).
public static class PixelArt
{
    // Small drawing surface.
    public class Pix
    {
        public readonly int w, h;
        public readonly Color32[] d;

        public Pix(int w, int h)
        {
            this.w = w; this.h = h;
            d = new Color32[w * h];
        }

        public void Set(int x, int y, Color32 c)
        {
            if (x < 0 || y < 0 || x >= w || y >= h) return;
            d[y * w + x] = c;
        }

        public Color32 Get(int x, int y)
        {
            if (x < 0 || y < 0 || x >= w || y >= h) return new Color32(0, 0, 0, 0);
            return d[y * w + x];
        }

        public void Fill(Color32 c)
        {
            for (int i = 0; i < d.Length; i++) d[i] = c;
        }

        // Inclusive rectangle.
        public void Box(int x0, int y0, int x1, int y1, Color32 c)
        {
            for (int y = y0; y <= y1; y++)
                for (int x = x0; x <= x1; x++)
                    Set(x, y, c);
        }

        public void Circle(int cx, int cy, int r, Color32 c)
        {
            for (int y = -r; y <= r; y++)
                for (int x = -r; x <= r; x++)
                    if (x * x + y * y <= r * r) Set(cx + x, cy + y, c);
        }

        // Blend a colour over the existing pixel.
        public void Blend(int x, int y, Color32 c, float a)
        {
            if (x < 0 || y < 0 || x >= w || y >= h) return;
            Color32 o = d[y * w + x];
            byte r = (byte)Mathf.Lerp(o.r, c.r, a);
            byte g = (byte)Mathf.Lerp(o.g, c.g, a);
            byte b = (byte)Mathf.Lerp(o.b, c.b, a);
            d[y * w + x] = new Color32(r, g, b, o.a == 0 ? (byte)255 : o.a);
        }

        public Sprite ToSprite()
        {
            var t = new Texture2D(w, h, TextureFormat.RGBA32, false);
            t.filterMode = FilterMode.Point;
            t.wrapMode = TextureWrapMode.Clamp;
            t.SetPixels32(d);
            t.Apply();
            return Sprite.Create(t, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), 100f);
        }
    }

    static float H(int x, int y, int s)
    {
        unchecked
        {
            uint n = (uint)(x * 374761393 + y * 668265263 + s * 1442695041);
            n = (n ^ (n >> 13)) * 1274126177u;
            n ^= n >> 16;
            return (n & 0xFFFF) / 65535f;
        }
    }

    static readonly int[,] Bayer =
    {
        {  0,  8,  2, 10 },
        { 12,  4, 14,  6 },
        {  3, 11,  1,  9 },
        { 15,  7, 13,  5 }
    };

    static Color32 C(int r, int g, int b, int a = 255) => new Color32((byte)r, (byte)g, (byte)b, (byte)a);

    static Color32 Scale(Color32 c, float k) =>
        new Color32((byte)Mathf.Clamp(c.r * k, 0, 255), (byte)Mathf.Clamp(c.g * k, 0, 255), (byte)Mathf.Clamp(c.b * k, 0, 255), c.a);

    static Sprite solid;
    public static Sprite Solid()
    {
        if (solid == null)
        {
            var p = new Pix(1, 1);
            p.Fill(C(255, 255, 255));
            solid = p.ToSprite();
        }
        return solid;
    }

    // ---------- Backdrop: night sky, city skyline, platform lamps ----------
    public static Sprite Night(int w = 320, int h = 180)
    {
        var p = new Pix(w, h);
        var horizon = new Color(0.23f, 0.17f, 0.33f);
        var zenith  = new Color(0.03f, 0.04f, 0.10f);

        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                float t = Mathf.Clamp01(((float)y / (h - 1) - 0.3f) / 0.7f);
                float dither = (Bayer[x & 3, y & 3] + 0.5f) / 16f - 0.5f;
                t = Mathf.Round(Mathf.Clamp01(t + dither * 0.08f) * 7f) / 7f;
                p.Set(x, y, (Color32)Color.Lerp(horizon, zenith, t));
            }
        }

        var r = new System.Random(13);

        // stars
        for (int i = 0; i < 110; i++)
        {
            int x = r.Next(0, w);
            int y = r.Next((int)(h * 0.4f), h);
            int b = r.Next(120, 256);
            p.Set(x, y, C(b, b, Mathf.Min(255, b + 20)));
        }

        // moon
        int mx = (int)(w * 0.78f), my = (int)(h * 0.82f);
        p.Circle(mx, my, 9, C(232, 226, 190));
        p.Circle(mx - 3, my + 2, 2, C(210, 204, 168));
        p.Circle(mx + 3, my - 3, 3, C(214, 208, 172));

        // distant skyline
        int baseY = 74;
        int xx = 0;
        while (xx < w)
        {
            int bw = r.Next(8, 20);
            int bh = r.Next(6, 26);
            p.Box(xx, baseY, xx + bw - 1, baseY + bh, C(38, 30, 60));
            xx += bw;
        }

        // near skyline with lit windows
        xx = -3;
        while (xx < w)
        {
            int bw = r.Next(7, 18);
            int bh = r.Next(8, 34);
            p.Box(xx, 0, xx + bw - 1, baseY + bh, C(17, 15, 32));
            for (int wy = baseY + 3; wy < baseY + bh - 1; wy += 4)
                for (int wx = xx + 2; wx < xx + bw - 2; wx += 3)
                    if (r.NextDouble() < 0.28)
                        p.Set(wx, wy, C(255, 200, 110));
            xx += bw + 1;
        }

        // ground under the platform
        p.Box(0, 0, w - 1, baseY - 1, C(14, 12, 24));

        // platform lamps with a warm glow
        int[] lampX = { 36, 150, 270 };
        foreach (int lx in lampX)
        {
            p.Box(lx, baseY, lx + 1, baseY + 40, C(30, 28, 40));
            p.Box(lx - 3, baseY + 40, lx + 4, baseY + 42, C(30, 28, 40));
            int gy = baseY + 39;
            for (int dy = -16; dy <= 16; dy++)
            {
                for (int dx = -16; dx <= 16; dx++)
                {
                    float dist = Mathf.Sqrt(dx * dx + dy * dy);
                    if (dist > 16f) continue;
                    float a = Mathf.Pow(1f - dist / 16f, 2f) * 0.45f;
                    p.Blend(lx + dx, gy + dy, C(255, 190, 90), a);
                }
            }
            p.Box(lx - 2, baseY + 39, lx + 3, baseY + 40, C(255, 230, 150));
        }

        return p.ToSprite();
    }

    // ---------- Desk: wooden planks ----------
    public static Sprite Wood(int w = 240, int h = 56)
    {
        var p = new Pix(w, h);
        for (int y = 0; y < h; y++)
        {
            int plank = y / 14;
            for (int x = 0; x < w; x++)
            {
                float n = H(x / 3, y, 3);
                Color32 c = C(92, 58, 38);
                if (n < 0.15f) c = C(80, 50, 33);
                else if (n > 0.9f) c = C(106, 68, 44);
                if (y % 14 == 0) c = C(48, 30, 20);
                int seam = (x + plank * 47) % 96;
                if (seam == 0 && y % 14 != 0) c = C(56, 35, 24);
                p.Set(x, y, c);
            }
        }
        // lit top edge
        p.Box(0, h - 2, w - 1, h - 1, C(150, 104, 68));
        return p.ToSprite();
    }

    // ---------- Paper sheet for documents ----------
    public static Sprite Paper(int w = 72, int h = 66, int seed = 1)
    {
        var p = new Pix(w, h);
        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                float n = H(x, y, seed);
                Color32 c = C(222, 208, 170);
                if (n < 0.07f) c = C(208, 192, 154);
                else if (n > 0.95f) c = C(234, 222, 188);
                p.Set(x, y, c);
            }
        }
        // stain
        int sx = (int)(H(seed, 3, 9) * w), sy = (int)(H(seed, 5, 9) * h);
        for (int y = -5; y <= 5; y++)
            for (int x = -5; x <= 5; x++)
                if (x * x + y * y <= 25 && H(sx + x, sy + y, 4) > 0.35f)
                    p.Blend(sx + x, sy + y, C(170, 140, 100), 0.18f);
        // border
        Color32 edge = C(140, 118, 84);
        p.Box(0, 0, w - 1, 0, edge);
        p.Box(0, h - 1, w - 1, h - 1, edge);
        p.Box(0, 0, 0, h - 1, edge);
        p.Box(w - 1, 0, w - 1, h - 1, edge);
        // torn corners
        p.Set(0, 0, C(0, 0, 0, 0)); p.Set(w - 1, 0, C(0, 0, 0, 0));
        p.Set(0, h - 1, C(0, 0, 0, 0)); p.Set(w - 1, h - 1, C(0, 0, 0, 0));
        return p.ToSprite();
    }

    // ---------- Dark UI panel with a light border ----------
    public static Sprite Panel(int w = 64, int h = 32)
    {
        var p = new Pix(w, h);
        for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
            {
                float n = H(x, y, 21);
                p.Set(x, y, n < 0.1f ? C(20, 18, 32, 235) : C(26, 24, 40, 235));
            }
        Color32 edge = C(120, 110, 150);
        p.Box(0, 0, w - 1, 1, edge);
        p.Box(0, h - 2, w - 1, h - 1, edge);
        p.Box(0, 0, 1, h - 1, edge);
        p.Box(w - 2, 0, w - 1, h - 1, edge);
        return p.ToSprite();
    }

    // ---------- Button: light bevelled sprite, tinted by Image.color ----------
    public static Sprite ButtonSprite(int w = 60, int h = 18)
    {
        var p = new Pix(w, h);
        p.Fill(C(230, 230, 230));
        p.Box(0, h - 3, w - 1, h - 1, C(255, 255, 255));  // highlight
        p.Box(0, 0, w - 1, 2, C(150, 150, 150));          // shade
        Color32 edge = C(30, 30, 30);
        p.Box(0, 0, w - 1, 0, edge);
        p.Box(0, h - 1, w - 1, h - 1, edge);
        p.Box(0, 0, 0, h - 1, edge);
        p.Box(w - 1, 0, w - 1, h - 1, edge);
        return p.ToSprite();
    }

    // ---------- Hollow frame (used by the stamps) ----------
    public static Sprite Frame(int w = 60, int h = 24, int t = 3)
    {
        var p = new Pix(w, h);
        Color32 white = C(255, 255, 255);
        p.Box(0, 0, w - 1, t - 1, white);
        p.Box(0, h - t, w - 1, h - 1, white);
        p.Box(0, 0, t - 1, h - 1, white);
        p.Box(w - t, 0, w - 1, h - 1, white);
        // ink noise
        for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
                if (p.Get(x, y).a > 0 && H(x, y, 8) < 0.12f) p.Set(x, y, C(0, 0, 0, 0));
        return p.ToSprite();
    }

    // ---------- Train ----------
    public const int WagonW = 64, WagonH = 32;
    public const int WinY = 14, WinSize = 7;
    public static readonly int[] WagonWinX = { 3, 14, 41, 52 };
    public const int DoorX0 = 26, DoorX1 = 37, DoorY0 = 7, DoorY1 = 24;
    public const int LocoW = 72, LocoH = 40;
    public const int LocoWinX = 7, LocoWinY = 19, LocoWinSize = 6;
    public const int ChimneyX = 55, ChimneyY = 39;

    public static Sprite Wagon(Color32 body)
    {
        var p = new Pix(WagonW, WagonH);
        Color32 dark = Scale(body, 0.6f);
        p.Box(1, 7, 62, 25, body);
        p.Box(0, 25, 63, 27, dark);
        p.Box(2, 27, 61, 28, Scale(body, 0.45f));
        p.Box(1, 5, 62, 6, C(28, 24, 30));
        for (int y = 8; y < 25; y += 3)
            for (int x = 1; x < 63; x++)
                if (H(x, y, 2) < 0.2f) p.Set(x, y, Scale(body, 0.85f));
        for (int i = 0; i < WagonWinX.Length; i++)
        {
            int wx = WagonWinX[i];
            p.Box(wx - 1, WinY - 1, wx + WinSize, WinY + WinSize, C(20, 18, 22));
            p.Box(wx, WinY, wx + WinSize - 1, WinY + WinSize - 1, C(10, 10, 16));
        }
        // door recess
        p.Box(DoorX0 - 1, DoorY0, DoorX1 + 1, DoorY1 + 1, C(60, 52, 56));
        p.Box(DoorX0, DoorY0, DoorX1, DoorY1, C(16, 14, 20));
        p.Box((DoorX0 + DoorX1) / 2, DoorY0, (DoorX0 + DoorX1) / 2 + 1, DoorY1, C(30, 26, 32));
        int[] wheels = { 10, 22, 42, 54 };
        foreach (int wx in wheels)
        {
            p.Circle(wx, 4, 3, C(24, 22, 26));
            p.Set(wx, 4, C(90, 90, 100));
        }
        p.Box(0, 8, 0, 10, C(50, 46, 50));
        p.Box(63, 8, 63, 10, C(50, 46, 50));
        return p.ToSprite();
    }

    public static Sprite Locomotive()
    {
        var p = new Pix(LocoW, LocoH);
        Color32 metal = C(40, 44, 58);
        p.Box(18, 7, 70, 24, metal);
        p.Box(18, 24, 70, 25, C(60, 66, 84));
        p.Box(2, 7, 20, 32, C(72, 40, 40));
        p.Box(0, 32, 22, 34, C(40, 24, 24));
        p.Box(6, 18, 6 + 7, 18 + 7, C(10, 10, 16));
        p.Box(50, 25, 58, 37, C(30, 30, 40));
        p.Box(48, 37, 60, 39, C(20, 20, 26));
        p.Box(69, 12, 71, 17, C(255, 232, 150));
        p.Box(66, 4, 71, 7, C(50, 46, 50));
        p.Box(1, 5, 70, 6, C(28, 24, 30));
        int[] wheels = { 10, 26, 42, 58 };
        foreach (int wx in wheels)
        {
            p.Circle(wx, 4, 4, C(24, 22, 26));
            p.Set(wx, 4, C(90, 90, 100));
        }
        return p.ToSprite();
    }

    // ---------- Platform floor strip (in front of the train) ----------
    public static Sprite Platform(int w = 320, int h = 12)
    {
        var p = new Pix(w, h);
        for (int x = 0; x < w; x++)
        {
            p.Set(x, 0, C(230, 190, 60));                                   // yellow safety line
            p.Set(x, 1, (x / 6) % 2 == 0 ? C(60, 56, 70) : C(230, 190, 60));
            for (int y = 2; y <= 9; y++)
            {
                float n = H(x, y, 31);
                Color32 c = ((x / 40 + y / 4) % 2 == 0) ? C(86, 84, 100) : C(78, 76, 92);
                if (n < 0.06f) c = C(68, 66, 82);
                if (x % 40 == 0) c = C(56, 54, 70);
                p.Set(x, y, c);
            }
            p.Set(x, 10, C(60, 58, 74));
            p.Set(x, 11, C(36, 34, 48));
        }
        return p.ToSprite();
    }

    // ---------- Soft ellipse used as a shadow under characters ----------
    static Sprite shadow;
    public static Sprite Shadow()
    {
        if (shadow == null)
        {
            var p = new Pix(16, 5);
            for (int y = 0; y < 5; y++)
                for (int x = 0; x < 16; x++)
                {
                    float dx = (x - 7.5f) / 8f, dy = (y - 2f) / 2.5f;
                    if (dx * dx + dy * dy <= 1f) p.Set(x, y, C(0, 0, 0, 110));
                }
            shadow = p.ToSprite();
        }
        return shadow;
    }

    // ---------- Helper: full-screen night backdrop ----------
    public static Image CreateBackdrop(Transform parent)
    {
        Image bg = UIKit.Img("Backdrop", parent, Night(), Color.white);
        UIKit.Stretch(bg.rectTransform);
        bg.transform.SetAsFirstSibling();
        return bg;
    }
}
