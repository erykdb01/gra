using System;

// Small event hub so systems (achievements, codex, HUD) do not need references to each other.
public static class GameEvents
{
    public static event Action<PassengerData, bool, bool> Decision;   // passenger, admitted, correct
    public static event Action<string> AnomalyFound;
    public static event Action<string> FlagSet;
    public static event Action<int, int> SkillLevelUp;                // skill index, new level
    public static event Action<string> CodexUnlocked;
    public static event Action<string> AchievementUnlocked;

    public static void RaiseDecision(PassengerData p, bool admitted, bool correct) { if (Decision != null) Decision(p, admitted, correct); }
    public static void RaiseAnomaly(string id) { if (AnomalyFound != null) AnomalyFound(id); }
    public static void RaiseFlag(string id) { if (FlagSet != null) FlagSet(id); }
    public static void RaiseSkillLevelUp(int skill, int level) { if (SkillLevelUp != null) SkillLevelUp(skill, level); }
    public static void RaiseCodex(string id) { if (CodexUnlocked != null) CodexUnlocked(id); }
    public static void RaiseAchievement(string id) { if (AchievementUnlocked != null) AchievementUnlocked(id); }
}
