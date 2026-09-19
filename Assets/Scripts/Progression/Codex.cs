using System.Collections.Generic;

public enum CodexCategory { People = 0, Locations = 1, Anomalies = 2, Documents = 3, Rules = 4, Events = 5, Line13 = 6, Unknown = 7 }

public class CodexEntry
{
    public string id;
    public CodexCategory category;
    public string titlePl, titleEn, textPl, textEn;
    public string title { get { return Loc.T(titlePl, titleEn); } }
    public string text { get { return Loc.T(textPl, textEn); } }
}

// The in-game encyclopaedia. Entries fill up while you play.
public static class Codex
{
    static readonly List<CodexEntry> all = new List<CodexEntry>();
    static readonly Dictionary<string, CodexEntry> byId = new Dictionary<string, CodexEntry>();
    static bool built;

    // Ids unlocked during the current shift (shown on the results screen).
    public static readonly List<string> NewThisShift = new List<string>();

    public static List<CodexEntry> All { get { Build(); return all; } }

    public static CodexEntry Get(string id)
    {
        Build();
        CodexEntry e;
        return byId.TryGetValue(id, out e) ? e : null;
    }

    public static bool IsUnlocked(string id) { return SaveSystem.Data.codex.Contains(id); }

    public static int UnlockedCount { get { return SaveSystem.Data.codex.Count; } }

    // silent = no toast (used for the "meta" entries)
    public static void Unlock(string id, bool silent = false)
    {
        if (string.IsNullOrEmpty(id)) return;
        Build();
        if (!byId.ContainsKey(id)) return;
        if (SaveSystem.Data.codex.Contains(id)) return;
        SaveSystem.Data.codex.Add(id);
        NewThisShift.Add(id);
        GameEvents.RaiseCodex(id);
        if (!silent) ToastUI.Show(Loc.T("KODEKS", "CODEX"), byId[id].title, new UnityEngine.Color(0.55f, 0.75f, 0.95f));
    }

    static void Add(string id, CodexCategory c, string tp, string te, string xp, string xe)
    {
        var e = new CodexEntry { id = id, category = c, titlePl = tp, titleEn = te, textPl = xp, textEn = xe };
        all.Add(e);
        byId[id] = e;
    }

