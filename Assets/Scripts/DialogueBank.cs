using System.Collections.Generic;
using UnityEngine;

// Builds the questions and prepared answers for a passenger. The answers depend on the hidden truth,
// on what the documents show, and on story scripts. Every line exists in Polish, English and Ukrainian; the passenger's gender is applied in all three.
public static class DialogueBank
{
    static string G(PassengerData p, string male, string female) { return p.female ? female : male; }

    static QA Q(string id, int priority, string qPl, string qEn, string qUk, string aPl, string aEn, string aUk, int kind, bool contextual = false, int stress = 0, string anomaly = null, string codex = null, string flag = null, int minAuthority = 0)
    {
        return new QA
        {
            id = id, priority = priority, question = Loc.T(qPl, qEn, qUk), answer = Loc.T(aPl, aEn, aUk),
            kind = kind, contextual = contextual, stress = stress, anomaly = anomaly, codex = codex, flag = flag, minAuthority = minAuthority
        };
    }

    // ---------------------------------------------------------------------------------------------

    public static List<QA> Build(PassengerData p, DayConfig day, ScriptDef script)
    {
        var list = new List<QA>();
        PassengerTruth t = p.truth;
        string bd = p.birthDate;

        // ---- the four base questions ----
        list.Add(Where(p));
        list.Add(Dob(p, bd));
        list.Add(Waiting(p));
        list.Add(TicketQ(p));

        // ---- more questions ----
        list.Add(Job(p));
        list.Add(Story(p));
        list.Add(Previous(p));
        list.Add(Secret(p));
        if (p.behaviour == "nervous" || p.behaviour == "angry" || p.behaviour == "scared") list.Add(Fear(p));
        if (day.Has(DocumentType.Medical)) list.Add(Ill(p));

        // ---- questions that follow from what the documents show ----
        if (!p.destinationRegistered) list.Add(Home(p));
        if (t == PassengerTruth.Impostor || p.playerPhoto) list.Add(Photo(p));
        if (t == PassengerTruth.Dead && day.Has(DocumentType.DeathRecord)) list.Add(DeathQ(p));
        if (t == PassengerTruth.NonExistent || t == PassengerTruth.Echo || t == PassengerTruth.Unknown) list.Add(Mother(p));
        if (p.orderState > 0 && day.Has(DocumentType.SpecialOrders)) list.Add(OrderQ(p));
        if (t == PassengerTruth.Loop) list.Add(Future(p));
        if (t == PassengerTruth.Altered) list.Add(Amend(p));
        if (day.act >= 3 || day.level >= 4) { list.Add(ConductorQ(p)); list.Add(Memory(p)); }
        if (p.behaviour != "calm" || p.aggressive) list.Add(Command(p));

        // ---- the story character's own questions come first ----
        if (script != null)
        {
            for (int i = 0; i < script.qas.Count; i++)
            {
                ScriptQA s = script.qas[i];
                list.Add(new QA
                {
                    id = s.id, priority = 120 - i, contextual = true,
                    question = Loc.T(s.qPl, s.qEn), answer = Loc.T(s.aPl, s.aEn), kind = s.kind, stress = s.stress, flag = s.flag, codex = s.codex
                });
            }
        }

        list.Sort((a, b) => b.priority.CompareTo(a.priority));
        return list;
    }

    // The conductor as a passenger (the twist).
    public static List<QA> BuildPlayer()
    {
        var l = new List<QA>();
        l.Add(Q("pl_where", 50, "Skąd i dokąd podróż?", "Where from and where to?", "Звідки й куди прямуєте?",
            "Znikąd. Od zawsze stoję na tym peronie. A dokąd? Tego nie wiesz, prawda?", "From nowhere. I have always stood on this platform. And where to? You do not know, do you?", "Нізвідки. Я стою на цьому пероні відвіку. А куди? Цього ти не знаєш, правда?", QA.Truth));
        l.Add(Q("pl_dob", 48, "Proszę podać datę urodzenia.", "Please state your date of birth.", "Назвіть, будь ласка, дату народження.",
            "Nie pamiętam. Tak długo tu jesteś, że nawet ty nie pamiętasz.", "I do not remember. You have been here so long that even you do not remember.", "Не пам’ятаю. Ти тут так давно, що навіть ти не пам’ятаєш.", QA.Truth));
        l.Add(Q("pl_wait", 46, "Czy ktoś czeka na stacji?", "Is anyone waiting at the station?", "Чи хтось чекає на станції?",
            "Nikt. Tylko ten pociąg. Czeka na mnie od bardzo dawna.", "No one. Only this train. It has been waiting for me for a very long time.", "Ніхто. Лише цей поїзд. Він чекає на мене дуже давно.", QA.Truth));
        l.Add(Q("pl_ticket", 44, "Czy bilet jest na Pana nazwisko?", "Is the ticket in your name?", "Квиток виписано на ваше ім’я?",
            "Bilet jest mój. Wypisałeś go sam, tyle nocy temu...", "The ticket is mine. You wrote it yourself, so many nights ago...", "Квиток мій. Ти сам його виписав, стільки ночей тому…", QA.Truth));
        l.Add(Q("pl_who", 40, "Kim jesteś?", "Who are you?", "Хто ти?",
            "Jestem tym, kim ty byłeś, zanim zacząłeś tylko sprawdzać papiery.", "I am who you were before you started only checking papers.", "Я — той, ким ти був, поки не почав лише перевіряти папери.", QA.Truth, true, 3));
        l.Add(Q("pl_last", 38, "Ilu pasażerów odesłałeś?", "How many passengers did you turn away?", "Скількох пасажирів ти відіслав?",
            "Wszystkich, którym nie wolno było. Każdy z nich stoi teraz za mną.", "Everyone who was not allowed. Every one of them stands behind me now.", "Усіх, кому не можна було. Кожен із них тепер стоїть за мною.", QA.Truth, true, 3));
        l.Add(Q("pl_board", 36, "Czy chcesz wsiąść?", "Do you want to board?", "Ти хочеш сісти?",
            "Nie o to chodzi, czego ja chcę. Chodzi o to, czego chcesz ty.", "It is not about what I want. It is about what you want.", "Справа не в тому, чого хочу я. Справа в тому, чого хочеш ти.", QA.Truth, true, 2, null, "unk_first"));
        return l;
    }

