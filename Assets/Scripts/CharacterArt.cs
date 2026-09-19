using UnityEngine;

// Look of one character (colours and clothing pieces).
public class CharStyle
{
    public Color32 skin = new Color32(232, 190, 150, 255);
    public Color32 hair = new Color32(60, 40, 30, 255);
    public Color32 coat = new Color32(80, 80, 90, 255);
    public Color32 pants = new Color32(50, 50, 60, 255);
    public Color32 shoes = new Color32(30, 24, 24, 255);
    public Color32 hat = new Color32(70, 60, 50, 255);
    public Color32 scarf = new Color32(200, 60, 60, 255);
    public Color32 accent = new Color32(120, 90, 60, 255);
    public int hairStyle;   // 0 short, 1 bald, 2 long, 3 bun, 4 braid
    public int hatStyle;    // 0 none, 1 flat cap, 2 fedora, 3 peaked cap, 4 beanie, 5 headscarf, 6 nurse cap, 7 helmet
    public int coatKind;    // 0 jacket, 1 long coat, 2 dress, 3 robe
    public int accessory;   // 0 none, 1 cane, 2 briefcase, 3 handbag, 4 backpack, 5 tool bag, 6 lantern
    public bool hasScarf, hasVest, buttons;
}

// All animation frames of one character.
public class CharFrames
{
    public Sprite idle, walkA, walkB, talk;
    public Sprite point, palm, stamp;    // conductor gestures
    public Sprite portrait;              // head and shoulders for the ID card
}

// Procedural pixel-art characters: 13 passenger looks and the conductor.
public static class CharacterArt
{
    public const int W = 20, H = 40;
    public const int LookCount = 13;     // 0-3 male, 4-7 female, 8 doctor, 9 policeman, 10 homeless, 11 mourner, 12 hooded stranger

    public static bool IsFemale(int look)
    {
        return look == 4 || look == 5 || look == 6 || look == 7 || look == 11;
    }

    static readonly CharFrames[] passengers = new CharFrames[LookCount];
    static CharFrames conductor;
    static readonly Color32 OutlineColor = new Color32(24, 18, 26, 255);

    static Color32 C(int r, int g, int b) { return new Color32((byte)r, (byte)g, (byte)b, 255); }

    static Color32 Shade(Color32 c, float k)
    {
        return new Color32((byte)Mathf.Clamp(c.r * k, 0, 255), (byte)Mathf.Clamp(c.g * k, 0, 255), (byte)Mathf.Clamp(c.b * k, 0, 255), c.a);
    }

