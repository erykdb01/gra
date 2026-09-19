using System.Collections.Generic;
using UnityEngine;

// Creates passengers: identity, hidden truth, documents and dialogue.
public static class PassengerGenerator
{
    static readonly string[] maleNames =
    {
        "Jan", "Piotr", "Tadeusz", "Stanisław", "Marek", "Andrzej", "Henryk", "Zbigniew", "Józef", "Władysław", "Kazimierz", "Ryszard",
        "Edward", "Leon", "Bogdan", "Jerzy", "Witold", "Ludwik", "Roman", "Feliks"
    };
    static readonly string[] femaleNames =
    {
        "Maria", "Anna", "Krystyna", "Ewa", "Halina", "Barbara", "Zofia", "Irena", "Helena", "Danuta", "Janina", "Teresa",
        "Wanda", "Jadwiga", "Stefania", "Lucyna", "Alicja", "Genowefa", "Bożena", "Renata"
    };
    // {male, female}
    static readonly string[][] surnames =
    {
        new[] { "Kowalski", "Kowalska" }, new[] { "Nowak", "Nowak" }, new[] { "Wiśniewski", "Wiśniewska" }, new[] { "Wójcik", "Wójcik" },
        new[] { "Kamiński", "Kamińska" }, new[] { "Lewandowski", "Lewandowska" }, new[] { "Zieliński", "Zielińska" }, new[] { "Szymański", "Szymańska" },
        new[] { "Dąbrowski", "Dąbrowska" }, new[] { "Kozłowski", "Kozłowska" }, new[] { "Jankowski", "Jankowska" }, new[] { "Mazur", "Mazur" },
        new[] { "Krawczyk", "Krawczyk" }, new[] { "Piotrowski", "Piotrowska" }, new[] { "Grabowski", "Grabowska" }, new[] { "Pawłowski", "Pawłowska" },
        new[] { "Michalski", "Michalska" }, new[] { "Zając", "Zając" }, new[] { "Król", "Król" }, new[] { "Wieczorek", "Wieczorek" }
    };
    static readonly string[] cities =
    {
        "Lublin", "Warszawa", "Kraków", "Gdańsk", "Poznań", "Łódź", "Radom", "Kielce", "Toruń", "Olsztyn", "Zamość", "Przemyśl", "Białystok", "Opole"
    };
    static readonly string[] stationNames =
    {
        "", "Szara Wola", "Zimna Woda", "Dolne Mosty", "Czarny Bór", "Przełaj", "Ostrowiec", "Kamienna Góra", "Biała Ława", "Stare Sady",
        "Krzyżówka", "Głuchy Las", "Nowa Zagroda"
    };

    // occupation by look: {pl, en}
    static readonly string[][] jobs =
    {
        new[] { "emeryt", "pensioner" }, new[] { "robotnik", "labourer" }, new[] { "przedsiębiorca", "businessman" }, new[] { "żołnierz na przepustce", "soldier on leave" },
        new[] { "emerytka", "pensioner" }, new[] { "nauczycielka", "teacher" }, new[] { "studentka", "student" }, new[] { "pielęgniarka", "nurse" },
        new[] { "lekarz", "doctor" }, new[] { "policjant", "policeman" }, new[] { "bezdomny", "homeless man" }, new[] { "wdowa", "widow" }, new[] { "podróżnik", "traveller" }
    };
    // age ranges by look
    static readonly int[][] ages =
    {
        new[] { 66, 86 }, new[] { 25, 50 }, new[] { 30, 55 }, new[] { 20, 35 }, new[] { 62, 84 }, new[] { 28, 55 }, new[] { 18, 26 },
        new[] { 24, 50 }, new[] { 32, 62 }, new[] { 25, 50 }, new[] { 40, 65 }, new[] { 40, 70 }, new[] { 30, 60 }
    };

