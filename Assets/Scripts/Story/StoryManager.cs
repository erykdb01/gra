using System.Collections.Generic;
using UnityEngine;

// Story Mode: loading and saving of the campaign and the calculation of the ending.
public static class StoryManager
{
    public static int NightCount { get { return StoryData.Nights.Count; } }

    public static DayConfig Night(int index)
    {
        List<DayConfig> n = StoryData.Nights;
        return n[Mathf.Clamp(index, 0, n.Count - 1)];
    }

    // Prepares PlayerStats, flags and skills for the story and returns the index of the night to play.
    public static int Begin()
    {
        StoryProgress s = SaveSystem.Data.story;
        if (!s.started) { s.started = true; s.finished = false; }
        PlayerStats.LoadFrom(s);
        StoryFlags.LoadFrom(s.flags);
        SkillSystem.Bind(s.skillXp);
        AchievementSystem.ResetRunCounters();

        int night = GameState.ForcedNight >= 0 ? GameState.ForcedNight : s.night;
        GameState.ForcedNight = -1;
        return Mathf.Clamp(night, 0, NightCount - 1);
    }

    public static int SavedIntegrity { get { return SaveSystem.Data.story.integrity; } }

    // Saves the state after a finished night; nextNight is the night to play next.
    public static void Commit(int nextNight, int integrity)
    {
        StoryProgress s = SaveSystem.Data.story;
        PlayerStats.StoreTo(s);
        s.flags = StoryFlags.ToList();
        s.night = nextNight;
        s.integrity = Mathf.Clamp(integrity, 1, 100);
        s.started = true;
        SaveSystem.Save();
    }

    // ---------------- endings ----------------

    // admitted = the conductor let himself board.
    public static string CalculateEnding(bool admitted)
    {
        if (StoryFlags.Has(Flag.ErasedRegistry)) return "b";
        int helped = StoryFlags.CountWithPrefix("HelpedPassenger_");
        if (admitted && StoryFlags.Has(Flag.RestoredRegistry) && StoryFlags.Has(Flag.FoundHiddenRegistry) && helped >= 4) return "s";
        if (admitted && StoryFlags.Has(Flag.FoundHiddenRegistry) && StoryFlags.Has(Flag.MetTheConductor) && PlayerStats.Conscience >= PlayerStats.Discipline) return "c";
        if (admitted) return "d";
        return "a";
    }