    static CharStyle StyleFor(int look)
    {
        switch (look)
        {
            case 0: // old man with a cane
                return new CharStyle { skin = C(214, 166, 124), hair = C(180, 180, 180), hairStyle = 0, hatStyle = 1, hat = C(110, 80, 50),
                                       coat = C(50, 80, 60), pants = C(60, 56, 50), coatKind = 1, accessory = 1 };
            case 1: // worker with a tool bag
                return new CharStyle { skin = C(232, 190, 150), hair = C(90, 60, 40), hairStyle = 0, hatStyle = 7, hat = C(240, 190, 40),
                                       coat = C(70, 110, 170), pants = C(50, 70, 120), coatKind = 0, hasVest = true, accent = C(240, 130, 30), accessory = 5 };
            case 2: // businessman with a briefcase
                return new CharStyle { skin = C(240, 200, 164), hair = C(30, 30, 34), hairStyle = 0, hatStyle = 2, hat = C(30, 30, 36),
                                       coat = C(90, 94, 104), pants = C(60, 62, 72), coatKind = 0, accessory = 2, hasScarf = true, scarf = C(150, 40, 50) };
            case 3: // soldier
                return new CharStyle { skin = C(200, 150, 110), hair = C(50, 40, 30), hairStyle = 0, hatStyle = 3, hat = C(80, 90, 60),
                                       coat = C(84, 96, 64), pants = C(60, 66, 48), coatKind = 1, buttons = true };
            case 4: // old woman with a handbag
                return new CharStyle { skin = C(214, 166, 124), hair = C(230, 230, 230), hairStyle = 3, hatStyle = 5, hat = C(140, 70, 110),
                                       coat = C(110, 60, 110), pants = C(60, 40, 60), coatKind = 2, accessory = 3, accent = C(90, 60, 40) };
            case 5: // woman in a red coat
                return new CharStyle { skin = C(232, 190, 150), hair = C(40, 26, 20), hairStyle = 2, hatStyle = 0,
                                       coat = C(190, 50, 50), pants = C(40, 40, 50), coatKind = 1, hasScarf = true, scarf = C(240, 230, 210) };
            case 6: // student with a backpack
                return new CharStyle { skin = C(240, 200, 164), hair = C(160, 90, 40), hairStyle = 4, hatStyle = 4, hat = C(60, 140, 150),
                                       coat = C(230, 190, 60), pants = C(60, 80, 130), coatKind = 0, accessory = 4, accent = C(50, 70, 110) };
            case 8: // doctor
                return new CharStyle { skin = C(232, 190, 150), hair = C(90, 70, 50), hairStyle = 0, hatStyle = 0,
                                       coat = C(228, 230, 236), pants = C(60, 70, 90), coatKind = 1, accessory = 2, hasScarf = true, scarf = C(60, 140, 160) };
            case 9: // policeman
                return new CharStyle { skin = C(220, 175, 135), hair = C(40, 32, 28), hairStyle = 0, hatStyle = 3, hat = C(30, 40, 80),
                                       coat = C(36, 50, 90), pants = C(28, 36, 64), coatKind = 0, buttons = true };
            case 10: // homeless man
                return new CharStyle { skin = C(200, 150, 120), hair = C(110, 100, 90), hairStyle = 2, hatStyle = 4, hat = C(90, 80, 70),
                                       coat = C(96, 80, 62), pants = C(60, 56, 50), coatKind = 1, accessory = 4, accent = C(70, 60, 50) };
            case 11: // woman in mourning
                return new CharStyle { skin = C(226, 184, 146), hair = C(50, 40, 40), hairStyle = 3, hatStyle = 5, hat = C(20, 20, 24),
                                       coat = C(24, 24, 30), pants = C(20, 20, 24), coatKind = 2, accessory = 3, accent = C(40, 40, 50) };
            case 12: // hooded stranger
                return new CharStyle { skin = C(190, 180, 170), hair = C(60, 60, 66), hairStyle = 1, hatStyle = 5, hat = C(58, 58, 66),
                                       coat = C(72, 72, 82), pants = C(50, 50, 56), coatKind = 3 };
            default: // nurse
                return new CharStyle { skin = C(232, 190, 150), hair = C(120, 70, 40), hairStyle = 3, hatStyle = 6,
                                       coat = C(80, 130, 170), pants = C(230, 230, 240), coatKind = 2, accent = C(200, 200, 210) };
        }
    }

    static CharStyle ConductorStyle()
    {
        return new CharStyle { skin = C(232, 190, 150), hair = C(50, 40, 36), hairStyle = 0, hatStyle = 3, hat = C(28, 44, 84),
                               coat = C(28, 44, 84), pants = C(22, 30, 56), coatKind = 1, buttons = true, accessory = 6 };
    }

    public static CharFrames Passenger(int look)
    {
        look = Mathf.Clamp(look, 0, LookCount - 1);
        if (passengers[look] == null) passengers[look] = Build(StyleFor(look), false);
        return passengers[look];
    }

    public static CharFrames Conductor()
    {
        if (conductor == null) conductor = Build(ConductorStyle(), true);
        return conductor;
    }

    static CharFrames Build(CharStyle s, bool gestures)
    {
        var f = new CharFrames();
        f.idle = Draw(s, 0, 0);
        f.walkA = Draw(s, 1, 0);
        f.walkB = Draw(s, 2, 0);
        f.talk = Draw(s, 0, 0, 1);
        if (gestures)
        {
            f.point = Draw(s, 0, 1);
            f.palm = Draw(s, 0, 2);
            f.stamp = Draw(s, 0, 3);
        }
        Texture2D tex = f.idle.texture;
        f.portrait = Sprite.Create(tex, new Rect(1, 21, 18, 19), new Vector2(0.5f, 0.5f), 100f);
        return f;
    }

