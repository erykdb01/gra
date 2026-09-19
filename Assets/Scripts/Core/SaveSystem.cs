using System;
using System.IO;
using UnityEngine;

// Loads and saves SaveData as JSON in the persistent data folder.
// The file is written to a temporary name first, so a crash never leaves a half-written save.
public static class SaveSystem
{
    static SaveData data;
    static string FilePath { get { return Path.Combine(Application.persistentDataPath, "ostatni_peron_save.json"); } }
    static string BackupPath { get { return FilePath + ".bak"; } }

    public static SaveData Data
    {
        get { if (data == null) Load(); return data; }
    }

    public static bool HasStorySave
    {
        get { return Data.story != null && Data.story.started && !Data.story.finished; }
    }

    public static void Load()
    {
        data = TryRead(FilePath);
        if (data == null) data = TryRead(BackupPath);
        if (data == null) data = new SaveData();
        Sanitize(data);
    }

    static SaveData TryRead(string path)
    {
        try
        {
            if (!File.Exists(path)) return null;
            string json = File.ReadAllText(path);
            if (string.IsNullOrEmpty(json)) return null;
            SaveData d = JsonUtility.FromJson<SaveData>(json);
            return d;
        }
        catch (Exception e)
        {
            Debug.LogWarning("SaveSystem: cannot read " + path + ": " + e.Message);
            return null;
        }
    }

    // Repairs missing pieces (old versions or a damaged file).
    static void Sanitize(SaveData d)
    {
        if (d.settings == null) d.settings = new SettingsData();
        if (d.story == null) d.story = new StoryProgress();
        if (d.endless == null) d.endless = new EndlessStats();
        if (d.codex == null) d.codex = new System.Collections.Generic.List<string>();
        if (d.achievements == null) d.achievements = new System.Collections.Generic.List<string>();
        if (d.endings == null) d.endings = new System.Collections.Generic.List<string>();
        if (d.story.flags == null) d.story.flags = new System.Collections.Generic.List<string>();
        if (d.story.skillXp == null || d.story.skillXp.Length != 5) d.story.skillXp = new int[5];
        if (d.endless.skillXp == null || d.endless.skillXp.Length != 5) d.endless.skillXp = new int[5];
        d.settings.master = Mathf.Clamp01(d.settings.master);
        d.settings.music = Mathf.Clamp01(d.settings.music);
        d.settings.sfx = Mathf.Clamp01(d.settings.sfx);
        d.settings.textSpeed = Mathf.Clamp(d.settings.textSpeed, 0, 3);
        d.story.night = Mathf.Max(0, d.story.night);
        d.story.conscience = Mathf.Clamp(d.story.conscience, 0, 10);
        d.story.discipline = Mathf.Clamp(d.story.discipline, 0, 10);
        d.story.stress = Mathf.Clamp(d.story.stress, 0, 100);
    }

    public static void Save()
    {
        if (data == null) return;
        try
        {
            string dir = Path.GetDirectoryName(FilePath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir)) Directory.CreateDirectory(dir);

            string json = JsonUtility.ToJson(data, true);
            string tmp = FilePath + ".tmp";
            File.WriteAllText(tmp, json);
            if (File.Exists(FilePath))
            {
                File.Copy(FilePath, BackupPath, true);
                File.Delete(FilePath);
            }
            File.Move(tmp, FilePath);
        }
        catch (Exception e)
        {
            Debug.LogWarning("SaveSystem: cannot write the save: " + e.Message);
        }
    }

    // Starts a completely new story (codex, achievements and settings are kept).
    public static void ResetStory()
    {
        Data.story = new StoryProgress();
        Data.story.started = true;
        Save();
    }
}
