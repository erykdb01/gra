using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

// A pixel-art train drawn from UI images. It idles at the platform, shakes and flashes red
// when you make mistakes, its windows flicker when tension is high, and it can arrive/depart.
public class TrainView : MonoBehaviour
{
    const int S = 3;   // pixel scale
    public const float TrainY = 500f;   // the platform strip hides the wheels

    RectTransform root;
    readonly List<Image> windows = new List<Image>();
    readonly List<Image> puffs = new List<Image>();
    readonly List<float> puffAge = new List<float>();
    Vector2 chimney;
    Image doorGlow;
    TextMeshProUGUI lineSign;
    bool blackout;
    float flickerUntil;
    float doorLocalX, doorAlpha, doorTarget;

    Vector2 basePos;
    Vector2 jitter;
    float nextJitter, nextPuff, shakeAmp, flash, stress01, clock;
    bool moving;

    static readonly Color Warm = new Color(1f, 0.82f, 0.45f, 1f);
    static readonly Color Red  = new Color(1f, 0.25f, 0.2f, 1f);
    static readonly Color Off  = new Color(0.08f, 0.07f, 0.10f, 1f);

    public static TrainView Create(Canvas canvas)
    {
        var go = new GameObject("TrainView", typeof(RectTransform));
        go.layer = 5;
        go.transform.SetParent(canvas.transform, false);
        TrainView tv = go.AddComponent<TrainView>();
        tv.Build();
        return tv;
    }

    void Build()
    {
        root = (RectTransform)transform;

        Color32[] bodies =
        {
            new Color32(96, 52, 52, 255), new Color32(70, 60, 52, 255), new Color32(96, 52, 52, 255),
            new Color32(70, 60, 52, 255), new Color32(96, 52, 52, 255)
        };

        float x = 0f;
        for (int i = 0; i < bodies.Length; i++)
        {
            Image w = UIKit.Img("Wagon" + i, root, PixelArt.Wagon(bodies[i]), Color.white);
            Place(w.rectTransform, x, 0, PixelArt.WagonW * S, PixelArt.WagonH * S);
            for (int k = 0; k < PixelArt.WagonWinX.Length; k++)
            {
                Image win = UIKit.Img("Window", w.transform, PixelArt.Solid(), Warm);
                Place(win.rectTransform,
                      PixelArt.WagonWinX[k] * S, PixelArt.WinY * S,
                      PixelArt.WinSize * S, PixelArt.WinSize * S);
                windows.Add(win);
            }
            if (i == bodies.Length - 1)
            {
                // the boarding door (last wagon): glows when a passenger is let in
                doorGlow = UIKit.Img("DoorGlow", w.transform, PixelArt.Solid(), new Color(1f, 0.85f, 0.5f, 0f));
                Place(doorGlow.rectTransform,
                      (PixelArt.DoorX0 + 1) * S, (PixelArt.DoorY0 + 1) * S,
                      (PixelArt.DoorX1 - PixelArt.DoorX0 - 1) * S, (PixelArt.DoorY1 - PixelArt.DoorY0 - 1) * S);
                doorLocalX = x + ((PixelArt.DoorX0 + PixelArt.DoorX1 + 1) * 0.5f) * S;
            }
            x += PixelArt.WagonW * S + 2 * S;
        }

        Image loco = UIKit.Img("Locomotive", root, PixelArt.Locomotive(), Color.white);
        Place(loco.rectTransform, x, 0, PixelArt.LocoW * S, PixelArt.LocoH * S);
        Image lw = UIKit.Img("Window", loco.transform, PixelArt.Solid(), Warm);
        Place(lw.rectTransform, PixelArt.LocoWinX * S, PixelArt.LocoWinY * S, PixelArt.LocoWinSize * S, PixelArt.LocoWinSize * S);
        windows.Add(lw);
        Image plate = UIKit.Img("LinePlate", loco.transform, PixelArt.Solid(), new Color(0.06f, 0.06f, 0.08f, 1f));
        Place(plate.rectTransform, 58f * S / 3f * 1f, 70f, 92f, 36f);
        lineSign = UIKit.Text("LineSign", plate.transform, "13", 30, new Color(1f, 0.85f, 0.45f), TextAlignmentOptions.Center);
        UIKit.Stretch(lineSign.rectTransform);
        lineSign.textWrappingMode = TextWrappingModes.NoWrap;
        chimney = new Vector2(x + PixelArt.ChimneyX * S, PixelArt.ChimneyY * S);

        float total = x + PixelArt.LocoW * S;
        UIKit.Anchor(root, UIKit.BC, UIKit.BC, UIKit.BC, new Vector2(0, TrainY), new Vector2(total, PixelArt.LocoH * S));
        basePos = new Vector2(0, TrainY);

        for (int i = 0; i < 8; i++)
        {
            Image puff = UIKit.Img("Smoke", root, PixelArt.Solid(), new Color(0.75f, 0.75f, 0.8f, 0f));
            Place(puff.rectTransform, 0, 0, 8, 8);
            puffs.Add(puff);
            puffAge.Add(-1f);
        }
    }

