using System.Collections.Generic;

// The twelve nights of the Story Mode campaign. All content is data: rules, mixes, documents, cast, events.
public static class StoryData
{
    static readonly List<DayConfig> nights = new List<DayConfig>();

    public static List<DayConfig> Nights
    {
        get { if (nights.Count == 0) Build(); return nights; }
    }

    static void W(DayConfig d, float alive, float dead, float non, float altered, float loop, float impostor, float echo, float unknown)
    {
        d.w[0] = alive; d.w[1] = dead; d.w[2] = non; d.w[3] = altered; d.w[4] = loop; d.w[5] = impostor; d.w[6] = echo; d.w[7] = unknown;
    }

    static void Allow(DayConfig d, params PassengerTruth[] t)
    {
        for (int i = 0; i < t.Length; i++) d.allow[(int)t[i]] = true;
    }

    static DayConfig Night(int act, string actPl, string actEn, string tp, string te, string rp, string re,
                           string bp, string be, string op, string oe, string date, int mask, int maxMistakes)
    {
        var d = new DayConfig
        {
            act = act, actPl = actPl, actEn = actEn, titlePl = tp, titleEn = te, rulePl = rp, ruleEn = re,
            briefingPl = bp, briefingEn = be, outroPl = op, outroEn = oe, date = date, docMask = mask, maxMistakes = maxMistakes
        };
        nights.Add(d);
        return d;
    }