    // ---------------------------------------------------------------------------------------------
    // Greetings

    public static string MakeGreeting(PassengerData p, ScriptDef script, string altPl, string altEn)
    {
        string s;
        if (p.isPlayer)
        {
            return Loc.T(
                "Przed okienkiem stoi ktoś w mundurze konduktora. Ma twoją twarz i twoje zmęczenie. \"Wpuścisz mnie?\" - pyta twoim głosem.",
                "In front of the window stands someone in a conductor's uniform. They have your face and your tiredness. \"Will you let me in?\" they ask in your voice.",
                "Перед віконцем стоїть хтось у формі провідника. У нього твоє обличчя й твоя втома. «Пропустиш мене?» — питає він твоїм голосом.");
        }
        if (script != null)
        {
            s = (altPl != null && altEn != null) ? Loc.T(altPl, altEn) : Loc.T(script.greetPl, script.greetEn);
        }
        else
        {
            switch (p.truth)
            {
                case PassengerTruth.Dead:
                    s = Loc.T("Dobry wieczór... Chciał" + G(p, "bym", "abym") + " wsiąść. Jest tu jakoś zimno, prawda?",
                              "Good evening... I would like to board. It is rather cold here, isn't it?",
                              "Добрий вечір… Я " + G(p, "хотів", "хотіла") + " б сісти. Тут якось холодно, правда?");
                    break;
                case PassengerTruth.NonExistent:
                    s = Loc.T("Dobry wieczór. To jest ten pociąg? Powiedziano mi, że tu mam wsiąść.", "Good evening. Is this the train? I was told to board here.",
                              "Добрий вечір. Це той поїзд? Мені сказали, що сідати треба тут.");
                    break;
                case PassengerTruth.Loop:
                    s = Loc.T("Dobry wieczór. Znowu... to znaczy po raz pierwszy. Proszę o bilet na dzisiaj i jutro.", "Good evening. Again... I mean for the first time. One ticket for today and tomorrow, please.",
                              "Добрий вечір. Знову… тобто вперше. Прошу квиток на сьогодні й на завтра.");
                    break;
                case PassengerTruth.Impostor:
                    s = Loc.T("Dobry wieczór! Śpieszę się. Wszystko mam w porządku, proszę spojrzeć.", "Good evening! I'm in a hurry. Everything is in order, please look.",
                              "Добрий вечір! Я поспішаю. У мене все гаразд, гляньте.");
                    break;
                case PassengerTruth.Echo:
                    s = Loc.T("Dobry wieczór. Ktoś mi kiedyś powiedział, że tu jest wejście.", "Good evening. Someone once told me that this is the entrance.",
                              "Добрий вечір. Хтось колись сказав мені, що тут вхід.");
                    break;
                case PassengerTruth.Unknown:
                    s = Loc.T("Dobry wieczór, konduktorze. Pan wie, po co tu jestem.", "Good evening, conductor. You know why I am here.",
                              "Добрий вечір, провіднику. Ви знаєте, навіщо я тут.");
                    break;
                case PassengerTruth.Altered:
                    s = Loc.T("Dobry wieczór. Jadę do stacji " + p.station + ". Coś nie tak z dokumentami?", "Good evening. I'm going to station " + p.station + ". Something wrong with the documents?",
                              "Добрий вечір. Їду до станції " + p.station + ". Щось не так із документами?");
                    break;
                default:
                    s = Loc.T("Dobry wieczór. Jadę do stacji " + p.station + ".", "Good evening. I'm going to station " + p.station + ".",
                              "Добрий вечір. Їду до станції " + p.station + ".");
                    break;
            }
        }
        if (p.sympathetic && !string.IsNullOrEmpty(p.plea)) s += "\n<i>" + p.plea + "</i>";
        return s;
    }

