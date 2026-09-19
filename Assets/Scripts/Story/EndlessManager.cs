using UnityEngine;

// Night Shift mode: every night is generated from the level. Higher level = more forgeries, more anomalies,
// more documents, less time and more events.
public static class EndlessManager
{
    public static int RunScore;
    public static int RunPassengers, RunCorrect, RunMistakes, RunBestStreak;

    public static void BeginRun()
    {
        RunScore = 0; RunPassengers = 0; RunCorrect = 0; RunMistakes = 0; RunBestStreak = 0;
        SaveSystem.Data.endless.runs++;
        PlayerStats.Reset();
        SkillSystem.Bind(SaveSystem.Data.endless.skillXp);
        StoryFlags.Clear();
        AchievementSystem.ResetRunCounters();
    }

    static readonly string[][] rules =
    {
        // 0: only the living
        new[] { "Wpuszczaj tylko żywych.", "Admit only the living." },
        // 1: the dead with a ticket
        new[] { "Żywi i zmarli z ważnym biletem mogą jechać.", "The living and the dead with a valid ticket may travel." },
        // 2: living, photo must match
        new[] { "Tylko żywi ze zgodnym zdjęciem.", "Only the living with a matching photo." },
        // 3: living or altered, nobody missing from the registry
        new[] { "Nikt spoza Rejestru. Poprawione dane są dozwolone.", "Nobody missing from the Registry. Corrected data is allowed." },
        // 4: living, and nothing that looks like a loop
        new[] { "Tylko żywi. Dokumenty z dziwnymi datami odrzucaj.", "Only the living. Reject documents with strange dates." },
    };

    public static DayConfig Build(int level)
    {
        level = Mathf.Max(1, level);
        var d = new DayConfig();
        d.act = 0;
        d.level = level;
        d.titlePl = "Zmiana " + level; d.titleEn = "Shift " + level;
        d.actPl = "NOCNA ZMIANA"; d.actEn = "NIGHT SHIFT";

        // choose a rule available at this level
        int maxRule = level >= 4 ? 4 : (level >= 3 ? 3 : (level >= 2 ? 2 : 0));
        int r = level == 1 ? 0 : Random.Range(0, maxRule + 1);
        if (level == 2) r = Random.Range(0, 2) == 0 ? 0 : 1;
        d.rulePl = rules[r][0]; d.ruleEn = rules[r][1];
        d.allow[(int)PassengerTruth.Alive] = true;
        if (r == 1) d.allow[(int)PassengerTruth.Dead] = true;
        if (r == 3) d.allow[(int)PassengerTruth.Altered] = true;

        // weights grow with the level
        float alive = Mathf.Max(28f, 72f - level * 5f);
        float dead = Mathf.Min(24f, 12f + level);
        d.w[0] = alive;
        d.w[1] = dead;
        d.w[2] = level >= 2 ? Mathf.Min(20f, 6f + level * 2f) : 0f;
        d.w[3] = level >= 3 ? Mathf.Min(12f, level * 1.5f) : 0f;
        d.w[5] = level >= 3 ? Mathf.Min(16f, 4f + level) : 0f;
        d.w[4] = level >= 4 ? Mathf.Min(10f, level) : 0f;
        d.w[6] = level >= 4 ? Mathf.Min(10f, level) : 0f;
        d.w[7] = level >= 5 ? Mathf.Min(10f, level - 2f) : 0f;

        d.passengers = 13;
        d.maxMistakes = Mathf.Max(2, 5 - level / 3);
        d.forgeChance = Mathf.Min(0.5f, 0.06f + 0.04f * level);
        d.pleaChance = Mathf.Min(0.5f, 0.12f + 0.03f * level);
        d.anomalyChance = Mathf.Min(0.7f, 0.05f + 0.06f * level);
        d.orderChance = level >= 3 ? Mathf.Min(0.15f, 0.04f + 0.01f * level) : 0f;
        d.timeLimit = level < 3 ? 0f : Mathf.Max(18f, 74f - level * 4f);
        d.docMask = level >= 6 ? 511 : (level >= 5 ? 255 : (level >= 4 ? 127 : (level >= 3 ? 63 : (level >= 2 ? 31 : 15))));
        d.judged = true;
        d.date = "13.11.1998";

        d.events = EventLibrary.EndlessPool(level);
        d.eventCount = Mathf.Min(4, level / 2);

        d.briefingPl = "Poziom " + level + ". Zasady zmieniają się co noc, a kolej nie wyjaśnia dlaczego. Trzynaście osób na peronie. Zegar tyka." +
                       (d.timeLimit > 0 ? " Na każdego pasażera masz " + Mathf.RoundToInt(d.timeLimit) + " sekund." : "");
        d.briefingEn = "Level " + level + ". The rules change every night and the railway does not explain why. Thirteen people on the platform. The clock is ticking." +
                       (d.timeLimit > 0 ? " You have " + Mathf.RoundToInt(d.timeLimit) + " seconds for each passenger." : "");
        d.outroPl = "Kolejna noc za tobą. Peron już czeka.";
        d.outroEn = "Another night behind you. The platform is already waiting.";
        d.codexAtStart = new[] { "rule_endless", "doc_ticket", "doc_id", "doc_raildb", "doc_registry" };
        return d;
    }

    public static int ScoreFor(bool correct, int level, int streak)
    {
        if (!correct) return -120;
        return 100 + 15 * level + 10 * Mathf.Min(streak, 10);
    }

    // Called at the end of a night (and when the run ends).
    public static void SaveStats(int level, bool runEnded)
    {
        EndlessStats s = SaveSystem.Data.endless;
        s.totalPassengers += 0;   // added per passenger by the game manager
        if (RunScore > s.highScore) s.highScore = RunScore;
        if (level > s.bestLevel) s.bestLevel = level;
        if (RunBestStreak > s.bestStreak) s.bestStreak = RunBestStreak;
        SaveSystem.Save();
    }
}
