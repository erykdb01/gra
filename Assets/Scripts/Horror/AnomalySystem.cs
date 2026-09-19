using System.Collections.Generic;
using UnityEngine;

// Tracks the anomalies the player discovers (in documents, answers, the database and on the person).
public static class AnomalySystem
{
    static readonly HashSet<string> foundNow = new HashSet<string>();
    public static int FoundThisShift;

    public static void BeginPassenger() { foundNow.Clear(); }

    public static bool IsFound(string id) { return foundNow.Contains(id); }

    public static string Title(string id)
    {
        switch (id)
        {
            case "a_dead": return Loc.T("Wpis o śmierci", "Entry about death");
            case "a_norecord": return Loc.T("Brak wpisu w księgach", "No entry in the books");
            case "a_birth": return Loc.T("Data urodzenia się nie zgadza", "Birth date does not match");
            case "a_date": return Loc.T("Niemożliwa data", "Impossible date");
            case "a_photo": return Loc.T("Zdjęcie się nie zgadza", "Photo does not match");
            case "a_signature": return Loc.T("Fałszywy podpis", "Forged signature");
            case "a_seal": return Loc.T("Pieczęć z innej stacji", "Seal from another station");
            case "a_amended": return Loc.T("Ręcznie poprawione dane", "Data amended by hand");
            case "a_echo": return Loc.T("Zgłoszenie od świadków", "Reported by witnesses");
            case "a_unknown": return Loc.T("System nie zna tej osoby", "The system does not know this person");
            case "a_name": return Loc.T("Bilet na inne nazwisko", "Ticket in a different name");
            case "a_manifest": return Loc.T("Nie ma na liście", "Not on the list");
            default: return Loc.T("Ukryty zapis", "Hidden inscription");
        }
    }

    // A clue that is really frightening raises stress.
    static int StressFor(string id)
    {
        switch (id)
        {
            case "a_dead": case "a_norecord": case "a_unknown": return 4;
            case "a_echo": case "a_date": return 3;
            case "a_micro": return 2;
            default: return 0;
        }
    }

    // Returns true if this anomaly was newly discovered for this passenger.
    public static bool Discover(string id, Skill via)
    {
        if (string.IsNullOrEmpty(id)) return false;
        if (!foundNow.Add(id)) return false;

        FoundThisShift++;
        SkillSystem.Give(via, 12);
        SkillSystem.Give(Skill.Observation, 3);
        string cx = DocumentSystem.CodexIdForAnomaly(id);
        if (cx != null) Codex.Unlock(cx);
        AchievementSystem.OnAnomaly(id);
        GameEvents.RaiseAnomaly(id);
        int st = StressFor(id);
        if (st > 0) PlayerStats.AddStress(st);
        ToastUI.Show(Loc.T("ANOMALIA", "ANOMALY"), Title(id), new Color(0.9f, 0.45f, 0.4f));
        if (SoundFX.I != null) SoundFX.I.Stinger(0.5f);
        return true;
    }
}
