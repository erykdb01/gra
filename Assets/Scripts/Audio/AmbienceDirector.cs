using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public enum TrainAudioState { Away, Arriving, Stopped, Departing }
public enum DistantKind { Train, Creak, Drip, Relay, Gust, Thud, Horn }

// Runs the looping "beds" of the soundscape (rain, station air, train, room tone, low unease) and a scheduler of rare
// far-away sounds. Every layer belongs to a Bus, fades smoothly, and is driven by the game state: menu or booth,
// stress, train state, night, the kind of passenger and story events. All clips are synthesised on a worker thread
// (see AudioDSP), so nothing hitches when a scene starts; layers fade in as soon as their clip is ready.
public class AmbienceDirector : MonoBehaviour
{
    class Layer
    {
        public string name;
        public AudioSource src;
        public AudioLowPassFilter lp;
        public Bus bus;
        public float baseVol, cur, target, fade = 0.3f;
        public bool loading;
        public Func<AudioDSP.Buf> gen;
    }

    readonly List<Layer> layers = new List<Layer>();
    Layer rainFar, rainNear, station, room, trainIdle, rails, drone;

    readonly AudioSource[] far = new AudioSource[2];
    readonly Dictionary<DistantKind, AudioClip> distantClips = new Dictionary<DistantKind, AudioClip>();
    readonly HashSet<DistantKind> distantLoading = new HashSet<DistantKind>();
    readonly Dictionary<DistantKind, float> lastPlayed = new Dictionary<DistantKind, float>();
    readonly ConcurrentQueue<Action> mainThread = new ConcurrentQueue<Action>();

    public float sfxVol = 0.9f;
    public Func<AudioClip> hornProvider;

    // ---- state ----
    bool menu = true, started;
    float stress, sStress;
    TrainAudioState train = TrainAudioState.Stopped;
    bool trainPresent = true;
    int night;
    float passCold, passColdT, passHush, passHushT, passUnease, passUneaseT;
    float eventDrone, eventDroneT;
    float duck, duckHold;
    float nextDistant = 5f;
    float rainCut = 9000f;

    // train-pass pan animation
    AudioSource panSrc; float panT, panDur, panFrom, panTo;

    public bool Menu { get { return menu; } }

    // ------------------------------------------------------------------ public API

    public void Begin(bool inGame)
    {
        menu = !inGame;
        if (!started)
        {
            started = true;
            rainFar = Add("RainFar", Bus.Rain, 0.55f, true, 9000f, () => AudioDSP.Rain(1, 24, 32000, 1.0, 30, 0.5, 0.25, 6500));
            rainNear = Add("RainNear", Bus.Rain, 0.40f, false, 0f, () => AudioDSP.Rain(2, 20, 32000, 0.0, 170, 0.9, 0.5, 6500));
            station = Add("Station", Bus.Station, 0.60f, false, 0f, () => AudioDSP.Station(3, 40, 16000));
            room = Add("Room", Bus.RoomTone, 0.45f, false, 0f, () => AudioDSP.RoomTone(4, 30, 16000));
            trainIdle = Add("TrainIdle", Bus.Train, 0.50f, false, 0f, () => AudioDSP.TrainIdle(5, 12, 16000));
            rails = Add("Rails", Bus.Train, 0.45f, false, 0f, () => AudioDSP.Rails(6, 8, 22050));
            drone = Add("Drone", Bus.Horror, 0.35f, false, 0f, () => AudioDSP.HorrorDrone(7, 20, 16000));
            for (int i = 0; i < far.Length; i++)
            {
                var go = new GameObject("Distant" + i);
                go.transform.SetParent(transform, false);
                var a = go.AddComponent<AudioSource>();
                a.playOnAwake = false; a.loop = false; a.spatialBlend = 0f;
                var lp = go.AddComponent<AudioLowPassFilter>(); lp.cutoffFrequency = 2400f;
                var rv = go.AddComponent<AudioReverbFilter>(); rv.reverbPreset = AudioReverbPreset.Hangar;
                far[i] = a;
            }
        }
        nextDistant = menu ? UnityEngine.Random.Range(7f, 12f) : UnityEngine.Random.Range(10f, 18f);
    }