    static readonly string[] pleasEn =
    {
        "Please. I have nowhere left to go.", "This train is the last thing I have left.", "My granddaughter is waiting at the station. Just this once.",
        "I know the documents don't match. But I won't hurt anyone.", "Give a person a chance. It's so cold outside.", "I'm going home. That's all. Please let me in.",
        "If you don't let me in, I won't survive the night.", "I have already lost everything. Don't take the train too."
    };
    static readonly string[] pleas =
    {
        "Proszę. Nie mam już dokąd wracać.", "Ten pociąg to ostatnia rzecz, która mi została.", "Moja wnuczka czeka na stacji. Proszę, tylko ten jeden raz.",
        "Wiem, że dokumenty się nie zgadzają. Ale nikomu nie zrobię krzywdy.", "Niech pan da człowiekowi szansę. Na zewnątrz jest tak zimno.", "Jadę do domu. Tylko tyle. Proszę mnie wpuścić.",
        "Jeśli mnie pan nie wpuści, nie przeżyję tej nocy.", "Straciłem już wszystko. Niech pan nie zabiera mi jeszcze pociągu."
    };

    // ---------- the deck: every night shows each look once before repeats ----------
    static readonly List<int> deck = new List<int>();
    public static void ResetDeck() { deck.Clear(); }

    static int NextLook()
    {
        if (deck.Count == 0)
        {
            for (int i = 0; i < CharacterArt.LookCount; i++) deck.Add(i);
            for (int i = deck.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                int tmp = deck[i]; deck[i] = deck[j]; deck[j] = tmp;
            }
        }
        int look = deck[deck.Count - 1];
        deck.RemoveAt(deck.Count - 1);
        return look;
    }

    static string Pick(string[] arr) { return arr[Random.Range(0, arr.Length)]; }
    static string Cap(string s) { return string.IsNullOrEmpty(s) ? s : char.ToUpper(s[0]) + s.Substring(1); }

    // ---------- public API ----------

    // Passenger number "index" of the night: a story character if the night has one in that slot, else random.
    public static PassengerData ForSlot(DayConfig day, int index)
    {
        if (day.scripted != null && GameState.Mode == GameMode.Story)
        {
            for (int i = 0; i < day.scripted.Count; i++)
            {
                if (day.scripted[i].slot == index)
                {
                    ScriptDef s = StoryCast.Get(day.scripted[i].script);
                    if (s != null) return FromScript(s, day);
                }
            }
        }
        return Generate(RandomTruth(day), day, null);
    }

    public static PassengerData FromScript(ScriptDef s, DayConfig day)
    {
        return Generate(s.truth, day, s);
    }

    public static PassengerTruth RandomTruth(DayConfig day)
    {
        float total = 0f;
        for (int i = 0; i < day.w.Length; i++) total += day.w[i];
        if (total <= 0f) return PassengerTruth.Alive;
        float r = Random.value * total;
        for (int i = 0; i < day.w.Length; i++)
        {
            if (r < day.w[i]) return (PassengerTruth)i;
            r -= day.w[i];
        }
        return PassengerTruth.Alive;
    }

    static bool HorrorReady(DayConfig day) { return day.act >= 3 || day.level >= 4; }

