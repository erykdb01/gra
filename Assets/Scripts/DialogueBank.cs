using System.Collections.Generic;
using UnityEngine;

// Builds the questions and prepared answers for a passenger. The answers depend on the hidden truth,
// on what the documents show, and on story scripts. Every line exists in Polish and English.
public static class DialogueBank
{
    static string G(PassengerData p, string male, string female) { return p.female ? female : male; }

    static QA Q(string id, int priority, string qPl, string qEn, string aPl, string aEn, int kind, bool contextual = false, int stress = 0, string anomaly = null, string codex = null, string flag = null, int minAuthority = 0)
    {
        return new QA
        {
            id = id, priority = priority, question = Loc.T(qPl, qEn), answer = Loc.T(aPl, aEn),
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
        l.Add(Q("pl_where", 50, "Skąd i dokąd podróż?", "Where from and where to?", "Znikąd. Od zawsze stoję na tym peronie. A dokąd? Tego nie wiesz, prawda?", "From nowhere. I have always stood on this platform. And where to? You do not know, do you?", QA.Truth));
        l.Add(Q("pl_dob", 48, "Proszę podać datę urodzenia.", "Please state your date of birth.", "Nie pamiętam. Tak długo tu jesteś, że nawet ty nie pamiętasz.", "I do not remember. You have been here so long that even you do not remember.", QA.Truth));
        l.Add(Q("pl_wait", 46, "Czy ktoś czeka na stacji?", "Is anyone waiting at the station?", "Nikt. Tylko ten pociąg. Czeka na mnie od bardzo dawna.", "No one. Only this train. It has been waiting for me for a very long time.", QA.Truth));
        l.Add(Q("pl_ticket", 44, "Czy bilet jest na Państwa nazwisko?", "Is the ticket in your name?", "Bilet jest mój. Wypisałeś go sam, tyle nocy temu...", "The ticket is mine. You wrote it yourself, so many nights ago...", QA.Truth));
        l.Add(Q("pl_who", 40, "Kim jesteś?", "Who are you?", "Jestem tym, kim ty byłeś, zanim zacząłeś tylko sprawdzać papiery.", "I am who you were before you started only checking papers.", QA.Truth, true, 3));
        l.Add(Q("pl_last", 38, "Ilu pasażerów odesłałeś?", "How many passengers did you turn away?", "Wszystkich, którym nie wolno było. Każdy z nich stoi teraz za mną.", "Everyone who was not allowed. Every one of them stands behind me now.", QA.Truth, true, 3));
        l.Add(Q("pl_board", 36, "Czy chcesz wsiąść?", "Do you want to board?", "Nie o to chodzi, czego ja chcę. Chodzi o to, czego chcesz ty.", "It is not about what I want. It is about what you want.", QA.Truth, true, 2, null, "unk_first"));
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
                "In front of the window stands someone in a conductor's uniform. They have your face and your tiredness. \"Will you let me in?\" they ask in your voice.");
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
                    s = Loc.T("Dobry wieczór... Chciał" + G(p, "bym", "abym") + " wsiąść. Jest tu jakoś zimno, prawda?", "Good evening... I would like to board. It is rather cold here, isn't it?");
                    break;
                case PassengerTruth.NonExistent:
                    s = Loc.T("Dobry wieczór. To jest ten pociąg? Powiedziano mi, że tu mam wsiąść.", "Good evening. Is this the train? I was told to board here.");
                    break;
                case PassengerTruth.Loop:
                    s = Loc.T("Dobry wieczór. Znowu... to znaczy po raz pierwszy. Proszę o bilet na dzisiaj i jutro.", "Good evening. Again... I mean for the first time. One ticket for today and tomorrow, please.");
                    break;
                case PassengerTruth.Impostor:
                    s = Loc.T("Dobry wieczór! Śpieszę się. Wszystko mam w porządku, proszę spojrzeć.", "Good evening! I'm in a hurry. Everything is in order, please look.");
                    break;
                case PassengerTruth.Echo:
                    s = Loc.T("Dobry wieczór. Ktoś mi kiedyś powiedział, że tu jest wejście.", "Good evening. Someone once told me that this is the entrance.");
                    break;
                case PassengerTruth.Unknown:
                    s = Loc.T("Dobry wieczór, konduktorze. Pan wie, po co tu jestem.", "Good evening, conductor. You know why I am here.");
                    break;
                case PassengerTruth.Altered:
                    s = Loc.T("Dobry wieczór. Jadę do stacji " + p.station + ". Coś nie tak z dokumentami?", "Good evening. I'm going to station " + p.station + ". Something wrong with the documents?");
                    break;
                default:
                    s = Loc.T("Dobry wieczór. Jadę do stacji " + p.station + ".", "Good evening. I'm going to station " + p.station + ".");
                    break;
            }
        }
        if (p.sympathetic && !string.IsNullOrEmpty(p.plea)) s += "\n<i>" + p.plea + "</i>";
        return s;
    }

    // A last line when the passenger walks away or boards.
    public static string Farewell(PassengerData p, bool admitted)
    {
        if (p.isPlayer) return admitted ? Loc.T("Dziękuję. Od dawna nikt nie mówił mi \"proszę wsiadać\".", "Thank you. No one has said \"please board\" to me in a long time.")
                                        : Loc.T("Rozumiem. Ktoś musi zostać.", "I understand. Someone has to stay.");
        if (admitted)
        {
            switch (p.behaviour)
            {
                case "angry": return Loc.T("No nareszcie.", "Finally.");
                case "cold": return Loc.T("...", "...");
                case "scared": return Loc.T("Dziękuję. Proszę tylko nie zamykać drzwi.", "Thank you. Just don't close the doors.");
                default: return Loc.T("Dziękuję. Dobrej nocy.", "Thank you. Good night.");
            }
        }
        switch (p.behaviour)
        {
            case "angry": return Loc.T("To skandal! Zobaczy pan, jak to się skończy.", "This is outrageous! You will see how this ends.");
            case "cold": return Loc.T("Wrócę.", "I will be back.");
            case "scared": return Loc.T("Proszę... nie zostawiajcie mnie tu.", "Please... do not leave me here.");
            case "nervous": return Loc.T("Rozumiem. Chyba.", "I understand. I think.");
            default: return Loc.T("Trudno. Dobranoc.", "Never mind. Good night.");
        }
    }

    // ---------------------------------------------------------------------------------------------
    // Individual questions

    static QA Where(PassengerData p)
    {
        switch (p.truth)
        {
            case PassengerTruth.Dead:
                return Q("q_where", 50, "Skąd i dokąd podróż?", "Where from and where to?", "Z daleka. Bardzo daleka. A dokąd... tam, gdzie jadą wszyscy.", "From far away. Very far. And where to... where everyone goes.", QA.Evasive);
            case PassengerTruth.NonExistent:
                return Q("q_where", 50, "Skąd i dokąd podróż?", "Where from and where to?", "Nie pamiętam dokładnie. Wiem tylko, że muszę tu wsiąść.", "I don't remember exactly. I only know I have to board here.", QA.Half);
            case PassengerTruth.Echo:
                return Q("q_where", 50, "Skąd i dokąd podróż?", "Where from and where to?", "Stąd. Stamtąd. Skądś, gdzie ktoś o mnie myśli.", "From here. From there. From somewhere someone is thinking of me.", QA.Evasive);
            case PassengerTruth.Unknown:
                return Q("q_where", 50, "Skąd i dokąd podróż?", "Where from and where to?", "Z miejsca, którego jeszcze nie ma. Do miejsca, które już było.", "From a place that does not exist yet. To a place that already was.", QA.Evasive, false, 2);
            case PassengerTruth.Impostor:
                return Q("q_where", 50, "Skąd i dokąd podróż?", "Where from and where to?", "Z miasta " + p.origin + ". Wszystko jest w dokumentach, proszę patrzeć.", "From " + p.origin + ". It is all in the documents, please look.", QA.Half);
            default:
                return Q("q_where", 50, "Skąd i dokąd podróż?", "Where from and where to?", "Z miasta " + p.origin + ", do stacji " + p.station + ". Wracam do domu.", "From " + p.origin + ", to station " + p.station + ". I'm going home.", QA.Truth);
        }
    }

    static QA Dob(PassengerData p, string bd)
    {
        string qp = "Proszę podać datę urodzenia.", qe = "Please state your date of birth.";
        switch (p.truth)
        {
            case PassengerTruth.Dead:
                return Q("q_dob", 48, qp, qe, bd + ". <i>(mówi to bardzo spokojnie)</i> Choć czasem wydaje mi się, że to było bardzo dawno.", bd + ". <i>(says it very calmly)</i> Though sometimes it feels like it was very long ago.", QA.Half);
            case PassengerTruth.NonExistent:
                return Q("q_dob", 48, qp, qe, "Nie... chwileczkę. Tak mi powiedziano: " + bd + ".", "No... wait. That is what I was told: " + bd + ".", QA.Half);
            case PassengerTruth.Loop:
                return Q("q_dob", 48, qp, qe, "Urodziłem się... jutro? Przepraszam, mylą mi się kolejności.", "I was born... tomorrow? Sorry, I mix up the order.", QA.Half, false, 1);
            case PassengerTruth.Impostor:
                return Q("q_dob", 48, qp, qe, bd + ". <i>(powiedziane jak wyuczona modlitwa)</i>", bd + ". <i>(said like a memorised prayer)</i>", QA.Lie);
            case PassengerTruth.Echo:
                return Q("q_dob", 48, qp, qe, "Data? Ktoś ją kiedyś wypowiedział. Nie pamiętam kto.", "A date? Someone once said it. I don't remember who.", QA.Evasive);
            case PassengerTruth.Unknown:
                return Q("q_dob", 48, qp, qe, "Nie mam daty. Mam tylko peron.", "I have no date. I only have the platform.", QA.Evasive, false, 2);
            case PassengerTruth.Altered:
                return Q("q_dob", 48, qp, qe, bd + ". A może... Ktoś mi kiedyś mówił, że inaczej.", bd + ". Or maybe... Someone once told me otherwise.", QA.Half);
            default:
                return Q("q_dob", 48, qp, qe, bd + ". Mam to nawet wypisane w dowodzie.", bd + ". It is even written on my ID.", QA.Truth);
        }
    }

    static QA Waiting(PassengerData p)
    {
        string qp = "Czy ktoś czeka na stacji?", qe = "Is anyone waiting at the station?";
        switch (p.truth)
        {
            case PassengerTruth.Dead:
                return Q("q_wait", 46, qp, qe, "Nikt. Już nikt. Ręce mam zimne, prawda? Od dawna.", "No one. Not anymore. My hands are cold, aren't they? For a long time.", QA.Truth, false, 1);
            case PassengerTruth.NonExistent:
                return Q("q_wait", 46, qp, qe, "Nie mam rodziców. Nigdy ich nie poznał" + G(p, "em", "am") + ".", "I have no parents. I never knew them.", QA.Half);
            case PassengerTruth.Loop:
                return Q("q_wait", 46, qp, qe, "Będą czekać. Albo już czekają.", "They will be waiting. Or already are.", QA.Half);
            case PassengerTruth.Impostor:
                return Q("q_wait", 46, qp, qe, "Tak, żona. <i>(odwraca wzrok)</i>", "Yes, my wife. <i>(looks away)</i>", QA.Lie);
            case PassengerTruth.Echo:
                return Q("q_wait", 46, qp, qe, "Ktoś o mnie pamięta. To wystarcza, żebym trwał" + G(p, "", "a") + ".", "Someone remembers me. That is enough for me to last.", QA.Half);
            case PassengerTruth.Unknown:
                return Q("q_wait", 46, qp, qe, "Pan. Od dawna.", "You. For a long time.", QA.Evasive, false, 3);
            case PassengerTruth.Altered:
                return Q("q_wait", 46, qp, qe, "Tak. Chociaż siostra twierdzi, że to nie ja wracam.", "Yes. Though my sister claims it isn't me coming back.", QA.Truth);
            default:
                return Q("q_wait", 46, qp, qe, "Tak, rodzina. Czekają z kolacją.", "Yes, my family. Dinner is waiting.", QA.Truth);
        }
    }

    static QA TicketQ(PassengerData p)
    {
        string qp = "Czy bilet jest na Państwa nazwisko?", qe = "Is the ticket in your name?";
        if (p.noTicket)
            return Q("q_ticket", 44, qp, qe, "Nie mam biletu. Nie mam też nazwiska, które by do niego pasowało.", "I have no ticket. I have no name that would fit it either.", QA.Truth);
        if (p.forgedTicket)
            return Q("q_ticket", 44, qp, qe, "Ee... tak. To znaczy, pożyczył" + G(p, "em", "am") + " od znajomego. Ale to prawie to samo.", "Uh... yes. I mean, I borrowed it from a friend. But it's practically the same.", QA.Lie);
        switch (p.truth)
        {
            case PassengerTruth.Dead:
                return Q("q_ticket", 44, qp, qe, "Tak. Kupił" + G(p, "em", "am") + " go dawno temu. Przed wszystkim.", "Yes. I bought it long ago. Before everything.", QA.Half);
            case PassengerTruth.NonExistent:
                return Q("q_ticket", 44, qp, qe, "Tak, chyba tak. Ktoś mi go dał.", "Yes, I think so. Someone gave it to me.", QA.Half);
            case PassengerTruth.Impostor:
                return Q("q_ticket", 44, qp, qe, "Oczywiście. Proszę sprawdzić. Szybko.", "Of course. Please check. Quickly.", QA.Lie);
            case PassengerTruth.Unknown:
                return Q("q_ticket", 44, qp, qe, "Bilet jest tylko formalnością.", "The ticket is only a formality.", QA.Evasive);
            default:
                return Q("q_ticket", 44, qp, qe, "Oczywiście. Proszę sprawdzić.", "Of course. Please check.", QA.Truth);
        }
    }

    static QA Job(PassengerData p)
    {
        string qp = "Czym się Państwo zajmują?", qe = "What do you do for a living?";
        switch (p.truth)
        {
            case PassengerTruth.Dead:
                return Q("q_job", 30, qp, qe, "Byłem... jestem: " + p.occupation + ". Trudno powiedzieć, w jakim czasie.", "I was... I am: " + p.occupation + ". Hard to say in which time.", QA.Half);
            case PassengerTruth.NonExistent:
                return Q("q_job", 30, qp, qe, "Chyba " + p.occupation + ". Tak mi się wydaje.", "I think " + p.occupation + ". So it seems to me.", QA.Half);
            case PassengerTruth.Impostor:
                return Q("q_job", 30, qp, qe, p.occupation + ". Zawsze byłem. <i>(zbyt szybko)</i>", p.occupation + ". Always was. <i>(too quickly)</i>", QA.Lie);
            case PassengerTruth.Echo:
                return Q("q_job", 30, qp, qe, "Pracuję w pamięci ludzi.", "I work in people's memories.", QA.Evasive);
            case PassengerTruth.Unknown:
                return Q("q_job", 30, qp, qe, "Pracuję tu. Tak jak pan.", "I work here. Like you.", QA.Evasive, false, 2);
            default:
                return Q("q_job", 30, qp, qe, Cap(p.occupation) + ".", Cap(p.occupation) + ".", QA.Truth);
        }
    }

    static QA Story(PassengerData p)
    {
        int kind = p.truth == PassengerTruth.Alive || p.truth == PassengerTruth.Altered ? QA.Truth : QA.Half;
        return Q("q_story", 28, "Proszę opowiedzieć o sobie.", "Tell me about yourself.", p.backstory, p.backstory, kind);
    }

    static QA Previous(PassengerData p)
    {
        string qp = "Czy jechał" + G(p, "", "a") + " już Pan" + G(p, "", "i") + " Linią 13?", qe = "Have you travelled on Line 13 before?";
        qp = p.female ? "Czy jechała już Pani Linią 13?" : "Czy jechał już Pan Linią 13?";
        switch (p.truth)
        {
            case PassengerTruth.Dead:
                return Q("q_prev", 26, qp, qe, "Tak. Wiele razy. Ale zawsze ktoś mnie odsyła.", "Yes. Many times. But someone always sends me back.", QA.Half);
            case PassengerTruth.NonExistent:
                return Q("q_prev", 26, qp, qe, "Chyba tak. Ciągle mam wrażenie, że tu byłem.", "I think so. I keep feeling I have been here.", QA.Half);
            case PassengerTruth.Loop:
                return Q("q_prev", 26, qp, qe, "Będę jutro. Ale pamiętam już wczoraj.", "I will be tomorrow. But I already remember yesterday.", QA.Half);
            case PassengerTruth.Impostor:
                return Q("q_prev", 26, qp, qe, "Nigdy w życiu.", "Never in my life.", QA.Lie);
            case PassengerTruth.Echo:
                return Q("q_prev", 26, qp, qe, "Ktoś tu był. Ja jestem jego wspomnieniem.", "Someone was here. I am their memory.", QA.Half);
            case PassengerTruth.Unknown:
                return Q("q_prev", 26, qp, qe, "Zawsze.", "Always.", QA.Evasive, false, 2);
            default:
                return Q("q_prev", 26, qp, qe, "To dopiero mój " + Random.Range(1, 4) + ". raz.", "This is only my " + Ordinal(Random.Range(1, 4)) + " time.", QA.Truth);
        }
    }

    static string Ordinal(int n) { return n == 1 ? "first" : (n == 2 ? "second" : "third"); }
    static string Cap(string s) { return string.IsNullOrEmpty(s) ? s : char.ToUpper(s[0]) + s.Substring(1); }

    static QA Secret(PassengerData p)
    {
        bool alive = p.truth == PassengerTruth.Alive || p.truth == PassengerTruth.Altered;
        string ap = alive ? "Każdy coś ukrywa. " : "";
        string ae = alive ? "Everyone hides something. " : "";
        return Q("q_secret", 24, "Czy coś Państwo ukrywają?", "Are you hiding something?", ap + p.secret, ae + p.secret, alive ? QA.Half : QA.Truth, false, 1);
    }

    static QA Fear(PassengerData p)
    {
        string a, e;
        switch (p.behaviour)
        {
            case "angry": a = "Bo mnie pan tu trzyma na zimnie! Mam prawo jechać!"; e = "Because you keep me out here in the cold! I have a right to travel!"; break;
            case "scared": a = "Coś... za panem. Nie, przepraszam. To tylko cień."; e = "Something... behind you. No, sorry. Just a shadow."; break;
            default: a = "Wie pan, jak to jest na peronie nocą? Każdy hałas wygląda jak ktoś."; e = "You know how it is on a platform at night? Every noise looks like someone."; break;
        }
        return Q("q_fear", 22, "Dlaczego jest Pan" + (p.female ? "i" : "") + " taki spięty?", "Why are you so tense?", a, e, QA.Half, false, p.behaviour == "angry" ? 1 : 0);
    }

    static QA Ill(PassengerData p)
    {
        string qp = "Czy chorował" + G(p, "", "a") + " Pan" + G(p, "", "i") + " ostatnio?", qe = "Have you been ill lately?";
        qp = p.female ? "Czy chorowała Pani ostatnio?" : "Czy chorował Pan ostatnio?";
        switch (p.truth)
        {
            case PassengerTruth.Dead: return Q("q_ill", 40, qp, qe, "Nie choruję już. To jedyna dobra rzecz.", "I do not fall ill anymore. That's the one good thing.", QA.Half);
            case PassengerTruth.NonExistent: return Q("q_ill", 40, qp, qe, "Nie wiem. Nikt mnie nigdy nie badał.", "I don't know. No one ever examined me.", QA.Half);
            case PassengerTruth.Impostor: return Q("q_ill", 40, qp, qe, "Zdrów jak ryba.", "Fit as a fiddle.", QA.Lie);
            case PassengerTruth.Unknown: return Q("q_ill", 40, qp, qe, "Nie ma na to nazwy.", "There is no name for it.", QA.Evasive);
            default: return Q("q_ill", 40, qp, qe, "Rozpoznanie: " + p.illness + ". Nic poważnego.", "Diagnosis: " + p.illness + ". Nothing serious.", QA.Truth);
        }
    }

    static QA Home(PassengerData p)
    {
        string qp = "W dokumentach brak celu podróży. Gdzie jest ten dom?", qe = "There is no destination in the documents. Where is home?";
        switch (p.truth)
        {
            case PassengerTruth.Dead:
                return Q("q_home", 90, qp, qe, "Tam, gdzie mnie pochowano. Chyba ktoś tam czeka.", "Where I was buried. I think someone is waiting there.", QA.Truth, true, 2, "a_date", "ano_dead");
            case PassengerTruth.NonExistent:
                return Q("q_home", 90, qp, qe, "Za tym peronem. Za wagonem 13.", "Behind this platform. Behind car 13.", QA.Half, true, 2, null, "loc_car13");
            case PassengerTruth.Echo:
                return Q("q_home", 90, qp, qe, "Tam, gdzie ktoś o mnie myśli.", "Where someone thinks of me.", QA.Half, true, 1, null, "ano_echo");
            default:
                return Q("q_home", 90, qp, qe, "Tam, gdzie pan.", "Where you are.", QA.Evasive, true, 3, null, "ano_unknown");
        }
    }

    static QA Photo(PassengerData p)
    {
        string qp = "Czy to zdjęcie przedstawia Pana/Panią?", qe = "Is this photo of you?";
        if (p.playerPhoto && p.truth != PassengerTruth.Impostor)
            return Q("q_photo", 85, qp, qe, "Nie wiem. Ktoś to wsunął w dowód. To pana twarz, prawda?", "I don't know. Someone slipped it into the ID. That is your face, isn't it?", QA.Truth, true, 5, "a_photo", "evt_photo");
        return Q("q_photo", 85, qp, qe, "Oczywiście, to ja! <i>(zbyt głośno)</i> Trochę schudł" + G(p, "em", "am") + ", tyle.", "Of course it's me! <i>(too loudly)</i> I've lost some weight, that's all.", QA.Lie, true, 0, "a_photo", "ano_photo");
    }

    static QA DeathQ(PassengerData p)
    {
        return Q("q_death", 88, "Ten akt zgonu. Wyjaśni Pan(i)?", "This death record. Can you explain it?", "Tak, to prawda. Nie wiedział" + G(p, "em", "am") + ", że to widać.", "Yes, it is true. I didn't know it showed.", QA.Truth, true, 3, "a_dead", "ano_dead");
    }

    static QA Mother(PassengerData p)
    {
        string qp = "Kim są Państwa rodzice?", qe = "Who are your parents?";
        if (p.truth == PassengerTruth.NonExistent)
            return Q("q_mother", 80, qp, qe, "Moja matka czeka na mnie w wagonie.", "My mother is waiting for me in the car.", QA.Half, true, 2);
        if (p.truth == PassengerTruth.Echo)
            return Q("q_mother", 80, qp, qe, "Nie mam. Jestem czyimś wspomnieniem, nie dzieckiem.", "I have none. I am someone's memory, not a child.", QA.Half, true, 1);
        return Q("q_mother", 80, qp, qe, "Pan. Pan mnie wychował. Nie pamięta pan?", "You. You raised me. Don't you remember?", QA.Evasive, true, 4);
    }

    static QA OrderQ(PassengerData p)
    {
        if (p.orderState == 2)
            return Q("q_order", 92, "Widzę rozkaz Dyrekcji. Od kogo?", "I see a Directorate order. From whom?", "Oczywiście prawdziwy. Proszę nie zawracać głowy podpisami.", "Of course genuine. Please don't bother with signatures.", QA.Lie, true, 0, "a_signature", "ano_signature");
        return Q("q_order", 92, "Widzę rozkaz Dyrekcji. Od kogo?", "I see a Directorate order. From whom?", "Z Dyrekcji. Osobiście podpisany. Chyba nie zamierza pan sprzeciwiać się Dyrekcji.", "From the Directorate. Personally signed. I trust you do not intend to oppose the Directorate.", QA.Truth, true);
    }

    static QA Future(PassengerData p)
    {
        return Q("q_future", 86, "Ta data pochodzi z przyszłości. Wyjaśni Pan(i)?", "This date is from the future. Can you explain it?", "Jeszcze nie... to znaczy: już. Proszę wybaczyć, mylą mi się kolejności.", "Not yet... I mean: already. Forgive me, I mix up the order.", QA.Half, true, 3, "a_date", "ano_loop");
    }

    static QA Amend(PassengerData p)
    {
        return Q("q_amend", 84, "Dokument był poprawiany. Dlaczego?", "The document was amended. Why?", "Nie wiedział" + G(p, "em", "am") + ", że coś poprawiano. Ktoś musiał to zrobić w nocy.", "I didn't know anything was amended. Someone must have done it in the night.", QA.Truth, true, 1, "a_amended", "ano_altered");
    }

    static QA ConductorQ(PassengerData p)
    {
        string qp = "Czy już się kiedyś spotkaliśmy?", qe = "Have we met before?";
        switch (p.truth)
        {
            case PassengerTruth.Dead: return Q("q_cond", 60, qp, qe, "Tak. Co noc. Pan nas wpuszcza. Albo nie.", "Yes. Every night. You let us in. Or not.", QA.Half, false, 2);
            case PassengerTruth.NonExistent: return Q("q_cond", 60, qp, qe, "Byłem tu kiedyś. Pan mnie nie zauważył.", "I was here once. You did not notice me.", QA.Half, false, 2);
            case PassengerTruth.Loop: return Q("q_cond", 60, qp, qe, "Spotkamy się jutro. Albo wczoraj.", "We will meet tomorrow. Or yesterday.", QA.Half, false, 2);
            case PassengerTruth.Impostor: return Q("q_cond", 60, qp, qe, "Nie.", "No.", QA.Lie);
            case PassengerTruth.Echo: return Q("q_cond", 60, qp, qe, "Zawsze. Jestem w panu jak wspomnienie.", "Always. I am in you like a memory.", QA.Half, false, 3);
            case PassengerTruth.Unknown: return Q("q_cond", 60, qp, qe, "Codziennie. Dziś w innej kolejności.", "Every day. Today in a different order.", QA.Half, false, 4);
            default: return Q("q_cond", 60, qp, qe, "Chyba nie. Choć ma pan znajomą twarz.", "I don't think so. Though you have a familiar face.", QA.Truth);
        }
    }

    static QA Memory(PassengerData p)
    {
        string qp = "Pamięta Pan(i) wczorajszą noc?", qe = "Do you remember last night?";
        switch (p.truth)
        {
            case PassengerTruth.Dead: return Q("q_mem", 35, qp, qe, "Wczoraj było bardzo dawno temu.", "Yesterday was a very long time ago.", QA.Half);
            case PassengerTruth.NonExistent: return Q("q_mem", 35, qp, qe, "Nie ma wczoraj. Jest tylko peron.", "There is no yesterday. There is only the platform.", QA.Half);
            case PassengerTruth.Loop: return Q("q_mem", 35, qp, qe, "Wczoraj jeszcze nie było.", "Yesterday has not happened yet.", QA.Half, false, 2);
            case PassengerTruth.Impostor: return Q("q_mem", 35, qp, qe, "Byłem w pracy. Cały dzień.", "I was at work. All day.", QA.Lie);
            case PassengerTruth.Echo: return Q("q_mem", 35, qp, qe, "Pan pyta, czy ja pamiętam? To pan mnie pamięta.", "You ask whether I remember? It is you who remembers me.", QA.Half, false, 2);
            case PassengerTruth.Unknown: return Q("q_mem", 35, qp, qe, "Pamiętam każdą noc. Ta jest trzynasta.", "I remember every night. This one is the thirteenth.", QA.Half, false, 3);
            default: return Q("q_mem", 35, qp, qe, "Wczoraj? Byłem w domu.", "Yesterday? I was at home.", QA.Truth);
        }
    }

    // AUTHORITY: an order. The passenger drops the guard and tells the truth about themselves.
    static QA Command(PassengerData p)
    {
        return Q("q_command", 20, "[AUTORYTET] Odpowiadać zgodnie z prawdą. To rozkaz.", "[AUTHORITY] Answer truthfully. That is an order.", p.secret, p.secret, QA.Truth, false, 0, null, null, null, 3);
    }
}
