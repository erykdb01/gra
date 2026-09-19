using System.Collections.Generic;

// A person who returns during the story (recurring character).
public class CastPerson
{
    public string key;
    public string first, last;
    public int look;
    public float scale = 1f;
    public int age;
    public bool female;
    public string occPl, occEn;
    public string originPl, originEn;
}

// One scripted appearance: who, in which state, what they say.
public class ScriptDef
{
    public string id;
    public CastPerson person;
    public PassengerTruth truth;
    public string altFlag;                 // when this flag is set the passenger is different
    public bool altChangesTruth;
    public PassengerTruth altTruth;
    public string greetPl, greetEn, altGreetPl, altGreetEn;
    public bool sympathetic;
    public string pleaPl, pleaEn;
    public bool noTicket;
    public int orderState;                 // 0 none, 1 valid, 2 forged
    public bool nameHidden;                // shows "???" instead of the name
    public bool useConductorName;          // documents carry the conductor's name
    public int station = 13;
    public string codexOnMeet;
    public List<ScriptQA> qas = new List<ScriptQA>();

    public ScriptDef Q(string id, string qPl, string qEn, string aPl, string aEn, int kind, string flag, string codex, int stress)
    {
        qas.Add(new ScriptQA { id = id, qPl = qPl, qEn = qEn, aPl = aPl, aEn = aEn, kind = kind, flag = flag, codex = codex, stress = stress });
        return this;
    }

    public ScriptDef Alt(string flag, PassengerTruth truth, string gPl, string gEn)
    {
        altFlag = flag; altChangesTruth = true; altTruth = truth; altGreetPl = gPl; altGreetEn = gEn;
        return this;
    }

    public ScriptDef AltGreeting(string flag, string gPl, string gEn)
    {
        altFlag = flag; altChangesTruth = false; altGreetPl = gPl; altGreetEn = gEn;
        return this;
    }
}

public class ScriptQA
{
    public string id, qPl, qEn, aPl, aEn, flag, codex;
    public int kind, stress;
}

public static class StoryCast
{
    static readonly Dictionary<string, CastPerson> people = new Dictionary<string, CastPerson>();
    static readonly Dictionary<string, ScriptDef> scripts = new Dictionary<string, ScriptDef>();

    public static CastPerson Person(string key) { Build(); CastPerson p; return people.TryGetValue(key, out p) ? p : null; }

    public static ScriptDef Get(string id)
    {
        Build();
        ScriptDef s;
        return scripts.TryGetValue(id, out s) ? s : null;
    }

    static CastPerson P(string key, string first, string last, int look, int age, bool female, string occPl, string occEn, string oPl, string oEn, float scale = 1f)
    {
        var c = new CastPerson { key = key, first = first, last = last, look = look, age = age, female = female, occPl = occPl, occEn = occEn, originPl = oPl, originEn = oEn, scale = scale };
        people[key] = c;
        return c;
    }

    static ScriptDef S(string id, string person, PassengerTruth truth, string gPl, string gEn)
    {
        var s = new ScriptDef { id = id, person = people[person], truth = truth, greetPl = gPl, greetEn = gEn };
        scripts[id] = s;
        return s;
    }

    static bool built;

