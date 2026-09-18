using TMPro;
using UnityEngine;

// Shows passenger data in the four desk panels,
// the score at the top and feedback after each decision.
public class UIController : MonoBehaviour
{
    [Header("Documents (4 panels on the desk)")]
    public TMP_Text ticketText;    // ticket
    public TMP_Text idText;        // ID card
    public TMP_Text railDbText;    // railway database
    public TMP_Text registryText;  // registry

    [Header("Score and messages")]
    public TMP_Text scoreText;     // counter (top left)
    public TMP_Text messageText;   // message (top center)

    void Awake()
    {
        if (messageText != null) messageText.text = "";
        if (scoreText != null) scoreText.text = "";
    }

    public void ShowPassenger(PassengerData p)
    {
        ticketText.text   = "BILET\n" + p.ticketText + "\n" + p.idCard.fullName;
        idText.text       = Format("DOWÓD", p.idCard);
        railDbText.text   = Format("BAZA KOLEJOWA", p.railDatabase);
        registryText.text = Format("REJESTR", p.registry);
    }

    public void ShowScore(int correct, int mistakes, int served, int total)
    {
        if (scoreText != null)
            scoreText.text = $"Pasażer {served + 1}/{total}    Dobrze: {correct}    Błędy: {mistakes}";
    }

    // Short feedback after a decision: right or wrong, and who the passenger really was.
    public void ShowFeedback(bool ok, PassengerTruth truth)
    {
        if (messageText == null) return;

        if (ok)
            messageText.text = "<color=#7FD08A>Dobra decyzja.</color>";
        else
            messageText.text = "<color=#E07070>Błąd. To był pasażer: " + TruthName(truth) + ".</color>";
    }

    public void ShowShiftEnd(int correct, int mistakes)
    {
        ticketText.text = "";
        idText.text = "";
        railDbText.text = "";
        registryText.text = "";

        if (scoreText != null) scoreText.text = "";
        if (messageText != null)
            messageText.text = $"Koniec zmiany.  Dobrze: {correct}   Błędy: {mistakes}";
    }

    static string TruthName(PassengerTruth t)
    {
        switch (t)
        {
            case PassengerTruth.Alive:       return "żywy";
            case PassengerTruth.Dead:        return "zmarły";
            case PassengerTruth.NonExistent: return "nieistniejący";
            default:                         return "";
        }
    }

    static string Format(string title, Record r)
    {
        string s = $"{title}\n{r.fullName}\nur. {r.birthDate}\nstatus: {r.status}";
        if (!string.IsNullOrEmpty(r.extra)) s += "\n" + r.extra;
        return s;
    }
}