    // A last line when the passenger walks away or boards.
    public static string Farewell(PassengerData p, bool admitted)
    {
        if (p.isPlayer) return admitted ? Loc.T("Dziękuję. Od dawna nikt nie mówił mi \"proszę wsiadać\".", "Thank you. No one has said \"please board\" to me in a long time.", "Дякую. Давно ніхто не казав мені «прошу сідати».")
                                        : Loc.T("Rozumiem. Ktoś musi zostać.", "I understand. Someone has to stay.", "Розумію. Хтось має залишитися.");
        if (admitted)
        {
            switch (p.behaviour)
            {
                case "angry": return Loc.T("No nareszcie.", "Finally.", "Нарешті.");
                case "cold": return Loc.T("...", "...", "…");
                case "scared": return Loc.T("Dziękuję. Proszę tylko nie zamykać drzwi.", "Thank you. Just don't close the doors.", "Дякую. Тільки не зачиняйте дверей.");
                default: return Loc.T("Dziękuję. Dobrej nocy.", "Thank you. Good night.", "Дякую. На добраніч.");
            }
        }
        switch (p.behaviour)
        {
            case "angry": return Loc.T("To skandal! Zobaczy pan, jak to się skończy.", "This is outrageous! You will see how this ends.", "Це скандал! Ось побачите, чим це закінчиться.");
            case "cold": return Loc.T("Wrócę.", "I will be back.", "Я повернуся.");
            case "scared": return Loc.T("Proszę... nie zostawiajcie mnie tu.", "Please... do not leave me here.", "Прошу… не залишайте мене тут.");
            case "nervous": return Loc.T("Rozumiem. Chyba.", "I understand. I think.", "Розумію. Здається.");
            default: return Loc.T("Trudno. Dobranoc.", "Never mind. Good night.", "Що ж. На добраніч.");
        }
    }

    // ---------------------------------------------------------------------------------------------
    // Individual questions.  Polish / English / Ukrainian; G() picks the passenger's gender.

    static string Sk(PassengerData p, string m, string f) { return p.female ? f : m; }

    static QA Where(PassengerData p)
    {
        const string qp = "Skąd i dokąd podróż?", qe = "Where from and where to?", qu = "Звідки й куди прямуєте?";
        switch (p.truth)
        {
            case PassengerTruth.Dead:
                return Q("q_where", 50, qp, qe, qu, "Z daleka. Bardzo daleka. A dokąd... tam, gdzie jadą wszyscy.", "From far away. Very far. And where to... where everyone goes.", "Здалеку. Дуже далеко. А куди… туди, куди їдуть усі.", QA.Evasive);
            case PassengerTruth.NonExistent:
                return Q("q_where", 50, qp, qe, qu, "Nie pamiętam dokładnie. Wiem tylko, że muszę tu wsiąść.", "I don't remember exactly. I only know I have to board here.", "Не пам’ятаю точно. Знаю лише, що сісти маю тут.", QA.Half);
            case PassengerTruth.Echo:
                return Q("q_where", 50, qp, qe, qu, "Stąd. Stamtąd. Skądś, gdzie ktoś o mnie myśli.", "From here. From there. From somewhere someone is thinking of me.", "Звідси. Звідти. Звідкись, де хтось про мене думає.", QA.Evasive);
            case PassengerTruth.Unknown:
                return Q("q_where", 50, qp, qe, qu, "Z miejsca, którego jeszcze nie ma. Do miejsca, które już było.", "From a place that does not exist yet. To a place that already was.", "З місця, якого ще немає. До місця, що вже було.", QA.Evasive, false, 2);
            case PassengerTruth.Impostor:
                return Q("q_where", 50, qp, qe, qu, "Z miasta " + p.origin + ". Wszystko jest w dokumentach, proszę patrzeć.", "From " + p.origin + ". It is all in the documents, please look.", "З міста " + p.origin + ". Усе є в документах, дивіться.", QA.Half);
            default:
                return Q("q_where", 50, qp, qe, qu, "Z miasta " + p.origin + ", do stacji " + p.station + ". Wracam do domu.", "From " + p.origin + ", to station " + p.station + ". I'm going home.", "З міста " + p.origin + ", до станції " + p.station + ". Повертаюся додому.", QA.Truth);
        }
    }

