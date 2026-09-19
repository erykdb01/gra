using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

// Psychological horror: screen shake, lantern flicker, glitching text, whispers and the scares
// of the dead. It is deliberately rare: a cooldown keeps scares from happening too often.
public class HorrorManager : MonoBehaviour
{
    LightingController lighting;
    TrainView train;
    StageView stage;
    readonly List<RectTransform> shakeTargets = new List<RectTransform>();
    readonly List<Vector2> shakeBase = new List<Vector2>();
    readonly List<TMP_Text> glitchTexts = new List<TMP_Text>();

    float stress01, shakeAmp, shakeUntil;
    float nextGlitch, nextWhisper, nextLanternFlicker, lastScare = -999f;
    bool frozen;

    public static HorrorManager Create(GameObject host, LightingController lighting, TrainView train, StageView stage)
    {
        HorrorManager h = host.AddComponent<HorrorManager>();
        h.lighting = lighting;
        h.train = train;
        h.stage = stage;
        h.nextGlitch = Time.time + 10f;
        h.nextWhisper = Time.time + 40f;
        h.nextLanternFlicker = Time.time + 25f;
        return h;
    }

    public void AddShakeTarget(RectTransform rt)
    {
        if (rt == null) return;
        shakeTargets.Add(rt);
        shakeBase.Add(rt.anchoredPosition);
    }

    public void AddGlitchText(TMP_Text t) { if (t != null) glitchTexts.Add(t); }

    public void SetStress(float s01) { stress01 = Mathf.Clamp01(s01); }

    bool Effects { get { return SaveSystem.Data.settings.horrorEffects; } }
    bool Shake { get { return SaveSystem.Data.settings.screenShake; } }

    // A one-off shake (a heavy door, a bump).
    public void Bump(float amplitude, float seconds)
    {
        shakeAmp = Mathf.Max(shakeAmp, amplitude);
        shakeUntil = Mathf.Max(shakeUntil, Time.time + seconds);
    }

    // Is it a good time for a scare? (respects the cooldown)
    public bool CanScare(float cooldown)
    {
        return Effects && Time.time - lastScare > cooldown;
    }

    public void MarkScare() { lastScare = Time.time; }

    void Update()
    {
        if (Time.deltaTime <= 0f) return;    // paused

        // screen shake: constant tremble at very high stress plus event bumps
        float amp = 0f;
        if (Shake)
        {
            if (stress01 > 0.6f && Effects) amp = (stress01 - 0.6f) * 7f;
            if (Time.time < shakeUntil) amp += shakeAmp;
        }
        for (int i = 0; i < shakeTargets.Count; i++)
        {
            if (shakeTargets[i] == null) continue;
            Vector2 off = amp > 0.05f ? new Vector2(Mathf.Round(Random.Range(-amp, amp)), Mathf.Round(Random.Range(-amp, amp))) : Vector2.zero;
            shakeTargets[i].anchoredPosition = shakeBase[i] + off;
        }

        if (!Effects || frozen) return;

        // text that goes wrong for a moment
        if (stress01 > 0.55f && Time.time > nextGlitch && glitchTexts.Count > 0)
        {
            nextGlitch = Time.time + Random.Range(6f, 14f) * Mathf.Lerp(1.4f, 0.6f, stress01);
            TMP_Text t = glitchTexts[Random.Range(0, glitchTexts.Count)];
            if (t != null && t.gameObject.activeInHierarchy && t.text.Length > 3) StartCoroutine(GlitchText(t));
        }

        // lantern flicker
        if (stress01 > 0.3f && Time.time > nextLanternFlicker)
        {
            nextLanternFlicker = Time.time + Random.Range(12f, 30f) * Mathf.Lerp(1.3f, 0.5f, stress01);
            lighting.FlickerLantern(Random.Range(0.3f, 0.9f));
        }

        // a whisper from nowhere
        if (stress01 > 0.6f && Time.time > nextWhisper && CanScare(30f))
        {
            nextWhisper = Time.time + Random.Range(35f, 70f);
            MarkScare();
            if (SoundFX.I != null) SoundFX.I.Whisper();
        }
    }

    IEnumerator GlitchText(TMP_Text t)
    {
        string original = t.text;
        char[] c = original.ToCharArray();
        string symbols = "#@%&?/\\▒";
        int n = Mathf.Clamp(c.Length / 6, 2, 6);
        for (int i = 0; i < n; i++)
        {
            int k = Random.Range(0, c.Length);
            if (c[k] != ' ' && c[k] != '\n' && c[k] != '<' && c[k] != '>') c[k] = symbols[Random.Range(0, symbols.Length)];
        }
        // rich-text tags would be broken by the noise, so only glitch plain text
        if (original.Contains("<")) yield break;
        t.text = new string(c);
        yield return new WaitForSeconds(0.14f);
        if (t != null && t.text == new string(c)) t.text = original;
    }

    // ---------------- passenger scares ----------------

    // Plays when a passenger with scaresOnArrival walks up: flicker, stop breathing, stinger, maybe a lunge.
    public IEnumerator ArrivalScare(PassengerData p, System.Action<string> say)
    {
        if (!CanScare(45f)) yield break;
        MarkScare();

        CharacterView cv = stage.Current;
        lighting.FlickerLantern(1.6f);
        if (SoundFX.I != null) { SoundFX.I.Stinger(p.aggressive ? 0.9f : 0.5f); }
        train.FlickerFor(1.2f);
        if (cv != null) cv.SetBreathes(false);
        PlayerStats.AddStress(p.aggressive ? 6 : 3);
        yield return new WaitForSeconds(1.2f);

        if (p.aggressive && cv != null)
        {
            // it steps toward the booth
            Bump(5f, 0.5f);
            train.Bump(1.2f);
            if (say != null) say(Loc.T("<color=#E07070>Dlaczego mnie pan nie wpuszcza? Dlaczego nikt mnie nie wpuszcza?</color>", "<color=#E07070>Why won't you let me in? Why does no one ever let me in?</color>"));
            cv.SetMood("angry");
            yield return cv.WalkTo(cv.FeetX + 70f, cv.FeetY, 1.08f, 260f);
            yield return new WaitForSeconds(0.5f);
            yield return cv.WalkTo(cv.FeetX - 70f, cv.FeetY, 1f, 120f);
            cv.SetMood("cold");
        }
    }

    // Dead passengers rarely stop breathing... living ones do not, so this is only for the twisted moments.
    public void FreezeEffects(bool on) { frozen = on; }
}