    static void Build()
    {
        if (built) return;
        built = true;

        P("anna", "Anna", "Wolska", 5, 58, true, "wdowa, dawna telegrafistka", "widow, former telegraphist", "Radom", "Radom");
        P("marek", "Marek", "Sowa", 0, 77, false, "emerytowany zegarmistrz", "retired watchmaker", "Sandomierz", "Sandomierz");
        P("lena", "Lena", "Wolska", 6, 8, true, "dziecko", "child", "Radom", "Radom", 0.68f);
        P("kruk", "Adam", "Kruk", 9, 45, false, "inspektor Dyrekcji Kolei", "Railway Directorate inspector", "Dyrekcja", "the Directorate");
        P("piotr", "Piotr", "Wrona", 1, 41, false, "pracownik kolei", "railway worker", "Zajezdnia 4", "Depot 4");
        P("grey", "", "", 12, 50, false, "brak danych", "no data", "brak danych", "no data");
        P("nodocs", "", "", 10, 55, false, "bez zajęcia", "no occupation", "nie wiadomo", "unknown");

        // ---------------- ANNA ----------------
        S("anna_1", "anna", PassengerTruth.Alive,
            "Dobry wieczór. Jadę jak co noc, do stacji 13. Ktoś tam na mnie czeka.",
            "Good evening. I'm going, like every night, to station 13. Someone is waiting for me there.")
            .Q("anna1a", "Jak długo pani jeździ?", "How long have you been riding?",
               "Odkąd pamiętam. Ten pociąg jeździ w kółko, a ja razem z nim.", "As long as I remember. This train goes round and round, and so do I.", QA.Truth, null, "ppl_anna", 0)
            .Q("anna1b", "Kto na panią czeka?", "Who is waiting for you?",
               "Córeczka. Ma na imię Lena. Zawsze czeka na peronie... tylko że peron jest zawsze o jeden za daleko.", "My little girl. Her name is Lena. She always waits on the platform... only the platform is always one too far.", QA.Truth, null, "ppl_lena", 0);
        S("anna_2", "anna", PassengerTruth.Alive,
            "Znowu ja. Pan mnie pamięta, prawda? Pan zawsze mnie wpuszcza.",
            "It's me again. You remember me, don't you? You always let me in.")
            .AltGreeting(Flag.Denied("Anna"), "Zeszłej nocy odesłał mnie pan. Nic nie szkodzi. Wystarczy, że jutro znów mogę spróbować.", "You sent me away last night. It doesn't matter. It's enough that I can try again tomorrow.")
            .Q("anna2a", "Czy widziała pani tu inspektora?", "Have you seen the inspector here?",
               "Kruka? Jest zawsze tam, gdzie jest lista. Nigdy nie widziałam, żeby wchodził po schodach.", "Kruk? He is always where the list is. I have never seen him climb the stairs.", QA.Truth, null, "ppl_kruk", 0);
        S("anna_3", "anna", PassengerTruth.Alive,
            "Przyniosłam panu herbatę. Nie zdążyła wystygnąć, choć wyszłam z domu... nie pamiętam, kiedy.",
            "I brought you tea. It has not had time to cool, though I left home... I don't remember when.")
            .Alt(Flag.Denied("Anna"), PassengerTruth.Dead, "Dobry wieczór. Jest tu jakoś zimno. Jestem umówiona z córką.", "Good evening. It is rather cold here. I have a meeting with my daughter.")
            .Q("anna3a", "Co się stało z pani córką?", "What happened to your daughter?",
               "Nie wróciła. Powiedzieli, że nie ma jej w Rejestrze. Że nigdy jej nie było. Ale ja ją urodziłam.", "She did not come back. They said she is not in the Registry. That she never was. But I gave birth to her.", QA.Truth, null, "lore_records", 3);
        S("anna_4", "anna", PassengerTruth.Alive,
            "Czuję, że pociąg mnie słucha. Pan też to czuje?",
            "I feel the train listening to me. Do you feel it too?")
            .Alt(Flag.Denied("Anna"), PassengerTruth.Dead, "Nie mam już żadnych pytań. Została mi tylko cierpliwość.", "I have no more questions. All I have left is patience.")
            .Q("anna4a", "Dlaczego pani tu ciągle wraca?", "Why do you keep coming back?",
               "Bo tylko tutaj mogę wypowiadać jej imię na głos.", "Because this is the only place I can say her name out loud.", QA.Truth, null, null, 0);
        S("anna_5", "anna", PassengerTruth.Alive,
            "Zostały już tylko dwie noce, prawda? Widzę to w pana oczach.",
            "Only two nights are left, aren't they? I can see it in your eyes.")
            .Alt(Flag.Denied("Anna"), PassengerTruth.Dead, "Wróciłam. Nie umiem inaczej.", "I came back. I don't know how else.")
            .Q("anna5a", "Co pani zrobi na stacji 13?", "What will you do at station 13?",
               "Wezmę ją za rękę. I tyle. Nic więcej mi nie trzeba.", "I will take her by the hand. That's all. I need nothing more.", QA.Truth, null, null, 0);
        S("anna_6", "anna", PassengerTruth.Alive,
            "Dziś jest ten dzień, prawda? Nie boję się. A pan?",
            "Today is the day, isn't it? I'm not afraid. Are you?")
            .Alt(Flag.Denied("Anna"), PassengerTruth.Dead, "Jestem tu od zawsze. Czekam na pana.", "I have been here forever. Waiting for you.")
            .Q("anna6a", "Czy córka jest w wagonie?", "Is your daughter in the car?",
               "Jest. Zawsze była. Tylko że nie mogłam do niej wejść bez pańskiego pozwolenia.", "She is. She always was. I just couldn't go in to her without your permission.", QA.Truth, null, "loc_car13", 0);

        // ---------------- MAREK ----------------
        S("marek_1", "marek", PassengerTruth.Alive,
            "Dobry wieczór, młody człowieku. Bilet mam, ale głowa nie ta co dawniej.",
            "Good evening, young man. I have my ticket, but my head is not what it was.")
            .Q("marek1a", "Czym się pan zajmował?", "What did you do for a living?",
               "Zegarmistrz. Naprawiałem cudzy czas. Swojego nie potrafiłem.", "Watchmaker. I mended other people's time. I could not mend my own.", QA.Truth, null, "ppl_marek", 0)
            .Q("marek1b", "Co pan wozi w tej torbie?", "What is in that bag?",
               "Rzeczy po ludziach, których kolej wykreśliła. Ktoś musi je trzymać.", "Things from people the railway struck out. Someone has to keep them.", QA.Half, null, "lore_records", 0);
        S("marek_2", "marek", PassengerTruth.Alive,
            "Pan mnie zna. Wpuścił mnie pan? Nie pamiętam już.",
            "You know me. Did you let me in? I no longer remember.")
            .Alt(Flag.Denied("Marek"), PassengerTruth.Dead, "Zeszłej nocy zostałem na peronie. Było mi zimno. Teraz już nie jest.", "Last night I stayed on the platform. I was cold. Now I am not.")
            .Q("marek2a", "Co pan wie o Rejestrze?", "What do you know about the Registry?",
               "Że są dwie księgi. Jedna dla Dyrekcji i jedna dla prawdy.", "That there are two books. One for the Directorate and one for the truth.", QA.Truth, null, "loc_registryhall", 0);
        S("marek_3", "marek", PassengerTruth.Alive,
            "Dziś przyniosłem tylko jeden zegarek. Pokazuje właściwą godzinę.",
            "Tonight I have brought only one watch. It shows the right time.")
            .Alt(Flag.Denied("Marek"), PassengerTruth.Dead, "Wszystkie moje zegarki stanęły w tej samej chwili. Wie pan, w której.", "All my watches stopped at the same moment. You know which.")
            .Q("marek3a", "Która jest godzina?", "What time is it?",
               "Za późno, konduktorze. Ale jeszcze nie za późno dla pana.", "Too late, conductor. But not yet too late for you.", QA.Half, null, null, 2);
        S("marek_4", "marek", PassengerTruth.Alive,
            "Przyszedłem się pożegnać. Zostało mi niewiele czasu do naprawy.",
            "I came to say goodbye. Not much time is left for repairs.")
            .Alt(Flag.Denied("Marek"), PassengerTruth.Dead, "Nie wyszedłem stąd. Nikt nie wychodzi.", "I did not leave. No one does.")
            .Q("marek4a", "Co pan zabiera ze sobą?", "What are you taking with you?",
               "Pamiątki. I pańskie imię, bo je pan zgubił.", "Mementos. And your name, because you lost it.", QA.Truth, null, "ppl_conductor", 2);
        S("marek_5", "marek", PassengerTruth.Alive,
            "Ostatni raz, konduktorze. Potem odwiedzę pana po tej stronie peronu.",
            "One last time, conductor. Then I will visit you on this side of the platform.")
            .Alt(Flag.Denied("Marek"), PassengerTruth.Dead, "Znów ja. Ostatnia noc, więc nie będę pukał.", "Me again. It is the last night, so I will not knock.");

        // ---------------- LENA ----------------
        S("lena_1", "lena", PassengerTruth.NonExistent,
            "Dzień dobry. Szukam mamy. Ona jest w pociągu, prawda?",
            "Hello. I'm looking for my mum. She is on the train, isn't she?")
            .Q("lena1a", "Jak masz na imię?", "What is your name?",
               "Lena. Wolska. Tak jak mama.", "Lena. Wolska. Just like Mum.", QA.Truth, null, "ppl_lena", 0)
            .Q("lena1b", "Gdzie jest mama?", "Where is Mum?",
               "W wagonie, do którego nikt nie może wejść. Ona puka, ale nikt nie otwiera.", "In the car where no one can go. She knocks, but no one opens.", QA.Half, null, "loc_car13", 2)
            .sympathetic = true;
        scripts["lena_1"].pleaPl = "Proszę. Ona mówiła, że zaraz przyjdzie.";
        scripts["lena_1"].pleaEn = "Please. She said she would come soon.";
        S("lena_2", "lena", PassengerTruth.Echo,
            "Mama znów puka. Słyszy pan?",
            "Mum is knocking again. Can you hear?")
            .Q("lena2a", "Kto puka?", "Who is knocking?",
               "Wszyscy, których pan nie wpuścił.", "Everyone you did not let in.", QA.Half, Flag.ListenedToKnocking, "evt_knock", 6);
        scripts["lena_2"].sympathetic = true; scripts["lena_2"].pleaPl = "Niech pan otworzy. Ona jest zmęczona."; scripts["lena_2"].pleaEn = "Please open it. She is tired.";
        S("lena_3", "lena", PassengerTruth.Echo,
            "Ty też jesteś jak ja? Zawsze na peronie?",
            "Are you like me too? Always on the platform?")
            .Q("lena3a", "Co masz na myśli?", "What do you mean?",
               "Nikt cię nie pamięta. Tylko ten pociąg.", "No one remembers you. Only this train.", QA.Half, null, null, 5);
        scripts["lena_3"].sympathetic = true; scripts["lena_3"].pleaPl = "Weź mnie ze sobą."; scripts["lena_3"].pleaEn = "Take me with you.";
        S("lena_4", "lena", PassengerTruth.Echo,
            "Pozwolisz mi wsiąść? Mama mówi, że to twoja decyzja.",
            "Will you let me board? Mum says it's your decision.")
            .Q("lena4a", "Kto ci to powiedział?", "Who told you that?",
               "Pan w szarym płaszczu. Mówi, że pierwszy raz wybrał źle.", "The man in the grey coat. He says the first time he chose wrong.", QA.Truth, null, "ppl_grey", 3);
        scripts["lena_4"].sympathetic = true; scripts["lena_4"].pleaPl = "Nie zostawiaj mnie tu."; scripts["lena_4"].pleaEn = "Don't leave me here.";
        S("lena_5", "lena", PassengerTruth.Echo,
            "Wpuścisz mnie? Mama czeka w wagonie.",
            "Will you let me in? Mum is waiting in the car.");
        scripts["lena_5"].sympathetic = true; scripts["lena_5"].pleaPl = "Już nigdy o nic nie poproszę."; scripts["lena_5"].pleaEn = "I will never ask for anything again.";

        // ---------------- KRUK ----------------
        S("kruk_1", "kruk", PassengerTruth.Unknown,
            "Inspektor Kruk, Dyrekcja Kolei. Nie muszę okazywać dokumentów, ale okażę.",
            "Inspector Kruk, Railway Directorate. I do not have to show documents, but I will.")
            .Q("kruk1a", "Co pan tu kontroluje?", "What are you inspecting here?",
               "Pana. I peron. W kolejności, która pana zaskoczy.", "You. And the platform. In an order that will surprise you.", QA.Evasive, Flag.MetInspector, "ppl_kruk", 2)
            .Q("kruk1b", "Dlaczego nie ma pana w bazie?", "Why are you not in the database?",
               "Bo baza jest pana narzędziem, nie moim.", "Because the database is your tool, not mine.", QA.Evasive, null, null, 0)
            .orderState = 1;
        S("kruk_2", "kruk", PassengerTruth.Unknown,
            "Kontrola. Proszę mnie natychmiast wpuścić, mam rozkaz.",
            "Inspection. Let me in at once, I have an order.")
            .Q("kruk2a", "Może pan pokazać rozkaz?", "May I see the order?",
               "Oczywiście. Wszystko jest w porządku. Proszę tylko nie patrzeć na podpis.", "Of course. Everything is in order. Just do not look at the signature.", QA.Lie, null, null, 3)
            .orderState = 2;
        S("kruk_3", "kruk", PassengerTruth.Unknown,
            "Zna pan już regulamin. Czas poznać jego autora.",
            "You already know the rulebook. It is time to meet its author.")
            .Q("kruk3a", "Kto napisał regulamin?", "Who wrote the rulebook?",
               "Konduktor. Pierwszy. Ten, który nie wsiadł.", "The conductor. The first. The one who did not board.", QA.Truth, null, "unk_first", 4)
            .orderState = 1;

        // ---------------- PIOTR ----------------
        S("piotr_1", "piotr", PassengerTruth.Alive,
            "Wieczór, szefie. Naprawa toru na końcu peronu. Papiery w porządku.",
            "Evening, boss. Repairing the track at the end of the platform. Papers are in order.")
            .Q("piotr1a", "Czy pociąg jest sprawny?", "Is the train in working order?",
               "Pociąg jest sprawny. To ludzie się psują.", "The train is in working order. It is people who break.", QA.Truth, null, "ppl_piotr", 0)
            .Q("piotr1b", "Co pan naprawia nocą?", "What do you repair at night?",
               "Zamki. Ostatnio jeden bardzo uparty. Wagon trzynasty.", "Locks. Lately a very stubborn one. Car thirteen.", QA.Truth, null, "loc_car13", 1);
        S("piotr_2", "piotr", PassengerTruth.Alive,
            "Szefie, mam coś dla ciebie. Ale nie tu. Kolej ma uszy.",
            "Boss, I have something for you. But not here. The railway has ears.")
            .Q("piotr2a", "Co pan ma?", "What do you have?",
               "Klucz. Numer trzynaście. Od zamka, o którym nie powinienem wiedzieć.", "A key. Number thirteen. To a lock I should not know about.", QA.Truth, Flag.LearnedLine13, "lore_line13", 2);
        S("piotr_3", "piotr", PassengerTruth.Alive,
            "Jeśli mnie dziś wpuścisz, powiem ci, gdzie leży druga księga.",
            "If you let me in tonight, I will tell you where the second book lies.")
            .Q("piotr3a", "Gdzie jest druga księga?", "Where is the second book?",
               "W wagonie trzynastym, pod siedzeniem trzynastym. Zawsze tam była.", "In car thirteen, under seat thirteen. It has always been there.", QA.Truth, null, "unk_hidden", 2);

        // ---------------- NO DOCUMENTS ----------------
        S("nodocs_1", "nodocs", PassengerTruth.Alive,
            "Nie mam nic. Ani biletu, ani nazwiska. Ale jestem. Proszę powiedzieć, że jestem.",
            "I have nothing. No ticket, no name. But I am here. Please tell me that I am.")
            .Q("nodocs1a", "Skąd pan jest?", "Where are you from?",
               "Stąd. Zawsze byłem stąd. Tylko papier o mnie zapomniał.", "From here. I have always been from here. Only the paper forgot about me.", QA.Truth, null, "ppl_nodocs", 0)
            .Q("nodocs1b", "Jak się pan nazywa?", "What is your name?",
               "Ludzie wołają na mnie \"Nikt\". Chyba lepsze to niż nic.", "People call me \"Nobody\". I suppose that is better than nothing.", QA.Truth, null, null, 0)
            .noTicket = true;
        scripts["nodocs_1"].sympathetic = true; scripts["nodocs_1"].pleaPl = "Niech mnie ktoś zobaczy. Chociaż raz."; scripts["nodocs_1"].pleaEn = "Let someone see me. Just once.";
        S("nodocs_2", "nodocs", PassengerTruth.Alive,
            "Znowu ja. Wciąż Nikt. Ale dziś wiem, że pociąg mnie słyszy.",
            "Me again. Still Nobody. But tonight I know the train hears me.")
            .Q("nodocs2a", "Co pan słyszy?", "What do you hear?",
               "Pukanie. Ono jest do mnie.", "The knocking. It is meant for me.", QA.Half, null, "evt_knock", 3)
            .noTicket = true;
        scripts["nodocs_2"].sympathetic = true; scripts["nodocs_2"].pleaPl = "Jeden raz. Do stacji 13."; scripts["nodocs_2"].pleaEn = "Just once. To station 13.";
        S("nodocs_3", "nodocs", PassengerTruth.Alive,
            "To już ostatnie noce, prawda?",
            "These are the last nights, aren't they?")
            .Q("nodocs3a", "Czego pan chce?", "What do you want?",
               "Nazwiska. Choćby cudzego. Choćby pańskiego.", "A name. Even someone else's. Even yours.", QA.Truth, null, null, 4)
            .noTicket = true;
        scripts["nodocs_3"].sympathetic = true; scripts["nodocs_3"].pleaPl = "Już nic więcej."; scripts["nodocs_3"].pleaEn = "Nothing more.";

        // ---------------- THE MAN IN GREY ----------------
        S("grey_1", "grey", PassengerTruth.Unknown,
            "Dobry wieczór, konduktorze.",
            "Good evening, conductor.")
            .Q("grey1a", "Skąd pan zna moje imię?", "How do you know my name?",
               "Ktoś je do mnie powtarza. Co noc, od bardzo dawna.", "Someone repeats it to me. Every night, for a very long time.", QA.Evasive, Flag.MetTheConductor, "ppl_grey", 4)
            .nameHidden = true;
        S("grey_2", "grey", PassengerTruth.Unknown,
            "Zapyta pan, czy jestem żywy. To niewłaściwe pytanie.",
            "You will ask whether I am alive. That is the wrong question.")
            .Q("grey2a", "Kim pan jest?", "Who are you?",
               "Wcześniej stałem tam, gdzie pan teraz.", "Earlier I stood where you stand now.", QA.Half, null, "unk_first", 4)
            .nameHidden = true;
        S("grey_3", "grey", PassengerTruth.Unknown,
            "Proszę spojrzeć na dowód. To pańskie nazwisko. Także moje.",
            "Please look at the ID. That is your name. Mine too.")
            .Q("grey3a", "Który z nas jest prawdziwy?", "Which of us is real?",
               "Obaj. Albo żaden. Rejestr nie odróżnia.", "Both. Or neither. The Registry cannot tell.", QA.Truth, Flag.ReadConductorFile, "unk_first", 5)
            .useConductorName = true;
        S("grey_4", "grey", PassengerTruth.Unknown,
            "Dziś kończę swoją zmianę. Pan zaczyna swoją. Albo odwrotnie.",
            "Tonight I finish my shift. You start yours. Or the other way round.")
            .Q("grey4a", "Co mam zrobić?", "What should I do?",
               "To, czego ja nie zrobiłem. Cokolwiek to jest.", "What I did not do. Whatever that is.", QA.Truth, null, "unk_first", 3)
            .useConductorName = true;
    }
}
