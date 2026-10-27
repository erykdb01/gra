using UnityEngine;
using UnityEngine.InputSystem;

// Developer panel for the soundscape (F9). Only exists in the editor and in development builds.
// Lets you test every layer without playing through the game: bus levels, stress, night, scene, train state,
// far-away sounds and horror accents.
public class AudioDebug : MonoBehaviour
{
    bool open;
    float stress, night;
    Vector2 scroll;
    SoundFX fx;

    public static void Create(SoundFX owner)
    {
        if (!Application.isEditor && !Debug.isDebugBuild) return;
        var d = owner.gameObject.AddComponent<AudioDebug>();
        d.fx = owner;
    }

    void Update()
    {
        Keyboard kb = Keyboard.current;
        if (kb != null && kb.f9Key.wasPressedThisFrame) open = !open;
    }

    void OnGUI()
    {
        if (!open || fx == null || fx.Ambience == null) return;
        AmbienceDirector a = fx.Ambience;
        GUILayout.BeginArea(new Rect(12, 12, 360, Screen.height - 24), GUI.skin.box);
        GUILayout.Label("AUDIO DEBUG (F9)");
        scroll = GUILayout.BeginScrollView(scroll);

        GUILayout.Label("Layers");
        for (int i = 0; i < (int)Bus.Count; i++)
        {
            Bus b = (Bus)i;
            GUILayout.BeginHorizontal();
            GUILayout.Label(b.ToString(), GUILayout.Width(90));
            float v = GUILayout.HorizontalSlider(AudioBus.Get(b), 0f, 1.5f);
            if (Mathf.Abs(v - AudioBus.Get(b)) > 0.001f) { AudioBus.Set(b, v); fx.RefreshBuses(); }
            GUILayout.Label(AudioBus.Get(b).ToString("0.00"), GUILayout.Width(36));
            GUILayout.EndHorizontal();
        }
        if (GUILayout.Button("Reset layers")) { AudioBus.ResetAll(); fx.RefreshBuses(); }

        GUILayout.Space(6);
        GUILayout.Label("Scene");
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Menu")) a.SetScene(true);
        if (GUILayout.Button("Booth")) a.SetScene(false);
        GUILayout.EndHorizontal();

        GUILayout.Label("Stress " + stress.ToString("0.00"));
        float ns = GUILayout.HorizontalSlider(stress, 0f, 1f);
        if (Mathf.Abs(ns - stress) > 0.001f) { stress = ns; fx.SetStress(stress); }
        GUILayout.Label("Night " + Mathf.RoundToInt(night));
        float nn = GUILayout.HorizontalSlider(night, 0f, 12f);
        if (Mathf.Abs(nn - night) > 0.01f) { night = nn; fx.SetNight(Mathf.RoundToInt(night)); }

        GUILayout.Label("Train");
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Away")) { fx.SetTrainPresent(false); fx.SetTrainState(TrainAudioState.Away); }
        if (GUILayout.Button("Arrive")) { fx.SetTrainPresent(true); fx.SetTrainState(TrainAudioState.Arriving); }
        if (GUILayout.Button("Stopped")) { fx.SetTrainPresent(true); fx.SetTrainState(TrainAudioState.Stopped); }
        if (GUILayout.Button("Depart")) fx.SetTrainState(TrainAudioState.Departing);
        GUILayout.EndHorizontal();

        GUILayout.Label("Passenger type");
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Alive")) fx.OnPassenger(PassengerTruth.Alive, false);
        if (GUILayout.Button("Dead")) fx.OnPassenger(PassengerTruth.Dead, false);
        if (GUILayout.Button("None")) fx.OnPassenger(PassengerTruth.NonExistent, false);
        if (GUILayout.Button("Unk")) fx.OnPassenger(PassengerTruth.Unknown, false);
        GUILayout.EndHorizontal();

        GUILayout.Label("Far-away sounds");
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Train")) a.TriggerDistant(DistantKind.Train);
        if (GUILayout.Button("Creak")) a.TriggerDistant(DistantKind.Creak);
        if (GUILayout.Button("Drip")) a.TriggerDistant(DistantKind.Drip);
        if (GUILayout.Button("Relay")) a.TriggerDistant(DistantKind.Relay);
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Gust")) a.TriggerDistant(DistantKind.Gust);
        if (GUILayout.Button("Thud")) a.TriggerDistant(DistantKind.Thud);
        if (GUILayout.Button("Horn")) a.TriggerDistant(DistantKind.Horn);
        GUILayout.EndHorizontal();

        GUILayout.Label("Accents");
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Stamp")) fx.Stamp();
        if (GUILayout.Button("Paper")) fx.Paper();
        if (GUILayout.Button("Step")) fx.Footstep();
        if (GUILayout.Button("Door")) fx.Door();
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Knock")) fx.Knock();
        if (GUILayout.Button("Whisper")) fx.Whisper();
        if (GUILayout.Button("Stinger")) fx.Stinger(0.6f);
        if (GUILayout.Button("Power")) { fx.PowerDown(); }
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Radio on")) fx.RadioOn(true);
        if (GUILayout.Button("Radio off")) fx.RadioOn(false);
        if (GUILayout.Button("Drone +")) a.SetEventDrone(1f);
        if (GUILayout.Button("Drone -")) a.SetEventDrone(0f);
        GUILayout.EndHorizontal();

        GUILayout.Label("Music");
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Menu")) fx.PlayMusic("menu");
        if (GUILayout.Button("Shift")) fx.PlayMusic("shift");
        if (GUILayout.Button("Tension")) fx.PlayMusic("tension");
        if (GUILayout.Button("Horror")) fx.PlayMusic("horror");
        if (GUILayout.Button("Off")) fx.PlayMusic(null);
        GUILayout.EndHorizontal();

        GUILayout.Space(6);
        GUILayout.Label(a.Status());
        GUILayout.EndScrollView();
        GUILayout.EndArea();
    }
}
