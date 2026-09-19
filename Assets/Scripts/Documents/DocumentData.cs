using System;
using System.Collections.Generic;

public enum DocumentType
{
    Ticket = 0,
    Id = 1,
    RailDatabase = 2,
    Registry = 3,
    Manifest = 4,
    Medical = 5,
    DeathRecord = 6,
    TravelHistory = 7,
    SpecialOrders = 8
}

// One line on a document: "label: value". Some lines hide something for the player to find.
[Serializable]
public class DocLine
{
    public string label;
    public string value;
    public string anomaly;       // id of the anomaly this line reveals (null = nothing wrong)
    public bool hidden;          // microtext, only readable with a scan and enough DOCUMENTS skill
    public int minSkill;         // DOCUMENTS level needed to read a hidden line
    public bool marked;          // the player marked it as suspicious
    public bool isPhoto;         // the photo slot
    public bool isSignature;     // a signature line (can be checked)
    public bool isDate;
    public bool isNumber;

    public DocLine() { }
    public DocLine(string label, string value) { this.label = label; this.value = value; }
}

[Serializable]
public class DocumentData
{
    public DocumentType type;
    public string number;                     // document number printed in the corner
    public string stamp;                      // text of the seal
    public List<DocLine> front = new List<DocLine>();
    public List<DocLine> back = new List<DocLine>();
    public int photoLook = -1;                // >= 0: this document shows a portrait of that look
    public bool photoIsPlayer;                // the portrait is the conductor's face
    public bool available = true;            // unlocked for the current night
}

// A question the passenger can be asked, together with the answer already prepared for the passenger.
[Serializable]
public class QA
{
    public const int Truth = 0, Half = 1, Lie = 2, Evasive = 3;

    public string id;
    public string question;
    public string answer;
    public int kind;            // Truth / Half / Lie / Evasive
    public int priority;        // higher = offered earlier
    public bool contextual;     // depends on a document or an earlier discovery
    public int stress;          // stress the conductor takes from hearing it
    public string flag;         // story flag set when asked
    public string codex;        // codex entry unlocked when asked
    public string anomaly;      // anomaly discovered by hearing it
    public int minAuthority;    // AUTHORITY level needed to use this question (0 = always)
    public bool used;
}
