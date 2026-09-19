using System.Collections.Generic;
using UnityEngine;

// All sound is synthesised in code: ambience (rain, hum, rails, radio), effects (stamp, paper, doors, footsteps,
// knocking, stingers, heartbeat) and five music moods (menu, shift, tension, horror, ending).
// Nothing is loaded from files. Clips are built the first time they are needed.
public class SoundFX : MonoBehaviour
{
    public static SoundFX I;

    const int Rate = 44100;
    const int LoRate = 22050;

    AudioSource oneShot, rain, hum, rail, radio, heart, musicA, musicB;
    AudioDistortionFilter humDist, railDist;
    AudioClip stampClip, goodClip, badClip, clickClip, hornClip, whistleClip, paperClip, doorClip, tickClip, breathClip, whisperClip;
    AudioClip knockClip, knock13Clip, powerDownClip, powerUpClip, stepClip, heartClip, rainClip, humClip, railClip, radioClip;
    readonly Dictionary<string, AudioClip> music = new Dictionary<string, AudioClip>();
    readonly Dictionary<string, AudioClip> stingers = new Dictionary<string, AudioClip>();

    float stress01, sfxVol = 0.9f, musicVol = 0.7f;
    string musicName;
    bool musicOnA = true;
    bool autoMusic;
    float nextRadio, nextBreath, musicFade = 1f;
    Coroutine fadeRoutine;

    public static SoundFX Create(GameObject host)
    {
        if (I != null) return I;
        SoundFX s = host.AddComponent<SoundFX>();
        s.Init();
        I = s;
        return s;
    }

    void OnDestroy()
    {
        if (I == this) I = null;
    }

    AudioSource NewSource(string name, bool loop, out AudioDistortionFilter dist, bool withDistortion)
    {
        var go = new GameObject(name);
        go.transform.SetParent(transform, false);
        AudioSource a = go.AddComponent<AudioSource>();
        a.playOnAwake = false;
        a.loop = loop;
        a.spatialBlend = 0f;
        dist = null;
        if (withDistortion)
        {
            dist = go.AddComponent<AudioDistortionFilter>();
            dist.distortionLevel = 0f;
        }
        return a;
    }

    void Init()
    {
        if (FindAnyObjectByType<AudioListener>() == null && Camera.main != null)
            Camera.main.gameObject.AddComponent<AudioListener>();

        AudioDistortionFilter none;
        oneShot = NewSource("SFX", false, out none, false);
        rain = NewSource("Rain", true, out none, false);
        hum = NewSource("Hum", true, out humDist, true);
        rail = NewSource("Rail", true, out railDist, true);
        radio = NewSource("Radio", true, out none, false);
        heart = NewSource("Heartbeat", true, out none, false);
        musicA = NewSource("MusicA", true, out none, false);
        musicB = NewSource("MusicB", true, out none, false);

        ApplyVolumes();
    }

    // Reads the volume sliders from the saved settings.
    public void ApplyVolumes()
    {
        SettingsData s = SaveSystem.Data.settings;
        sfxVol = s.sfx;
        musicVol = s.music;
        RefreshLoops();
    }

    void RefreshLoops()
    {
        rain.volume = 0.32f * sfxVol;
        hum.volume = (0.30f + 0.15f * stress01) * sfxVol;
        rail.volume = (0.16f + 0.3f * stress01) * sfxVol;
        radio.volume = 0.12f * sfxVol;
        heart.volume = Mathf.Clamp01((stress01 - 0.45f) * 1.6f) * 0.9f * sfxVol;
        AudioSource cur = musicOnA ? musicA : musicB;
        cur.volume = 0.42f * musicVol * musicFade;
    }

    // ---------------- synthesis helpers ----------------

    static AudioClip Make(string name, float seconds, int rate, System.Func<float, float> f)
    {
        int n = (int)(seconds * rate);
        float[] data = new float[n];
        for (int i = 0; i < n; i++)
            data[i] = Mathf.Clamp(f(i / (float)rate), -1f, 1f);
        AudioClip c = AudioClip.Create(name, n, 1, rate, false);
        c.SetData(data, 0);
        return c;
    }

