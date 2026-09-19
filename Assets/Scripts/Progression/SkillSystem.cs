using UnityEngine;

public enum Skill { Observation = 0, Documents = 1, Interrogation = 2, Composure = 3, Authority = 4 }

// Five skills, levels 1-10, raised with XP. The XP array belongs to the current mode (story / endless).
public static class SkillSystem
{
    public const int MaxLevel = 10;
    static int[] xp = new int[5];

    public static void Bind(int[] storage)
    {
        xp = (storage != null && storage.Length == 5) ? storage : new int[5];
    }

    public static int[] Storage { get { return xp; } }

    // XP needed to reach a level (level 1 = 0).
    public static int XpFor(int level)
    {
        if (level <= 1) return 0;
        return 50 * (level - 1) * level / 2;
    }

    public static int Xp(Skill s) { return xp[(int)s]; }

    public static int Level(Skill s)
    {
        int v = xp[(int)s];
        int lvl = 1;
        while (lvl < MaxLevel && v >= XpFor(lvl + 1)) lvl++;
        return lvl;
    }

    public static float Progress01(Skill s)
    {
        int lvl = Level(s);
        if (lvl >= MaxLevel) return 1f;
        int a = XpFor(lvl), b = XpFor(lvl + 1);
        return Mathf.Clamp01((xp[(int)s] - a) / (float)(b - a));
    }

    // Adds XP; returns true when the level went up.
    public static bool AddXp(Skill s, int amount)
    {
        if (amount <= 0) return false;
        int before = Level(s);
        xp[(int)s] += amount;
        int after = Level(s);
        if (after > before)
        {
            GameEvents.RaiseSkillLevelUp((int)s, after);
            return true;
        }
        return false;
    }

    public static int TotalXpGained;   // XP gained during the current shift (for the results screen)

    public static void Give(Skill s, int amount)
    {
        TotalXpGained += amount;
        AddXp(s, amount);
    }

    public static string Name(Skill s)
    {
        switch (s)
        {
            case Skill.Observation:   return Loc.T("SPOSTRZEGAWCZOŚĆ", "OBSERVATION");
            case Skill.Documents:     return Loc.T("DOKUMENTY", "DOCUMENTS");
            case Skill.Interrogation: return Loc.T("PRZESŁUCHANIE", "INTERROGATION");
            case Skill.Composure:     return Loc.T("OPANOWANIE", "COMPOSURE");
            default:                  return Loc.T("AUTORYTET", "AUTHORITY");
        }
    }

    public static string Description(Skill s)
    {
        switch (s)
        {
            case Skill.Observation:   return Loc.T("Więcej oznaczeń podejrzanych szczegółów i wyczucie, że coś jest nie tak z osobą.", "More suspect marks and a feeling that something is wrong with the person.");
            case Skill.Documents:     return Loc.T("Odczytujesz ukryty mikrotekst, sprawdzasz podpisy i lepiej korzystasz z bazy.", "You read hidden microtext, verify signatures and get more from the database.");
            case Skill.Interrogation: return Loc.T("Więcej pytań i wykrywanie kłamstw w odpowiedziach.", "More questions and the ability to spot lies in answers.");
            case Skill.Composure:     return Loc.T("Mniejszy przyrost stresu i słabsze efekty grozy.", "Less stress gained and weaker horror effects.");
            default:                  return Loc.T("Uspokajanie pasażerów i większy szacunek kolei.", "Calm passengers down and earn the railway's respect.");
        }
    }
}
