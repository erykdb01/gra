using System;
using System.Collections.Generic;

// Everything that is written to disk lives here (plain serializable data, no MonoBehaviours).
[Serializable]
public class SettingsData
{
    public float master = 0.8f;
    public float music = 0.7f;
    public float sfx = 0.9f;
    public bool fullscreen = true;
    public int resolutionIndex = -1;    // -1 = keep the current resolution
    public bool screenShake = true;
    public bool horrorEffects = true;
    public int textSpeed = 1;           // 0 slow, 1 normal, 2 fast, 3 instant
    public int language = -1;           // -1 = not chosen yet
}

[Serializable]
public class StoryProgress
{
    public bool started;
    public bool finished;
    public int night;                   // index of the next night to play
    public int conscience = 5;
    public int discipline = 5;
    public int stress;
    public int reputation = 50;
    public int integrity = 100;
    public int totalCorrect;
    public int totalMistakes;
    public int endingId = -1;
    public List<string> flags = new List<string>();
    public int[] skillXp = new int[5];
}

[Serializable]
public class EndlessStats
{
    public int highScore;
    public int bestLevel;
    public int totalPassengers;
    public int totalCorrect;
    public int totalMistakes;
    public int bestStreak;
    public int runs;
    public int[] skillXp = new int[5];
}

[Serializable]
public class SaveData
{
    public int version = 2;
    public SettingsData settings = new SettingsData();
    public StoryProgress story = new StoryProgress();
    public EndlessStats endless = new EndlessStats();
    public List<string> codex = new List<string>();
    public List<string> achievements = new List<string>();
    public List<string> endings = new List<string>();
}