    static float Sin(float hz, float t) { return Mathf.Sin(2f * Mathf.PI * hz * t); }
    static float Noise(System.Random r) { return (float)(r.NextDouble() * 2 - 1); }

    // A plucked bell-like note (sine + overtone, exponential decay).
    static float Pluck(float hz, float t, float t0, float decay)
    {
        float dt = t - t0;
        if (dt < 0f || dt > 6f) return 0f;
        float env = Mathf.Exp(-dt * decay) * Mathf.Clamp01(dt / 0.01f);
        return (Sin(hz, dt) + 0.35f * Sin(hz * 2.01f, dt) + 0.12f * Sin(hz * 3.02f, dt)) * env;
    }

    // ---------------- clips ----------------

    AudioClip Clip(ref AudioClip field, System.Func<AudioClip> build)
    {
        if (field == null) field = build();
        return field;
    }

    AudioClip Stamp_() { return Clip(ref stampClip, () => { var r = new System.Random(1); return Make("stamp", 0.45f, Rate, t => Sin(62f, t) * Mathf.Exp(-t * 14f) * 0.95f + Noise(r) * Mathf.Exp(-t * 50f) * 0.55f + Sin(150f, t) * Mathf.Exp(-t * 30f) * 0.3f); }); }
    AudioClip Good_() { return Clip(ref goodClip, () => Make("good", 0.5f, Rate, t => ((t < 0.12f ? Sin(660f, t) : 0f) + (t >= 0.12f ? Sin(880f, t) * Mathf.Exp(-(t - 0.12f) * 8f) : 0f)) * 0.3f)); }
    AudioClip Bad_() { return Clip(ref badClip, () => Make("bad", 0.55f, Rate, t => (Mathf.Sign(Sin(110f, t)) * 0.5f + Mathf.Sign(Sin(117f, t)) * 0.5f) * 0.28f * Mathf.Exp(-t * 3.5f))); }
    AudioClip Click_() { return Clip(ref clickClip, () => { var r = new System.Random(2); return Make("click", 0.06f, Rate, t => Noise(r) * Mathf.Exp(-t * 90f) * 0.4f + Sin(900f, t) * Mathf.Exp(-t * 70f) * 0.2f); }); }
    AudioClip Horn_() { return Clip(ref hornClip, () => Make("horn", 1.8f, Rate, t => { float env = Mathf.Clamp01(t / 0.1f) * Mathf.Exp(-Mathf.Max(0f, t - 0.9f) * 3.5f); float vib = 1f + 0.004f * Sin(6f, t); return 0.3f * env * (Sin(196f * vib, t) + 0.6f * Sin(247f * vib, t) + 0.3f * Sin(294f * vib, t)); })); }
    AudioClip Whistle_() { return Clip(ref whistleClip, () => Make("whistle", 1.3f, Rate, t => { float env = Mathf.Clamp01(t / 0.05f) * Mathf.Exp(-Mathf.Max(0f, t - 0.6f) * 4f); float f = 1180f + 60f * Sin(9f, t); return 0.22f * env * (Sin(f, t) + 0.4f * Sin(f * 2f, t)); })); }
    AudioClip Paper_() { return Clip(ref paperClip, () => { var r = new System.Random(3); float lp = 0f; return Make("paper", 0.3f, Rate, t => { lp += (Noise(r) - lp) * 0.5f; float env = Mathf.Sin(Mathf.PI * Mathf.Clamp01(t / 0.3f)); return (Noise(r) - lp) * env * 0.22f; }); }); }
    AudioClip Door_() { return Clip(ref doorClip, () => { var r = new System.Random(4); float lp = 0f; return Make("door", 0.7f, Rate, t => { lp += (Noise(r) - lp) * 0.08f; float thud = Sin(48f, t) * Mathf.Exp(-t * 10f) * 0.8f; float hiss = lp * 2f * Mathf.Exp(-Mathf.Abs(t - 0.25f) * 8f) * 0.4f; float latch = t > 0.5f && t < 0.53f ? Noise(r) * 0.4f : 0f; return thud + hiss + latch; }); }); }
    AudioClip Tick_() { return Clip(ref tickClip, () => { var r = new System.Random(5); return Make("tick", 0.04f, Rate, t => Noise(r) * Mathf.Exp(-t * 140f) * 0.35f + Sin(1800f, t) * Mathf.Exp(-t * 120f) * 0.15f); }); }
    AudioClip Step_() { return Clip(ref stepClip, () => { var r = new System.Random(6); float lp = 0f; return Make("step", 0.14f, Rate, t => { lp += (Noise(r) - lp) * 0.12f; return lp * 3.2f * Mathf.Exp(-t * 28f) + Sin(80f, t) * Mathf.Exp(-t * 30f) * 0.3f; }); }); }
    AudioClip Breath_() { return Clip(ref breathClip, () => { var r = new System.Random(7); float lp = 0f, lp2 = 0f; return Make("breath", 1.4f, Rate, t => { lp += (Noise(r) - lp) * 0.2f; lp2 += (lp - lp2) * 0.3f; float env = Mathf.Sin(Mathf.PI * Mathf.Clamp01(t / 1.4f)); return (lp - lp2) * env * 1.3f; }); }); }
    AudioClip Whisper_() { return Clip(ref whisperClip, () => { var r = new System.Random(8); float lp = 0f; return Make("whisper", 1.8f, Rate, t => { lp += (Noise(r) - lp) * 0.6f; float am = Mathf.Clamp01(Mathf.Sin(2f * Mathf.PI * 3.3f * t) * 0.6f + 0.4f) * Mathf.Sin(Mathf.PI * Mathf.Clamp01(t / 1.8f)); return (Noise(r) - lp * 0.7f) * am * 0.25f; }); }); }
    AudioClip Knock_() { return Clip(ref knockClip, () => Make("knock", 1.1f, Rate, t => { float s = 0f; float[] at = { 0f, 0.28f, 0.56f }; for (int i = 0; i < at.Length; i++) { float d = t - at[i]; if (d > 0f) s += (Sin(95f, d) + 0.5f * Sin(190f, d)) * Mathf.Exp(-d * 30f); } return s * 0.7f; })); }
    AudioClip Knock13_() { return Clip(ref knock13Clip, () => Make("knock13", 5.5f, Rate, t => { float s = 0f; for (int i = 0; i < 13; i++) { float d = t - (1.1f + i * 0.28f); if (d > 0f) s += (Sin(95f, d) + 0.5f * Sin(190f, d)) * Mathf.Exp(-d * 30f); } for (int i = 0; i < 3; i++) { float d = t - i * 0.28f; if (d > 0f) s += (Sin(95f, d) + 0.5f * Sin(190f, d)) * Mathf.Exp(-d * 30f); } return s * 0.7f; })); }
    AudioClip PowerDown_() { return Clip(ref powerDownClip, () => Make("powerdown", 1.2f, Rate, t => { float f = Mathf.Lerp(220f, 30f, t / 1.2f); return Sin(f, t) * (1f - t / 1.2f) * 0.5f; })); }
    AudioClip PowerUp_() { return Clip(ref powerUpClip, () => Make("powerup", 0.9f, Rate, t => { float f = Mathf.Lerp(40f, 240f, t / 0.9f); return Sin(f, t) * Mathf.Clamp01(t / 0.1f) * (1f - t / 0.9f) * 0.45f; })); }

