using System.Collections.Generic;

// A passenger that is placed in a fixed position of the night (a story character).
public class ScriptedSlot
{
    public int slot;          // 0-based position in the queue
    public string script;     // id of the script in StoryCast
    public ScriptedSlot(int slot, string script) { this.slot = slot; this.script = script; }
}

// A special event that runs before a given passenger.
public class ScriptedEvent
{
    public int before;        // runs before the passenger with this index
    public string eventId;
    public ScriptedEvent(int before, string eventId) { this.before = before; this.eventId = eventId; }
}

// Configuration of one night: rules, difficulty, passenger mix, available documents, events.
public class DayConfig
{
    public int act;                            // 1-7 (story), 0 = endless
    public string actPl, actEn;
    public string titlePl, titleEn;
    public string rulePl, ruleEn;              // short rule shown on the HUD
    public string briefingPl, briefingEn;      // shown before the shift
    public string outroPl, outroEn;            // shown after the shift

    public string title { get { return Loc.T(titlePl, titleEn); } }
    public string rule { get { return Loc.T(rulePl, ruleEn); } }
    public string briefing { get { return Loc.T(briefingPl, briefingEn); } }
    public string outro { get { return Loc.T(outroPl, outroEn); } }
    public string actTitle { get { return Loc.T(actPl, actEn); } }

    public int passengers = 13;
    public int maxMistakes = 4;                // -1 = unlimited
    public bool[] allow = new bool[8];         // indexed by PassengerTruth
    public float[] w = new float[8];           // weights of the truths
    public float forgeChance;                  // chance of a forged ticket on a normal passenger
    public float pleaChance;                   // chance that a "should be denied" passenger begs
    public float anomalyChance;                // chance of an extra hidden anomaly on a document
    public float orderChance;                  // chance of a Directorate order
    public float timeLimit;                    // seconds per passenger (0 = no limit)
    public int docMask = 15;                   // which documents are available (bit per DocumentType)
    public bool judged = true;                 // false: nobody checks your decisions
    public bool finalDay;                      // the last passenger is the conductor
    public int level;                          // endless level (0 in story)
    public string date = "13.11.1998";         // date printed on the tickets

    public List<ScriptedSlot> scripted = new List<ScriptedSlot>();
    public List<ScriptedEvent> scriptedEvents = new List<ScriptedEvent>();
    public List<string> events = new List<string>();   // random events that may happen tonight
    public int eventCount = 1;                 // how many random events tonight
    public string[] codexAtStart;              // codex entries unlocked when the night begins

    public bool Has(DocumentType t) { return (docMask & (1 << (int)t)) != 0; }
}

public static class Days
{
    // The rule of the night: should this passenger be admitted?
    public static bool ShouldAdmit(PassengerData p, DayConfig d)
    {
        if (p.orderState == 1) return true;          // a valid Directorate order overrides everything
        if (p.noTicket || p.forgedTicket) return false;
        int i = (int)p.truth;
        return i >= 0 && i < d.allow.Length && d.allow[i];
    }
}