    // leg: 0 idle, 1 left foot lifted, 2 right foot lifted.
    // arm: 0 down, 1 pointing, 2 palm raised, 3 stamping.
    static Sprite Draw(CharStyle s, int leg, int arm, int mouth = 0)
    {
        var p = new PixelArt.Pix(W, H);
        const int OX = 2, OY = 2;

        // local drawing helpers in figure coordinates (0..15 x 0..35)
        void B(int x0, int y0, int x1, int y1, Color32 c) { p.Box(OX + x0, OY + y0, OX + x1, OY + y1, c); }
        void P(int x, int y, Color32 c) { p.Set(OX + x, OY + y, c); }

        Color32 skinS = Shade(s.skin, 0.82f);
        Color32 coatS = Shade(s.coat, 0.78f);
        Color32 eye = C(25, 20, 30);

        // backpack (behind the body)
        if (s.accessory == 4)
        {
            B(10, 14, 14, 24, s.accent);
            B(10, 14, 10, 24, Shade(s.accent, 0.7f));
        }

        // legs and shoes
        int lLift = leg == 1 ? 2 : 0;
        int rLift = leg == 2 ? 2 : 0;
        B(4, lLift, 6, 11, s.pants);
        B(9, rLift, 11, 11, s.pants);
        B(4, lLift, 7, lLift + 1, s.shoes);
        B(9, rLift, 12, rLift + 1, s.shoes);

        // coat / dress / robe
        int k = s.coatKind;
        if (k == 0)
        {
            B(3, 12, 12, 23, s.coat);
        }
        else if (k == 1)
        {
            B(3, 5, 12, 23, s.coat);
            B(3, 5, 12, 5, coatS);
            B(7, 6, 8, 22, coatS);
        }
        else if (k == 2)
        {
            B(4, 14, 11, 23, s.coat);
            B(3, 12, 12, 13, s.coat);
            B(2, 9, 13, 11, s.coat);
            B(2, 6, 13, 8, s.coat);
            B(2, 5, 13, 5, coatS);
        }
        else
        {
            B(3, 1, 12, 23, s.coat);
            B(3, 1, 12, 1, coatS);
            B(7, 2, 8, 22, coatS);
        }
        B(12, k == 0 ? 12 : 6, 12, 23, coatS);

        if (s.hasVest)
        {
            B(5, 12, 10, 21, s.accent);
            B(5, 12, 10, 12, Shade(s.accent, 0.6f));
        }
        if (s.buttons)
        {
            P(8, 14, C(235, 195, 70));
            P(8, 17, C(235, 195, 70));
            P(8, 20, C(235, 195, 70));
            B(3, 12, 12, 12, Shade(s.coat, 0.6f));
        }
        if (s.hasScarf)
        {
            B(4, 21, 11, 22, s.scarf);
            B(8, 15, 9, 20, s.scarf);
        }

        // left arm
        B(1, 13, 2, 22, s.coat);
        B(1, 11, 2, 12, s.skin);

        // right arm
        if (arm == 0)
        {
            B(13, 13, 14, 22, s.coat);
            B(13, 11, 14, 12, s.skin);
        }
        else if (arm == 1)
        {
            B(13, 19, 15, 20, s.coat);
            B(16, 19, 17, 20, s.skin);
        }
        else if (arm == 2)
        {
            B(13, 17, 14, 25, s.coat);
            B(13, 26, 15, 28, s.skin);
        }
        else
        {
            B(13, 15, 15, 16, s.coat);
            B(16, 14, 17, 15, s.skin);
            B(16, 11, 17, 13, C(120, 120, 130));
        }

        // accessories
        switch (s.accessory)
        {
            case 1:
                if (arm == 0) { B(15, 0, 15, 13, C(120, 80, 50)); P(14, 13, C(120, 80, 50)); }
                break;
            case 2:
                B(0, 4, 4, 9, C(92, 60, 40));
                B(1, 10, 3, 10, OutlineColor);
                P(2, 7, C(235, 195, 70));
                break;
            case 3:
                if (arm == 0) { B(13, 7, 16, 10, s.accent); B(14, 11, 14, 12, s.accent); }
                break;
            case 5:
                B(0, 3, 4, 8, s.accent);
                B(1, 9, 3, 9, OutlineColor);
                break;
            case 6:
                B(0, 4, 3, 10, C(40, 32, 28));
                B(1, 5, 2, 9, C(255, 214, 120));
                B(1, 11, 2, 11, C(40, 32, 28));
                break;
        }

        // head
        B(4, 24, 11, 31, s.skin);
        B(4, 24, 4, 31, skinS);
        B(3, 26, 3, 27, skinS);
        B(12, 26, 12, 27, skinS);
        P(7, 28, eye);
        P(10, 28, eye);
        P(11, 26, skinS);
        if (mouth == 1) B(8, 24, 10, 25, C(70, 30, 34));
        else B(8, 25, 10, 25, C(170, 90, 80));

        // hair
        Color32 h = s.hair;
        switch (s.hairStyle)
        {
            case 0:
                B(4, 30, 11, 31, h); B(4, 26, 4, 29, h);
                break;
            case 1:
                P(6, 31, Shade(s.skin, 1.1f));
                break;
            case 2:
                B(4, 30, 11, 31, h); B(3, 17, 4, 29, h); B(12, 18, 12, 29, h);
                break;
            case 3:
                B(4, 30, 11, 31, h); B(4, 26, 4, 29, h); B(6, 32, 9, 34, h);
                break;
            case 4:
                B(4, 30, 11, 31, h); B(4, 26, 4, 29, h); B(2, 16, 3, 29, h); P(2, 15, C(200, 60, 60));
                break;
        }

        // hat
        Color32 hc = s.hat;
        switch (s.hatStyle)
        {
            case 1:
                B(4, 30, 12, 31, hc); B(5, 32, 10, 32, hc); B(11, 29, 14, 30, Shade(hc, 0.7f));
                break;
            case 2:
                B(2, 30, 13, 30, hc); B(5, 31, 10, 34, hc); B(5, 31, 10, 31, Shade(hc, 0.6f));
                break;
            case 3:
                B(4, 30, 11, 31, hc); B(3, 32, 12, 33, hc);
                P(7, 31, C(235, 195, 70)); P(8, 31, C(235, 195, 70));
                B(9, 29, 13, 29, C(30, 26, 30));
                break;
            case 4:
                B(4, 30, 11, 33, hc); B(4, 30, 11, 30, Shade(hc, 0.7f)); B(7, 34, 8, 35, hc);
                break;
            case 5:
                B(3, 29, 12, 31, hc); B(3, 24, 4, 28, hc); B(11, 24, 12, 28, hc);
                break;
            case 6:
                B(5, 32, 10, 34, C(245, 245, 245)); P(7, 33, C(210, 50, 50)); P(8, 33, C(210, 50, 50));
                B(4, 30, 11, 31, C(245, 245, 245));
                break;
            case 7:
                B(4, 30, 11, 33, hc); B(3, 30, 12, 30, Shade(hc, 0.7f)); B(5, 34, 10, 34, hc);
                break;
        }

        // dark outline around the whole figure
        Color32[] src = (Color32[])p.d.Clone();
        for (int y = 0; y < H; y++)
        {
            for (int x = 0; x < W; x++)
            {
                if (src[y * W + x].a != 0) continue;
                bool near =
                    (x + 1 < W && src[y * W + x + 1].a != 0) ||
                    (x - 1 >= 0 && src[y * W + x - 1].a != 0) ||
                    (y + 1 < H && src[(y + 1) * W + x].a != 0) ||
                    (y - 1 >= 0 && src[(y - 1) * W + x].a != 0);
                if (near) p.Set(x, y, OutlineColor);
            }
        }

        return p.ToSprite();
    }
}