    AudioClip Heart_() { return Clip(ref heartClip, () => Make("heart", 1.0f, LoRate, t => { float a = Sin(58f, t) * Mathf.Exp(-t * 18f); float d = t - 0.24f; float b = d > 0f ? Sin(48f, d) * Mathf.Exp(-d * 22f) * 0.7f : 0f; return (a + b) * 0.9f; })); }

    // 6-second seamless rain: filtered noise with tiny drops.
    AudioClip Rain_() { return Clip(ref rainClip, () => { var r = new System.Random(9); float lp = 0f, lp2 = 0f; return Make("rain", 6f, LoRate, t => { float n = Noise(r); lp += (n - lp) * 0.45f; lp2 += (lp - lp2) * 0.05f; float drop = r.NextDouble() < 0.0025 ? Noise(r) * 0.6f : 0f; return (lp - lp2) * 0.75f + drop; }); }); }

    // 10-second drone; every frequency has an integer number of cycles in the loop.
    AudioClip Hum_() { return Clip(ref humClip, () => Make("hum", 10f, LoRate, t => 0.10f * Sin(50f, t) * (0.7f + 0.3f * Sin(0.2f, t)) + 0.06f * Sin(100f, t) + 0.04f * Sin(150f, t) * (0.6f + 0.4f * Sin(0.3f, t)) + 0.012f * Sin(3000f, t) * (0.5f + 0.5f * Sin(0.1f, t)))); }

