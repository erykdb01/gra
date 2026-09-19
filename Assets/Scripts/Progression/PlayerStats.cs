using UnityEngine;

// The conductor's meters for the current run. Plain static data; saved through SaveSystem.
public static class PlayerStats
{
    public static int Conscience = 5;    // 0..10
    public static int Discipline = 5;    // 0..10
    public static int Stress;            // 0..100
    public static int Reputation = 50;   // 0..100
    public static int TotalCorrect, TotalMistakes;

    public const int MaxStress = 100;

    public static int Authority
    {
        get { return Mathf.Clamp(SkillSystem.Level(Skill.Authority) * 8 + Reputation / 5, 0, 100); }
    }

    public static float Stress01 { get { return Stress / (float)MaxStress; } }

    public static void Reset()
    {
        Conscience = 5; Discipline = 5; Stress = 0; Reputation = 50;
        TotalCorrect = 0; TotalMistakes = 0;
    }

    public static void LoadFrom(StoryProgress s)
    {
        Conscience = s.conscience; Discipline = s.discipline; Stress = s.stress; Reputation = s.reputation;
        TotalCorrect = s.totalCorrect; TotalMistakes = s.totalMistakes;
    }

    public static void StoreTo(StoryProgress s)
    {
        s.conscience = Conscience; s.discipline = Discipline; s.stress = Stress; s.reputation = Reputation;
        s.totalCorrect = TotalCorrect; s.totalMistakes = TotalMistakes;
    }

    // Composure lowers the stress you take (up to -45% at level 10).
    public static int AddStress(int raw)
    {
        if (raw <= 0) { Stress = Mathf.Clamp(Stress + raw, 0, MaxStress); return raw; }
        float k = 1f - 0.05f * (SkillSystem.Level(Skill.Composure) - 1);
        int add = Mathf.Max(1, Mathf.RoundToInt(raw * k));
        Stress = Mathf.Clamp(Stress + add, 0, MaxStress);
        return add;
    }

    public static void Relax(int amount) { Stress = Mathf.Clamp(Stress - Mathf.Abs(amount), 0, MaxStress); }

    public static void AddConscience(int d) { Conscience = Mathf.Clamp(Conscience + d, 0, 10); }
    public static void AddDiscipline(int d) { Discipline = Mathf.Clamp(Discipline + d, 0, 10); }
    public static void AddReputation(int d) { Reputation = Mathf.Clamp(Reputation + d, 0, 100); }
}
