using UnityEngine;

// Static generator: creates a random passenger with the requested hidden truth.
// Surnames match gender (Kamiński / Kamińska).
public static class PassengerGenerator
{
    static readonly string[] maleNames   = { "Jan", "Piotr", "Tadeusz", "Stanisław", "Marek", "Andrzej", "Henryk", "Zbigniew" };
    static readonly string[] femaleNames = { "Maria", "Anna", "Krystyna", "Ewa", "Halina", "Barbara", "Zofia", "Irena" };

    // {male, female}
    static readonly string[][] surnames =
    {
        new[] { "Kowalski",    "Kowalska"    },
        new[] { "Nowak",       "Nowak"       },
        new[] { "Wiśniewski",  "Wiśniewska"  },
        new[] { "Wójcik",      "Wójcik"      },
        new[] { "Kamiński",    "Kamińska"    },
        new[] { "Lewandowski", "Lewandowska" },
        new[] { "Zieliński",   "Zielińska"   },
        new[] { "Szymański",   "Szymańska"   },
    };

    static readonly string[] cities = { "Lublin", "Warszawa", "Kraków", "Gdańsk", "Poznań", "Łódź" };

    public static PassengerData Generate(PassengerTruth truth)
    {
        bool female = Random.value < 0.5f;
        string first = female ? Pick(femaleNames) : Pick(maleNames);
        string[] sn  = surnames[Random.Range(0, surnames.Length)];
        string last  = female ? sn[1] : sn[0];
        string name  = first + " " + last;

        int year  = Random.Range(1930, 1995);
        int month = Random.Range(1, 13);
        int day   = Random.Range(1, 29);
        string birth = $"{day:00}.{month:00}.{year}";

        var p = new PassengerData { truth = truth };

        p.ticketText = $"{Pick(cities)} → Stacja {Random.Range(1, 13)}\n" +
                       $"Wagon: {Random.Range(1, 8)}  Miejsce: {Random.Range(1, 60)}";

        // By default everything matches (a clean, living passenger).
        p.idCard       = new Record { fullName = name, birthDate = birth, status = "WAŻNY",   extra = "zam. " + Pick(cities) };
        p.railDatabase = new Record { fullName = name, birthDate = birth, status = "AKTYWNY", extra = "ostatnia podróż: " + Random.Range(2015, 2026) };
        p.registry     = new Record { fullName = name, birthDate = birth, status = "ŻYJE",    extra = "" };
        p.debugHint    = "no discrepancies";

        switch (truth)
        {
            case PassengerTruth.Dead:
                // The registry says the person is dead, even though they stand in front of you.
                int deathYear = Random.Range(year + 20, 2010);
                p.registry.status = "ZMARŁ";
                p.registry.extra  = $"data: {Random.Range(1, 29):00}.{Random.Range(1, 13):00}.{deathYear}";
                p.railDatabase.status = "NIEAKTYWNY";
                p.railDatabase.extra  = "ostatnia podróż: " + deathYear;
                p.debugHint = "registry: died in " + deathYear;
                break;

            case PassengerTruth.NonExistent:
                // Subtle discrepancy: birth year in the database differs by 1,
                // and the registry says the parents died before the birth.
                p.railDatabase.birthDate = $"{day:00}.{month:00}.{year + 1}";
                p.registry.birthDate     = $"{day:00}.{month:00}.{year + 1}";
                p.registry.extra         = $"rodzice zmarli: {year}";
                p.debugHint = "birth date in database differs by a year, parents died before the birth";
                break;
        }

        return p;
    }

    // Random "truth" with weights: 60% alive, 25% dead, 15% non-existent.
    public static PassengerTruth RandomTruth()
    {
        float r = Random.value;
        if (r < 0.60f) return PassengerTruth.Alive;
        if (r < 0.85f) return PassengerTruth.Dead;
        return PassengerTruth.NonExistent;
    }

    static string Pick(string[] arr) => arr[Random.Range(0, arr.Length)];
}