    static QA Dob(PassengerData p, string bd)
    {
        const string qp = "Proszę podać datę urodzenia.", qe = "Please state your date of birth.", qu = "Назвіть, будь ласка, дату народження.";
        switch (p.truth)
        {
            case PassengerTruth.Dead:
                return Q("q_dob", 48, qp, qe, qu, bd + ". <i>(mówi to bardzo spokojnie)</i> Choć czasem wydaje mi się, że to było bardzo dawno.", bd + ". <i>(says it very calmly)</i> Though sometimes it feels like it was very long ago.", bd + ". <i>(каже це дуже спокійно)</i> Хоча іноді здається, що це було дуже давно.", QA.Half);
            case PassengerTruth.NonExistent:
                return Q("q_dob", 48, qp, qe, qu, "Nie... chwileczkę. Tak mi powiedziano: " + bd + ".", "No... wait. That is what I was told: " + bd + ".", "Ні… зачекайте. Мені так сказали: " + bd + ".", QA.Half);
            case PassengerTruth.Loop:
                return Q("q_dob", 48, qp, qe, qu, G(p, "Urodziłem", "Urodziłam") + " się... jutro? Przepraszam, mylą mi się kolejności.", "I was born... tomorrow? Sorry, I mix up the order.", "Я " + Sk(p, "народився", "народилася") + "… завтра? Перепрошую, я плутаю послідовність.", QA.Half, false, 1);
            case PassengerTruth.Impostor:
                return Q("q_dob", 48, qp, qe, qu, bd + ". <i>(powiedziane jak wyuczona modlitwa)</i>", bd + ". <i>(said like a memorised prayer)</i>", bd + ". <i>(сказано, як завчену молитву)</i>", QA.Lie);
            case PassengerTruth.Echo:
                return Q("q_dob", 48, qp, qe, qu, "Data? Ktoś ją kiedyś wypowiedział. Nie pamiętam kto.", "A date? Someone once said it. I don't remember who.", "Дата? Хтось колись її вимовив. Не пам’ятаю, хто.", QA.Evasive);
            case PassengerTruth.Unknown:
                return Q("q_dob", 48, qp, qe, qu, "Nie mam daty. Mam tylko peron.", "I have no date. I only have the platform.", "У мене немає дати. У мене є лише перон.", QA.Evasive, false, 2);
            case PassengerTruth.Altered:
                return Q("q_dob", 48, qp, qe, qu, bd + ". A może... Ktoś mi kiedyś mówił, że inaczej.", bd + ". Or maybe... Someone once told me otherwise.", bd + ". А може… Хтось колись казав мені, що інакше.", QA.Half);
            default:
                return Q("q_dob", 48, qp, qe, qu, bd + ". Mam to nawet wypisane w dowodzie.", bd + ". It is even written on my ID.", bd + ". Це навіть записано в моєму посвідченні.", QA.Truth);
        }
    }

    static QA Waiting(PassengerData p)
    {
        const string qp = "Czy ktoś czeka na stacji?", qe = "Is anyone waiting at the station?", qu = "Чи хтось чекає на вас на станції?";
        switch (p.truth)
        {
            case PassengerTruth.Dead:
                return Q("q_wait", 46, qp, qe, qu, "Nikt. Już nikt. Ręce mam zimne, prawda? Od dawna.", "No one. Not anymore. My hands are cold, aren't they? For a long time.", "Ніхто. Уже ніхто. У мене холодні руки, правда? Давно.", QA.Truth, false, 1);
            case PassengerTruth.NonExistent:
                return Q("q_wait", 46, qp, qe, qu, "Nie mam rodziców. Nigdy ich nie poznał" + G(p, "em", "am") + ".", "I have no parents. I never knew them.", "У мене немає батьків. Я ніколи їх не " + Sk(p, "знав", "знала") + ".", QA.Half);
            case PassengerTruth.Loop:
                return Q("q_wait", 46, qp, qe, qu, "Będą czekać. Albo już czekają.", "They will be waiting. Or already are.", "Чекатимуть. Або вже чекають.", QA.Half);
            case PassengerTruth.Impostor:
                return Q("q_wait", 46, qp, qe, qu, G(p, "Tak, żona.", "Tak, mąż.") + " <i>(odwraca wzrok)</i>", (p.female ? "Yes, my husband." : "Yes, my wife.") + " <i>(looks away)</i>", Sk(p, "Так, дружина.", "Так, чоловік.") + " <i>(відводить погляд)</i>", QA.Lie);
            case PassengerTruth.Echo:
                return Q("q_wait", 46, qp, qe, qu, "Ktoś o mnie pamięta. To wystarcza, żebym trwał" + G(p, "", "a") + ".", "Someone remembers me. That is enough for me to last.", "Хтось про мене пам’ятає. Цього досить, щоб я " + Sk(p, "тривав", "тривала") + ".", QA.Half);
            case PassengerTruth.Unknown:
                return Q("q_wait", 46, qp, qe, qu, "Pan. Od dawna.", "You. For a long time.", "Ви. Давно.", QA.Evasive, false, 3);
            case PassengerTruth.Altered:
                return Q("q_wait", 46, qp, qe, qu, "Tak. Chociaż siostra twierdzi, że to nie " + "ja wracam" + ".", "Yes. Though my sister claims it isn't me coming back.", "Так. Хоча сестра стверджує, що це повертаюся не я.", QA.Truth);
            default:
                return Q("q_wait", 46, qp, qe, qu, "Tak, rodzina. Czekają z kolacją.", "Yes, my family. Dinner is waiting.", "Так, родина. Чекають із вечерею.", QA.Truth);
        }
    }

