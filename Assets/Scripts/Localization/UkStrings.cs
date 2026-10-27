// Ukrainian localization: lookup by exact Polish source string (see Loc.T).
using System.Collections.Generic;
using UnityEngine;

public static partial class UkStrings
{
    static Dictionary<string, string> map;

    static void Build()
    {
        map = new Dictionary<string, string>(4096);
        Add0(map);
        Add1(map);
        Add2(map);
        Add3(map);
    }

    /// <summary>Ukrainian text for a Polish key; falls back to English if missing.</summary>
    public static string Get(string pl, string en)
    {
        if (string.IsNullOrEmpty(pl)) return en;
        if (map == null) Build();
        string uk;
        if (map.TryGetValue(pl, out uk)) return uk;
#if UNITY_EDITOR
        if (!warned.Contains(pl)) { warned.Add(pl); Debug.LogWarning("[UkStrings] missing: " + pl); }
#endif
        return en;
    }
#if UNITY_EDITOR
    static readonly HashSet<string> warned = new HashSet<string>();
#endif
}