    public void SetScene(bool menuScene) { menu = menuScene; }
    public void SetStress(float s01) { stress = Mathf.Clamp01(s01); }
    public void SetTrainState(TrainAudioState s) { train = s; }
    public void SetTrainPresent(bool present) { trainPresent = present; }
    public void SetNight(int n) { night = Mathf.Max(0, n); }
    public void SetEventDrone(float v) { eventDroneT = Mathf.Clamp01(v); }

    // The kind of passenger at the window subtly changes the room. Nothing obvious: a little more hush, a little colder.
    public void OnPassenger(PassengerTruth t, bool isPlayer)
    {
        passColdT = 0f; passHushT = 0f; passUneaseT = 0f;
        switch (t)
        {
            case PassengerTruth.Dead: passColdT = 0.6f; break;
            case PassengerTruth.NonExistent: passHushT = 0.6f; break;
            case PassengerTruth.Unknown: passUneaseT = 0.7f; passHushT = 0.3f; break;
            case PassengerTruth.Echo: passHushT = 0.25f; break;
            case PassengerTruth.Loop: passUneaseT = 0.25f; break;
        }
        if (isPlayer) { passUneaseT = 1f; passHushT = 0.5f; }
    }

    // Lowers the beds for a moment (under a stamp, a stinger, a knock) so accents never pile up into a loud wall.
    public void Duck(float amount, float hold)
    {
        duck = Mathf.Max(duck, Mathf.Clamp01(amount));
        duckHold = Mathf.Max(duckHold, hold);
    }

    public void TriggerDistant(DistantKind k) { PlayDistant(k, true); }

    public string Status()
    {
        var sb = new System.Text.StringBuilder();
        for (int i = 0; i < layers.Count; i++)
        {
            Layer L = layers[i];
            sb.Append(L.name).Append(' ').Append(L.src.clip == null ? (L.loading ? "building" : "-") : (L.cur * L.baseVol).ToString("0.00")).Append("   ");
        }
        return sb.ToString();
    }

    // ------------------------------------------------------------------ internals

    Layer Add(string name, Bus bus, float baseVol, bool lowpass, float cutoff, Func<AudioDSP.Buf> gen)
    {
        var go = new GameObject("Amb_" + name);
        go.transform.SetParent(transform, false);
        var a = go.AddComponent<AudioSource>();
        a.playOnAwake = false; a.loop = true; a.spatialBlend = 0f; a.volume = 0f;
        var L = new Layer { name = name, src = a, bus = bus, baseVol = baseVol, gen = gen };
        if (lowpass) { L.lp = go.AddComponent<AudioLowPassFilter>(); L.lp.cutoffFrequency = cutoff; }
        layers.Add(L);
        return L;
    }

    void Request(Layer L)
    {
        if (L.loading || L.gen == null) return;
        L.loading = true;
        Func<AudioDSP.Buf> g = L.gen;
        Task.Run(() =>
        {
            try
            {
                AudioDSP.Buf b = g();
                mainThread.Enqueue(() => Deliver(L, b));
            }
            catch (Exception e)
            {
                string m = e.Message;
                mainThread.Enqueue(() => { L.loading = false; L.gen = null; Debug.LogWarning("[Audio] " + L.name + ": " + m); });
            }
        });
    }

    static AudioClip ToClip(string name, AudioDSP.Buf b)
    {
        int frames = b.data.Length / b.channels;
        AudioClip c = AudioClip.Create(name, frames, b.channels, b.rate, false);
        c.SetData(b.data, 0);
        return c;
    }