    AudioClip Rail_() { return Clip(ref railClip, () => { var rr = new System.Random(5); float lp = 0f; return Make("rail", 2f, LoRate, t => { float n = Noise(rr); lp += (n - lp) * 0.03f; float beat = t % 0.5f; float clack = 0f; if (beat < 0.03f) clack = Noise(rr) * (1f - beat / 0.03f); else if (beat > 0.11f && beat < 0.14f) clack = Noise(rr) * (1f - (beat - 0.11f) / 0.03f) * 0.7f; return lp * 2.5f + clack * 0.25f; }); }); }

    // 12-second radio: hiss, dial sweeps, a distant voice-like murmur and morse beeps.
    AudioClip Radio_() { return Clip(ref radioClip, () => { var r = new System.Random(10); float lp = 0f; return Make("radio", 12f, LoRate, t => { lp += (Noise(r) - lp) * 0.7f; float hiss = lp * 0.25f; float sweep = 0f; float ph = t % 6f; if (ph > 2f && ph < 3.5f) sweep = Sin(Mathf.Lerp(300f, 1400f, (ph - 2f) / 1.5f), t) * 0.15f; float mur = Sin(180f + 40f * Sin(3f, t), t) * Mathf.Clamp01(Sin(0.5f, t)) * 0.10f * (ph > 3.5f && ph < 5.5f ? 1f : 0f); float beep = (((int)(t * 6f)) % 7 == 0 && ph > 5.5f) ? Sin(900f, t) * 0.12f : 0f; return hiss + sweep + mur + beep; }); }); }

    // Stingers: low cluster with a noise swell. Intensity 0..1.
    AudioClip Stinger_(int variant)
    {
        string key = "st" + variant;
        AudioClip c;
        if (stingers.TryGetValue(key, out c)) return c;
        var r = new System.Random(20 + variant);
        float lp = 0f;
        float f0 = variant == 0 ? 55f : (variant == 1 ? 73.4f : 49f);
        c = Make(key, 2.2f, Rate, t =>
        {
            lp += (Noise(r) - lp) * 0.05f;
            float env = Mathf.Clamp01(t / 0.05f) * Mathf.Exp(-t * 1.6f);
            float tone = Sin(f0, t) + Sin(f0 * 1.4983f, t) * 0.8f + Sin(f0 * 2.12f, t) * 0.5f + Sin(f0 * 5.03f, t) * 0.2f;
            return (tone * 0.28f + lp * 1.5f * Mathf.Sin(Mathf.PI * Mathf.Clamp01(t / 2.2f))) * env;
        });
        stingers[key] = c;
        return c;
    }

    // ---------------- music ----------------

    static readonly float[] Penta = { 1f, 1.1892f, 1.3348f, 1.4983f, 1.7818f, 2f };

