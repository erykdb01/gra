using System.Collections.Generic;
using UnityEngine;

// Runtime side of a conversation: which questions are offered, what an answer does.
public class DialogueManager
{
    PassengerData passenger;
    public int Asked { get; private set; }

    public void Begin(PassengerData p)
    {
        passenger = p;
        Asked = 0;
        for (int i = 0; i < p.dialogue.Count; i++) p.dialogue[i].used = false;
    }

    public static int SlotCount()
    {
        int lvl = SkillSystem.Level(Skill.Interrogation);
        return Mathf.Clamp(4 + lvl / 3, 4, 6);
    }

    // The questions currently on offer (highest priority first, unused, allowed by AUTHORITY).
    public List<QA> Offered()
    {
        var res = new List<QA>();
        if (passenger == null) return res;
        int slots = SlotCount();
        int auth = SkillSystem.Level(Skill.Authority);
        for (int i = 0; i < passenger.dialogue.Count && res.Count < slots; i++)
        {
            QA q = passenger.dialogue[i];
            if (q.used) continue;
            if (q.minAuthority > auth) continue;
            res.Add(q);
        }
        return res;
    }

    // Applies the effects of asking and returns the text to show (with the tell of a lie if the conductor is skilled).
    public string Ask(QA q)
    {
        q.used = true;
        Asked++;
        int lvl = SkillSystem.Level(Skill.Interrogation);

        SkillSystem.Give(Skill.Interrogation, q.contextual ? 7 : 3);
        if (q.kind == QA.Lie) SkillSystem.Give(Skill.Interrogation, 4);
        if (q.minAuthority > 0) SkillSystem.Give(Skill.Authority, 6);

        if (q.stress > 0) PlayerStats.AddStress(q.stress);
        if (!string.IsNullOrEmpty(q.flag)) StoryFlags.Set(q.flag);
        if (!string.IsNullOrEmpty(q.codex)) Codex.Unlock(q.codex);
        if (!string.IsNullOrEmpty(q.anomaly)) AnomalySystem.Discover(q.anomaly, Skill.Interrogation);

        string text = q.answer;
        string tell = null;
        if (q.kind == QA.Lie)
        {
            if (lvl >= 5) tell = Loc.T("<color=#E07070>(kłamie)</color>", "<color=#E07070>(lying)</color>");
            else if (lvl >= 3) tell = Loc.T("<color=#9AA8C0>(unika wzroku)</color>", "<color=#9AA8C0>(avoids your eyes)</color>");
        }
        else if (q.kind == QA.Half)
        {
            if (lvl >= 7) tell = Loc.T("<color=#E0B070>(półprawda)</color>", "<color=#E0B070>(half-truth)</color>");
            else if (lvl >= 3) tell = Loc.T("<color=#9AA8C0>(waha się)</color>", "<color=#9AA8C0>(hesitates)</color>");
        }
        else if (q.kind == QA.Evasive && lvl >= 4)
        {
            tell = Loc.T("<color=#9AA8C0>(wymija odpowiedź)</color>", "<color=#9AA8C0>(dodges the answer)</color>");
        }
        if (tell != null) text += " " + tell;
        return text;
    }
}
