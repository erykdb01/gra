using System.Collections.Generic;
using UnityEngine;

public class AchievementDef
{
    public string id;
    public string titlePl, titleEn, descPl, descEn;
    public bool secret;
    public string title { get { return Loc.T(titlePl, titleEn); } }
    public string desc { get { return Loc.T(descPl, descEn); } }
}

// Our own achievement system: definitions, unlocking, saving and the popup.
public static class AchievementSystem
{
    static readonly List<AchievementDef> all = new List<AchievementDef>();
    static bool built;

    public static List<AchievementDef> All { get { Build(); return all; } }

    static void A(string id, string tp, string te, string dp, string de, bool secret = false)
    {
        all.Add(new AchievementDef { id = id, titlePl = tp, titleEn = te, descPl = dp, descEn = de, secret = secret });
    }

    static void Build()
    {
        if (built) return;
        built = true;
        A("first_shift", "PIERWSZA ZMIANA", "FIRST SHIFT", "Ukończ pierwszą noc.", "Complete your first night.");
        A("thirteen", "TRZYNASTU", "THIRTEEN", "Obsłuż 13 pasażerów bezbłędnie.", "Process 13 passengers perfectly.");
        A("no_mercy", "BEZ LITOŚCI", "NO MERCY", "Ukończ zmianę, nie łamiąc żadnej zasady.", "Finish a shift without breaking the rules.");
        A("something_wrong", "COŚ JEST NIE TAK", "SOMETHING IS WRONG", "Odkryj pierwszą anomalię.", "Discover your first anomaly.");
        A("dead_man_walking", "MARTWY W DRODZE", "DEAD MAN WALKING", "Wpuść zmarłego pasażera.", "Admit a dead passenger.");
        A("wrong_name", "ZŁE NAZWISKO", "THE WRONG NAME", "Odkryj nieistniejącego pasażera.", "Discover a non-existent passenger.");
        A("line13", "LINIA 13", "LINE 13", "Dotrzyj do ukrytego wydarzenia w pociągu.", "Reach the hidden train event.");
        A("last_passenger", "OSTATNI PASAŻER", "THE LAST PASSENGER", "Spotkaj samego siebie.", "Meet yourself.");
        A("merciful", "MIŁOSIERNY", "MERCIFUL", "Pomóż 5 błagającym pasażerom.", "Help 5 begging passengers.");
        A("by_the_book", "WEDŁUG PRZEPISÓW", "BY THE BOOK", "Podejmij 8 poprawnych decyzji z rzędu.", "Make 8 correct decisions in a row.");
        A("streak_13", "SERIA", "STREAK", "Podejmij 13 poprawnych decyzji z rzędu.", "Make 13 correct decisions in a row.");
        A("sharp_eye", "CZUJNE OKO", "SHARP EYE", "Odkryj 10 anomalii w dokumentach.", "Find 10 anomalies in documents.");
        A("skilled", "DOŚWIADCZONY", "SKILLED", "Osiągnij 5. poziom w dowolnej umiejętności.", "Reach level 5 in any skill.");
        A("well_rounded", "WSZECHSTRONNY", "WELL ROUNDED", "Osiągnij 3. poziom we wszystkich umiejętnościach.", "Reach level 3 in every skill.");
        A("archivist", "ARCHIWISTA", "ARCHIVIST", "Odblokuj 30 wpisów Kodeksu.", "Unlock 30 Codex entries.");
        A("iron_nerves", "STALOWE NERWY", "IRON NERVES", "Ukończ zmianę ze stresem poniżej 20%.", "Finish a shift with stress under 20%.");
        A("edge", "NA KRAWĘDZI", "ON THE EDGE", "Ukończ zmianę ze stresem powyżej 80%.", "Finish a shift with stress above 80%.");
        A("night5", "PIĄTA NOC", "FIFTH NIGHT", "Dotrwaj do szóstej nocy w trybie fabularnym.", "Reach the sixth night in Story Mode.");
        A("night10", "DESZCZ NA PERONIE", "RAIN ON THE PLATFORM", "Dotrwaj do jedenastej nocy w trybie fabularnym.", "Reach the eleventh night in Story Mode.");
        A("endless5", "DŁUGA ZMIANA", "LONG SHIFT", "Osiągnij 5. poziom w trybie nieskończonym.", "Reach level 5 in Night Shift mode.");
        A("endless10", "BEZ KOŃCA", "WITHOUT END", "Osiągnij 10. poziom w trybie nieskończonym.", "Reach level 10 in Night Shift mode.");
        A("ending_a", "ROZKAZY", "ORDERS", "Zobacz zakończenie A.", "See ending A.");
        A("ending_b", "ZŁAMANY SYSTEM", "BROKEN SYSTEM", "Zobacz zakończenie B.", "See ending B.");
        A("ending_c", "PRAWDA", "THE TRUTH", "Zobacz zakończenie C.", "See ending C.");
        A("ending_d", "CZĘŚĆ LINII", "PART OF THE LINE", "Zobacz zakończenie D.", "See ending D.");
        A("ending_s", "PERON CZTERNASTY", "PLATFORM FOURTEEN", "Zobacz sekretne zakończenie.", "See the secret ending.", true);
        A("listened", "PUKANIE", "THE KNOCKING", "Posłuchaj pukania z wagonu 13.", "Listen to the knocking from car 13.", true);
        A("fourteen", "CZTERNASTY", "FOURTEEN", "Zobacz, jak liczba pasażerów się zmienia.", "See the passenger count change.", true);
        A("mirror", "ODBICIE", "MIRROR", "Znajdź swoje zdjęcie na cudzym dokumencie.", "Find your own photo on someone else's document.", true);
        A("nameless", "BEZ NAZWISKA", "NAMELESS", "Wpuść człowieka bez dokumentów.", "Admit the man without papers.", true);
        A("insomnia", "BEZSENNOŚĆ", "INSOMNIA", "Zostaw menu głównego włączone na dwie minuty.", "Leave the main menu running for two minutes.", true);
    }