    public static PassengerData Generate(PassengerTruth truth, DayConfig day, ScriptDef script)
    {
        // a script may change the truth depending on earlier choices
        string altGreetPl = null, altGreetEn = null;
        if (script != null && script.altFlag != null && StoryFlags.Has(script.altFlag))
        {
            if (script.altChangesTruth) truth = script.altTruth;
            altGreetPl = script.altGreetPl; altGreetEn = script.altGreetEn;
        }

        var p = new PassengerData { truth = truth };
        p.storyId = script != null ? script.id : null;

        // ----- who is it -----
        if (script != null)
        {
            CastPerson c = script.person;
            p.look = c.look;
            p.female = c.female;
            p.scale = c.scale;
            p.age = c.age;
            p.firstName = c.first;
            p.lastName = c.last;
            p.occupation = Loc.T(c.occPl, c.occEn);
            p.origin = Loc.T(c.originPl, c.originEn);
            if (script.nameHidden) { p.firstName = Loc.T("Nieznany", "Unknown"); p.lastName = ""; }
            if (script.useConductorName) { p.firstName = "Jan"; p.lastName = "Ostoja"; }
            if (c.key == "nodocs") { p.firstName = Loc.T("Nieznany", "Unknown"); p.lastName = ""; }
        }
        else
        {
            p.look = NextLook();
            p.female = CharacterArt.IsFemale(p.look);
            p.scale = 1f;
            p.firstName = p.female ? Pick(femaleNames) : Pick(maleNames);
            string[] sn = surnames[Random.Range(0, surnames.Length)];
            p.lastName = p.female ? sn[1] : sn[0];
            int[] ar = ages[Mathf.Clamp(p.look, 0, ages.Length - 1)];
            p.age = Random.Range(ar[0], ar[1] + 1);
            string[] jb = jobs[Mathf.Clamp(p.look, 0, jobs.Length - 1)];
            p.occupation = Loc.T(jb[0], jb[1]);
            p.origin = Pick(cities);
        }
        p.fullName = (p.firstName + " " + p.lastName).Trim();

        int dead = truth == PassengerTruth.Dead ? 1 : 0;
        p.birthYear = DocumentSystem.CurrentYear - p.age;
        p.birthDate = Random.Range(1, 29).ToString("00") + "." + Random.Range(1, 13).ToString("00") + "." + p.birthYear;

        p.station = script != null ? script.station : Random.Range(1, 13);
        string stn = p.station <= 12 ? stationNames[p.station] : Loc.T("Ostatnia Stacja", "The Last Station");
        p.destination = Loc.T("Stacja ", "Station ") + p.station + " " + stn;
        bool oddTruth = truth == PassengerTruth.Dead || truth == PassengerTruth.NonExistent || truth == PassengerTruth.Echo || truth == PassengerTruth.Unknown;
        p.destinationRegistered = !(oddTruth && Random.value < 0.5f);
        if (script != null) p.destinationRegistered = true;

        p.ticketNo = "LN13-" + Random.Range(1000, 9999);
        p.docNo = (char)('A' + Random.Range(0, 26)) + "" + (char)('A' + Random.Range(0, 26)) + " " + Random.Range(100000, 999999);
        p.ticketName = p.fullName;
        p.ticketText = Loc.T("Wagon ", "Car ") + Random.Range(1, 6) + "  " + Loc.T("Miejsce ", "Seat ") + Random.Range(1, 60);
        p.illness = DocumentSystem.RandomIllness();
        p.motherName = (Pick(femaleNames) + " " + p.lastName).Trim();
        p.motherDeathYear = p.birthYear - Random.Range(1, 4);
        p.deathYear = Mathf.Min(DocumentSystem.CurrentYear - 1, p.birthYear + Mathf.Max(10, p.age - Random.Range(2, 30)));
        if (dead == 1)
        {
            p.deathYear = Mathf.Clamp(p.deathYear, p.birthYear + 5, DocumentSystem.CurrentYear - 1);
            p.deathDate = Random.Range(1, 29).ToString("00") + "." + Random.Range(1, 13).ToString("00") + "." + p.deathYear;
            p.deathCause = DocumentSystem.RandomCause();
        }

        // ----- forged ticket, no ticket, impostor photo -----
        if (script != null && script.noTicket) p.noTicket = true;
        if (script == null && truth == PassengerTruth.Alive && day.forgeChance > 0f && Random.value < day.forgeChance)
        {
            string other;
            do { other = p.female ? Pick(femaleNames) : Pick(maleNames); } while (other == p.firstName);
            p.ticketName = other + " " + p.lastName;
            p.forgedTicket = true;
        }
        if (truth == PassengerTruth.Impostor)
        {
            int other;
            do { other = Random.Range(0, CharacterArt.LookCount); } while (other == p.look || CharacterArt.IsFemale(other) != p.female);
            p.idPhotoLook = other;
        }

        // ----- Directorate order -----
        if (script != null) p.orderState = script.orderState;
        else if (day.Has(DocumentType.SpecialOrders) && day.orderChance > 0f && Random.value < day.orderChance)
        {
            bool base_ = Days.ShouldAdmit(p, day);
            if (!base_) p.orderState = Random.value < 0.5f ? 1 : 2;
        }

        // ----- behaviour -----
        AssignBehaviour(p);

        // ----- horror flags -----
        if (script == null && truth == PassengerTruth.Dead && HorrorReady(day) && Random.value < 0.14f) { p.aggressive = true; p.scaresOnArrival = true; }
        if (script == null && truth == PassengerTruth.Unknown && Random.value < 0.25f) p.scaresOnArrival = true;

        // ----- story text -----
        p.backstory = MakeBackstory(p);
        p.secret = MakeSecret(p);

        // ----- documents -----
        DocumentSystem.Build(p, day);

        // ----- begging -----
        if (script != null)
        {
            p.sympathetic = script.sympathetic;
            if (p.sympathetic) p.plea = Loc.T(script.pleaPl, script.pleaEn);
            string key = Cap(script.person.key);
            p.onAdmitFlag = Flag.Helped(key);
            p.onDenyFlag = Flag.Denied(key);
            p.codexOnMeet = script.codexOnMeet;
        }
        else if (!Days.ShouldAdmit(p, day) && Random.value < day.pleaChance)
        {
            p.sympathetic = true;
            int pi = Random.Range(0, pleas.Length);
            p.plea = Loc.T(pleas[pi], pleasEn[pi]);
        }

        // ----- dialogue -----
        p.dialogue = DialogueBank.Build(p, day, script);
        p.greeting = DialogueBank.MakeGreeting(p, script, altGreetPl, altGreetEn);

        p.debugHint = truth + (p.forgedTicket ? "+forged" : "") + (p.noTicket ? "+noticket" : "") + (p.orderState > 0 ? "+order" + p.orderState : "");
        return p;
    }

