using TMPro;
using UnityEngine;

// Pokazuje dane pasażera w czterech panelach na biurku,
// wynik na górze oraz informację zwrotną po każdej decyzji.
public class UIController : MonoBehaviour
{
    [Header("Dokumenty (4 panele na biurku)")]
    public TMP_Text ticketText;    // Bilet
    public TMP_Text idText;        // Dowód
    public TMP_Text railDbText;    // Baza kolejowa
    public TMP_Text registryText;  // Rejestr

    [Header("Wynik i komunikaty")]
    public TMP_Text scoreText;     // licznik (lewy górny róg)
    public TMP_Text messageText;   // komunikat (środek u góry)

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

    // Krótka informacja po decyzji: dobrze czy źle i kim naprawdę był pasażer.
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