    void Deliver(Layer L, AudioDSP.Buf b)
    {
        L.loading = false;
        if (L.src == null) return;
        L.src.clip = ToClip(L.name, b);
    }

    Func<AudioDSP.Buf> DistantGen(DistantKind k, int seed)
    {
        switch (k)
        {
            case DistantKind.Train: return () => AudioDSP.DistantTrain(seed, 16000);
            case DistantKind.Creak: return () => AudioDSP.Creak(seed, 16000);
            case DistantKind.Drip: return () => AudioDSP.Drip(seed, 16000);
            case DistantKind.Relay: return () => AudioDSP.Relay(seed, 16000);
            case DistantKind.Gust: return () => AudioDSP.Gust(seed, 16000);
            default: return () => AudioDSP.Thud(seed, 16000);
        }
    }

    void PlayDistant(DistantKind k, bool force)
    {
        AudioClip clip;
        if (k == DistantKind.Horn)
        {
            clip = hornProvider != null ? hornProvider() : null;
            if (clip == null) return;
        }
        else if (!distantClips.TryGetValue(k, out clip))
        {
            if (distantLoading.Add(k))
            {
                Func<AudioDSP.Buf> g = DistantGen(k, 100 + (int)k);
                DistantKind kk = k;
                Task.Run(() =>
                {
                    try { AudioDSP.Buf b = g(); mainThread.Enqueue(() => { distantClips[kk] = ToClip("far_" + kk, b); distantLoading.Remove(kk); }); }
                    catch (Exception e) { string m = e.Message; mainThread.Enqueue(() => { distantLoading.Remove(kk); Debug.LogWarning("[Audio] " + kk + ": " + m); }); }
                });
            }
            return;
        }

        AudioSource s = far[0].isPlaying && !far[1].isPlaying ? far[1] : far[0];
        if (s.isPlaying && !force) return;
        float vol;
        switch (k)
        {
            case DistantKind.Train: vol = 0.9f; break;
            case DistantKind.Creak: vol = 0.55f; break;
            case DistantKind.Drip: vol = 0.6f; break;
            case DistantKind.Relay: vol = 0.55f; break;
            case DistantKind.Gust: vol = 0.7f; break;
            case DistantKind.Thud: vol = 0.5f; break;
            default: vol = 0.35f; break;
        }
        s.pitch = k == DistantKind.Train ? 1f : UnityEngine.Random.Range(0.94f, 1.06f);
        s.panStereo = UnityEngine.Random.Range(-0.85f, 0.85f);
        if (k == DistantKind.Train)
        {
            panSrc = s; panT = 0f; panDur = clip.length; panFrom = UnityEngine.Random.value < 0.5f ? -0.85f : 0.85f; panTo = -panFrom;
            s.panStereo = panFrom;
        }
        s.PlayOneShot(clip, vol * AudioBus.Get(Bus.Distant) * sfxVol * BedDuck());
        lastPlayed[k] = Time.unscaledTime;
    }

    float BedDuck() { return 1f - 0.4f * duck; }

    void PickDistant()
    {
        float night01 = Mathf.Clamp01(night / 12f);
        DistantKind[] kinds = { DistantKind.Train, DistantKind.Creak, DistantKind.Drip, DistantKind.Relay, DistantKind.Gust, DistantKind.Thud, DistantKind.Horn };
        float[] w = { 1.6f, 2f, 3f, 1.2f, 2f, 0.4f + night01 * 1.0f + stress * 0.8f, 0.5f };
        float[] cool = { 60f, 25f, 10f, 30f, 20f, 45f, 90f };
        if (menu) w[5] = 0.1f;
        float total = 0f;
        for (int i = 0; i < kinds.Length; i++)
        {
            float lp;
            if (lastPlayed.TryGetValue(kinds[i], out lp) && Time.unscaledTime - lp < cool[i]) w[i] = 0f;
            total += w[i];
        }
        if (total <= 0f) return;
        float r = UnityEngine.Random.value * total;
        for (int i = 0; i < kinds.Length; i++)
        {
            if (r < w[i]) { PlayDistant(kinds[i], false); return; }
            r -= w[i];
        }
    }