    static void AssignBehaviour(PassengerData p)
    {
        switch (p.truth)
        {
            case PassengerTruth.Dead:        p.behaviour = Random.value < 0.6f ? "cold" : "calm"; break;
            case PassengerTruth.NonExistent: p.behaviour = Random.value < 0.6f ? "scared" : "calm"; break;
            case PassengerTruth.Loop:        p.behaviour = "nervous"; break;
            case PassengerTruth.Impostor:    p.behaviour = Random.value < 0.5f ? "nervous" : "angry"; break;
            case PassengerTruth.Echo:        p.behaviour = "calm"; break;
            case PassengerTruth.Unknown:     p.behaviour = "cold"; break;
            default:
            {
                float r = Random.value;
                p.behaviour = r < 0.5f ? "calm" : (r < 0.75f ? "nervous" : (r < 0.9f ? "angry" : "scared"));
                break;
            }
        }
        p.stressLevel = (p.behaviour == "nervous" || p.behaviour == "scared") ? Random.Range(2, 4) : (p.behaviour == "angry" ? 2 : Random.Range(0, 2));
        p.traits = TraitsFor(p.behaviour);
    }

    public static string TraitsFor(string behaviour)
    {
        switch (behaviour)
        {
            case "nervous": return Loc.T("nerwowy, unika wzroku", "nervous, avoids your eyes");
            case "angry":   return Loc.T("rozdrażniony, mówi ostro", "irritated, speaks sharply");
            case "scared":  return Loc.T("przestraszony, rozgląda się", "frightened, keeps looking around");
            case "cold":    return Loc.T("zimny, nieruchomy, patrzy wprost", "cold, very still, stares straight at you");
            default:        return Loc.T("spokojny", "calm");
        }
    }