    static QA TicketQ(PassengerData p)
    {
        string qp = "Czy bilet jest na " + G(p, "Pana", "Pani") + " nazwisko?", qe = "Is the ticket in your name?", qu = "Квиток виписано на ваше ім’я?";
        if (p.noTicket)
            return Q("q_ticket", 44, qp, qe, qu, "Nie mam biletu. Nie mam też nazwiska, które by do niego pasowało.", "I have no ticket. I have no name that would fit it either.", "У мене немає квитка. Немає й імені, яке б до нього пасувало.", QA.Truth);
        if (p.forgedTicket)
            return Q("q_ticket", 44, qp, qe, qu, "Ee... tak. To znaczy, pożyczył" + G(p, "em", "am") + " od znajomego. Ale to prawie to samo.", "Uh... yes. I mean, I borrowed it from a friend. But it's practically the same.", "Е-е… так. Тобто я " + Sk(p, "позичив", "позичила") + " його в знайомого. Але це майже те саме.", QA.Lie);
        switch (p.truth)
        {
            case PassengerTruth.Dead:
                return Q("q_ticket", 44, qp, qe, qu, "Tak. Kupił" + G(p, "em", "am") + " go dawno temu. Przed wszystkim.", "Yes. I bought it long ago. Before everything.", "Так. Я " + Sk(p, "купив", "купила") + " його давно. Ще до всього.", QA.Half);
            case PassengerTruth.NonExistent:
                return Q("q_ticket", 44, qp, qe, qu, "Tak, chyba tak. Ktoś mi go dał.", "Yes, I think so. Someone gave it to me.", "Так, здається, так. Мені його хтось дав.", QA.Half);
            case PassengerTruth.Impostor:
                return Q("q_ticket", 44, qp, qe, qu, "Oczywiście. Proszę sprawdzić. Szybko.", "Of course. Please check. Quickly.", "Звісно. Перевіряйте. Швидше.", QA.Lie);
            case PassengerTruth.Unknown:
                return Q("q_ticket", 44, qp, qe, qu, "Bilet jest tylko formalnością.", "The ticket is only a formality.", "Квиток — лише формальність.", QA.Evasive);
            default:
                return Q("q_ticket", 44, qp, qe, qu, "Oczywiście. Proszę sprawdzić.", "Of course. Please check.", "Звісно. Перевіряйте.", QA.Truth);
        }
    }

    static QA Job(PassengerData p)
    {
        string qp = "Czym " + G(p, "Pan", "Pani") + " się zajmuje?", qe = "What do you do for a living?", qu = "Чим ви займаєтеся?";
        switch (p.truth)
        {
            case PassengerTruth.Dead:
                return Q("q_job", 30, qp, qe, qu, G(p, "Byłem", "Byłam") + "... jestem: " + p.occupation + ". Trudno powiedzieć, w jakim czasie.", "I was... I am: " + p.occupation + ". Hard to say in which time.", Sk(p, "Був", "Була") + "… є: " + p.occupation + ". Важко сказати, у якому часі.", QA.Half);
            case PassengerTruth.NonExistent:
                return Q("q_job", 30, qp, qe, qu, "Chyba " + p.occupation + ". Tak mi się wydaje.", "I think " + p.occupation + ". So it seems to me.", "Здається, " + p.occupation + ". Принаймні мені так здається.", QA.Half);
            case PassengerTruth.Impostor:
                return Q("q_job", 30, qp, qe, qu, Cap(p.occupation) + ". Zawsze " + G(p, "byłem", "byłam") + ". <i>(zbyt szybko)</i>", p.occupation + ". Always was. <i>(too quickly)</i>", Cap(p.occupation) + ". Завжди " + Sk(p, "був", "була") + ". <i>(надто швидко)</i>", QA.Lie);
            case PassengerTruth.Echo:
                return Q("q_job", 30, qp, qe, qu, "Pracuję w pamięci ludzi.", "I work in people's memories.", "Працюю в людській пам’яті.", QA.Evasive);
            case PassengerTruth.Unknown:
                return Q("q_job", 30, qp, qe, qu, "Pracuję tu. Tak jak pan.", "I work here. Like you.", "Працюю тут. Як і ви.", QA.Evasive, false, 2);
            default:
                return Q("q_job", 30, qp, qe, qu, Cap(p.occupation) + ".", Cap(p.occupation) + ".", Cap(p.occupation) + ".", QA.Truth);
        }
    }

    static QA Story(PassengerData p)
    {
        int kind = p.truth == PassengerTruth.Alive || p.truth == PassengerTruth.Altered ? QA.Truth : QA.Half;
        return Q("q_story", 28, "Proszę opowiedzieć o sobie.", "Tell me about yourself.", "Розкажіть про себе, будь ласка.", p.backstory, p.backstory, p.backstory, kind);
    }

    static QA Previous(PassengerData p)
    {
        string qp = p.female ? "Czy jechała już Pani Linią 13?" : "Czy jechał już Pan Linią 13?", qe = "Have you travelled on Line 13 before?", qu = "Ви вже їздили Лінією 13?";
        switch (p.truth)
        {
            case PassengerTruth.Dead:
                return Q("q_prev", 26, qp, qe, qu, "Tak. Wiele razy. Ale zawsze ktoś mnie odsyła.", "Yes. Many times. But someone always sends me back.", "Так. Багато разів. Але мене завжди хтось відсилає.", QA.Half);
            case PassengerTruth.NonExistent:
                return Q("q_prev", 26, qp, qe, qu, "Chyba tak. Ciągle mam wrażenie, że tu " + G(p, "byłem", "byłam") + ".", "I think so. I keep feeling I have been here.", "Здається, так. Мене не полишає відчуття, що я тут уже " + Sk(p, "був", "була") + ".", QA.Half);
            case PassengerTruth.Loop:
                return Q("q_prev", 26, qp, qe, qu, "Będę jutro. Ale pamiętam już wczoraj.", "I will be tomorrow. But I already remember yesterday.", "Я буду завтра. Але вже пам’ятаю вчора.", QA.Half);
            case PassengerTruth.Impostor:
                return Q("q_prev", 26, qp, qe, qu, "Nigdy w życiu.", "Never in my life.", "Ніколи в житті.", QA.Lie);
            case PassengerTruth.Echo:
                return Q("q_prev", 26, qp, qe, qu, "Ktoś tu był. Ja jestem " + G(p, "jego", "jej") + " wspomnieniem.", "Someone was here. I am their memory.", "Тут хтось був. Я — " + Sk(p, "його", "її") + " спогад.", QA.Half);
            case PassengerTruth.Unknown:
                return Q("q_prev", 26, qp, qe, qu, "Zawsze.", "Always.", "Завжди.", QA.Evasive, false, 2);
            default:
            {
                int n = Random.Range(1, 4);
                return Q("q_prev", 26, qp, qe, qu,
                    "To dopiero mój " + (n == 1 ? "pierwszy" : (n == 2 ? "drugi" : "trzeci")) + " raz.",
                    "This is only my " + Ordinal(n) + " time.",
                    "Це лише мій " + (n == 1 ? "перший" : (n == 2 ? "другий" : "третій")) + " раз.", QA.Truth);
            }
        }
    }

