using System;
using UnityEngine;

// Localisation: Polish, English and Ukrainian.
//   Loc.T("polski tekst", "English text")               - Ukrainian comes from the UkStrings table (keyed by the Polish text)
//   Loc.T("polski", "English", "український")            - all three given explicitly (needed for text built at run time)
//   Loc.Plural(n, ...)                                   - correct noun forms after a number (1 / 2-4 / 5+ rules of each language)
// The chosen language is saved and applied everywhere; screens refresh via the Changed event.
public static class Loc
{
    public enum Language { Polish = 0, English = 1, Ukrainian = 2 }

    const string Key = "opt_lang";

    static bool loaded;
    static Language current = Language.Polish;

    public static event Action Changed;

    public static Language Current
    {
        get { Load(); return current; }
    }

    public static bool IsPolish { get { Load(); return current == Language.Polish; } }
    public static bool IsEnglish { get { Load(); return current == Language.English; } }
    public static bool IsUkrainian { get { Load(); return current == Language.Ukrainian; } }

    static void Load()
    {
        if (loaded) return;
        loaded = true;
        // first launch: follow the system language (Polish -> Polish, Ukrainian -> Ukrainian, everything else -> English)
        int def = 1;
        if (Application.systemLanguage == SystemLanguage.Polish) def = 0;
        else if (Application.systemLanguage == SystemLanguage.Ukrainian) def = 2;
        int v = PlayerPrefs.GetInt(Key, def);
        current = (Language)Mathf.Clamp(v, 0, 2);
        LocFont.Ensure();
    }

    public static void Set(Language lang)
    {
        Load();
        if (current == lang) return;
        current = lang;
        PlayerPrefs.SetInt(Key, (int)lang);
        PlayerPrefs.Save();
        if (lang == Language.Ukrainian) LocFont.Ensure();
        if (Changed != null) Changed();
    }

    // Pick the text for the current language. Ukrainian is looked up by the Polish text.
    public static string T(string pl, string en)
    {
        Load();
        switch (current)
        {
            case Language.Polish: return pl;
            case Language.Ukrainian: return UkStrings.Get(pl, en);
            default: return en;
        }
    }

    // All three languages given explicitly (for text assembled at run time: names, numbers, gendered forms).
    public static string T(string pl, string en, string uk)
    {
        Load();
        switch (current)
        {
            case Language.Polish: return pl;
            case Language.Ukrainian: return uk ?? UkStrings.Get(pl, en);
            default: return en;
        }
    }

    // Noun after a number.
    //   Polish:    one / few (2-4, 22-24 ...) / many (0, 5-21 ...)
    //   Ukrainian: one (1, 21 ...) / few (2-4, 22-24 ...) / many (0, 5-20 ...)
    //   English:   one / other
    public static string Plural(int n, string pl1, string pl2, string pl5, string en1, string enN, string uk1, string uk2, string uk5)
    {
        Load();
        int a = Mathf.Abs(n);
        switch (current)
        {
            case Language.Polish:
                if (a == 1) return pl1;
                return (a % 10 >= 2 && a % 10 <= 4 && (a % 100 < 12 || a % 100 > 14)) ? pl2 : pl5;
            case Language.Ukrainian:
                if (a % 10 == 1 && a % 100 != 11) return uk1;
                return (a % 10 >= 2 && a % 10 <= 4 && (a % 100 < 12 || a % 100 > 14)) ? uk2 : uk5;
            default:
                return a == 1 ? en1 : enN;
        }
    }

    // Same rules, but for an explicit language (0 = Polish, 1 = English, 2 = Ukrainian): used to build a text in all languages at once.
    public static string PluralOf(int n, int lang, string pl1, string pl2, string pl5, string en1, string enN, string uk1, string uk2, string uk5)
    {
        int a = Mathf.Abs(n);
        if (lang == 0)
        {
            if (a == 1) return pl1;
            return (a % 10 >= 2 && a % 10 <= 4 && (a % 100 < 12 || a % 100 > 14)) ? pl2 : pl5;
        }
        if (lang == 2)
        {
            if (a % 10 == 1 && a % 100 != 11) return uk1;
            return (a % 10 >= 2 && a % 10 <= 4 && (a % 100 < 12 || a % 100 > 14)) ? uk2 : uk5;
        }
        return a == 1 ? en1 : enN;
    }

    // Convenience: number + noun, e.g. Loc.Count(5, ...) -> "5 pasażerów".
    public static string Count(int n, string pl1, string pl2, string pl5, string en1, string enN, string uk1, string uk2, string uk5)
    {
        return n + " " + Plural(n, pl1, pl2, pl5, en1, enN, uk1, uk2, uk5);
    }

    // Proper name (person, town, station) in the current language; Polish and English keep the original spelling.
    public static string Name(string pl)
    {
        Load();
        return current == Language.Ukrainian ? UkNames.Get(pl) : pl;
    }

    // Short language code for buttons.
    public static string Code
    {
        get { Load(); return current == Language.Polish ? "PL" : (current == Language.Ukrainian ? "UA" : "EN"); }
    }
}
