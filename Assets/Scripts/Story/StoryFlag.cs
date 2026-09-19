using System.Collections.Generic;

// Names of the story flags used by the game. A flag is just a string that is either set or not.
public static class Flag
{
    public const string SawTheDead = "SawTheDead";
    public const string FoundHiddenRegistry = "FoundHiddenRegistry";
    public const string TrustedRailway = "TrustedRailway";
    public const string DistrustedRailway = "DistrustedRailway";
    public const string EnteredRestrictedCar = "EnteredRestrictedCar";
    public const string MetTheConductor = "MetTheConductor";
    public const string SawOwnPhoto = "SawOwnPhoto";
    public const string RememberedLastNight = "RememberedLastNight";
    public const string ForgotLastNight = "ForgotLastNight";
    public const string ListenedToKnocking = "ListenedToKnocking";
    public const string LearnedLine13 = "LearnedLine13";
    public const string ReadConductorFile = "ReadConductorFile";
    public const string AdmittedDead = "AdmittedDead";
    public const string MetInspector = "MetInspector";
    public const string ErasedRegistry = "ErasedRegistry";
    public const string RestoredRegistry = "RestoredRegistry";

    public static string Helped(string who) { return "HelpedPassenger_" + who; }
    public static string Denied(string who) { return "DeniedPassenger_" + who; }
}

// The set of story flags for the current run. In Story mode the set is stored in the save file.
public static class StoryFlags
{
    static readonly HashSet<string> set = new HashSet<string>();

    public static bool Has(string id) { return !string.IsNullOrEmpty(id) && set.Contains(id); }

    public static void Set(string id)
    {
        if (string.IsNullOrEmpty(id)) return;
        if (set.Add(id)) GameEvents.RaiseFlag(id);
    }

    public static void Clear() { set.Clear(); }

    public static void LoadFrom(List<string> list)
    {
        set.Clear();
        if (list == null) return;
        for (int i = 0; i < list.Count; i++) set.Add(list[i]);
    }

    public static List<string> ToList() { return new List<string>(set); }

    public static int CountWithPrefix(string prefix)
    {
        int n = 0;
        foreach (string s in set) if (s.StartsWith(prefix)) n++;
        return n;
    }
}