    static string Ordinal(int n) { return n == 1 ? "first" : (n == 2 ? "second" : "third"); }
    static string Cap(string s) { return string.IsNullOrEmpty(s) ? s : char.ToUpper(s[0]) + s.Substring(1); }

    static QA Secret(PassengerData p)
    {
        bool alive = p.truth == PassengerTruth.Alive || p.truth == PassengerTruth.Altered;
        string ap = alive ? "Każdy coś ukrywa. " : "";
        string ae = alive ? "Everyone hides something. " : "";
        string au = alive ? "Кожен щось приховує. " : "";
        return Q("q_secret", 24, "Czy coś " + G(p, "Pan", "Pani") + " ukrywa?", "Are you hiding something?", "Ви щось приховуєте?", ap + p.secret, ae + p.secret, au + p.secret, alive ? QA.Half : QA.Truth, false, 1);
    }

    static QA Fear(PassengerData p)
    {
        string a, e, u;
        switch (p.behaviour)
        {
            case "angry":
                a = "Bo mnie pan tu trzyma na zimnie! Mam prawo jechać!"; e = "Because you keep me out here in the cold! I have a right to travel!"; u = "Бо ви тримаєте мене тут на морозі! Я маю право їхати!"; break;
            case "scared":
                a = "Coś... za panem. Nie, przepraszam. To tylko cień."; e = "Something... behind you. No, sorry. Just a shadow."; u = "Щось… за вами. Ні, вибачте. Це лише тінь."; break;
            default:
                a = "Wie pan, jak to jest na peronie nocą? Każdy hałas wygląda jak ktoś."; e = "You know how it is on a platform at night? Every noise looks like someone."; u = "Знаєте, як буває на пероні вночі? Кожен шурхіт скидається на когось."; break;
        }
        return Q("q_fear", 22, "Dlaczego jest " + G(p, "Pan taki spięty", "Pani taka spięta") + "?", "Why are you so tense?", "Чому ви такі напружені?", a, e, u, QA.Half, false, p.behaviour == "angry" ? 1 : 0);
    }

    static QA Ill(PassengerData p)
    {
        string qp = p.female ? "Czy chorowała Pani ostatnio?" : "Czy chorował Pan ostatnio?", qe = "Have you been ill lately?", qu = "Ви нещодавно хворіли?";
        switch (p.truth)
        {
            case PassengerTruth.Dead: return Q("q_ill", 40, qp, qe, qu, "Nie choruję już. To jedyna dobra rzecz.", "I do not fall ill anymore. That's the one good thing.", "Я вже не хворію. Це єдине добре.", QA.Half);
            case PassengerTruth.NonExistent: return Q("q_ill", 40, qp, qe, qu, "Nie wiem. Nikt mnie nigdy nie badał.", "I don't know. No one ever examined me.", "Не знаю. Мене ніхто ніколи не обстежував.", QA.Half);
            case PassengerTruth.Impostor: return Q("q_ill", 40, qp, qe, qu, G(p, "Zdrów jak ryba.", "Zdrowa jak ryba."), "Fit as a fiddle.", Sk(p, "Здоровий, як ніколи.", "Здорова, як ніколи."), QA.Lie);
            case PassengerTruth.Unknown: return Q("q_ill", 40, qp, qe, qu, "Nie ma na to nazwy.", "There is no name for it.", "Для цього немає назви.", QA.Evasive);
            default: return Q("q_ill", 40, qp, qe, qu, "Rozpoznanie: " + p.illness + ". Nic poważnego.", "Diagnosis: " + p.illness + ". Nothing serious.", "Діагноз: " + p.illness + ". Нічого серйозного.", QA.Truth);
        }
    }