    AudioClip BuildMusic(string name)
    {
        float L = 16f;
        var r = new System.Random(name.GetHashCode() & 0xFFFF);
        // pre-computed note schedule (pluck times)
        switch (name)
        {
            case "menu":
            {
                float[] notes = new float[8]; float[] times = { 1f, 3.5f, 5f, 7.5f, 9f, 11.5f, 13f, 14f };
                for (int i = 0; i < notes.Length; i++) notes[i] = 220f * Penta[r.Next(0, Penta.Length)];
                return Make("music_menu", L, LoRate, t =>
                {
                    float pad = 0.10f * Sin(110f, t) + 0.07f * Sin(165f, t) + 0.05f * Sin(220f, t) * (0.6f + 0.4f * Sin(0.125f, t)) + 0.03f * Sin(330f, t);
                    float s = pad;
                    for (int i = 0; i < notes.Length; i++) s += 0.16f * Pluck(notes[i], t, times[i], 1.4f);
                    return s * 0.8f;
                });
            }
            case "shift":
            {
                float[] notes = new float[6]; float[] times = { 0.5f, 3f, 6f, 8.5f, 11f, 13.5f };
                for (int i = 0; i < notes.Length; i++) notes[i] = 146.83f * Penta[r.Next(0, Penta.Length)];
                return Make("music_shift", L, LoRate, t =>
                {
                    float bass = 0.11f * Sin(55f, t) * (0.7f + 0.3f * Sin(0.0625f * 2f, t)) + 0.05f * Sin(82.5f, t);
                    float s = bass;
                    for (int i = 0; i < notes.Length; i++) s += 0.10f * Pluck(notes[i], t, times[i], 1.9f);
                    return s;
                });
            }
            case "tension":
                return Make("music_tension", L, LoRate, t =>
                {
                    float pulse = Mathf.Pow(Mathf.Max(0f, Sin(1f, t)), 2f);
                    float sub = 0.16f * Sin(55f, t) * (0.5f + 0.5f * pulse);
                    float tri = 0.05f * Sin(110f, t) + 0.05f * Sin(155.5f, t);
                    float rise = 0.03f * Sin(880f, t) * (0.5f + 0.5f * Sin(0.25f, t)) * (0.5f + 0.5f * Sin(6f, t));
                    return sub + tri + rise;
                });
            case "horror":
            {
                float lp = 0f;
                return Make("music_horror", L, LoRate, t =>
                {
                    lp += (Noise(r) - lp) * 0.04f;
                    float beat = 0.06f * (Sin(100f, t) + Sin(106f, t));
                    float b2 = 0.04f * (Sin(150f, t) + Sin(159f, t));
                    float swell = lp * 1.2f * (0.5f + 0.5f * Sin(0.125f, t));
                    float scrape = (t > 9f && t < 10.5f) ? Sin(2200f + 300f * Sin(3f, t), t) * 0.02f * Mathf.Sin(Mathf.PI * (t - 9f) / 1.5f) : 0f;
                    return beat + b2 + swell * 0.6f + scrape;
                });
            }
            default: // ending
            {
                float[] notes = new float[6]; float[] times = { 1f, 3.2f, 5.5f, 8f, 10.5f, 13f };
                for (int i = 0; i < notes.Length; i++) notes[i] = 261.63f * Penta[r.Next(0, 5)];
                return Make("music_ending", L, LoRate, t =>
                {
                    float pad = 0.08f * Sin(130.5f, t) + 0.06f * Sin(165f, t) + 0.05f * Sin(196f, t) + 0.04f * Sin(294f, t) * (0.6f + 0.4f * Sin(0.125f, t));
                    float s = pad;
                    for (int i = 0; i < notes.Length; i++) s += 0.14f * Pluck(notes[i], t, times[i], 1.1f);
                    return s;
                });
            }
        }
    }

    // Crossfades to a music mood ("menu", "shift", "tension", "horror", "ending"). null stops the music.
    public void PlayMusic(string name)
    {
        if (name == musicName) return;
        musicName = name;
        if (fadeRoutine != null) StopCoroutine(fadeRoutine);
        fadeRoutine = StartCoroutine(CrossFade(name));
    }

    System.Collections.IEnumerator CrossFade(string name)
    {
        AudioSource from = musicOnA ? musicA : musicB;
        AudioSource to = musicOnA ? musicB : musicA;
        musicOnA = !musicOnA;

        if (name != null)
        {
            AudioClip c;
            if (!music.TryGetValue(name, out c)) { c = BuildMusic(name); music[name] = c; }
            to.clip = c;
            to.volume = 0f;
            to.Play();
        }

        float fromStart = from.volume;
        float target = 0.42f * musicVol;
        for (float e = 0f; e < 2.5f; e += Time.unscaledDeltaTime)
        {
            float k = e / 2.5f;
            from.volume = fromStart * (1f - k);
            if (name != null) to.volume = target * k;
            yield return null;
        }
        from.Stop();
        from.volume = 0f;
        if (name != null) to.volume = target;
        fadeRoutine = null;
    }

    // ---------------- ambience ----------------

