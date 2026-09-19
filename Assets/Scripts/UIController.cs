using TMPro;
using UnityEngine;

// Shows passenger data in the four desk panels plus the score line and the message line.
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
        SetMessage("");
        SetScore("");
    }

    // Dark ink on paper, auto-sizing text, readable score/message lines.
    public void ApplyStyle()
    {
        TMP_Text[] docs = { ticketText, idText, railDbText, registryText };
        foreach (TMP_Text t in docs)
        {
            if (t == null) continue;
            t.color = UIKit.Ink;
            t.enableAutoSizing = true;
            t.fontSizeMin = 18;
            t.fontSizeMax = 34;
            t.textWrappingMode = TextWrappingModes.Normal;
            t.alignment = TextAlignmentOptions.TopLeft;
        }
        if (idText != null) idText.margin = new Vector4(0, 0, 124, 0);   // room for the photo
        if (scoreText != null)
        {
            scoreText.fontSize = 30;
            scoreText.textWrappingMode = TextWrappingModes.NoWrap;
        }
        if (messageText != null)
        {
            messageText.fontSize = 34;
            messageText.textWrappingMode = TextWrappingModes.Normal;
        }
    }

    public void ShowPassenger(PassengerData p)
    {
        ticketText.text   = Desk(p, DocumentType.Ticket);
        idText.text       = Desk(p, DocumentType.Id);
        railDbText.text   = Desk(p, DocumentType.RailDatabase);
        registryText.text = Desk(p, DocumentType.Registry);
    }

    static string Desk(PassengerData p, DocumentType t)
    {
        DocumentData d = DocumentSystem.Get(p, t);
        return d == null ? "" : DocumentSystem.DeskText(d);
    }

    public void ClearDocuments()
    {
        ticketText.text = "";
        idText.text = "";
        railDbText.text = "";
        registryText.text = "";
    }

    public void SetScore(string s)
    {
        if (scoreText != null) scoreText.text = s;
    }

    public void SetMessage(string s)
    {
        if (messageText != null) messageText.text = s;
    }

    public static string TruthName(PassengerTruth t)
    {
        switch (t)
        {
            case PassengerTruth.Alive:       return Loc.T("żywy", "alive");
            case PassengerTruth.Dead:        return Loc.T("zmarły", "dead");
            case PassengerTruth.NonExistent: return Loc.T("nieistniejący", "non-existent");
            case PassengerTruth.Altered:     return Loc.T("zmieniony", "altered");
            case PassengerTruth.Loop:        return Loc.T("pętla", "loop");
            case PassengerTruth.Impostor:    return Loc.T("podszywacz", "impostor");
            case PassengerTruth.Echo:        return Loc.T("echo", "echo");
            default:                         return Loc.T("nieznany", "unknown");
        }
    }
}
