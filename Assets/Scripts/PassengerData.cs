using System;
using System.Collections.Generic;

// The hidden TRUTH about a passenger. The player is never told which one it is.
public enum PassengerTruth
{
    Alive = 0,
    Dead = 1,
    NonExistent = 2,
    Altered = 3,     // exists, but part of the data was changed
    Loop = 4,        // documents dated in the future or the past
    Impostor = 5,    // uses somebody else's documents
    Echo = 6,        // behaves like a memory of a person
    Unknown = 7      // the system cannot tell what this is
}

// A whole passenger = hidden truth + a generated identity + documents + dialogue.
[Serializable]
public class PassengerData
{
    public PassengerTruth truth;

    // --- looks ---
    public bool female;
    public int look;             // which drawn character (see CharacterArt)
    public int idPhotoLook = -1; // look shown on the ID photo (-1 = same as the person)
    public float scale = 1f;     // children are smaller

    // --- identity ---
    public string firstName, lastName, fullName;
    public int age;
    public string occupation;
    public string origin;        // city the passenger claims to come from
    public string destination;   // where they say they are going
    public bool destinationRegistered = true;
    public int station;          // destination station number
    public string backstory;
    public string traits;
    public string secret;
    public string behaviour;     // calm, nervous, angry, scared, cold
    public int stressLevel;      // how nervous the passenger is (0-3)

    // --- numbers and dates ---
    public string birthDate;     // "12.06.1974"
    public int birthYear;
    public string ticketNo, docNo;
    public string ticketText;    // route + car + seat, printed on the ticket
    public string ticketName;    // name on the ticket (differs from ID when forged)
    public bool forgedTicket;
    public bool noTicket;        // has no ticket at all (the man without papers)
    public string deathDate, deathCause;
    public int deathYear;
    public string illness;
    public string motherName;
    public int motherDeathYear;

    // --- documents / dialogue ---
    public List<DocumentData> documents = new List<DocumentData>();
    public List<QA> dialogue = new List<QA>();
    public List<string> anomalies = new List<string>();   // anomaly ids that really exist on this passenger
    public int orderState;       // 0 none, 1 valid Directorate order, 2 forged order

    // --- moral dilemma ---
    public string greeting;      // what the passenger says first
    public bool sympathetic;
    public string plea;

    // --- story ---
    public bool isPlayer;
    public string storyId;       // recurring / scripted character id, null for a random passenger
    public string onAdmitFlag, onDenyFlag;
    public string codexOnMeet;
    public bool playerPhoto;     // the ID shows the conductor's face (event)

    // --- horror ---
    public bool aggressive;      // a dead passenger that may frighten the conductor
    public bool scaresOnArrival; // triggers a scare when the passenger walks up

    // For debugging: what is "wrong" with this passenger (never shown to the player).
    public string debugHint;

    public string DisplayName { get { return string.IsNullOrEmpty(fullName) ? (firstName + " " + lastName) : fullName; } }
}

// What can be seen from the outside (used by the stage to draw the person).
public static class PassengerTraits
{
    public static bool HasBreath(PassengerData p)
    {
        if (p.isPlayer) return false;
        switch (p.truth)
        {
            case PassengerTruth.Alive:
            case PassengerTruth.Altered:
            case PassengerTruth.Impostor:
            case PassengerTruth.Loop:
                return true;
            default:
                return false;
        }
    }

    public static bool HasShadow(PassengerData p)
    {
        if (p.isPlayer) return false;
        return p.truth != PassengerTruth.NonExistent && p.truth != PassengerTruth.Unknown && p.truth != PassengerTruth.Echo;
    }

    // 1 = solid, lower = see-through.
    public static float Alpha(PassengerData p)
    {
        if (p.isPlayer) return 0.8f;
        if (p.truth == PassengerTruth.Echo) return 0.72f;
        return 1f;
    }

    // Does the person glitch now and then?
    public static bool Glitches(PassengerData p)
    {
        return p.truth == PassengerTruth.NonExistent || p.truth == PassengerTruth.Loop || p.truth == PassengerTruth.Unknown;
    }

    public static bool IsColdTint(PassengerData p)
    {
        return p.truth == PassengerTruth.Dead || p.truth == PassengerTruth.Echo;
    }
}