    static QA Home(PassengerData p)
    {
        const string qp = "W dokumentach brak celu podróży. Gdzie jest ten dom?", qe = "There is no destination in the documents. Where is home?", qu = "У документах не вказано пункту призначення. Де ваш дім?";
        switch (p.truth)
        {
            case PassengerTruth.Dead:
                return Q("q_home", 90, qp, qe, qu, "Tam, gdzie mnie pochowano. Chyba ktoś tam czeka.", "Where I was buried. I think someone is waiting there.", "Там, де мене поховали. Здається, там хтось чекає.", QA.Truth, true, 2, "a_date", "ano_dead");
            case PassengerTruth.NonExistent:
                return Q("q_home", 90, qp, qe, qu, "Za tym peronem. Za wagonem 13.", "Behind this platform. Behind car 13.", "За цим пероном. За вагоном 13.", QA.Half, true, 2, null, "loc_car13");
            case PassengerTruth.Echo:
                return Q("q_home", 90, qp, qe, qu, "Tam, gdzie ktoś o mnie myśli.", "Where someone thinks of me.", "Там, де хтось про мене думає.", QA.Half, true, 1, null, "ano_echo");
            default:
                return Q("q_home", 90, qp, qe, qu, "Tam, gdzie pan.", "Where you are.", "Там, де ви.", QA.Evasive, true, 3, null, "ano_unknown");
        }
    }

    static QA Photo(PassengerData p)
    {
        string qp = "Czy to zdjęcie przedstawia " + G(p, "Pana", "Panią") + "?", qe = "Is this photo of you?", qu = "Це ваше фото?";
        if (p.playerPhoto && p.truth != PassengerTruth.Impostor)
            return Q("q_photo", 85, qp, qe, qu, "Nie wiem. Ktoś to wsunął w dowód. To pana twarz, prawda?", "I don't know. Someone slipped it into the ID. That is your face, isn't it?", "Не знаю. Хтось вклав це в посвідчення. Це ж ваше обличчя, правда?", QA.Truth, true, 5, "a_photo", "evt_photo");
        return Q("q_photo", 85, qp, qe, qu, "Oczywiście, to ja! <i>(zbyt głośno)</i> Trochę schudł" + G(p, "em", "am") + ", tyle.", "Of course it's me! <i>(too loudly)</i> I've lost some weight, that's all.", "Звісно, це я! <i>(надто голосно)</i> Трохи " + Sk(p, "схуд", "схудла") + ", от і все.", QA.Lie, true, 0, "a_photo", "ano_photo");
    }

    static QA DeathQ(PassengerData p)
    {
        return Q("q_death", 88, "Ten akt zgonu. Wyjaśni " + G(p, "Pan", "Pani") + "?", "This death record. Can you explain it?", "Цей акт про смерть. Можете пояснити?",
                 "Tak, to prawda. Nie wiedział" + G(p, "em", "am") + ", że to widać.", "Yes, it is true. I didn't know it showed.", "Так, це правда. Я не " + Sk(p, "знав", "знала") + ", що це видно.", QA.Truth, true, 3, "a_dead", "ano_dead");
    }

    static QA Mother(PassengerData p)
    {
        string qp = "Kim są " + G(p, "Pana", "Pani") + " rodzice?", qe = "Who are your parents?", qu = "Хто ваші батьки?";
        if (p.truth == PassengerTruth.NonExistent)
            return Q("q_mother", 80, qp, qe, qu, "Moja matka czeka na mnie w wagonie.", "My mother is waiting for me in the car.", "Моя мати чекає на мене у вагоні.", QA.Half, true, 2);
        if (p.truth == PassengerTruth.Echo)
            return Q("q_mother", 80, qp, qe, qu, "Nie mam. Jestem czyimś wspomnieniem, nie dzieckiem.", "I have none. I am someone's memory, not a child.", "У мене їх немає. Я — чийсь спогад, а не дитина.", QA.Half, true, 1);
        return Q("q_mother", 80, qp, qe, qu, "Pan. Pan mnie wychował. Nie pamięta pan?", "You. You raised me. Don't you remember?", "Ви. Це ви мене виховали. Не пам’ятаєте?", QA.Evasive, true, 4);
    }

    static QA OrderQ(PassengerData p)
    {
        const string qp = "Widzę rozkaz Dyrekcji. Od kogo?", qe = "I see a Directorate order. From whom?", qu = "Бачу наказ Дирекції. Від кого?";
        if (p.orderState == 2)
            return Q("q_order", 92, qp, qe, qu, "Oczywiście prawdziwy. Proszę nie zawracać głowy podpisami.", "Of course genuine. Please don't bother with signatures.", "Звісно, справжній. Не морочте голову підписами.", QA.Lie, true, 0, "a_signature", "ano_signature");
        return Q("q_order", 92, qp, qe, qu, "Z Dyrekcji. Osobiście podpisany. Chyba nie zamierza pan sprzeciwiać się Dyrekcji.", "From the Directorate. Personally signed. I trust you do not intend to oppose the Directorate.", "З Дирекції. Особисто підписаний. Сподіваюся, ви не збираєтеся суперечити Дирекції.", QA.Truth, true);
    }

    static QA Future(PassengerData p)
    {
        return Q("q_future", 86, "Ta data pochodzi z przyszłości. Wyjaśni " + G(p, "Pan", "Pani") + "?", "This date is from the future. Can you explain it?", "Ця дата з майбутнього. Можете пояснити?",
                 "Jeszcze nie... to znaczy: już. Proszę wybaczyć, mylą mi się kolejności.", "Not yet... I mean: already. Forgive me, I mix up the order.", "Ще ні… тобто вже. Вибачте, я плутаю послідовність.", QA.Half, true, 3, "a_date", "ano_loop");
    }

