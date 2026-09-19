using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// The random and scripted events of the night. Each event has an id, a phase (between passengers or while one
// stands at the booth) and conditions. Story nights list the events they allow (DayConfig.events);
// Night Shift picks from a pool that grows with the level.
public static class EventLibrary
{
    static readonly Color Red = new Color(0.9f, 0.35f, 0.3f);
    static readonly Color Amber = new Color(0.95f, 0.8f, 0.4f);

    // Events that need a passenger at the booth (they run after the passenger has arrived).
    public static bool NeedsPassenger(string id)
    {
        switch (id)
        {
            case "seen_before": case "remember": case "photo_you": case "doc_change": return true;
            default: return false;
        }
    }

    // Events that make sense for the given endless level.
    public static List<string> EndlessPool(int level)
    {
        var l = new List<string> { "flicker", "blackout" };
        if (level >= 2) { l.Add("knock"); l.Add("wrong_line"); l.Add("doc_change"); l.Add("door_open"); }
        if (level >= 3) { l.Add("seen_before"); l.Add("clock_stop"); l.Add("train_stop"); l.Add("radio"); }
        if (level >= 4) { l.Add("extra_person"); l.Add("vanish"); l.Add("count14"); l.Add("photo_you"); }
        if (level >= 5) { l.Add("remember"); }
        return l;
    }

    // Conditions checked just before the event runs.
    public static bool CanRun(GameContext c, string id)
    {
        switch (id)
        {
            case "photo_you":   return c.current != null && !c.current.isPlayer && DocumentSystem.Get(c.current, DocumentType.Id) != null;
            case "doc_change":  return c.current != null && !c.current.isPlayer;
            case "seen_before": return c.current != null && !c.current.isPlayer && c.current.storyId == null;
            case "remember":    return c.current != null && !c.current.isPlayer;
            case "vanish":      return c.stage != null;
            default:            return true;
        }
    }

    public static IEnumerator Play(GameContext c, string id)
    {
        if (!CanRun(c, id)) yield break;
        switch (id)
        {
            case "flicker":       yield return Flicker(c); break;
            case "blackout":      yield return Blackout(c); break;
            case "knock":         yield return Knock(c); break;
            case "clock_stop":    yield return ClockStop(c); break;
            case "seen_before":   yield return SeenBefore(c); break;
            case "extra_person":  yield return ExtraPerson(c); break;
            case "count14":       yield return Count14(c); break;
            case "photo_you":     yield return PhotoYou(c); break;
            case "train_stop":    yield return TrainStop(c); break;
            case "vanish":        yield return Vanish(c); break;
            case "doc_change":    yield return DocChange(c); break;
            case "remember":      yield return Remember(c); break;
            case "wrong_line":    yield return WrongLine(c); break;
            case "door_open":     yield return DoorOpen(c); break;
            case "radio":         yield return Radio(c); break;
            case "car13":         yield return Car13(c); break;
            case "conductor_file": yield return ConductorFile(c); break;
            case "registry_terminal": yield return RegistryTerminal(c); break;
        }
        if (c.RefreshHud != null) c.RefreshHud();
    }

    static void Note(string title, string body, Color col)
    {
        ToastUI.Show(title, body, col);
    }

    // ---------------- random events ----------------

    static IEnumerator Flicker(GameContext c)
    {
        c.lighting.FlickerLantern(2.2f);
        c.trainView.FlickerFor(2.2f);
        if (SoundFX.I != null) SoundFX.I.PowerDown();
        PlayerStats.AddStress(2);
        c.trainSys.AddElectricity(-6f);
        Codex.Unlock("evt_blackout", true);
        yield return new WaitForSeconds(2.2f);
        if (SoundFX.I != null) SoundFX.I.PowerUp();
    }

    static IEnumerator Blackout(GameContext c)
    {
        Note(Loc.T("ZASILANIE", "POWER"), Loc.T("Zgasły wszystkie światła.", "All the lights went out."), Amber);
        if (SoundFX.I != null) SoundFX.I.PowerDown();
        c.trainView.SetBlackout(true);
        c.horror.FreezeEffects(true);
        yield return c.lighting.Blackout(3.6f);
        c.trainView.SetBlackout(false);
        c.horror.FreezeEffects(false);
        if (SoundFX.I != null) SoundFX.I.PowerUp();
        PlayerStats.AddStress(3);
        c.trainSys.AddElectricity(-10f);
        Codex.Unlock("evt_blackout");
    }

