using System;
using UnityEngine;

// Simple localisation: Polish and English. Use Loc.T("polski tekst", "English text").
// The chosen language is saved and applied everywhere; screens refresh via the Changed event.
public static class Loc
{
    public enum Language { Polish = 0, English = 1 }

    const string Key = "opt_lang";

    static bool loaded;
    static Language current = Language.Polish;

    public static event Action Changed;

    public static Language Current
    {
        get { Load(); return current; }
    }

    public static bool IsPolish
    {
        get { Load(); return current == Language.Polish; }
    }

    static void Load()
    {
        if (loaded) return;
        loaded = true;
        // first launch: follow the system language (Polish -> Polish, everything else -> English)
        int def = Application.systemLanguage == SystemLanguage.Polish ? 0 : 1;
        current = (Language)PlayerPrefs.GetInt(Key, def);
    }

    public static void Set(Language lang)
    {
        Load();
        if (current == lang) return;
        current = lang;
        PlayerPrefs.SetInt(Key, (int)lang);
        PlayerPrefs.Save();
        if (Changed != null) Changed();
    }

    // Pick the text for the current language.
    public static string T(string pl, string en)
    {
        return IsPolish ? pl : en;
    }
}