    static void Place(RectTransform r, float x, float y, float w, float h)
    {
        UIKit.Anchor(r, Vector2.zero, Vector2.zero, Vector2.zero, new Vector2(x, y), new Vector2(w, h));
    }

    public void SetStress(float s01) { stress01 = Mathf.Clamp01(s01); }

    // X of the boarding door (relative to the screen centre) and Y of its threshold.
    public float DoorX { get { return -root.sizeDelta.x * 0.5f + doorLocalX + basePos.x; } }
    public float DoorY { get { return TrainY + PixelArt.DoorY0 * S; } }

    public void SetDoorOpen(bool open) { doorTarget = open ? 1f : 0f; }

    // The number on the locomotive (it sometimes shows the wrong line).
    public void SetLineNumber(string s) { if (lineSign != null) lineSign.text = s; }
    public void SetBlackout(bool on) { blackout = on; }
    public void FlickerFor(float seconds) { flickerUntil = Time.time + seconds; }
    public bool IsMoving { get { return moving; } }

    // Mistake: shake and flash red.
    public void Bump(float amount)
    {
        shakeAmp = Mathf.Max(shakeAmp, 7f * amount);
        flash = Mathf.Max(flash, Mathf.Clamp01(amount));
    }

    public IEnumerator Arrive()
    {
        moving = true;
        float dur = 2.6f;
        for (float e = 0f; e < dur; e += Time.deltaTime)
        {
            float k = 1f - Mathf.Pow(1f - Mathf.Clamp01(e / dur), 3f);
            basePos = new Vector2(Mathf.Lerp(-1900f, 0f, k), TrainY);
            yield return null;
        }
        basePos = new Vector2(0f, TrainY);
        moving = false;
    }

    public IEnumerator Depart()
    {
        moving = true;
        float dur = 3f;
        for (float e = 0f; e < dur; e += Time.deltaTime)
        {
            float k = Mathf.Pow(Mathf.Clamp01(e / dur), 3f);
            basePos = new Vector2(Mathf.Lerp(0f, 1900f, k), TrainY);
            yield return null;
        }
        basePos = new Vector2(1900f, TrainY);
        moving = false;
    }

    public void ParkOffscreen()
    {
        basePos = new Vector2(-1900f, TrainY);
    }

    void Update()
    {
        clock += Time.deltaTime;

        // pixel-snapped idle jitter grows with tension
        if (Time.time > nextJitter)
        {
            nextJitter = Time.time + 0.07f;
            float amp = 0.6f + stress01 * 2.5f + (moving ? 1.5f : 0f);
            jitter = new Vector2(Mathf.Round(Random.Range(-amp, amp)), Mathf.Round(Random.Range(-amp, amp)));
        }

        shakeAmp = Mathf.MoveTowards(shakeAmp, 0f, Time.deltaTime * 14f);
        Vector2 shake = new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f)) * shakeAmp;
        root.anchoredPosition = basePos + jitter + shake;

        flash = Mathf.MoveTowards(flash, 0f, Time.deltaTime * 1.5f);

        // window flicker when tension is high
        float redAmount = Mathf.Clamp01(flash + stress01 * 0.6f);
        for (int i = 0; i < windows.Count; i++)
        {
            bool dark = blackout || (Time.time < flickerUntil && Mathf.PerlinNoise(i * 3.7f, clock * 18f) < 0.55f) ||
                        (stress01 > 0.35f && Mathf.PerlinNoise(i * 7.31f, clock * 5f) < (stress01 - 0.35f) * 1.1f);
            windows[i].color = dark ? Off : Color.Lerp(Warm, Red, redAmount);
        }

        // door glow
        if (doorGlow != null)
        {
            doorAlpha = Mathf.MoveTowards(doorAlpha, doorTarget, Time.deltaTime * 4f);
            doorGlow.color = new Color(1f, 0.85f, 0.5f, blackout ? 0f : doorAlpha * 0.9f);
        }

        // smoke puffs
        if (Time.time > nextPuff)
        {
            nextPuff = Time.time + (moving ? 0.15f : 0.4f);
            for (int i = 0; i < puffs.Count; i++)
            {
                if (puffAge[i] < 0f) { puffAge[i] = 0f; break; }
            }
        }
        for (int i = 0; i < puffs.Count; i++)
        {
            if (puffAge[i] < 0f) continue;
            puffAge[i] += Time.deltaTime;
            float a = puffAge[i];
            if (a > 2f) { puffAge[i] = -1f; puffs[i].color = new Color(0.75f, 0.75f, 0.8f, 0f); continue; }
            float size = 8f + a * 14f;
            puffs[i].rectTransform.sizeDelta = new Vector2(size, size);
            puffs[i].rectTransform.anchoredPosition = chimney + new Vector2(-a * 26f - (moving ? a * 60f : 0f), a * 42f);
            puffs[i].color = new Color(0.75f, 0.75f, 0.8f, 0.55f * (1f - a / 2f));
        }
    }
}