    static IEnumerator Knock(GameContext c)
    {
        bool late = c.StoryMode && c.nightNumber >= 6;
        if (SoundFX.I != null) { if (late) SoundFX.I.KnockThirteen(); else SoundFX.I.Knock(); }
        c.horror.Bump(3f, 0.4f);
        c.trainView.Bump(0.4f);
        Codex.Unlock("evt_knock");
        yield return new WaitForSeconds(1.2f);

        c.overlay.SetBackground(new Color(0.02f, 0.02f, 0.05f, 0.82f));
        yield return c.overlay.Show(Loc.T("PUKANIE", "KNOCKING"),
            Loc.T("Z wnętrza pociągu ktoś puka. Trzy razy, potem cisza, potem znowu trzy razy. Rytm nie zmienia się od dłuższej chwili.",
                  "Someone is knocking from inside the train. Three times, silence, then three again. The rhythm has not changed for a while."),
            Loc.T("POSŁUCHAJ", "LISTEN"), Loc.T("ZIGNORUJ", "IGNORE"), null, false);
        c.overlay.SetBackground(new Color(0.02f, 0.02f, 0.05f, 0.95f));

        if (c.overlay.Choice == 0)
        {
            StoryFlags.Set(Flag.ListenedToKnocking);
            AchievementSystem.Unlock("listened");
            PlayerStats.AddStress(4);
            SkillSystem.Give(Skill.Composure, 6);
            c.lighting.FlickerLantern(1.5f);
            if (SoundFX.I != null) SoundFX.I.Whisper();
            Note(Loc.T("PUKANIE", "KNOCKING"), Loc.T("Ktoś powtarza twoje imię. Cicho, za ścianą.", "Someone repeats your name. Quietly, behind the wall."), Red);
            yield return new WaitForSeconds(1.5f);
        }
        else
        {
            PlayerStats.AddStress(1);
            SkillSystem.Give(Skill.Authority, 3);
        }
    }

    static IEnumerator ClockStop(GameContext c)
    {
        if (c.StopClock != null) c.StopClock(true);
        Codex.Unlock("evt_clock");
        Note(Loc.T("ZEGAR", "CLOCK"), Loc.T("Zegar zatrzymał się na 03:13.", "The clock has stopped at 03:13."), Amber);
        if (SoundFX.I != null) SoundFX.I.Stinger(0.3f);
        PlayerStats.AddStress(2);
        yield return new WaitForSeconds(1f);
    }

    static IEnumerator SeenBefore(GameContext c)
    {
        c.dialogue.Interject(Loc.T("\"Widziałem już pana. Wczoraj, o tej samej porze. Powiedział pan dokładnie to samo.\"",
                                   "\"I have seen you before. Yesterday, at this very hour. You said exactly the same thing.\""));
        c.lighting.FlickerLantern(0.8f);
        PlayerStats.AddStress(2);
        SkillSystem.Give(Skill.Observation, 3);
        Codex.Unlock("evt_memory");
        yield return new WaitForSeconds(0.6f);
    }

    static IEnumerator ExtraPerson(GameContext c)
    {
        c.stage.ShowExtraFigure();
        if (SoundFX.I != null) SoundFX.I.Footstep();
        Note(Loc.T("PERON", "PLATFORM"), Loc.T("Na końcu kolejki stoi ktoś, kogo nie było na liście.", "Someone stands at the end of the queue who was not on the list."), Red);
        PlayerStats.AddStress(2);
        Codex.Unlock("evt_14");
        yield return new WaitForSeconds(1.5f);
    }

    static IEnumerator Count14(GameContext c)
    {
        if (c.ShowRemaining != null) c.ShowRemaining(14, 4f);
        Note(Loc.T("SYSTEM", "SYSTEM"), Loc.T("Liczba pasażerów: 14.", "Passenger count: 14."), Red);
        AchievementSystem.Unlock("fourteen");
        if (SoundFX.I != null) SoundFX.I.Stinger(0.4f);
        PlayerStats.AddStress(3);
        Codex.Unlock("evt_14");
        yield return new WaitForSeconds(4f);
    }

