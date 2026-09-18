using System;

// Яка ПРАВДА про пасажира. Гравець цього не бачить, він має сам до цього дійти.
public enum PassengerTruth
{
    Alive,        // Żywy
    Dead,         // Zmarły
    NonExistent   // Nieistniejący
}

// Один "запис" в одному джерелі (квиток, документ, база, реєстр).
// Кожне джерело може містити свою версію даних. Розбіжності між ними й є головоломкою.
[Serializable]
public class Record
{
    public string fullName;
    public string birthDate;   // "12.06.1974"
    public string status;      // "AKTYWNY", "NIEAKTYWNY", "ZMARŁ" ...
    public string extra;       // додаткова інформація (дата смерті, остання подорож тощо)
}

// Весь пасажир = правда + 4 джерела інформації.
[Serializable]
public class PassengerData
{
    public PassengerTruth truth;

    public string ticketText;   // "Lublin → Stacja 7 | Wagon 3 | Miejsce 42"
    public Record idCard;       // DOWÓD
    public Record railDatabase; // BAZA KOLEJOWA
    public Record registry;     // REJESTR MIESZKAŃCÓW

    // Для дебагу: що саме "не так" у цього пасажира (гравцю не показуємо).
    public string debugHint;
}