    void ComputeTargets()
    {
        float s = sStress;
        bool moving = train == TrainAudioState.Arriving || train == TrainAudioState.Departing;
        if (menu)
        {
            rainFar.target = 1f; rainNear.target = 0.85f; station.target = 0.75f; room.target = 0f;
            trainIdle.target = trainPresent && !moving ? 0.35f : 0f;
            rails.target = moving ? 0.6f : 0f;
            drone.target = 0f;
            rainCut = 9000f;
        }
        else
        {
            float hush = Mathf.Clamp01(1f - 0.4f * s - 0.3f * passHush);
            rainFar.target = 0.65f * hush;
            rainNear.target = 0.22f * hush;
            station.target = 0.5f * (1f - 0.6f * passHush);
            room.target = Mathf.Clamp01(0.75f + 0.25f * s - 0.35f * passCold);
            trainIdle.target = trainPresent ? (moving ? 0.8f : 0.45f) : 0f;
            rails.target = moving ? 0.85f : Mathf.Clamp01((s - 0.6f) * 1.2f) * 0.35f;
            float night01 = Mathf.Clamp01(night / 12f);
            drone.target = Mathf.Clamp01(Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0.45f, 1f, s)) * 0.5f + 0.06f * night01 + eventDrone + passUnease * 0.25f);
            rainCut = Mathf.Lerp(2600f, 1500f, s) - 600f * passCold;
        }
    }

    void Update()
    {
        if (!started) return;
        float dt = Time.unscaledDeltaTime;
        Action a;
        while (mainThread.TryDequeue(out a)) a();

        sStress = Mathf.MoveTowards(sStress, stress, dt * 0.5f);
        passCold = Mathf.MoveTowards(passCold, passColdT, dt * 0.25f);
        passHush = Mathf.MoveTowards(passHush, passHushT, dt * 0.25f);
        passUnease = Mathf.MoveTowards(passUnease, passUneaseT, dt * 0.25f);
        eventDrone = Mathf.MoveTowards(eventDrone, eventDroneT, dt * 0.4f);
        if (duckHold > 0f) duckHold -= dt; else duck = Mathf.MoveTowards(duck, 0f, dt * 0.9f);

        ComputeTargets();
        float bd = BedDuck();
        for (int i = 0; i < layers.Count; i++)
        {
            Layer L = layers[i];
            L.cur = Mathf.MoveTowards(L.cur, L.target, dt * L.fade);
            if (L.src.clip == null)
            {
                if (L.target > 0.01f) Request(L);
                continue;
            }
            float v = L.cur * L.baseVol * AudioBus.Get(L.bus) * bd * sfxVol;
            L.src.volume = v;
            if (v > 0.0005f) { if (!L.src.isPlaying) L.src.Play(); }
            else if (L.target <= 0.001f && L.src.isPlaying) L.src.Pause();
        }
        if (rainFar.lp != null) rainFar.lp.cutoffFrequency = Mathf.MoveTowards(rainFar.lp.cutoffFrequency, rainCut, dt * 2500f);

        if (panSrc != null)
        {
            panT += dt;
            panSrc.panStereo = Mathf.Lerp(panFrom, panTo, Mathf.Clamp01(panT / Mathf.Max(0.1f, panDur)));
            if (panT > panDur) panSrc = null;
        }

        nextDistant -= dt;
        if (nextDistant <= 0f)
        {
            float density = menu ? 1f : Mathf.Lerp(1f, 0.45f, sStress);   // more silence when tension is high
            nextDistant = UnityEngine.Random.Range(9f, 24f) / Mathf.Max(0.3f, density);
            PickDistant();
        }
    }
}
