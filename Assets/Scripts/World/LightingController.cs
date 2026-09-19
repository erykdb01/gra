using System.Collections;
using UnityEngine;
using UnityEngine.UI;

// Atmosphere layers: rain, fog, platform lamps, the conductor's lantern and the darkness around it.
// Built from a few pooled UI images (no Light2D needed), so it stays cheap.
public class LightingController : MonoBehaviour
{
    public RectTransform BackLayer;    // lamps: between the train and the people
    public RectTransform FrontLayer;   // rain, fog, glow, vignette: above the people, below the desk

    const int DropCount = 80;
    const float TopY = 1080f, BottomY = 452f;

    RectTransform[] drops = new RectTransform[DropCount];
    Vector2[] dropPos = new Vector2[DropCount];
    float[] dropSpeed = new float[DropCount];
    Image[] dropImg = new Image[DropCount];

    RectTransform fogA, fogB;
    Image lanternGlow, vignette, darkness, coldTint;
    Image[] lampGlow = new Image[3];
    float[] lampSeed = new float[3];
    RectTransform lanternRt;

    float stress01, flickerUntil, lanternBase = 1f, darknessTarget, darknessNow;
    Vector2 lanternPos;
    float halfW = 960f;

    public static LightingController Create(Canvas canvas, StageView stage)
    {
        var go = new GameObject("Lighting", typeof(RectTransform));
        go.layer = 5;
        go.transform.SetParent(canvas.transform, false);
        LightingController lc = go.AddComponent<LightingController>();
        lc.Build(canvas, stage);
        return lc;
    }

    void Build(Canvas canvas, StageView stage)
    {
        ((RectTransform)transform).sizeDelta = Vector2.zero;
        halfW = ((RectTransform)canvas.transform).rect.width * 0.5f;
        if (halfW < 100f) halfW = 960f;

        BackLayer = UIKit.NewRect("LightsBack", canvas.transform);
        UIKit.Stretch(BackLayer);
        FrontLayer = UIKit.NewRect("LightsFront", canvas.transform);
        UIKit.Stretch(FrontLayer);

        // platform lamps
        float[] lampX = { -760f, -60f, 640f };
        for (int i = 0; i < 3; i++)
        {
            Image lamp = UIKit.Img("Lamp" + i, BackLayer, PixelArtExtras.Lamp(), Color.white);
            UIKit.Anchor(lamp.rectTransform, UIKit.BC, UIKit.BC, new Vector2(0.5f, 0f), new Vector2(lampX[i], 458f), new Vector2(48f, 224f));
            Image g = UIKit.Img("LampGlow" + i, BackLayer, PixelArtExtras.Glow(), new Color(1f, 0.82f, 0.45f, 0.5f));
            UIKit.Anchor(g.rectTransform, UIKit.BC, UIKit.BC, UIKit.MC, new Vector2(lampX[i], 458f + 195f), new Vector2(360f, 360f));
            lampGlow[i] = g;
            lampSeed[i] = Random.value * 50f;
        }

        // rain streaks
        for (int i = 0; i < DropCount; i++)
        {
            Image d = UIKit.Img("Drop", FrontLayer, PixelArt.Solid(), new Color(0.75f, 0.82f, 0.95f, 0.35f));
            RectTransform r = d.rectTransform;
            UIKit.Anchor(r, UIKit.BC, UIKit.BC, UIKit.MC, Vector2.zero, new Vector2(2f, Random.Range(14f, 26f)));
            r.localRotation = Quaternion.Euler(0f, 0f, 8f);
            drops[i] = r;
            dropImg[i] = d;
            dropPos[i] = new Vector2(Random.Range(0f, halfW * 2f), Random.Range(BottomY, TopY));
            dropSpeed[i] = Random.Range(650f, 950f);
        }

        // fog
        fogA = MakeFog("FogA", 470f, 0.55f);
        fogB = MakeFog("FogB", 520f, 0.35f);

        // cold tint (frost) and the darkness with a lit middle
        coldTint = UIKit.Img("Cold", FrontLayer, PixelArt.Solid(), new Color(0.35f, 0.5f, 0.9f, 0f));
        UIKit.Stretch(coldTint.rectTransform);

        vignette = UIKit.Img("Vignette", FrontLayer, PixelArtExtras.Vignette(), new Color(1f, 1f, 1f, 0.85f));
        UIKit.Stretch(vignette.rectTransform);

        lanternGlow = UIKit.Img("LanternGlow", FrontLayer, PixelArtExtras.Glow(), new Color(1f, 0.78f, 0.4f, 0.4f));
        lanternRt = lanternGlow.rectTransform;
        UIKit.Anchor(lanternRt, UIKit.BC, UIKit.BC, UIKit.MC, Vector2.zero, new Vector2(620f, 620f));
        lanternPos = stage != null ? stage.LanternPos : new Vector2(30f, 560f);

        darkness = UIKit.Img("Darkness", FrontLayer, PixelArt.Solid(), new Color(0f, 0f, 0.01f, 0f));
        UIKit.Stretch(darkness.rectTransform);
    }

