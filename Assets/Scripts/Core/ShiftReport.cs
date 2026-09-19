// Numbers collected during one night, shown on the SHIFT COMPLETE screen.
public class ShiftReport
{
    public int passengers;
    public int correct;
    public int mistakes;
    public int denied;
    public int admitted;
    public int discipline;      // change during the shift
    public int conscience;      // change during the shift
    public int stress;          // stress at the end (0-100)
    public int xp;
    public int score;
    public int bestStreak;
    public int rulesBroken;     // times the passenger was admitted/denied against the rule
    public string discovery;    // codex title or ""

    public bool Perfect { get { return mistakes == 0 && correct >= 13; } }
}