    static string MakeBackstory(PassengerData p)
    {
        string city = p.origin;
        string job = p.occupation;
        switch (p.truth)
        {
            case PassengerTruth.Dead:
                return Pick(new[]
                {
                    Loc.T("Zmarł" + (p.female ? "a" : "") + " po długiej chorobie w " + city + ". Nikt nie zgłosił zaginięcia, bo wszyscy wiedzieli.", "Died after a long illness in " + city + ". No one reported a disappearance, because everyone knew."),
                    Loc.T("Zginął" + (p.female ? "a" : "") + " w wypadku, wracając do domu z pracy (" + job + "). Do dziś próbuje wrócić.", "Died in an accident on the way home from work (" + job + "). Still trying to get home."),
                    Loc.T("Zasnął" + (p.female ? "a" : "") + " w poczekalni w " + city + " i się nie obudził" + (p.female ? "a" : "") + ".", "Fell asleep in a waiting room in " + city + " and never woke up.")
                });
            case PassengerTruth.NonExistent:
                return Pick(new[]
                {
                    Loc.T("W księgach miasta " + city + " nie ma po tej osobie śladu. Świadkowie twierdzą, że nikogo takiego nie widzieli.", "There is no trace of this person in the books of " + city + ". Witnesses say they never saw anyone like that."),
                    Loc.T("Dawno temu ktoś wykreślił jej wpis w Rejestrze. Papier został, człowiek też.", "Long ago someone struck this person's entry from the Registry. The paper stayed, and so did the person.")
                });
            case PassengerTruth.Loop:
                return Loc.T("Jest tu drugi raz. Albo pierwszy. Zależy, z której strony czasu liczyć.", "Is here for the second time. Or the first. Depends which side of time you count from.");
            case PassengerTruth.Impostor:
                return Loc.T("Kupił" + (p.female ? "a" : "") + " dokumenty od kogoś na dworcu w " + city + ". Chce po prostu wyjechać.", "Bought the papers from someone at the station in " + city + ". Just wants to leave.");
            case PassengerTruth.Echo:
                return Loc.T("Jest wspomnieniem człowieka, którego już nikt nie pamięta poza kimś, kto siedzi w wagonie.", "Is the memory of a person no one recalls except someone sitting in the car.");
            case PassengerTruth.Unknown:
                return Loc.T("System nie ma na ten temat żadnej wiedzy.", "The system holds no knowledge about this.");
            case PassengerTruth.Altered:
                return Loc.T("Ktoś poprawił dane tej osoby w nocy. Ona sama nie wie, że na papierze jest inna.", "Someone corrected this person's data in the night. They do not know they differ on paper.");
            default:
                return Pick(new[]
                {
                    Loc.T("Wraca z pogrzebu w " + city + ". Pracuje jako " + job + ", mieszka sam" + (p.female ? "a" : "") + ".", "Returns from a funeral in " + city + ". Works as a " + job + ", lives alone."),
                    Loc.T("Jedzie do rodziny po kłótni. Od lat nie widział" + (p.female ? "a" : "") + " brata.", "Going to see family after an argument. Has not seen a brother for years."),
                    Loc.T("Jedzie do nowej pracy jako " + job + ". Ma w torbie wszystko, co posiada.", "Going to a new job as a " + job + ". Carries everything they own in a bag."),
                    Loc.T("Uciekł" + (p.female ? "a" : "") + " z domu w " + city + ". Nikomu nic nie powiedział" + (p.female ? "a" : "") + ".", "Ran away from home in " + city + ". Told no one.")
                });
        }
    }

    static string MakeSecret(PassengerData p)
    {
        switch (p.truth)
        {
            case PassengerTruth.Dead:        return Loc.T("Wie, że nie żyje. Boi się, że pan to powie na głos.", "Knows they are dead. Afraid you will say it out loud.");
            case PassengerTruth.NonExistent: return Loc.T("Nie pamięta dzieciństwa. Pamięta tylko ten peron.", "Has no memory of childhood. Remembers only this platform.");
            case PassengerTruth.Loop:        return Loc.T("Widział" + (p.female ? "a" : "") + " ten peron w snach, na długo zanim przyjechał" + (p.female ? "a" : "") + ".", "Saw this platform in dreams long before arriving.");
            case PassengerTruth.Impostor:    return Loc.T("Prawdziwy właściciel dokumentów jest w wagonie 13.", "The real owner of the documents is in car 13.");
            case PassengerTruth.Echo:        return Loc.T("Nie jest pewn" + (p.female ? "a" : "y") + ", czy jest kimś, czy tylko czyimś zdaniem.", "Is not sure whether they are someone or only someone's sentence.");
            case PassengerTruth.Unknown:     return Loc.T("Zna pana imię i numer służbowy.", "Knows your name and employee number.");
            case PassengerTruth.Altered:     return Loc.T("Kiedyś miał" + (p.female ? "a" : "") + " inne nazwisko. Nie pamięta jakie.", "Once had a different name. Cannot remember which.");
            default:
                return Pick(new[]
                {
                    Loc.T("Wiezie w torbie list, którego nie odważy się wysłać.", "Carries a letter in the bag that they dare not send."),
                    Loc.T("Nie kupił" + (p.female ? "a" : "") + " biletu powrotnego.", "Did not buy a return ticket."),
                    Loc.T("Ma dług, którego nie da się spłacić pieniędzmi.", "Has a debt that cannot be paid with money.")
                });
        }
    }