    static QA Amend(PassengerData p)
    {
        return Q("q_amend", 84, "Dokument był poprawiany. Dlaczego?", "The document was amended. Why?", "Документ виправляли. Чому?",
                 "Nie wiedział" + G(p, "em", "am") + ", że coś poprawiano. Ktoś musiał to zrobić w nocy.", "I didn't know anything was amended. Someone must have done it in the night.", "Я не " + Sk(p, "знав", "знала") + ", що щось виправляли. Напевно, хтось зробив це вночі.", QA.Truth, true, 1, "a_amended", "ano_altered");
    }

    static QA ConductorQ(PassengerData p)
    {
        const string qp = "Czy już się kiedyś spotkaliśmy?", qe = "Have we met before?", qu = "Ми вже зустрічалися?";
        switch (p.truth)
        {
            case PassengerTruth.Dead: return Q("q_cond", 60, qp, qe, qu, "Tak. Co noc. Pan nas wpuszcza. Albo nie.", "Yes. Every night. You let us in. Or not.", "Так. Щоночі. Ви нас пропускаєте. Або ні.", QA.Half, false, 2);
            case PassengerTruth.NonExistent: return Q("q_cond", 60, qp, qe, qu, G(p, "Byłem", "Byłam") + " tu kiedyś. Pan mnie nie zauważył.", "I was here once. You did not notice me.", "Я тут колись " + Sk(p, "був", "була") + ". Ви мене не помітили.", QA.Half, false, 2);
            case PassengerTruth.Loop: return Q("q_cond", 60, qp, qe, qu, "Spotkamy się jutro. Albo wczoraj.", "We will meet tomorrow. Or yesterday.", "Зустрінемося завтра. Або вчора.", QA.Half, false, 2);
            case PassengerTruth.Impostor: return Q("q_cond", 60, qp, qe, qu, "Nie.", "No.", "Ні.", QA.Lie);
            case PassengerTruth.Echo: return Q("q_cond", 60, qp, qe, qu, "Zawsze. Jestem w panu jak wspomnienie.", "Always. I am in you like a memory.", "Завжди. Я живу у вас як спогад.", QA.Half, false, 3);
            case PassengerTruth.Unknown: return Q("q_cond", 60, qp, qe, qu, "Codziennie. Dziś w innej kolejności.", "Every day. Today in a different order.", "Щодня. Сьогодні — в іншому порядку.", QA.Half, false, 4);
            default: return Q("q_cond", 60, qp, qe, qu, "Chyba nie. Choć ma pan znajomą twarz.", "I don't think so. Though you have a familiar face.", "Здається, ні. Хоча обличчя у вас знайоме.", QA.Truth);
        }
    }

    static QA Memory(PassengerData p)
    {
        string qp = "Pamięta " + G(p, "Pan", "Pani") + " wczorajszą noc?", qe = "Do you remember last night?", qu = "Ви пам’ятаєте вчорашню ніч?";
        switch (p.truth)
        {
            case PassengerTruth.Dead: return Q("q_mem", 35, qp, qe, qu, "Wczoraj było bardzo dawno temu.", "Yesterday was a very long time ago.", "Вчора було дуже давно.", QA.Half);
            case PassengerTruth.NonExistent: return Q("q_mem", 35, qp, qe, qu, "Nie ma wczoraj. Jest tylko peron.", "There is no yesterday. There is only the platform.", "Вчора немає. Є лише перон.", QA.Half);
            case PassengerTruth.Loop: return Q("q_mem", 35, qp, qe, qu, "Wczoraj jeszcze nie było.", "Yesterday has not happened yet.", "Вчора ще не було.", QA.Half, false, 2);
            case PassengerTruth.Impostor: return Q("q_mem", 35, qp, qe, qu, G(p, "Byłem", "Byłam") + " w pracy. Cały dzień.", "I was at work. All day.", "Я " + Sk(p, "був", "була") + " на роботі. Цілий день.", QA.Lie);
            case PassengerTruth.Echo: return Q("q_mem", 35, qp, qe, qu, "Pan pyta, czy ja pamiętam? To pan mnie pamięta.", "You ask whether I remember? It is you who remembers me.", "Ви питаєте, чи пам’ятаю я? Це ви мене пам’ятаєте.", QA.Half, false, 2);
            case PassengerTruth.Unknown: return Q("q_mem", 35, qp, qe, qu, "Pamiętam każdą noc. Ta jest trzynasta.", "I remember every night. This one is the thirteenth.", "Я пам’ятаю кожну ніч. Ця — тринадцята.", QA.Half, false, 3);
            default: return Q("q_mem", 35, qp, qe, qu, "Wczoraj? " + G(p, "Byłem", "Byłam") + " w domu.", "Yesterday? I was at home.", "Вчора? Я " + Sk(p, "був", "була") + " удома.", QA.Truth);
        }
    }

    // AUTHORITY: an order. The passenger drops the guard and tells the truth about themselves.
    static QA Command(PassengerData p)
    {
        return Q("q_command", 20, "[AUTORYTET] Odpowiadać zgodnie z prawdą. To rozkaz.", "[AUTHORITY] Answer truthfully. That is an order.", "[АВТОРИТЕТ] Відповідайте правдиво. Це наказ.", p.secret, p.secret, p.secret, QA.Truth, false, 0, null, null, null, 3);
    }
}