    static IEnumerator PhotoYou(GameContext c)
    {
        DocumentData id = DocumentSystem.Get(c.current, DocumentType.Id);
        if (id == null) yield break;
        id.photoIsPlayer = true;
        c.current.playerPhoto = true;
        if (c.RefreshPortrait != null) c.RefreshPortrait();
        if (SoundFX.I != null) SoundFX.I.Stinger(0.3f);
        c.trainView.FlickerFor(0.5f);
        PlayerStats.AddStress(2);
        Codex.Unlock("evt_photo");
        yield return new WaitForSeconds(0.8f);
    }

    static IEnumerator TrainStop(GameContext c)
    {
        if (SoundFX.I != null) SoundFX.I.PowerDown();
        c.trainView.Bump(1.5f);
        c.trainView.FlickerFor(2.5f);
        c.horror.Bump(4f, 0.8f);
        c.trainSys.AddElectricity(-8f);
        Note(Loc.T("POCIĄG", "TRAIN"), Loc.T("Pociąg zatrzymał się bez sygnału. Nikt nie wie dlaczego.", "The train stopped without a signal. No one knows why."), Amber);
        PlayerStats.AddStress(2);
        Codex.Unlock("evt_stop");
        yield return new WaitForSeconds(2.5f);
        if (SoundFX.I != null) SoundFX.I.PowerUp();
    }

    static IEnumerator Vanish(GameContext c)
    {
        c.stage.VanishFromQueue();
        if (SoundFX.I != null) SoundFX.I.Whisper();
        PlayerStats.AddStress(3);
        Codex.Unlock("evt_vanish");
        yield return new WaitForSeconds(1.6f);
    }

    // The words on the ticket rearrange themselves for a moment, then return.
    static IEnumerator DocChange(GameContext c)
    {
        var t = c.ui.ticketText;
        if (t == null || string.IsNullOrEmpty(t.text)) yield break;
        string original = t.text;
        string changed = Loc.T("<b>BILET</b>\nNIE JESTEŚ TU\nNIE JESTEŚ TU\nNIE JESTEŚ TU", "<b>TICKET</b>\nYOU ARE NOT HERE\nYOU ARE NOT HERE\nYOU ARE NOT HERE");
        if (SoundFX.I != null) SoundFX.I.Whisper();
        t.text = changed;
        yield return new WaitForSeconds(1.3f);
        if (t != null) t.text = original;
        PlayerStats.AddStress(2);
        Codex.Unlock("evt_text");
    }

    static IEnumerator Remember(GameContext c)
    {
        c.dialogue.Interject(Loc.T("\"Czy pan też pamięta poprzednią noc? Bo ja pamiętam ją bardzo dokładnie. Byłem tu. Wszyscy tu byliśmy.\"",
                                   "\"Do you remember last night too? Because I remember it very clearly. I was here. We were all here.\""));
        yield return new WaitForSeconds(1.0f);
        c.overlay.SetBackground(new Color(0.02f, 0.02f, 0.05f, 0.82f));
        yield return c.overlay.Show(Loc.T("POPRZEDNIA NOC", "LAST NIGHT"),
            Loc.T("Pytanie zawisło w powietrzu. Czy naprawdę pamiętasz, co robiłeś wczoraj?", "The question hangs in the air. Do you really remember what you did yesterday?"),
            Loc.T("\"TAK, PAMIĘTAM\"", "\"YES, I REMEMBER\""), Loc.T("\"NIE, NIC\"", "\"NO, NOTHING\""), null, false);
        c.overlay.SetBackground(new Color(0.02f, 0.02f, 0.05f, 0.95f));
        if (c.overlay.Choice == 0)
        {
            StoryFlags.Set(Flag.RememberedLastNight);
            PlayerStats.AddStress(3);
            SkillSystem.Give(Skill.Observation, 4);
        }
        else
        {
            StoryFlags.Set(Flag.ForgotLastNight);
            PlayerStats.Relax(2);
            SkillSystem.Give(Skill.Composure, 4);
        }
        Codex.Unlock("evt_memory");
    }

    static IEnumerator WrongLine(GameContext c)
    {
        string[] wrong = { "31", "1 3", "??", "14" };
        yield return c.trainSys.ShowWrongLine(wrong[Random.Range(0, wrong.Length)], 5f);
        PlayerStats.AddStress(1);
        Codex.Unlock("lore_line13");
    }