    public static bool IsUnlocked(string id) { return SaveSystem.Data.achievements.Contains(id); }

    public static int UnlockedCount { get { return SaveSystem.Data.achievements.Count; } }

    public static void Unlock(string id)
    {
        Build();
        if (IsUnlocked(id)) return;
        AchievementDef def = null;
        for (int i = 0; i < all.Count; i++) if (all[i].id == id) { def = all[i]; break; }
        if (def == null) return;
        SaveSystem.Data.achievements.Add(id);
        SaveSystem.Save();
        GameEvents.RaiseAchievement(id);
        ToastUI.Show(Loc.T("OSIĄGNIĘCIE", "ACHIEVEMENT"), def.title, new Color(0.95f, 0.8f, 0.4f));
        if (SoundFX.I != null) SoundFX.I.Good();
    }

    // Called after every decision.
    public static int Streak;
    public static int HelpedBeggars;
    public static int DocAnomalies;

    public static void ResetRunCounters() { Streak = 0; }

    public static void OnDecision(PassengerData p, bool admitted, bool correct)
    {
        if (correct) Streak++; else Streak = 0;
        if (Streak >= 8) Unlock("by_the_book");
        if (Streak >= 13) Unlock("streak_13");
        if (admitted && p.truth == PassengerTruth.Dead) Unlock("dead_man_walking");
        if (admitted && p.sympathetic) { HelpedBeggars++; if (HelpedBeggars >= 5) Unlock("merciful"); }
        if (admitted && p.storyId != null && p.storyId.StartsWith("nodocs")) Unlock("nameless");
    }

    public static void OnAnomaly(string id)
    {
        Unlock("something_wrong");
        DocAnomalies++;
        if (DocAnomalies >= 10) Unlock("sharp_eye");
    }

    public static void OnPassengerMet(PassengerData p)
    {
        if (p.truth == PassengerTruth.NonExistent) Unlock("wrong_name");
    }

    public static void CheckSkills()
    {
        int min = 99;
        for (int i = 0; i < 5; i++)
        {
            int lv = SkillSystem.Level((Skill)i);
            if (lv >= 5) Unlock("skilled");
            if (lv < min) min = lv;
        }
        if (min >= 3) Unlock("well_rounded");
    }

    public static void OnShiftComplete(ShiftReport r, int nightNumber, bool story)
    {
        Unlock("first_shift");
        if (r.Perfect) Unlock("thirteen");
        if (r.rulesBroken == 0 && r.mistakes == 0) Unlock("no_mercy");
        if (r.stress < 20) Unlock("iron_nerves");
        if (r.stress > 80) Unlock("edge");
        if (story && nightNumber >= 5) Unlock("night5");
        if (story && nightNumber >= 10) Unlock("night10");
        if (!story && nightNumber >= 5) Unlock("endless5");
        if (!story && nightNumber >= 10) Unlock("endless10");
        if (Codex.UnlockedCount >= 30) Unlock("archivist");
        CheckSkills();
    }
}
