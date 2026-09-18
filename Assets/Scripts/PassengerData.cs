using System;

// The hidden TRUTH about a passenger. The player never sees it and has to work it out.
public enum PassengerTruth
{
    Alive,
    Dead,
    NonExistent
}

// One "record" in one source (ticket, ID, database, registry).
// Each source may hold its own version of the data; the discrepancies are the puzzle.
[Serializable]
public class Record
{
    public string fullName;
    public string birthDate;   // "12.06.1974"
    public string status;      // "AKTYWNY", "NIEAKTYWNY", "ZMARŁ" ...
    public string extra;       // extra info (date of death, last trip, etc.)
}

// A whole passenger = truth + 4 information sources.
[Serializable]
public class PassengerData
{
    public PassengerTruth truth;

    public string ticketText;   // "Lublin → Stacja 7 | Wagon 3 | Miejsce 42"
    public Record idCard;       // ID card
    public Record railDatabase; // railway database
    public Record registry;     // residents registry

    // For debugging: what exactly is "wrong" with this passenger (never shown to the player).
    public string debugHint;
}