    static void Build()
    {
        DayConfig d;

        // ================= ACT I - THE SHIFT =================
        d = Night(1, "AKT I: ZMIANA", "ACT I: THE SHIFT",
            "Pierwsza zmiana", "The First Shift",
            "Wpuszczaj tylko żywych.", "Admit only the living.",
            "Dostałeś pracę konduktora na nocnym pociągu Linii 13. Peron jest mokry, latarnia ciężka, a kolejka cicha. " +
            "Porównaj bilet, dowód, bazę kolejową i rejestr. Jeśli dane się nie zgadzają, coś jest nie tak. " +
            "Dokumenty można przeciągać, powiększać i oznaczać. Patrz też na samych ludzi: żywym idzie para z ust i każdy rzuca cień.",
            "You have been given a job as a conductor on the night train of Line 13. The platform is wet, the lantern heavy, the queue quiet. " +
            "Compare the ticket, ID, railway database and registry. If the data does not match, something is wrong. " +
            "Documents can be dragged, enlarged and marked. Watch the people too: the living breathe steam and everyone casts a shadow.",
            "Pociąg odjeżdża. Na biurku ktoś zostawił kartkę: \"Dobrze ci poszło. Jutro będzie ciężej.\" Nie pamiętasz, kto ją napisał.",
            "The train departs. Someone left a note on the desk: \"You did well. Tomorrow will be harder.\" You do not remember who wrote it.",
            "02.11.1998", 15, 5);
        W(d, 70, 30, 0, 0, 0, 0, 0, 0); Allow(d, PassengerTruth.Alive);
        d.pleaChance = 0.15f; d.eventCount = 0;
        d.scripted.Add(new ScriptedSlot(3, "anna_1"));
        d.codexAtStart = new[] { "loc_platform13", "loc_booth", "doc_ticket", "doc_id", "doc_raildb", "doc_registry", "ano_dead", "ano_breath", "ano_shadow", "ppl_conductor" };

        d = Night(1, "AKT I: ZMIANA", "ACT I: THE SHIFT",
            "Fałszerze", "The Forgers",
            "Żywi z biletem na własne nazwisko. Reszta zostaje.", "The living with a ticket in their own name. The rest stay.",
            "Ktoś sprzedaje bilety na cudze nazwiska. Sprawdzaj, czy imię na bilecie zgadza się z dowodem, a pieczęć jest z właściwej stacji. " +
            "Od dziś masz też manifest: listę osób, które mają dziś jechać.",
            "Someone is selling tickets under other people's names. Check that the name on the ticket matches the ID and the seal is from the right station. " +
            "From tonight you also have the manifest: the list of people due to travel.",
            "Na peronie został zapach wilgotnej wełny. Rano na twoim biurku leżał bilet bez nazwiska i bez daty. Nie oddałeś go nikomu.",
            "The smell of damp wool lingered on the platform. In the morning a ticket with no name and no date lay on your desk. You gave it to no one.",
            "03.11.1998", 31, 4);
        W(d, 65, 35, 0, 0, 0, 0, 0, 0); Allow(d, PassengerTruth.Alive);
        d.forgeChance = 0.3f; d.pleaChance = 0.25f; d.eventCount = 1;
        d.events.Add("flicker");
        d.scripted.Add(new ScriptedSlot(4, "marek_1"));
        d.scripted.Add(new ScriptedSlot(9, "piotr_1"));
        d.codexAtStart = new[] { "doc_manifest", "ppl_marek", "ppl_piotr", "ano_seal" };

        // ================= ACT II - THE WRONG NAMES =================
        d = Night(2, "AKT II: ZŁE NAZWISKA", "ACT II: THE WRONG NAMES",
            "Brakujące wpisy", "Missing Entries",
            "Wpuszczaj tylko żywych. Osobom bez wpisu w Rejestrze odmawiaj.", "Admit only the living. Deny anyone missing from the Registry.",
            "W dokumentach zaczynają pojawiać się ludzie, których nie ma w Rejestrze. Mają zdjęcie, bilet, czasem nawet imię, które ktoś kiedyś nosił. " +
            "Nie rzucają cienia. Do teczki dokładamy kartę medyczną.",
            "People start to appear in the documents who are not in the Registry. They have a photo, a ticket, sometimes even a name someone once carried. " +
            "They cast no shadow. The medical record joins your folder.",
            "Któryś z nieistniejących powiedział na odchodnym: \"Dziękuję, że pan mnie zobaczył\". Nie wiesz, czy to była groźba.",
            "One of the non-existent said on leaving: \"Thank you for seeing me\". You do not know whether it was a threat.",
            "04.11.1998", 63, 4);
        W(d, 45, 20, 35, 0, 0, 0, 0, 0); Allow(d, PassengerTruth.Alive);
        d.forgeChance = 0.2f; d.pleaChance = 0.3f; d.eventCount = 1; d.anomalyChance = 0.15f;
        d.events.AddRange(new[] { "blackout", "vanish", "wrong_line" });
        d.scripted.Add(new ScriptedSlot(1, "nodocs_1"));
        d.scripted.Add(new ScriptedSlot(10, "lena_1"));
        d.codexAtStart = new[] { "ano_nonex", "doc_medical", "ppl_nodocs", "ppl_lena", "lore_records" };

        d = Night(2, "AKT II: ZŁE NAZWISKA", "ACT II: THE WRONG NAMES",
            "Inspekcja", "Inspection",
            "Żadnej litości. Żywi ze zgodnym zdjęciem. Rozkaz Dyrekcji jest ważny.", "No mercy. The living with a matching photo. A Directorate order is valid.",
            "Na stację przyjechał inspektor Dyrekcji. Każdy błąd trafia do raportu. Pojawili się też podszywacze: ludzie w cudzych dokumentach. " +
            "Najprościej poznać ich po zdjęciu. Do teczki dochodzi akt zgonu.",
            "An inspector of the Directorate has arrived at the station. Every mistake goes into the report. Impostors have appeared too: people in other people's papers. " +
            "The photo is the easiest way to spot them. The death record joins your folder.",
            "Inspektor podpisał raport i zniknął w mgle. Nikt nie widział, jak wychodzi z peronu. Zostawił po sobie zapach zimnego żelaza.",
            "The inspector signed the report and vanished into the fog. No one saw him leave the platform. He left behind the smell of cold iron.",
            "05.11.1998", 127, 3);
        W(d, 35, 15, 20, 0, 0, 30, 0, 0); Allow(d, PassengerTruth.Alive);
        d.forgeChance = 0.25f; d.pleaChance = 0.4f; d.orderChance = 0.05f; d.eventCount = 1; d.anomalyChance = 0.2f;
        d.events.AddRange(new[] { "seen_before", "door_open" });
        d.scripted.Add(new ScriptedSlot(0, "kruk_1"));
        d.scripted.Add(new ScriptedSlot(9, "anna_2"));
        d.codexAtStart = new[] { "ano_impostor", "ano_photo", "doc_death", "ppl_kruk", "lore_railway" };

        // ================= ACT III - THE DEAD PLATFORM =================
        d = Night(3, "AKT III: MARTWY PERON", "ACT III: THE DEAD PLATFORM",
            "Zimno", "The Cold",
            "Zmarli z ważnym biletem mogą jechać. Nieistniejącym i fałszerzom odmawiaj.", "The dead with a valid ticket may travel. Deny the non-existent and forgers.",
            "Dyrekcja zmieniła zdanie: zmarli mają prawo do przejazdu. Na peronie robi się zimniej i lampy migają częściej. " +
            "Do teczki dołącza historia podróży. Zwracaj uwagę na to, co dzieje się ze światłem.",
            "The Directorate has changed its mind: the dead are entitled to travel. The platform grows colder and the lamps flicker more often. " +
            "Travel history joins the folder. Pay attention to what happens to the light.",
            "Zegar na peronie zatrzymał się o 03:13. Rano znów chodził, jakby nic się nie stało. Ale wskazówki były przesunięte o jedną kreskę.",
            "The platform clock stopped at 03:13. By morning it was running again as if nothing had happened. But the hands were one mark off.",
            "06.11.1998", 255, 4);
        W(d, 30, 25, 15, 10, 5, 10, 5, 0); Allow(d, PassengerTruth.Alive, PassengerTruth.Dead);
        d.forgeChance = 0.2f; d.pleaChance = 0.35f; d.orderChance = 0.05f; d.eventCount = 2; d.anomalyChance = 0.25f;
        d.events.AddRange(new[] { "clock_stop", "train_stop", "extra_person", "knock", "flicker" });
        d.scripted.Add(new ScriptedSlot(11, "grey_1"));
        d.scripted.Add(new ScriptedSlot(5, "anna_3"));
        d.codexAtStart = new[] { "doc_travel", "evt_clock", "ano_altered", "ano_loop", "ano_echo" };

        d = Night(3, "AKT III: MARTWY PERON", "ACT III: THE DEAD PLATFORM",
            "Sprzeczne bazy", "Conflicting Databases",
            "Żywi i zmarli z biletem. Zmienione dane oznaczają odmowę. Rozkaz Dyrekcji jest ważny, jeśli podpis jest prawdziwy.",
            "The living and the dead with a ticket. Altered data means denial. A Directorate order is valid if its signature is genuine.",
            "Baza kolejowa i Rejestr zaczęły sobie przeczyć. Ktoś poprawia dane w nocy, ręcznie, jedną literą. " +
            "Od dziś dostajesz też rozkazy specjalne. Sprawdzaj podpisy.",
            "The railway database and the Registry have started to contradict each other. Someone corrects data at night, by hand, one letter at a time. " +
            "From tonight you also receive special orders. Check the signatures.",
            "Piotr Wrona zostawił na biurku klucz z numerem 13. \"Nie wiem, do czego pasuje\" - napisał. Zacząłeś się zastanawiać, czy to prawda.",
            "Piotr Wrona left a key with the number 13 on your desk. \"I do not know what it fits\", he wrote. You began to wonder whether that was true.",
            "07.11.1998", 511, 4);
        W(d, 25, 15, 10, 35, 5, 5, 5, 0); Allow(d, PassengerTruth.Alive, PassengerTruth.Dead);
        d.forgeChance = 0.2f; d.pleaChance = 0.4f; d.orderChance = 0.12f; d.eventCount = 2; d.anomalyChance = 0.3f;
        d.events.AddRange(new[] { "blackout", "doc_change", "radio", "wrong_line" });
        d.scripted.Add(new ScriptedSlot(2, "piotr_2"));
        d.scripted.Add(new ScriptedSlot(8, "marek_2"));
        d.codexAtStart = new[] { "doc_orders", "ano_signature", "loc_registryhall" };

        // ================= ACT IV - LINE 13 =================
        d = Night(4, "AKT IV: LINIA 13", "ACT IV: LINE 13",
            "Wagon 13", "Car 13",
            "Wagon 13 jest zamknięty. Żywi i zmarli z biletem mogą jechać.", "Car 13 is locked. The living and the dead with a ticket may travel.",
            "Ktoś puka od środka wagonu, którego nikt nie wpuszczał. W manifeście go nie ma. W liczniku pasażerów też nie. " +
            "Dyrekcja zabrania zbliżać się do zamka. Ale klucz od Piotra Wrony leży w twojej kieszeni.",
            "Someone knocks from inside a car nobody boarded. It is not on the manifest. It is not in the passenger count either. " +
            "The Directorate forbids going near the lock. But Piotr Wrona's key lies in your pocket.",
            "Cokolwiek jest w wagonie 13, wie już, że go słyszysz. Pukanie ustało dopiero po odjeździe pociągu.",
            "Whatever is in car 13 now knows that you can hear it. The knocking stopped only after the train had left.",
            "08.11.1998", 511, 4);
        W(d, 25, 20, 10, 10, 5, 10, 10, 10); Allow(d, PassengerTruth.Alive, PassengerTruth.Dead);
        d.forgeChance = 0.2f; d.pleaChance = 0.4f; d.orderChance = 0.1f; d.eventCount = 2; d.anomalyChance = 0.3f;
        d.events.AddRange(new[] { "knock", "count14", "photo_you", "door_open" });
        d.scriptedEvents.Add(new ScriptedEvent(6, "car13"));
        d.scripted.Add(new ScriptedSlot(5, "lena_2"));
        d.scripted.Add(new ScriptedSlot(9, "nodocs_2"));
        d.scripted.Add(new ScriptedSlot(12, "kruk_2"));
        d.codexAtStart = new[] { "loc_car13", "evt_knock", "lore_line13" };

        d = Night(4, "AKT IV: LINIA 13", "ACT IV: LINE 13",
            "Pociąg słucha", "The Train Listens",
            "Tylko żywi z biletem. Nie daj się przekonać.", "Only the living with a ticket. Do not let yourself be persuaded.",
            "Pasażerowie zaczynają mówić to, co chcesz usłyszeć. Echa powtarzają cudze zdania, a pociąg reaguje na każdą twoją decyzję: " +
            "gaśnie, drży, wyświetla zły numer linii. Nie wszystko, co mówią, jest kłamstwem. Ale wszystko jest przemyślane.",
            "Passengers begin to say what you want to hear. Echoes repeat other people's sentences, and the train reacts to every decision: " +
            "it dims, trembles, shows the wrong line number. Not everything they say is a lie. But all of it is calculated.",
            "W radiu na peronie ktoś przeczytał twoje imię i numer służbowy. Potem długo szumiało.",
            "On the platform radio someone read out your name and employee number. Then it hissed for a long time.",
            "09.11.1998", 511, 4);
        W(d, 30, 10, 10, 5, 5, 10, 25, 5); Allow(d, PassengerTruth.Alive);
        d.forgeChance = 0.2f; d.pleaChance = 0.6f; d.orderChance = 0.1f; d.eventCount = 2; d.anomalyChance = 0.3f;
        d.events.AddRange(new[] { "remember", "radio", "door_open", "wrong_line", "seen_before" });
        d.scripted.Add(new ScriptedSlot(6, "anna_4"));
        d.scripted.Add(new ScriptedSlot(11, "grey_2"));
        d.codexAtStart = new[] { "lore_line13b", "lore_admit", "evt_memory" };

        // ================= ACT V - THE CONDUCTOR =================
        d = Night(5, "AKT V: KONDUKTOR", "ACT V: THE CONDUCTOR",
            "Odbicia", "Reflections",
            "Żywi i poprawieni z zgodnym biletem. Daty muszą się zgadzać z dzisiejszą.", "The living and the amended with a matching ticket. Dates must agree with today's.",
            "Rzeczywistość zaczyna się rozjeżdżać. Dokumenty mają daty z przyszłości. Ktoś przychodzi drugi raz, choć jeszcze nie był. " +
            "Zegar na peronie chodzi w obie strony. Nie ufaj dniom.",
            "Reality begins to come apart. Documents carry dates from the future. Someone arrives for the second time though they have not yet been. " +
            "The platform clock runs both ways. Do not trust the days.",
            "Na peronie stały dwie latarnie. Rano była jedna. Nie jesteś pewien, która z nich była twoja.",
            "Two lanterns stood on the platform. In the morning there was one. You are not sure which of them was yours.",
            "10.11.1998", 511, 3);
        W(d, 25, 10, 10, 10, 25, 5, 10, 5); Allow(d, PassengerTruth.Alive, PassengerTruth.Altered);
        d.forgeChance = 0.2f; d.pleaChance = 0.4f; d.orderChance = 0.1f; d.eventCount = 3; d.anomalyChance = 0.4f;
        d.events.AddRange(new[] { "doc_change", "clock_stop", "vanish", "photo_you", "extra_person", "count14" });
        d.scripted.Add(new ScriptedSlot(3, "marek_3"));
        d.scripted.Add(new ScriptedSlot(11, "piotr_3"));
        d.codexAtStart = new[] { "lore_loop", "evt_photo", "evt_text" };

        d = Night(5, "AKT V: KONDUKTOR", "ACT V: THE CONDUCTOR",
            "Konduktor", "The Conductor",
            "Tylko wpisani w Rejestr. Żadnych zdjęć innych niż twarz przed tobą.", "Only those entered in the Registry. No photo but the face in front of you.",
            "Dokumenty zaczynają mówić o tobie. Data zatrudnienia, numer K-13-0013, zdjęcie, którego nie robiłeś. " +
            "Inspektor wraca. Człowiek w szarym płaszczu mówi twoje imię, zanim je przeczyta.",
            "The documents begin to speak about you. Hire date, number K-13-0013, a photo you never had taken. " +
            "The inspector returns. The Man in Grey says your name before he reads it.",
            "Teczka konduktora leżała na biurku otwarta. Zamknąłeś ją. Rano znów była otwarta na tej samej stronie.",
            "The conductor's file lay open on the desk. You closed it. By morning it was open again at the same page.",
            "11.11.1998", 511, 3);
        W(d, 25, 10, 10, 10, 10, 10, 10, 15); Allow(d, PassengerTruth.Alive, PassengerTruth.Altered);
        d.forgeChance = 0.2f; d.pleaChance = 0.5f; d.orderChance = 0.12f; d.eventCount = 2; d.anomalyChance = 0.4f;
        d.events.AddRange(new[] { "photo_you", "remember", "knock", "doc_change" });
        d.scriptedEvents.Add(new ScriptedEvent(7, "conductor_file"));
        d.scripted.Add(new ScriptedSlot(7, "lena_3"));
        d.scripted.Add(new ScriptedSlot(2, "kruk_3"));
        d.scripted.Add(new ScriptedSlot(5, "grey_3"));
        d.codexAtStart = new[] { "ppl_grey", "unk_thirteen" };

        // ================= ACT VI - THE LAST PASSENGER =================
        d = Night(6, "AKT VI: OSTATNI PASAŻER", "ACT VI: THE LAST PASSENGER",
            "Nikt nie kontroluje", "Nobody Is Checking",
            "Nikt już nie sprawdza twoich decyzji. Decyduj sam.", "Nobody reviews your decisions anymore. Decide for yourself.",
            "Dyrekcja milczy. Radio milczy. Kolejka składa się z twarzy, które już znasz: ludzi, którym pomogłeś, i tych, którym odmówiłeś. " +
            "Nikt nie ocenia, czy masz rację. Zostajesz z własnym sumieniem i regulaminem, który nikogo już nie obchodzi.",
            "The Directorate is silent. The radio is silent. The queue is made of faces you already know: those you helped and those you turned away. " +
            "No one judges whether you are right. You are left with your conscience and a rulebook no one cares about anymore.",
            "Pociąg stał na peronie dłużej niż zwykle. Zanim odjechał, ktoś wpisał w rozkładzie jazdy: \"Jutro ostatni\".",
            "The train stood at the platform longer than usual. Before it left, someone wrote in the timetable: \"Tomorrow the last\".",
            "12.11.1998", 511, -1);
        W(d, 20, 15, 10, 10, 10, 10, 15, 10); Allow(d, PassengerTruth.Alive);
        d.judged = false; d.pleaChance = 0.7f; d.forgeChance = 0.1f; d.eventCount = 2; d.anomalyChance = 0.4f;
        d.events.AddRange(new[] { "remember", "knock", "vanish", "radio", "clock_stop" });
        d.scripted.Add(new ScriptedSlot(4, "anna_5"));
        d.scripted.Add(new ScriptedSlot(8, "marek_4"));
        d.scripted.Add(new ScriptedSlot(10, "lena_4"));
        d.scripted.Add(new ScriptedSlot(6, "nodocs_3"));
        d.codexAtStart = new[] { "unk_first", "unk_you" };

        // ================= ACT VII - THE LAST PLATFORM =================
        d = Night(7, "AKT VII: OSTATNI PERON", "ACT VII: THE LAST PLATFORM",
            "Ostatni peron", "The Last Platform",
            "Decyduj sam. Kolejka się kończy.", "Decide for yourself. The queue is ending.",
            "Dwunastu pasażerów. Potem jeszcze jeden. System pokaże liczbę, która się zgadza z twoim numerem służbowym. " +
            "Zanim wpuścisz ostatniego, terminal Rejestru zapyta cię o coś jeszcze.",
            "Twelve passengers. Then one more. The system will show a number that matches your employee number. " +
            "Before you admit the last one, the Registry terminal will ask you something else.",
            "", "", "13.11.1998", 511, -1);
        W(d, 20, 15, 15, 10, 10, 5, 10, 15); Allow(d, PassengerTruth.Alive);
        d.judged = false; d.finalDay = true; d.passengers = 13; d.pleaChance = 0.8f; d.eventCount = 1; d.anomalyChance = 0.4f;
        d.events.AddRange(new[] { "remember", "knock" });
        d.scripted.Add(new ScriptedSlot(0, "grey_4"));
        d.scripted.Add(new ScriptedSlot(4, "anna_6"));
        d.scripted.Add(new ScriptedSlot(7, "lena_5"));
        d.scripted.Add(new ScriptedSlot(10, "marek_5"));
        d.scriptedEvents.Add(new ScriptedEvent(12, "registry_terminal"));
        d.codexAtStart = new[] { "unk_secret" };
    }
}