    public static void EndingText(string id, out string title, out string text)
    {
        bool warm = PlayerStats.Conscience >= PlayerStats.Discipline;
        switch (id)
        {
            case "b":
                title = Loc.T("KONIEC B: ZŁAMANY SYSTEM", "ENDING B: THE BROKEN SYSTEM");
                text = Loc.T("Terminal gaśnie. Rejestr znika kolumna po kolumnie, a razem z nim lista, która decydowała, kto istnieje. Na peronie stoją ludzie bez nazwisk, bez dat, bez numerów. Nikt już nie może ich odmówić.\n\n" +
                             "Pociąg nie odjeżdża. Kolej nie wie, dokąd jechać, skoro nikt nie ma biletu. Ty stoisz z latarnią i pierwszy raz od dawna nikt nie mówi ci, co masz robić. To wolność albo pustka. Może jedno i drugie.",
                             "The terminal goes dark. The Registry disappears column by column, and with it the list that decided who exists. On the platform stand people without names, without dates, without numbers. No one can turn them away anymore.\n\n" +
                             "The train does not leave. The railway does not know where to go if no one holds a ticket. You stand with your lantern and for the first time in a long while no one tells you what to do. It is freedom or emptiness. Perhaps both.");
                break;
            case "s":
                title = Loc.T("KONIEC: PERON CZTERNASTY", "ENDING: PLATFORM FOURTEEN");
                text = Loc.T("Drugi Rejestr otwiera się w twoich rękach. Wykreśleni wracają na listę jeden po drugim: Anna, Marek, Lena, człowiek bez dokumentów. Ci, którym pomogłeś, czekają w wagonach i robią ci miejsce.\n\n" +
                             "Wsiadasz. Pociąg rusza, ale nie w stronę stacji 13. Za peronem 13 jest jeszcze jeden, którego nie ma w żadnym rozkładzie. Na tablicy błyska napis: PASAŻERÓW: 14. Po raz pierwszy liczba się zgadza.",
                             "The second Registry opens in your hands. The struck-out return to the list one by one: Anna, Marek, Lena, the man without papers. Those you helped wait in the cars and make room for you.\n\n" +
                             "You board. The train departs, but not toward station 13. Behind platform 13 there is another one, in no timetable. The board flashes: PASSENGERS: 14. For the first time the number is right.");
                break;
            case "c":
                title = Loc.T("KONIEC C: PRAWDA", "ENDING C: THE TRUTH");
                text = Loc.T("Wpuszczasz siebie i wiesz już, kim jesteś. Jan Ostoja, konduktor, który przed laty odmówił sobie miejsca w pociągu i został na peronie. Każdy kolejny konduktor to on. Każda noc to ta sama noc.\n\n" +
                             "Drzwi otwierają się z cichym westchnieniem. W wagonach siedzą ci, których nie zostawiłeś na peronie. Pociąg rusza, a peron 13 po raz pierwszy jest pusty. Nie musiałeś być tylko konduktorem od zasad. Musiałeś tylko przeczytać własne dokumenty.",
                             "You let yourself in and now you know who you are. Jan Ostoja, the conductor who years ago denied himself a seat on the train and stayed on the platform. Every conductor since has been him. Every night is the same night.\n\n" +
                             "The doors open with a quiet sigh. In the cars sit those you did not leave on the platform. The train departs, and platform 13 is empty for the first time. You did not have to be only a conductor of rules. You only had to read your own documents.");
                break;
            case "d":
                title = Loc.T("KONIEC D: CZĘŚĆ LINII 13", "ENDING D: PART OF LINE 13");
                text = Loc.T("Wpuszczasz siebie, ale nie wiesz, po co. Pociąg rusza, wagony są pełne i ciche. Za oknem migają lampy peronu, ciągle te same. Ktoś obok mówi twoim głosem: \"Bilety do kontroli\".\n\n" +
                             "Jesteś teraz częścią składu. Następnej nocy na peronie pojawi się nowy konduktor i nikt mu nie powie, że poprzedni jedzie w wagonie trzynastym. Kolej działa bez zarzutu. Tylko dokąd?",
                             "You let yourself in, but you do not know why. The train departs, the cars are full and quiet. Outside the window the platform lamps flicker past, always the same ones. Someone beside you says in your voice: \"Tickets, please\".\n\n" +
                             "You are now part of the train. Next night a new conductor will appear on the platform and no one will tell him that the last one rides in car thirteen. The railway runs flawlessly. But to where?");
                break;
            default:
                title = warm ? Loc.T("KONIEC A: ZOSTAJESZ", "ENDING A: YOU STAY") : Loc.T("KONIEC A: IDEALNY KONDUKTOR", "ENDING A: THE PERFECT CONDUCTOR");
                text = warm
                    ? Loc.T("Odmawiasz sobie. Pociąg odjeżdża bez ciebie, a ty stoisz na peronie jak co noc. Ale teraz wiesz, ile osób zostawiłeś, i ile wpuściłeś wbrew rozkazom.\n\nBędą kolejne noce, kolejne kolejki i ludzie, którzy poproszą o jeszcze jedną szansę. Zostajesz, żeby ich wpuszczać.",
                            "You deny yourself. The train leaves without you, and you stand on the platform like every night. But now you know how many you left behind, and how many you admitted against orders.\n\nThere will be more nights, more queues and people asking for one more chance. You stay to let them in.")
                    : Loc.T("Odmawiasz sobie. Zgodnie z przepisami zmarły konduktor nie ma prawa do biletu. Pociąg odjeżdża, peron cichnie.\n\nJutro zaczniesz nową zmianę i nie będziesz pamiętał tej nocy. Dyrekcja jest z ciebie dumna.",
                            "You deny yourself. By the rules a dead conductor has no right to a ticket. The train leaves, the platform falls silent.\n\nTomorrow you will start a new shift and you will not remember this night. The Directorate is proud of you.");
                break;
        }
    }

    public static string EndingAchievement(string id)
    {
        switch (id)
        {
            case "a": return "ending_a";
            case "b": return "ending_b";
            case "c": return "ending_c";
            case "d": return "ending_d";
            default: return "ending_s";
        }
    }
}