    RectTransform MakeFog(string name, float y, float alpha)
    {
        Image f = UIKit.Img(name, FrontLayer, PixelArtExtras.Fog(), new Color(1f, 1f, 1f, alpha));
        UIKit.Anchor(f.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, y), new Vector2(halfW * 4f, 110f));
        return f.rectTransform;
    }

    // ---------------- control ----------------

    public void SetStress(float s01) { stress01 = Mathf.Clamp01(s01); }

    public void FlickerLantern(float seconds) { flickerUntil = Mathf.Max(flickerUntil, Time.time + seconds); }

    // Fade to (almost) black and back, used by the blackout event.
    public IEnumerator Blackout(float seconds)
    {
        darknessTarget = 0.93f;
        yield return new WaitForSeconds(seconds);
        darknessTarget = 0f;
    }

    public void SetDarkness(float a) { darknessTarget = Mathf.Clamp01(a); }

    // Position of the lantern in screen coordinates (x from the centre, y from the bottom).
    public void SetLantern(Vector2 pos) { lanternPos = pos; }

    void Update()
    {
        float dt = Time.deltaTime;
        if (dt <= 0f) return;   // paused: the scene stays frozen

        // rain
        float wind = 130f;
        for (int i = 0; i < DropCount; i++)
        {
            dropPos[i].y -= dropSpeed[i] * dt;
            dropPos[i].x -= wind * dt;
            if (dropPos[i].y < BottomY)
            {
                dropPos[i] = new Vector2(Random.Range(0f, halfW * 2f + 200f), TopY + Random.Range(0f, 200f));
            }
            if (dropPos[i].x < -20f) dropPos[i].x += halfW * 2f + 40f;
            drops[i].anchoredPosition = new Vector2(dropPos[i].x - halfW, dropPos[i].y);
        }

        // fog drifts
        float t = Time.time;
        fogA.anchoredPosition = new Vector2(Mathf.Sin(t * 0.09f) * 240f, fogA.anchoredPosition.y);
        fogB.anchoredPosition = new Vector2(Mathf.Sin(t * 0.06f + 2f) * 300f, fogB.anchoredPosition.y);

        // lamps flicker (more with stress)
        for (int i = 0; i < 3; i++)
        {
            float n = Mathf.PerlinNoise(lampSeed[i], t * (1.5f + stress01 * 5f));
            float a = 0.42f + 0.18f * n;
            if (stress01 > 0.5f && n < 0.25f) a *= 0.3f;
            if (Time.time < flickerUntil && Random.value < 0.5f) a *= 0.2f;
            Color c = lampGlow[i].color; c.a = a; lampGlow[i].color = c;
        }

        // the conductor's lantern: steady pulse, jitter with stress, hard flicker on events
        float pulse = 1f + 0.05f * Mathf.Sin(t * 2.1f) + 0.04f * Mathf.Sin(t * 5.3f);
        float jitter = Mathf.PerlinNoise(t * (3f + stress01 * 12f), 7.7f);
        float level = lanternBase * pulse;
        if (stress01 > 0.4f && SaveSystem.Data.settings.horrorEffects) level *= 1f - Mathf.Clamp01((stress01 - 0.4f) * 1.3f) * (jitter < 0.35f ? 0.7f : 0.15f);
        if (Time.time < flickerUntil) level *= Random.value < 0.5f ? 0.15f : 0.9f;
        lanternRt.anchoredPosition = lanternPos + new Vector2(0f, 0f);
        lanternRt.sizeDelta = new Vector2(620f, 620f) * Mathf.Clamp(level, 0.2f, 1.2f);
        Color lc = lanternGlow.color; lc.a = 0.42f * Mathf.Clamp01(level); lanternGlow.color = lc;

        // vignette gets darker with stress, and the air turns cold blue
        Color vc = vignette.color; vc.a = Mathf.Lerp(0.8f, 1f, stress01); vignette.color = vc;
        Color cc = coldTint.color; cc.a = Mathf.Lerp(cc.a, 0.06f * stress01, dt * 2f); coldTint.color = cc;

        // darkness
        darknessNow = Mathf.MoveTowards(darknessNow, darknessTarget, dt * (darknessTarget > darknessNow ? 6f : 2.5f));
        Color dc = darkness.color; dc.a = darknessNow; darkness.color = dc;
    }
}