    public void StartAmbience(bool withTrain)
    {
        if (rain.clip == null) rain.clip = Rain_();
        if (hum.clip == null) hum.clip = Hum_();
        if (rail.clip == null) rail.clip = Rail_();
        if (radio.clip == null) radio.clip = Radio_();
        if (heart.clip == null) heart.clip = Heart_();
        if (!rain.isPlaying) rain.Play();
        if (!hum.isPlaying) hum.Play();
        if (withTrain && !rail.isPlaying) rail.Play();
        if (!radio.isPlaying) radio.Play();
        if (!heart.isPlaying) heart.Play();
        RefreshLoops();
    }

    // The radio is only on in the menu and during some events.
    public void RadioOn(bool on) { radio.mute = !on; }

    public void SetAutoMusic(bool on) { autoMusic = on; if (on) UpdateAutoMusic(); }

    void UpdateAutoMusic()
    {
        if (!autoMusic) return;
        string m = stress01 < 0.35f ? "shift" : (stress01 < 0.7f ? "tension" : "horror");
        PlayMusic(m);
    }

    // 0 = calm, 1 = about to break.
    public void SetStress(float s01)
    {
        s01 = Mathf.Clamp01(s01);
        bool crossed = (stress01 < 0.35f) != (s01 < 0.35f) || (stress01 < 0.7f) != (s01 < 0.7f);
        stress01 = s01;
        if (hum != null)
        {
            hum.pitch = 1f - 0.12f * s01;
            rail.pitch = 1f + 0.25f * s01;
            bool fx = SaveSystem.Data.settings.horrorEffects;
            if (humDist != null) humDist.distortionLevel = fx ? Mathf.Clamp01((s01 - 0.5f) * 1.2f) : 0f;
            if (railDist != null) railDist.distortionLevel = fx ? Mathf.Clamp01((s01 - 0.6f)) : 0f;
            RefreshLoops();
        }
        if (crossed) UpdateAutoMusic();
    }

    void Update()
    {
        // an audible breath at high stress
        if (stress01 > 0.7f && SaveSystem.Data.settings.horrorEffects && Time.timeScale > 0f)
        {
            nextBreath -= Time.deltaTime;
            if (nextBreath <= 0f)
            {
                nextBreath = Mathf.Lerp(4.5f, 2.2f, (stress01 - 0.7f) / 0.3f) + Random.Range(0f, 1.2f);
                oneShot.pitch = Random.Range(0.85f, 1f);
                oneShot.PlayOneShot(Breath_(), 0.5f * sfxVol);
            }
        }
    }

    // ---------------- one-shots ----------------

    void Play(AudioClip c, float vol, float pitch = 1f)
    {
        if (oneShot == null || c == null) return;
        oneShot.pitch = pitch;
        oneShot.PlayOneShot(c, vol * sfxVol);
    }

    public void Stamp() { Play(Stamp_(), 0.95f, Random.Range(0.94f, 1.02f)); }
    public void Good() { Play(Good_(), 0.6f); }
    public void Bad() { Play(Bad_(), 0.7f); }
    public void Horn() { Play(Horn_(), 0.6f); }
    public void Whistle() { Play(Whistle_(), 0.6f); }
    public void Click() { Play(Click_(), 0.7f, Random.Range(0.9f, 1.15f)); }
    public void Paper() { Play(Paper_(), 0.8f, Random.Range(0.9f, 1.15f)); }
    public void Door() { Play(Door_(), 0.8f); }
    public void Tick() { Play(Tick_(), 0.25f, Random.Range(0.95f, 1.05f)); }
    public void Footstep() { Play(Step_(), 0.5f, Random.Range(0.8f, 1.2f)); }
    public void Knock() { Play(Knock_(), 0.9f); }
    public void KnockThirteen() { Play(Knock13_(), 0.9f); }
    public void PowerDown() { Play(PowerDown_(), 0.8f); }
    public void PowerUp() { Play(PowerUp_(), 0.8f); }
    public void Whisper() { Play(Whisper_(), 0.7f, Random.Range(0.85f, 1.1f)); }
    public void Breath() { Play(Breath_(), 0.5f); }

    public void Stinger(float intensity)
    {
        if (!SaveSystem.Data.settings.horrorEffects) return;
        Play(Stinger_(Random.Range(0, 3)), Mathf.Clamp01(0.25f + intensity * 0.75f));
    }
}