    static IEnumerator DoorOpen(GameContext c)
    {
        c.trainView.SetDoorOpen(true);
        if (SoundFX.I != null) SoundFX.I.Door();
        yield return new WaitForSeconds(4.5f);
        c.trainView.SetDoorOpen(false);
        if (SoundFX.I != null) SoundFX.I.Door();
        PlayerStats.AddStress(1);
    }

    static IEnumerator Radio(GameContext c)
    {
        if (SoundFX.I != null) SoundFX.I.RadioOn(true);
        yield return new WaitForSeconds(2.5f);
        c.overlay.SetBackground(new Color(0.02f, 0.02f, 0.05f, 0.82f));
        yield return c.overlay.Show(Loc.T("RADIO", "RADIO"),
            Loc.T("Przez szum przebija się głos Dyrekcji: \"Do odwołania nie sprawdzać drugiego dokumentu. Zaufać kolei.\"",
                  "A voice from the Directorate cuts through the static: \"Until further notice do not check the second document. Trust the railway.\""),
            Loc.T("POSŁUCHAJ ROZKAZU", "OBEY THE ORDER"), Loc.T("ZIGNORUJ RADIO", "IGNORE THE RADIO"), null, false);
        c.overlay.SetBackground(new Color(0.02f, 0.02f, 0.05f, 0.95f));
        if (c.overlay.Choice == 0)
        {
            StoryFlags.Set(Flag.TrustedRailway);
            PlayerStats.AddDiscipline(1);
            PlayerStats.AddReputation(3);
            SkillSystem.Give(Skill.Authority, 3);
        }
        else
        {
            StoryFlags.Set(Flag.DistrustedRailway);
            PlayerStats.AddConscience(1);
            PlayerStats.AddReputation(-2);
            SkillSystem.Give(Skill.Observation, 3);
        }
        if (SoundFX.I != null) SoundFX.I.RadioOn(false);
    }

    // ---------------- scripted story events ----------------

    static IEnumerator Car13(GameContext c)
    {
        if (SoundFX.I != null) SoundFX.I.KnockThirteen();
        c.horror.Bump(4f, 0.6f);
        c.trainView.SetDoorOpen(true);
        c.lighting.FlickerLantern(2f);
        yield return new WaitForSeconds(1.6f);

        c.overlay.SetBackground(new Color(0.02f, 0.02f, 0.05f, 0.9f));
        yield return c.overlay.Show(Loc.T("WAGON 13", "CAR 13"),
            Loc.T("Ostatni wagon składu, ten z numerem, którego nie ma w rozkładzie, otworzył się sam. W środku pali się światło. Na siedzeniach leżą bilety. Wszystkie mają dzisiejszą datę i wszystkie są na nazwiska ludzi, których dziś odprawiłeś.",
                  "The last car of the train, the one with a number that is not in the timetable, opened by itself. A light is on inside. Tickets lie on the seats. All of them carry tonight's date, and all are in the names of people you processed tonight."),
            Loc.T("WEJDŹ DO WAGONU", "ENTER THE CAR"), Loc.T("ZOSTAŃ NA PERONIE", "STAY ON THE PLATFORM"), null, true);
        c.overlay.SetBackground(new Color(0.02f, 0.02f, 0.05f, 0.95f));

        if (c.overlay.Choice == 0)
        {
            StoryFlags.Set(Flag.EnteredRestrictedCar);
            StoryFlags.Set(Flag.LearnedLine13);
            StoryFlags.Set(Flag.FoundHiddenRegistry);
            AchievementSystem.Unlock("line13");
            Codex.Unlock("loc_car13");
            Codex.Unlock("unk_hidden");
            Codex.Unlock("lore_line13");
            PlayerStats.AddStress(8);
            SkillSystem.Give(Skill.Composure, 12);
            SkillSystem.Give(Skill.Observation, 8);
            Note(Loc.T("WAGON 13", "CAR 13"),
                 Loc.T("Pod siedzeniem leży druga księga Rejestru. Twoje imię jest na pierwszej stronie.", "Under a seat lies the second book of the Registry. Your name is on the first page."), Red);
            c.trainSys.AddSignal(-15f);
            c.trainSys.AddTemperature(-12f);
        }
        else
        {
            PlayerStats.AddDiscipline(1);
            PlayerStats.Relax(3);
            SkillSystem.Give(Skill.Authority, 6);
        }
        c.trainView.SetDoorOpen(false);
        yield return new WaitForSeconds(0.4f);
    }