    static void Build()
    {
        if (built) return;
        built = true;

        // ---- PEOPLE ----
        Add("ppl_conductor", CodexCategory.People, "Konduktor", "The Conductor",
            "Ty. Jan Ostoja, numer służbowy K-13-0013. W aktach brakuje daty urodzenia i daty zatrudnienia. Jest tylko dopisek: \"na stanowisku od zawsze\".",
            "You. Jan Ostoja, employee number K-13-0013. The files miss a birth date and a hiring date. There is only a note: \"at the post since forever\".");
        Add("ppl_anna", CodexCategory.People, "Anna Wolska", "Anna Wolska",
            "Kobieta, która jeździ tym samym pociągiem od trzydziestu lat. Zawsze ma bilet na stację 13 i zawsze mówi, że ktoś tam na nią czeka.",
            "A woman who has taken the same train for thirty years. She always holds a ticket to station 13 and always says someone is waiting for her there.");
        Add("ppl_marek", CodexCategory.People, "Marek Sowa", "Marek Sowa",
            "Starszy pasażer z laską. Zbiera pamiątki po ludziach, których kolej wykreśliła. Ktoś, kto pamięta, jak było przed Linią 13.",
            "An old passenger with a cane. He collects mementos of people the railway struck out. Someone who remembers what it was like before Line 13.");
        Add("ppl_lena", CodexCategory.People, "Lena", "Lena",
            "Dziecko, które czeka na mamę. Kolejne noce, ta sama sukienka, to samo pytanie: \"Czy mama już jest w wagonie?\"",
            "A child waiting for her mother. Night after night, the same dress, the same question: \"Is Mum already in the car?\"");
        Add("ppl_kruk", CodexCategory.People, "Inspektor Kruk", "Inspector Kruk",
            "Inspektor Dyrekcji Kolei. Zawsze punktualny, zawsze bez dokumentów. Nikt nie widział, jak wchodzi na peron.",
            "Inspector of the Railway Directorate. Always punctual, always without documents. No one has seen him enter the platform.");
        Add("ppl_piotr", CodexCategory.People, "Piotr Wrona", "Piotr Wrona",
            "Pracownik kolei z torbą narzędzi. Naprawia rzeczy, których nie da się naprawić, i mówi za dużo jak na kogoś, kto ma pracę.",
            "A railway worker with a tool bag. He fixes what cannot be fixed and says too much for someone who wants to keep his job.");
        Add("ppl_grey", CodexCategory.People, "Człowiek w szarym płaszczu", "The Man in Grey",
            "Nikt nie zna jego nazwiska. W systemie: STATUS NIEZNANY. Zna twoje imię.",
            "No one knows his name. In the system: STATUS UNKNOWN. He knows your name.");
        Add("ppl_nodocs", CodexCategory.People, "Człowiek bez dokumentów", "The Man Without Papers",
            "Nie ma biletu, dowodu ani wpisu w bazie. Oddycha jednak jak wszyscy. Kolej nie wie, co z nim zrobić.",
            "No ticket, no ID, no database entry. Yet he breathes like everyone else. The railway does not know what to do with him.");

        // ---- LOCATIONS ----
        Add("loc_platform13", CodexCategory.Locations, "Peron 13", "Platform 13",
            "Boczny peron, którego nie ma na żadnej mapie. Zawsze pada tu deszcz, nawet gdy niebo jest czyste.",
            "A side platform that is on no map. It always rains here, even when the sky is clear.");
        Add("loc_booth", CodexCategory.Locations, "Okienko kontroli", "The Checkpoint Booth",
            "Twoje biurko, lampa i stos dokumentów. Drewno jest tak stare, że pamięta zmiany, których ty nie pamiętasz.",
            "Your desk, a lamp and a pile of documents. The wood is so old it remembers shifts that you do not.");
        Add("loc_car13", CodexCategory.Locations, "Wagon 13", "Car 13",
            "Wagon zamknięty od zewnątrz. Nie figuruje w manifeście. Czasem ktoś puka od środka.",
            "A car locked from the outside. It is not on the manifest. Sometimes someone knocks from inside.");
        Add("loc_registryhall", CodexCategory.Locations, "Sala Rejestru", "The Registry Hall",
            "Miejsce, gdzie kolej trzyma prawdę o ludziach. Podobno jest tam druga księga, ukryta przed Dyrekcją.",
            "The place where the railway keeps the truth about people. There is said to be a second book, hidden from the Directorate.");
        Add("loc_station13", CodexCategory.Locations, "Ostatnia Stacja", "The Last Station",
            "Stacja 13. Nikt z niej nie wrócił, więc nikt nie potrafi jej opisać. W rozkładzie jazdy godzina przyjazdu to: \"kiedy będziesz gotowy\".",
            "Station 13. No one has returned from it, so no one can describe it. In the timetable the arrival time reads: \"when you are ready\".");

        // ---- ANOMALIES ----
        Add("ano_dead", CodexCategory.Anomalies, "Zmarli", "The Dead",
            "Ludzie, którzy już nie żyją, ale stoją przed tobą. Nie oddychają, ich dłonie są zimne, a w Rejestrze widnieje data śmierci.",
            "People who are no longer alive but stand before you. They do not breathe, their hands are cold, and the Registry shows a date of death.");
        Add("ano_nonex", CodexCategory.Anomalies, "Nieistniejący", "The Non-Existent",
            "Mają dokumenty i twarz, ale rzeczywistość ich nie pamięta. Nie rzucają cienia, a ich rodzice umarli, zanim się urodzili.",
            "They have documents and a face, but reality does not remember them. They cast no shadow, and their parents died before they were born.");
        Add("ano_altered", CodexCategory.Anomalies, "Zmienieni", "The Altered",
            "Żywi ludzie, w których danych ktoś coś poprawił. Zwykle jedna litera, jedna cyfra. Zawsze z dopiskiem \"sprostowano\".",
            "Living people whose data someone corrected. Usually one letter, one digit. Always with the note \"amended\".");
        Add("ano_loop", CodexCategory.Anomalies, "Pętla", "The Loop",
            "Dokumenty z przyszłej lub przeszłej daty. Ktoś, kto już tu był albo dopiero będzie. Widzenie ich boli w głowie.",
            "Documents dated in the future or the past. Someone who has already been here or is yet to come. Looking at them hurts your head.");
        Add("ano_impostor", CodexCategory.Anomalies, "Podszywacze", "The Impostors",
            "Ktoś używa cudzych dokumentów. Zdjęcie na dowodzie nie zgadza się z twarzą, a podpis jest odrobinę za równy.",
            "Someone uses another person's papers. The ID photo does not match the face, and the signature is a little too even.");
        Add("ano_echo", CodexCategory.Anomalies, "Echa", "The Echoes",
            "Nie ludzie, tylko wspomnienia ludzi. Mówią cudzymi zdaniami i nie zauważają, że są półprzezroczyste.",
            "Not people but memories of people. They speak in other people's sentences and do not notice they are see-through.");
        Add("ano_unknown", CodexCategory.Anomalies, "Nieznani", "The Unknown",
            "System nie potrafi ich sklasyfikować. Nie ma cienia, oddechu ani wpisu. Są za to spokojni, jakby wiedzieli coś, czego ty nie wiesz.",
            "The system cannot classify them. No shadow, no breath, no entry. They are calm, though, as if they knew something you do not.");
        Add("ano_photo", CodexCategory.Anomalies, "Zdjęcie się nie zgadza", "The Photo Does Not Match",
            "Twarz na dowodzie należy do kogoś innego. Najprostszy sposób na poznanie podszywacza.",
            "The face on the ID belongs to someone else. The simplest way to spot an impostor.");
        Add("ano_date", CodexCategory.Anomalies, "Niemożliwa data", "Impossible Date",
            "Data wydania z przyszłości, podróż po śmierci, urodziny rok później niż w bazie. Daty kłamią najgłośniej.",
            "An issue date from the future, a trip after death, a birthday a year off from the database. Dates lie the loudest.");
        Add("ano_signature", CodexCategory.Anomalies, "Fałszywy podpis", "Forged Signature",
            "Podpis urzędnika jest zbyt równy albo napisany w złą stronę. Prawdziwe podpisy drżą.",
            "The clerk's signature is too even or written the wrong way. Real signatures tremble.");
        Add("ano_seal", CodexCategory.Anomalies, "Nieprawidłowa pieczęć", "Wrong Seal",
            "Pieczęć nie z tej stacji, nie z tego roku albo bez numeru. Kolej lubi porządek, więc bałagan wyróżnia.",
            "A seal from another station, another year, or without a number. The railway likes order, so mess stands out.");
        Add("ano_breath", CodexCategory.Anomalies, "Brak oddechu", "No Breath",
            "W zimnym powietrzu żywym idzie para z ust. Kto stoi bez pary, ten albo nie żyje, albo nie powinien stać.",
            "In the cold air the living breathe steam. Whoever stands without any either is dead or should not be standing.");
        Add("ano_shadow", CodexCategory.Anomalies, "Brak cienia", "No Shadow",
            "Każdy prawdziwy człowiek rzuca cień. Jeśli go nie ma, rzeczywistość o nim zapomniała.",
            "Every real person casts a shadow. If there is none, reality has forgotten them.");

        // ---- DOCUMENTS ----
        Add("doc_ticket", CodexCategory.Documents, "Bilet", "Ticket",
            "Trasa, wagon, miejsce i numer. Imię na bilecie musi zgadzać się z dowodem.",
            "Route, car, seat and number. The name on the ticket must match the ID.");
        Add("doc_id", CodexCategory.Documents, "Dowód", "ID Card",
            "Imię, data urodzenia, numer dokumentu i zdjęcie. Sprawdzaj zdjęcie, zanim uwierzysz w resztę.",
            "Name, date of birth, document number and photo. Check the photo before you believe the rest.");
        Add("doc_raildb", CodexCategory.Documents, "Baza kolejowa", "Railway Database",
            "Kolej zapisuje wszystkie podróże. Zapisuje też te, które się jeszcze nie odbyły.",
            "The railway records every journey. It also records the ones that have not happened yet.");
        Add("doc_registry", CodexCategory.Documents, "Rejestr", "Registry",
            "Urzędowa prawda o tym, kto żyje. Bywa, że Rejestr i rzeczywistość się nie zgadzają. Wtedy rację ma Rejestr.",
            "The official truth about who lives. Sometimes the Registry and reality disagree. Then the Registry is right.");
        Add("doc_manifest", CodexCategory.Documents, "Manifest pasażerów", "Passenger Manifest",
            "Lista osób, które mają dziś jechać. Ktoś spoza listy nie jest gościem, tylko problemem.",
            "The list of people who are to travel tonight. Anyone off the list is not a guest but a problem.");
        Add("doc_medical", CodexCategory.Documents, "Karta medyczna", "Medical Record",
            "Choroby, wizyty, ostatnie badania. Zmarli często mają w niej wpis z dnia, który już minął.",
            "Illnesses, visits, latest exams. The dead often have an entry from a day that has already passed.");
        Add("doc_death", CodexCategory.Documents, "Akt zgonu", "Death Record",
            "Data i przyczyna śmierci. Jeśli człowiek przed tobą ma akt zgonu, masz problem albo cud.",
            "Date and cause of death. If the person in front of you has a death record, you have a problem or a miracle.");
        Add("doc_travel", CodexCategory.Documents, "Historia podróży", "Travel History",
            "Poprzednie przejazdy Linią 13. Kto jeździ po własnej śmierci, jeździ długo.",
            "Previous journeys on Line 13. Whoever rides after their own death has been riding for a long time.");
        Add("doc_orders", CodexCategory.Documents, "Rozkaz specjalny", "Special Orders",
            "Polecenie Dyrekcji dla konkretnego pasażera. Ważny rozkaz znosi zasadę dnia. Fałszywy kończy się zwolnieniem.",
            "A Directorate order for a specific passenger. A valid order overrides the rule of the day. A forged one ends with dismissal.");

        // ---- EVENTS ----
        Add("evt_knock", CodexCategory.Events, "Pukanie", "The Knocking",
            "Ktoś puka od środka wagonu, którego nikt nie wpuszczał. Pukanie ma zawsze ten sam rytm: trzy, pauza, trzynaście.",
            "Someone knocks from inside a car nobody boarded. Always the same rhythm: three, a pause, thirteen.");
        Add("evt_blackout", CodexCategory.Events, "Brak prądu", "Blackout",
            "Światła gasną na kilka sekund. Kiedy wracają, coś stoi trochę bliżej.",
            "The lights go out for a few seconds. When they return, something is standing a little closer.");
        Add("evt_14", CodexCategory.Events, "Czternasty pasażer", "The Fourteenth Passenger",
            "System pokazuje 14 pasażerów, choć w kolejce jest 13. Nikt nie wie, kim jest ten dodatkowy.",
            "The system shows 14 passengers though the queue holds 13. No one knows who the extra one is.");
        Add("evt_clock", CodexCategory.Events, "Zatrzymany zegar", "The Stopped Clock",
            "Zegar na peronie staje o 03:13. Godzinę później nadal jest 03:13.",
            "The platform clock stops at 03:13. An hour later it is still 03:13.");
        Add("evt_photo", CodexCategory.Events, "Twoje zdjęcie", "Your Photo",
            "Na cudzym dokumencie znajdujesz swoją twarz. Ktoś ją tam wkleił, albo zawsze tam była.",
            "You find your own face on someone else's document. Someone pasted it there, or it was always there.");
        Add("evt_vanish", CodexCategory.Events, "Zniknięcie z kolejki", "Vanished From the Queue",
            "Ktoś z kolejki znika i nikt nie zauważa, że go nie ma. Nikt oprócz ciebie.",
            "Someone in the queue vanishes and no one notices they are gone. No one but you.");
        Add("evt_memory", CodexCategory.Events, "Pamiętasz poprzednią noc?", "Do You Remember Last Night?",
            "Pasażer pyta, czy pamiętasz poprzednią noc. Twoja odpowiedź zmienia to, jak traktuje cię kolej.",
            "A passenger asks whether you remember last night. Your answer changes how the railway treats you.");
        Add("evt_stop", CodexCategory.Events, "Pociąg staje", "The Train Stops",
            "Skład hamuje bez powodu. Za oknami nie ma niczego. Ani nocy, ani peronu.",
            "The train brakes for no reason. There is nothing behind the windows. No night, no platform.");
        Add("evt_text", CodexCategory.Events, "Dokument zmienia tekst", "The Document Changes",
            "Litery na papierze przestawiają się, gdy nie patrzysz. Na końcu zostaje jedno zdanie.",
            "The letters on the paper rearrange when you look away. In the end one sentence remains.");

        // ---- LINE 13 ----
        Add("lore_line13", CodexCategory.Line13, "Linia 13", "Line 13",
            "Nocny pociąg bez rozkładu. Zabiera tych, których rzeczywistość zostawiła w rozsypce: zmarłych, wykreślonych i zagubionych.",
            "A night train without a timetable. It takes those reality left in pieces: the dead, the struck-out and the lost.");
        Add("lore_railway", CodexCategory.Line13, "Dyrekcja Kolei", "The Railway Directorate",
            "Urząd, który pilnuje porządku między papierem a światem. Kiedy się rozchodzą, poprawia świat.",
            "The office that keeps order between paper and world. When they diverge, it corrects the world.");
        Add("lore_records", CodexCategory.Line13, "Rejestr i rzeczywistość", "Records and Reality",
            "Człowiek istnieje, dopóki jest zapisany. Wykreśl wpis, a ktoś przestanie być. Dopisz kogoś, a ktoś zacznie.",
            "A person exists as long as they are written down. Strike an entry and someone ceases to be. Add one and someone begins.");
        Add("lore_admit", CodexCategory.Line13, "Co dzieje się z wpuszczonymi", "What Happens to the Admitted",
            "Wpuszczeni jadą do stacji 13. Nikt ich nie widział ponownie. Nikt też nie widział, żeby ktoś stamtąd wracał głodny.",
            "The admitted ride to station 13. No one has seen them again. No one has seen anyone come back hungry, either.");
        Add("lore_loop", CodexCategory.Line13, "Zmiana bez końca", "The Shift Without End",
            "Każdy konduktor kiedyś skończył zmianę. Żaden nie pamięta, czy wsiadł.",
            "Every conductor once finished a shift. None of them remembers whether they boarded.");
        Add("lore_line13b", CodexCategory.Line13, "Pociąg słucha", "The Train Listens",
            "Skład reaguje na twoje decyzje: gaśnie, drży, zmienia numer linii. Nie jest zły. Jest uważny.",
            "The train reacts to your decisions: it dims, it trembles, it changes the line number. It is not evil. It is attentive.");

        // ---- UNKNOWN (secrets) ----
        Add("unk_you", CodexCategory.Unknown, "Ten wpis już czytałeś", "You Have Read This Before",
            "Kodeks zawiera wpis, którego nie widziałeś. Napisano go twoim pismem.",
            "The Codex holds an entry you have not seen. It was written in your handwriting.");
        Add("unk_hidden", CodexCategory.Unknown, "Ukryty Rejestr", "The Hidden Registry",
            "Druga księga. Zapisano w niej wszystkich, których Dyrekcja wykreśliła. Twoje imię jest na pierwszej stronie.",
            "The second book. It lists everyone the Directorate struck out. Your name is on the first page.");
        Add("unk_first", CodexCategory.Unknown, "Pierwszy konduktor", "The First Conductor",
            "Człowiek, który odmówił sobie miejsca w pociągu i został na peronie. Od tej pory każdy konduktor to on.",
            "The man who denied himself a seat on the train and stayed on the platform. Every conductor since has been him.");
        Add("unk_thirteen", CodexCategory.Unknown, "Trzynasty", "The Thirteenth",
            "Zawsze jest trzynaście osób. Dwanaście z nich to pasażerowie. Kim jest trzynasta?",
            "There are always thirteen people. Twelve of them are passengers. Who is the thirteenth?");
        Add("unk_secret", CodexCategory.Unknown, "Peron Czternasty", "Platform Fourteen",
            "Za peronem 13 jest jeszcze jeden. Nie ma go w Rejestrze, więc jest jedynym miejscem, gdzie kolej nie sięga.",
            "Behind platform 13 there is one more. It is not in the Registry, so it is the only place the railway cannot reach.");

        // ---- RULES: one entry per story night ----
        for (int i = 0; i < StoryData.Nights.Count; i++)
        {
            DayConfig d = StoryData.Nights[i];
            Add("rule_n" + i, CodexCategory.Rules, "Noc " + (i + 1) + ": " + d.titlePl, "Night " + (i + 1) + ": " + d.titleEn,
                d.rulePl + "\n\n" + d.briefingPl, d.ruleEn + "\n\n" + d.briefingEn);
        }
        Add("rule_endless", CodexCategory.Rules, "Nieskończona zmiana", "Endless Shift",
            "Poza kampanią zasady zmieniają się co noc losowo. Kolej nie tłumaczy się z tego.",
            "Outside the campaign the rules change at random every night. The railway does not explain itself.");
    }
}
