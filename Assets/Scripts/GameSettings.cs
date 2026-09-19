using System.Collections.Generic;
using UnityEngine;

// Player settings, stored inside the JSON save (SaveData.settings).
// Applied once when the game starts and whenever the player changes something.
public static class GameSettings
{
    static SettingsData S { get { return SaveSystem.Data.settings; } }

    public static float Master
    {
        get { return S.master; }
        set { S.master = Mathf.Clamp01(value); AudioListener.volume = S.master; Changed(); }
    }

    public static float Music
    {
        get { return S.music; }
        set { S.music = Mathf.Clamp01(value); Changed(); if (SoundFX.I != null) SoundFX.I.ApplyVolumes(); }
    }

    public static float Sfx
    {
        get { return S.sfx; }
        set { S.sfx = Mathf.Clamp01(value); Changed(); if (SoundFX.I != null) SoundFX.I.ApplyVolumes(); }
    }

    // kept for older callers: the master volume
    public static float Volume
    {
        get { return Master; }
        set { Master = value; }
    }

    public static bool Fullscreen
    {
        get { return S.fullscreen; }
        set { S.fullscreen = value; Changed(); ApplyScreen(); }
    }

    public static bool ScreenShake
    {
        get { return S.screenShake; }
        set { S.screenShake = value; Changed(); }
    }

    public static bool HorrorEffects
    {
        get { return S.horrorEffects; }
        set { S.horrorEffects = value; Changed(); }
    }

    public static int TextSpeed
    {
        get { return S.textSpeed; }
        set { S.textSpeed = Mathf.Clamp(value, 0, 3); Changed(); }
    }

    // ---------- resolutions ----------

    static readonly List<Vector2Int> resolutions = new List<Vector2Int>();

    public static List<Vector2Int> Resolutions
    {
        get
        {
            if (resolutions.Count == 0)
            {
                foreach (Resolution r in Screen.resolutions)
                {
                    if (r.width < 960 || r.height < 540) continue;
                    var v = new Vector2Int(r.width, r.height);
                    if (!resolutions.Contains(v)) resolutions.Add(v);
                }
                if (resolutions.Count == 0)
                {
                    resolutions.Add(new Vector2Int(1280, 720));
                    resolutions.Add(new Vector2Int(1920, 1080));
                }
            }
            return resolutions;
        }
    }

    // -1 = keep the current resolution.
    public static int ResolutionIndex
    {
        get { return Mathf.Clamp(S.resolutionIndex, -1, Resolutions.Count - 1); }
        set { S.resolutionIndex = Mathf.Clamp(value, -1, Resolutions.Count - 1); Changed(); ApplyScreen(); }
    }

    public static string ResolutionLabel()
    {
        int i = ResolutionIndex;
        if (i < 0) return Loc.T("AUTOMATYCZNA", "AUTO");
        Vector2Int r = Resolutions[i];
        return r.x + " x " + r.y;
    }

    // ---------- applying ----------

    static void Changed()
    {
        // the file is written when the settings screen closes (Flush) or at the next autosave
    }

    public static void Flush()
    {
        SaveSystem.Save();
    }

    // Applied once when the game starts, before the first scene loads.
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Init()
    {
        AudioListener.volume = S.master;
        ApplyScreen();
    }

    static void ApplyScreen()
    {
        if (Application.isEditor) return;   // the Editor Game view is not affected

        FullScreenMode mode = S.fullscreen ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed;
        int i = ResolutionIndex;
        if (i >= 0)
        {
            Vector2Int r = Resolutions[i];
            Screen.SetResolution(r.x, r.y, mode);
        }
        else if (S.fullscreen)
        {
            Screen.fullScreenMode = mode;
        }
        else
        {
            Screen.SetResolution(1280, 720, FullScreenMode.Windowed);
        }
    }

    // Quits the built game; in the Editor it stops Play mode instead.
    public static void QuitGame()
    {
        SaveSystem.Save();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