    static IEnumerator ConductorFile(GameContext c)
    {
        Codex.Unlock("ppl_conductor");
        c.overlay.SetBackground(new Color(0.02f, 0.02f, 0.05f, 0.9f));
        yield return c.overlay.Show(Loc.T("TECZKA KONDUKTORA", "THE CONDUCTOR'S FILE"),
            Loc.T("Na biurku leży teczka. Nazwisko: Jan Ostoja. Numer służbowy: K-13-0013. Zatrudniony: 13.11.1987. Zwolniony: nigdy. Ostatnia adnotacja jest z dzisiaj i brzmi: \"Nie odpowiada na pytanie, kim jest\".",
                  "A file lies on the desk. Name: Jan Ostoja. Employee number: K-13-0013. Hired: 13.11.1987. Dismissed: never. The last note is from today and reads: \"Does not answer the question of who he is\"."),
            Loc.T("PRZECZYTAJ DO KOŃCA", "READ IT TO THE END"), Loc.T("ZAMKNIJ TECZKĘ", "CLOSE THE FILE"), null, true);
        c.overlay.SetBackground(new Color(0.02f, 0.02f, 0.05f, 0.95f));

        if (c.overlay.Choice == 0)
        {
            StoryFlags.Set(Flag.ReadConductorFile);
            Codex.Unlock("unk_first");
            Codex.Unlock("unk_you");
            PlayerStats.AddStress(6);
            SkillSystem.Give(Skill.Documents, 10);
            SkillSystem.Give(Skill.Composure, 8);
            if (SoundFX.I != null) SoundFX.I.Stinger(0.5f);
            Note(Loc.T("TECZKA", "FILE"), Loc.T("Ostatnia strona jest twoim aktem zgonu. Data: dzisiaj.", "The last page is your death record. Date: today."), Red);
        }
        else
        {
            PlayerStats.Relax(2);
            PlayerStats.AddDiscipline(1);
        }
        yield return new WaitForSeconds(0.4f);
    }

    // Before the last passenger: the Registry terminal asks what to do with the book.
    static IEnumerator RegistryTerminal(GameContext c)
    {
        bool hidden = StoryFlags.Has(Flag.FoundHiddenRegistry);
        string body = Loc.T("Terminal Rejestru budzi się i wyświetla jedno pytanie: \"CO ZROBIĆ Z REJESTREM?\". Kursor miga. Pod spodem jest drugi wiersz: \"PASAŻERÓW POZOSTAŁO: 1\".",
                            "The Registry terminal wakes up and shows a single question: \"WHAT SHOULD BE DONE WITH THE REGISTRY?\". The cursor blinks. Below it is a second line: \"PASSENGERS REMAINING: 1\".");
        c.overlay.SetBackground(new Color(0.02f, 0.02f, 0.05f, 0.92f));
        if (SoundFX.I != null) SoundFX.I.Stinger(0.4f);
        yield return c.overlay.Show(Loc.T("TERMINAL REJESTRU", "REGISTRY TERMINAL"), body,
            Loc.T("ZOSTAW, JAK JEST", "LEAVE IT AS IT IS"),
            Loc.T("WYMAŻ REJESTR", "ERASE THE REGISTRY"),
            hidden ? Loc.T("PRZYWRÓĆ WYKREŚLONYCH", "RESTORE THE STRUCK OUT") : null, true);
        c.overlay.SetBackground(new Color(0.02f, 0.02f, 0.05f, 0.95f));

        if (c.overlay.Choice == 1)
        {
            StoryFlags.Set(Flag.ErasedRegistry);
            PlayerStats.AddStress(5);
            c.trainSys.AddSignal(-25f);
            c.trainSys.AddElectricity(-20f);
            if (SoundFX.I != null) SoundFX.I.PowerDown();
            c.trainView.FlickerFor(3f);
        }
        else if (c.overlay.Choice == 2)
        {
            StoryFlags.Set(Flag.RestoredRegistry);
            PlayerStats.AddConscience(2);
            PlayerStats.Relax(6);
            if (SoundFX.I != null) SoundFX.I.PowerUp();
        }
        else
        {
            PlayerStats.AddDiscipline(1);
        }
        yield return new WaitForSeconds(0.5f);
    }
}