    // ---------- the twist: the last passenger is the conductor ----------
    public static PassengerData GeneratePlayer(DayConfig day)
    {
        string cond = "Jan Ostoja";
        var p = new PassengerData
        {
            truth = PassengerTruth.Unknown,
            isPlayer = true,
            look = -1,
            firstName = "Jan", lastName = "Ostoja", fullName = cond,
            age = 0, occupation = Loc.T("konduktor Linii 13", "Line 13 conductor"),
            origin = Loc.T("Peron 13", "Platform 13"),
            destination = Loc.T("Ostatnia Stacja", "The Last Station"),
            station = 13,
            ticketNo = "LN13-0013", docNo = "K-13-0013",
            ticketName = cond,
            ticketText = Loc.T("Wagon 13  Miejsce 13", "Car 13  Seat 13"),
            birthDate = "??.??.19??", birthYear = 1962,
            behaviour = "calm",
            debugHint = "the passenger is the conductor",
            playerPhoto = true,
            destinationRegistered = true
        };
        p.illness = Loc.T("zmęczenie", "exhaustion");
        p.motherName = "?";
        DocumentSystem.Build(p, day);

        // the conductor's documents: everything says "unknown" or "on duty"
        DocumentData id = DocumentSystem.Get(p, DocumentType.Id);
        id.photoLook = -1; id.photoIsPlayer = true;
        id.front[0].anomaly = null;
        id.front[2].value = "??.??.19??";
        id.front[3].value = "K-13-0013";
        id.front[4].value = Loc.T("13.11.1987", "13.11.1987");
        id.front[5].value = Loc.T("Peron 13", "Platform 13");
        id.front[7].value = Loc.T("NA SŁUŻBIE", "ON DUTY");
        id.front.Add(new DocLine(Loc.T("HISTORIA PRACY", "WORK HISTORY"), Loc.T("od 1987, bez przerw, bez urlopów", "since 1987, no breaks, no leave")));
        DocumentData db = DocumentSystem.Get(p, DocumentType.RailDatabase);
        db.front[1].value = Loc.T("brak danych", "no data"); db.front[2].value = Loc.T("ZAWIESZONY", "SUSPENDED");
        db.front[3].value = Loc.T("nigdy", "never"); db.front[4].value = "0";
        DocumentData reg = DocumentSystem.Get(p, DocumentType.Registry);
        reg.front[1].value = "??.??.19??"; reg.front[2].value = Loc.T("NIEZNANY", "UNKNOWN");
        DocumentData death = DocumentSystem.Get(p, DocumentType.DeathRecord);
        death.front.Clear();
        death.front.Add(new DocLine(Loc.T("STATUS", "STATUS"), Loc.T("NIE MOŻNA USTALIĆ", "UNABLE TO DETERMINE")));
        death.front.Add(new DocLine(Loc.T("DATA", "DATE"), Loc.T("13.11.1998 (dzisiaj)", "13.11.1998 (today)")));

        p.dialogue = DialogueBank.BuildPlayer();
        p.greeting = DialogueBank.MakeGreeting(p, null, null, null);
        return p;
    }
}
