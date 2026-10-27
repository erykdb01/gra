using UnityEngine;

// Independent volume "buses" (layers) of the soundscape. Every layer has its own multiplier that can be changed
// at run time (debug panel, story events) without touching the player's saved volume sliders.
public enum Bus
{
    Rain = 0,       // rain on the platform and on the glass
    Station,        // wind, air, the hush of the station
    Train,          // idle rumble, rails, the train passing
    RoomTone,       // the booth: low room tone
    Distant,        // far-away one-shots (train, metal, drips, wind gusts)
    Horror,         // low drone, whispers, knocks, stingers
    Music,
    UI,             // clicks and ticks
    Documents,      // paper
    Stamp,          // the stamp and result sounds
    Footsteps,
    Dialogue,       // typewriter ticks, breathing, voices
    Count
}

public static class AudioBus
{
    static readonly float[] user = new float[(int)Bus.Count];
    static bool init;

    static void Init()
    {
        if (init) return;
        init = true;
        for (int i = 0; i < user.Length; i++) user[i] = 1f;
    }

    public static float Get(Bus b) { Init(); return user[(int)b]; }

    public static void Set(Bus b, float v) { Init(); user[(int)b] = Mathf.Clamp(v, 0f, 1.5f); }

    public static void ResetAll() { init = false; Init(); }
}
