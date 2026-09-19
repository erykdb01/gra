using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// The game's conductor: sets up the scene, runs the nights one after another (Story or Night Shift),
// takes the decisions and hands the details to the systems (documents, dialogue, horror, train, progression).
public class GameManager : MonoBehaviour
{
    public UIController ui;                 // drag the UI object here
    public int passengersPerShift = 13;     // kept for the scene; the day configs decide the real numbers
    public GameObject menuButton;           // old MENU button (kept hidden; the pause menu replaces it)

    // used by the pause menu ("Restart night"): story night index or endless level - 1
    public static int CurrentNight;

    const int PassengersPerNight = 13;

    Canvas canvas;
    TrainView train;
    TrainSystem trainSys;
    HudPanel hud;
    DialoguePanel dialogue;
    Overlay overlay;
    ShiftResultsUI results;
    StampEffect stamp;
    StageView stage;
    LightingController lighting;
    HorrorManager horror;
    ClockUI clock;
    DocumentInspector inspector;
    PauseMenu pause;
    SoundFX sfx;
    GameObject portraitRoot;
    Image portraitImage;
    readonly List<DraggableDocument> docs = new List<DraggableDocument>();
    Button btnAdmit, btnDeny, btnInspect, btnStatus;
    TextMeshProUGUI lblInspect, lblStatus;

    readonly GameContext ctx = new GameContext();
    readonly DialogueManager dm = new DialogueManager();

    PassengerData current;
    DayConfig day;
    bool endless;
    int nightIndex;                 // story: index of the night; endless: level - 1
    int decision;                   // 0 = waiting, 1 = admit, 2 = deny
    bool acceptInput, timedOut;
    bool failed;
    string failTitle, failText;

    ShiftReport report;
    int startDiscipline, startConscience;
    int streak;
    int clockMinutes;
    bool clockStopped;
    int passengerNumber;            // 1..13
    int fakeRemaining = -1;
    float timeLeft;
    int shownSeconds = -1;
    bool revealTruth;

    // snapshot to retry a failed night
    int snapCon, snapDis, snapStress, snapRep, snapCorrect, snapMistakes;
    List<string> snapFlags = new List<string>();
    float snapIntegrity;

    void Start()
    {
        if (ui == null)
        {
            Debug.LogError("GameManager: the 'ui' field is not assigned.");
            return;
        }
        Setup();
        StartCoroutine(Run());
    }

    // ---------- setup ----------

    void Setup()
    {
        Time.timeScale = 1f;
        UIState.Modal = 0;
        UIState.EscFrame = -1;
        PauseMenu.IsPaused = false;

        canvas = ui.GetComponentInParent<Canvas>();
        if (canvas == null) canvas = FindAnyObjectByType<Canvas>();
        canvas = canvas.rootCanvas;
        UIKit.Font = ui.ticketText.font;
        ui.ApplyStyle();

        Transform ct = canvas.transform;

        // wooden desk
        Transform desk = ct.Find("Desk");
        if (desk != null)
        {
            Image di = desk.GetComponent<Image>();
            if (di != null) { di.sprite = PixelArt.Wood(); di.color = Color.white; }
        }

        // paper documents that can be dragged (and clicked to inspect)
        Transform documents = desk != null ? desk.Find("Documents") : null;
        if (documents != null)
        {
            List<Transform> kids = new List<Transform>();
            foreach (Transform child in documents) kids.Add(child);
            for (int i = 0; i < kids.Count; i++)
            {
                Image im = kids[i].GetComponent<Image>();
                if (im != null)
                {
                    im.sprite = PixelArt.Paper(72, 66, i + 1);
                    im.color = Color.white;
                    im.raycastTarget = true;
                }
                DraggableDocument dd = kids[i].gameObject.AddComponent<DraggableDocument>();
                dd.Init((RectTransform)documents);
                DocumentType type = TypeOfDesk(kids[i], i);
                dd.OnClicked = () => OpenInspector(type);
                docs.Add(dd);
            }
        }

        // photo on the ID card
        Transform idCard = documents != null ? documents.Find("IdCard") : null;
        if (idCard != null)
        {
            Image frame = UIKit.Img("PortraitFrame", idCard, PixelArt.Solid(), new Color(0.15f, 0.12f, 0.10f, 1f));
            UIKit.Anchor(frame.rectTransform, Vector2.one, Vector2.one, Vector2.one, new Vector2(-12, -12), new Vector2(112, 118));
            Image bg = UIKit.Img("PortraitBg", frame.transform, PixelArt.Solid(), new Color(0.27f, 0.34f, 0.50f, 1f));
            UIKit.Stretch(bg.rectTransform);
            bg.rectTransform.offsetMin = new Vector2(4, 4);
            bg.rectTransform.offsetMax = new Vector2(-4, -4);
            portraitImage = UIKit.Img("Portrait", bg.transform, null, Color.white);
            UIKit.Stretch(portraitImage.rectTransform);
            portraitRoot = frame.gameObject;
            portraitRoot.SetActive(false);
        }

        // buttons
        btnAdmit = FindButton(ct, "BtnAdmit");
        btnDeny = FindButton(ct, "BtnDeny");
        Skin(btnAdmit, new Color(0.55f, 0.85f, 0.55f));
        Skin(btnDeny, new Color(0.9f, 0.5f, 0.45f));
        AddButtonFX(btnAdmit); AddButtonFX(btnDeny);
        if (menuButton != null) menuButton.SetActive(false);

        // world layers: backdrop, train, lamps, people, rain and fog (all below the desk)
        Image backdrop = PixelArt.CreateBackdrop(ct);
        train = TrainView.Create(canvas);
        stage = StageView.Create(canvas, train);
        lighting = LightingController.Create(canvas, stage);
        backdrop.transform.SetSiblingIndex(0);
        train.transform.SetSiblingIndex(1);
        lighting.BackLayer.SetSiblingIndex(2);
        stage.transform.SetSiblingIndex(3);
        lighting.FrontLayer.SetSiblingIndex(4);

        // interface
        hud = HudPanel.Create(canvas);
        dialogue = DialoguePanel.Create(canvas);
        dialogue.OnAsk = OnAsk;
        dialogue.OnTalking = stage.SetTalking;
        stamp = StampEffect.Create(canvas);
        clock = ClockUI.Create(canvas);
        overlay = Overlay.Create(canvas);
        results = ShiftResultsUI.Create(canvas);
        sfx = SoundFX.Create(gameObject);
        trainSys = TrainSystem.Create(gameObject, train);
        horror = HorrorManager.Create(gameObject, lighting, train, stage);
        hud.SetTrain(trainSys);
        inspector = DocumentInspector.Create(canvas);
        inspector.Setup(trainSys, SpendMinutes);
        pause = PauseMenu.Create(canvas);

        // extra buttons under ADMIT / DENY
        btnInspect = MakeSideButton("BtnInspect", -260, 90, new Color(0.85f, 0.85f, 0.9f), out lblInspect, 30);
        btnInspect.onClick.AddListener(() => OpenInspector(DocumentType.Id));
        btnStatus = MakeSideButton("BtnStatus", -360, 64, new Color(0.75f, 0.8f, 0.9f), out lblStatus, 24);
        btnStatus.onClick.AddListener(() => hud.ToggleStatus());

        // horror effects: what shakes and what may glitch
        if (desk != null) horror.AddShakeTarget((RectTransform)desk);
        horror.AddShakeTarget((RectTransform)dialogue.transform);
        horror.AddShakeTarget((RectTransform)hud.transform);
        horror.AddGlitchText(ui.messageText);
        horror.AddGlitchText(ui.scoreText);

        // context for events
        ctx.canvas = canvas; ctx.runner = this; ctx.ui = ui; ctx.trainView = train; ctx.trainSys = trainSys;
        ctx.stage = stage; ctx.lighting = lighting; ctx.horror = horror; ctx.hud = hud; ctx.dialogue = dialogue;
        ctx.overlay = overlay; ctx.clock = clock; ctx.inspector = inspector;
        ctx.RefreshPortrait = () => SetPortrait(current);
        ctx.RefreshDesk = () => { if (current != null) ui.ShowPassenger(current); };
        ctx.ShowRemaining = (n, s) => StartCoroutine(FakeRemaining(n, s));
        ctx.StopClock = on => { clockStopped = on; clock.Stop(on); };
        ctx.RefreshHud = RefreshHud;

        // the pause menu and the codex fill the top-level order last
        SetButtonLabels();
        Loc.Changed += OnLanguageChanged;
        train.ParkOffscreen();
    }

    DocumentType TypeOfDesk(Transform t, int index)
    {
        if (ui.ticketText != null && ui.ticketText.transform.IsChildOf(t)) return DocumentType.Ticket;
        if (ui.idText != null && ui.idText.transform.IsChildOf(t)) return DocumentType.Id;
        if (ui.railDbText != null && ui.railDbText.transform.IsChildOf(t)) return DocumentType.RailDatabase;
        if (ui.registryText != null && ui.registryText.transform.IsChildOf(t)) return DocumentType.Registry;
        return (DocumentType)Mathf.Clamp(index, 0, 3);
    }

    Button MakeSideButton(string name, float y, float height, Color tint, out TextMeshProUGUI label, float font)
    {
        Button b = UIKit.MakeButton(name, canvas.transform, "", new Vector2(300, height), tint, font, out label);
        UIKit.Anchor((RectTransform)b.transform, UIKit.TR, UIKit.TR, UIKit.TR, new Vector2(-40, y), new Vector2(300, height));
        return b;
    }

    static void AddButtonFX(Button b)
    {
        if (b != null && b.GetComponent<ButtonFX>() == null) b.gameObject.AddComponent<ButtonFX>();
    }

    void OnDestroy()
    {
        Loc.Changed -= OnLanguageChanged;
        Time.timeScale = 1f;
    }

    void SetButtonLabels()
    {
        SetLabel(btnAdmit, Loc.T("WPUŚĆ", "ADMIT"));
        SetLabel(btnDeny, Loc.T("ODMÓW", "DENY"));
        if (lblInspect != null) lblInspect.text = Loc.T("DOKUMENTY", "INSPECT");
        if (lblStatus != null) lblStatus.text = Loc.T("STATUS (TAB)", "STATUS (TAB)");
        if (btnInspect != null)
        {
            ButtonFX fx = btnInspect.GetComponent<ButtonFX>();
            if (fx != null) fx.tooltip = Loc.T("Obejrzyj dokumenty z bliska, sprawdź bazę danych", "Examine the documents up close, query the database");
        }
    }

    static void SetLabel(Button b, string text)
    {
        if (b == null) return;
        TMP_Text label = b.GetComponentInChildren<TMP_Text>();
        if (label != null) label.text = text;
    }

    void OnLanguageChanged()
    {
        SetButtonLabels();
        if (day != null)
        {
            hud.SetRule(day.rule);
            RefreshScore();
        }
        if (pause != null) pause.SetInfo(NightInfo());
    }

    static Button FindButton(Transform root, string name)
    {
        Transform t = root.Find(name);
        return t != null ? t.GetComponent<Button>() : null;
    }

    static void Skin(Button b, Color tint)
    {
        if (b == null) return;
        Image img = b.GetComponent<Image>();
        if (img != null) { img.sprite = PixelArt.ButtonSprite(); img.color = tint; }
        TMP_Text label = b.GetComponentInChildren<TMP_Text>();
        if (label != null) label.color = UIKit.Ink;
    }

    // ---------- main flow ----------

    string NightInfo()
    {
        if (day == null) return "";
        return endless ? Loc.T("Nocna zmiana, poziom ", "Night Shift, level ") + (nightIndex + 1)
                       : Loc.T("Noc ", "Night ") + (nightIndex + 1) + ": " + day.title;
    }

    IEnumerator Run()
    {
        yield return null;   // let the layout settle
        endless = GameState.Mode == GameMode.Endless;
        sfx.StartAmbience(true);
        sfx.SetAutoMusic(true);
        train.ParkOffscreen();
        SetDecisionButtons(false);
        dialogue.Clear();

        if (endless)
        {
            EndlessManager.BeginRun();
            nightIndex = GameState.ForcedNight >= 0 ? GameState.ForcedNight : 0;
            GameState.ForcedNight = -1;
        }
        else
        {
            nightIndex = StoryManager.Begin();
            trainSys.LoadIntegrity(StoryManager.SavedIntegrity);
        }
        RefreshHud();

        while (true)
        {
            day = endless ? EndlessManager.Build(nightIndex + 1) : StoryManager.Night(nightIndex);
            CurrentNight = nightIndex;
            ctx.day = day;
            ctx.StoryMode = !endless;
            ctx.nightNumber = nightIndex + 1;
            pause.SetInfo(NightInfo());

            yield return PlayNight();

            if (failed)
            {
                bool retry = false;
                if (endless) yield return EndlessGameOver();
                else
                {
                    yield return StoryGameOver();
                    retry = overlay.Choice == 0;
                }
                if (!retry) yield break;
                RestoreSnapshot();
                failed = false;
                continue;
            }

            // finished night
            if (!endless && day.finalDay) yield break;   // the ending routine already ran

            nightIndex++;
            if (!endless && nightIndex >= StoryManager.NightCount) yield break;
        }
    }

    void TakeSnapshot()
    {
        snapCon = PlayerStats.Conscience; snapDis = PlayerStats.Discipline; snapStress = PlayerStats.Stress;
        snapRep = PlayerStats.Reputation; snapCorrect = PlayerStats.TotalCorrect; snapMistakes = PlayerStats.TotalMistakes;
        snapFlags = StoryFlags.ToList();
        snapIntegrity = trainSys.Integrity;
    }

    void RestoreSnapshot()
    {
        PlayerStats.Conscience = snapCon; PlayerStats.Discipline = snapDis;
        PlayerStats.Stress = Mathf.Min(snapStress, 60);
        PlayerStats.Reputation = snapRep; PlayerStats.TotalCorrect = snapCorrect; PlayerStats.TotalMistakes = snapMistakes;
        StoryFlags.LoadFrom(snapFlags);
        trainSys.Integrity = Mathf.Max(snapIntegrity, 40f);
        train.ParkOffscreen();
        clock.Stop(false);
        RefreshHud();
    }

    // one whole night: briefing, 13 passengers, results
    IEnumerator PlayNight()
    {
        TakeSnapshot();
        failed = false;
        report = new ShiftReport();
        streak = 0;
        passengerNumber = 0;
        fakeRemaining = -1;
        clockMinutes = 22 * 60;
        clockStopped = false;
        clock.Stop(false);
        clock.SetTime(clockMinutes);
        startDiscipline = PlayerStats.Discipline;
        startConscience = PlayerStats.Conscience;
        SkillSystem.TotalXpGained = 0;
        AnomalySystem.FoundThisShift = 0;
        Codex.NewThisShift.Clear();
        PlayerStats.Relax(20);
        DocumentSystem.CurrentDate = day.date;
        revealTruth = day.act > 0 ? day.act <= 3 : day.level < 3;
        PassengerGenerator.ResetDeck();
        trainSys.ResetNight();
        train.SetLineNumber("13");

        ResetBoard();
        hud.SetRule(day.rule);
        hud.SetScore(0);
        RefreshHud();
        RefreshScore();

        // codex entries of the night
        int fresh = 0;
        if (day.codexAtStart != null)
            foreach (string id in day.codexAtStart) { if (!Codex.IsUnlocked(id)) fresh++; Codex.Unlock(id, true); }
        Codex.Unlock(endless ? "rule_endless" : "rule_n" + nightIndex, true);
        if (fresh > 0) ToastUI.Show(Loc.T("KODEKS", "CODEX"), Loc.T("Nowe wpisy: ", "New entries: ") + fresh, new Color(0.55f, 0.75f, 0.95f));

        // briefing
        string head = endless
            ? Loc.T("NOCNA ZMIANA: POZIOM ", "NIGHT SHIFT: LEVEL ") + (nightIndex + 1)
            : Loc.T("NOC ", "NIGHT ") + (nightIndex + 1) + ": " + day.title.ToUpper();
        string body = (endless ? "" : "<color=#8FA6C8>" + day.actTitle + "</color>\n\n") + day.briefing +
                      Loc.T("\n\nZASADA: ", "\n\nRULE: ") + day.rule;
        sfx.PlayMusic("shift");
        yield return overlay.Show(head, body, Loc.T("ROZPOCZNIJ ZMIANĘ", "START THE SHIFT"), null, null, true);

        // schedule the events
        var before = new Dictionary<int, List<string>>();
        if (day.scriptedEvents != null)
            foreach (ScriptedEvent se in day.scriptedEvents) AddEvent(before, se.before, se.eventId);
        var pool = new List<string>(day.events ?? new List<string>());
        for (int k = 0; k < day.eventCount && pool.Count > 0; k++)
        {
            string id = pool[Random.Range(0, pool.Count)];
            pool.Remove(id);
            for (int tries = 0; tries < 10; tries++)
            {
                int at = Random.Range(1, 12);
                if (before.ContainsKey(at)) continue;
                AddEvent(before, at, id);
                break;
            }
        }

        // the train arrives
        sfx.Horn();
        yield return train.Arrive();

        int normalCount = day.finalDay ? day.passengers - 1 : day.passengers;
        for (int i = 0; i < normalCount && !failed; i++)
        {
            yield return HandlePassenger(i, false, before);
        }

        if (!failed && day.finalDay)
        {
            yield return PlayEventsAt(before, PassengersPerNight - 1, false);
            yield return HandlePassenger(PassengersPerNight - 1, true, before);
            if (!failed) yield return EndingRoutine();
            yield break;
        }
        if (failed) yield break;

        // end of the night
        while (stage.Busy) yield return null;
        ResetBoard();
        sfx.Horn();
        yield return train.Depart();
        train.ParkOffscreen();

        yield return FinishNight();
    }

    static void AddEvent(Dictionary<int, List<string>> map, int at, string id)
    {
        List<string> l;
        if (!map.TryGetValue(at, out l)) { l = new List<string>(); map[at] = l; }
        l.Add(id);
    }

    IEnumerator PlayEventsAt(Dictionary<int, List<string>> map, int index, bool needsPassenger)
    {
        List<string> l;
        if (!map.TryGetValue(index, out l)) yield break;
        foreach (string id in l.ToArray())
        {
            if (EventLibrary.NeedsPassenger(id) != needsPassenger) continue;
            yield return EventLibrary.Play(ctx, id);
            if (failed) yield break;
            CheckFailure();
        }
    }

    // ---------- one passenger ----------

    IEnumerator HandlePassenger(int index, bool isPlayer, Dictionary<int, List<string>> events)
    {
        passengerNumber = index + 1;

        // events that happen between passengers
        if (!isPlayer) yield return PlayEventsAt(events, index, false);
        if (failed) yield break;

        current = isPlayer ? PassengerGenerator.GeneratePlayer(day) : PassengerGenerator.ForSlot(day, index);
        Debug.Log("Passenger " + passengerNumber + ": truth=" + current.truth + ", forged=" + current.forgedTicket + ", story=" + current.storyId + ", hint=" + current.debugHint);

        foreach (DraggableDocument d in docs) d.ResetToDesk();
        ui.ClearDocuments();
        dialogue.Clear();
        SetPortrait(null);
        inspector.ResetFor(current, day);
        if (clockStopped) { clockStopped = false; clock.Stop(false); }
        stage.SetQueue(PassengersPerNight - passengerNumber);
        RefreshScore();

        if (isPlayer)
        {
            // the queue is over; one more person steps up
            yield return overlay.Show(Loc.T("OSTATNI PASAŻER", "THE LAST PASSENGER"),
                Loc.T("Kolejka się skończyła.\n\nPASAŻERÓW POZOSTAŁO: 1\n\nNa peronie stoi jeszcze jedna osoba. Podchodzi do okienka wolno, tak jak się podchodzi do własnego odbicia.",
                      "The queue is over.\n\nPASSENGERS REMAINING: 1\n\nOne more person stands on the platform. They approach the window slowly, the way you approach your own reflection."),
                Loc.T("PODEJDŹ", "STEP UP"), null, null, true);
            AchievementSystem.Unlock("last_passenger");
            sfx.PlayMusic("horror");
        }

        yield return stage.Enter(current);
        ctx.current = current;

        ui.SetMessage("");
        ui.ShowPassenger(current);
        SetPortrait(current);
        if (!string.IsNullOrEmpty(current.codexOnMeet)) Codex.Unlock(current.codexOnMeet);
        AchievementSystem.OnPassengerMet(current);
        if (current.isPlayer) Codex.Unlock("unk_you", true);

        // a dead passenger may frighten the conductor (rare)
        if (current.scaresOnArrival && !isPlayer)
        {
            yield return horror.ArrivalScare(current, s => dialogue.Interject(s));
        }

        dm.Begin(current);
        dialogue.Open(current.greeting, dm.Offered());
        dialogue.SetInteractable(true);
        RefreshHud();
        RefreshScore();
        sfx.Click();

        // events that need the passenger at the booth
        if (!isPlayer) yield return PlayEventsAt(events, index, true);
        if (failed) { CleanupPassenger(); yield break; }

        decision = 0;
        timedOut = false;
        timeLeft = day.timeLimit;
        shownSeconds = -1;
        acceptInput = true;
        SetDecisionButtons(true);
        while (decision == 0)
        {
            if (day.timeLimit > 0f && Time.timeScale > 0f)
            {
                timeLeft -= Time.deltaTime;
                int secs = Mathf.CeilToInt(Mathf.Max(0f, timeLeft));
                if (secs != shownSeconds) { shownSeconds = secs; RefreshScore(); if (secs <= 8 && secs > 0) sfx.Tick(); }
                if (timeLeft <= 0f) { timedOut = true; decision = 2; }
            }
            yield return null;
        }

        acceptInput = false;
        SetDecisionButtons(false);
        dialogue.SetInteractable(false);
        if (DocumentInspector.IsOpen) inspector.Close();
        if (hud.StatusOpen) hud.CloseStatus();
        shownSeconds = -1;

        bool admit = decision == 1;
        stage.PlayDecision(admit);
        yield return stamp.Play(admit, train);

        if (isPlayer)
        {
            playerAdmitted = admit;
            dialogue.Interject(DialogueBank.Farewell(current, admit));
            stage.ExitCurrent(current, admit);
            yield break;
        }

        clockMinutes += 25;
        clock.SetTime(clockMinutes);
        Resolve(admit);
        dialogue.Interject(DialogueBank.Farewell(current, admit));
        stage.ExitCurrent(current, admit);
        ctx.current = null;
        yield return new WaitForSeconds(1.3f);
    }

    bool playerAdmitted;

    void CleanupPassenger()
    {
        acceptInput = false;
        SetDecisionButtons(false);
        dialogue.SetInteractable(false);
        stage.ExitCurrent(current, false);
        ctx.current = null;
    }

    // ---------- judging ----------

    void Resolve(bool admit)
    {
        PassengerData p = current;
        bool should = Days.ShouldAdmit(p, day);
        bool correct = admit == should;
        bool judged = day.judged;
        string msg = "";
        string truthName = UIController.TruthName(p.truth) + (p.forgedTicket ? Loc.T(", z fałszywym biletem", ", with a forged ticket") : "");

        report.passengers++;
        if (admit) report.admitted++; else report.denied++;
        SaveSystem.Data.endless.totalPassengers += endless ? 1 : 0;

        // story flags and recurring characters
        if (admit && !string.IsNullOrEmpty(p.onAdmitFlag)) StoryFlags.Set(p.onAdmitFlag);
        if (!admit && !string.IsNullOrEmpty(p.onDenyFlag)) StoryFlags.Set(p.onDenyFlag);
        if (p.truth == PassengerTruth.Dead)
        {
            StoryFlags.Set(Flag.SawTheDead);
            if (admit) StoryFlags.Set(Flag.AdmittedDead);
        }

        if (judged)
        {
            if (correct)
            {
                report.correct++;
                PlayerStats.TotalCorrect++;
                streak++;
                if (streak > report.bestStreak) report.bestStreak = streak;
                if (report.correct % 2 == 0) PlayerStats.AddDiscipline(1);
                PlayerStats.Relax(3);
                PlayerStats.AddReputation(1);
                SkillSystem.Give(Skill.Documents, 3);
                SkillSystem.Give(Skill.Observation, p.truth != PassengerTruth.Alive ? 5 : 2);
                if (!admit && p.aggressive) SkillSystem.Give(Skill.Authority, 5);
                if (PlayerStats.Stress > 50) SkillSystem.Give(Skill.Composure, 3);

                if (!admit && p.sympathetic)
                {
                    PlayerStats.AddConscience(-1);
                    msg = Loc.T("Zgodnie z przepisami. Ale on tak prosił...", "By the rules. But they begged so much...");
                }
                else msg = Loc.T("Dobra decyzja.", "Good decision.");
                sfx.Good();
            }
            else
            {
                report.mistakes++;
                report.rulesBroken++;
                PlayerStats.TotalMistakes++;
                streak = 0;
                PlayerStats.AddDiscipline(-1);
                PlayerStats.AddReputation(-3);
                sfx.Bad();

                if (admit)
                {
                    if (p.sympathetic)
                    {
                        PlayerStats.AddConscience(1);
                        PlayerStats.AddStress(3);
                        msg = Loc.T("Złamałeś przepisy z litości. Pociąg to zauważył.", "You broke the rules out of mercy. The train noticed.");
                    }
                    else
                    {
                        PlayerStats.AddStress(p.truth != PassengerTruth.Alive ? 10 : 6);
                        msg = Loc.T("Błąd. Wpuściłeś: ", "Mistake. You admitted: ") + truthName + ".";
                    }
                }
                else
                {
                    PlayerStats.AddStress(5);
                    if (p.sympathetic) PlayerStats.AddConscience(-1);
                    msg = Loc.T("Błąd. To był pasażer: ", "Mistake. The passenger was: ") + truthName + ".";
                }
                if (day.maxMistakes >= 0 && report.mistakes > day.maxMistakes)
                {
                    failed = true;
                    failTitle = Loc.T("ZWOLNIONY", "FIRED");
                    failText = Loc.T("Zbyt wiele błędów w jedną noc. Dyrekcja zabiera ci mundur.\n\nBłędy tej nocy: ", "Too many mistakes in one night. The Directorate takes your uniform.\n\nMistakes tonight: ") +
                               report.mistakes + " (limit: " + day.maxMistakes + ")";
                }
            }
            if (!revealTruth) msg = correct ? Loc.T("Decyzja zapisana.", "Decision recorded.") : Loc.T("Decyzja zapisana. Coś jest nie tak.", "Decision recorded. Something is off.");
        }
        else
        {
            // nobody checks you: only your conscience counts
            if (p.sympathetic)
            {
                if (admit) { PlayerStats.AddConscience(1); PlayerStats.Relax(2); msg = Loc.T("Litość.", "Mercy."); }
                else       { PlayerStats.AddConscience(-1); PlayerStats.AddStress(3); msg = Loc.T("Zostawiasz go na peronie.", "You leave them on the platform."); }
            }
            else if (admit && p.truth != PassengerTruth.Alive)
            {
                PlayerStats.AddDiscipline(-1); PlayerStats.AddStress(4);
                msg = Loc.T("Wpuszczasz kogoś, kogo nie ma.", "You admit someone who should not be here.");
            }
            else if (!admit && p.truth == PassengerTruth.Alive && !p.forgedTicket)
            {
                PlayerStats.AddConscience(-1);
                msg = Loc.T("Odmawiasz żywemu człowiekowi.", "You turn away a living person.");
            }
            else msg = Loc.T("Twoja decyzja.", "Your decision.");
            report.correct += correct ? 1 : 0;
            sfx.Click();
            correct = true;   // nothing is a "mistake" tonight
        }

        if (timedOut)
        {
            PlayerStats.AddStress(4);
            msg = Loc.T("Czas minął. Kolejka nie czeka.", "Time is up. The queue does not wait.") + " " + msg;
        }

        // score
        int level = endless ? nightIndex + 1 : Mathf.Max(1, day.act);
        int delta = EndlessManager.ScoreFor(correct, level, streak);
        report.score = Mathf.Max(0, report.score + delta);
        hud.SetScore(report.score);

        // the rest of the systems
        trainSys.OnDecision(p, admit, correct);
        GameEvents.RaiseDecision(p, admit, correct);
        AchievementSystem.OnDecision(p, admit, correct);
        AchievementSystem.CheckSkills();
        if (endless)
        {
            EndlessManager.RunScore += Mathf.Max(0, delta);
            EndlessManager.RunPassengers++;
            if (correct) EndlessManager.RunCorrect++; else EndlessManager.RunMistakes++;
            if (streak > EndlessManager.RunBestStreak) EndlessManager.RunBestStreak = streak;
        }

        CheckFailure();
        ui.SetMessage(msg);
        RefreshHud();
        RefreshScore();
    }

    void CheckFailure()
    {
        if (failed) return;
        if (PlayerStats.Stress >= PlayerStats.MaxStress)
        {
            failed = true;
            failTitle = Loc.T("ZAŁAMANIE", "BREAKDOWN");
            failText = Loc.T("Stres przekroczył granicę. Lampy zgasły, w uszach rośnie szum, a pociąg nigdy nie opuścił peronu.",
                             "The stress passed the limit. The lamps went out, a roar grows in your ears, and the train never left the platform.");
        }
        else if (trainSys.Destroyed)
        {
            failed = true;
            failTitle = Loc.T("POCIĄG ZNISZCZONY", "TRAIN DESTROYED");
            failText = Loc.T("Kadłub nie wytrzymał. Linia 13 zatrzymała się na dobre, a peron zapadł w ciszę.",
                             "The hull gave way. Line 13 has stopped for good and the platform fell silent.");
        }
    }

    // ---------- end of the night ----------

    IEnumerator FinishNight()
    {
        report.stress = PlayerStats.Stress;
        report.discipline = PlayerStats.Discipline - startDiscipline;
        report.conscience = PlayerStats.Conscience - startConscience;
        report.xp = SkillSystem.TotalXpGained;
        if (Codex.NewThisShift.Count > 0)
        {
            CodexEntry e = Codex.Get(Codex.NewThisShift[Codex.NewThisShift.Count - 1]);
            report.discovery = e != null ? e.title : "";
            if (Codex.NewThisShift.Count > 1) report.discovery += " (+" + (Codex.NewThisShift.Count - 1) + ")";
        }
        else report.discovery = "";

        AchievementSystem.OnShiftComplete(report, nightIndex + 1, !endless);

        // save
        if (endless)
        {
            EndlessStats s = SaveSystem.Data.endless;
            s.totalCorrect += report.correct;
            s.totalMistakes += report.mistakes;
            EndlessManager.SaveStats(nightIndex + 1, false);
        }
        else
        {
            StoryManager.Commit(nightIndex + 1, Mathf.RoundToInt(trainSys.Integrity));
        }

        string head = Loc.T("ZMIANA UKOŃCZONA", "SHIFT COMPLETE") + (report.Perfect ? "  -  " + Loc.T("BEZ BŁĘDU", "PERFECT") : "");
        string outro = day.outro;
        if (endless)
        {
            outro = Loc.T("Poziom ", "Level ") + (nightIndex + 1) + ". " + Loc.T("Wynik serii: ", "Run score: ") + EndlessManager.RunScore +
                    "   " + Loc.T("Rekord: ", "High score: ") + Mathf.Max(SaveSystem.Data.endless.highScore, EndlessManager.RunScore) + "\n" + outro;
        }
        sfx.PlayMusic("menu");
        yield return results.Show(report, head, outro, Loc.T("NASTĘPNA NOC", "NEXT NIGHT"), Loc.T("MENU GŁÓWNE", "MAIN MENU"), PassengersPerNight);
        if (results.Choice == 1)
        {
            BackToMenu();
            while (true) yield return null;
        }
    }

    IEnumerator StoryGameOver()
    {
        sfx.Bad();
        ui.SetMessage("");
        sfx.PlayMusic("horror");
        yield return overlay.Show(failTitle, failText, Loc.T("SPRÓBUJ TĘ NOC PONOWNIE", "TRY THIS NIGHT AGAIN"), Loc.T("MENU GŁÓWNE", "MAIN MENU"), null, true);
        if (overlay.Choice != 0) BackToMenu();
    }

    IEnumerator EndlessGameOver()
    {
        sfx.Bad();
        ui.SetMessage("");
        sfx.PlayMusic("horror");
        EndlessStats s = SaveSystem.Data.endless;
        s.totalCorrect += report.correct;
        s.totalMistakes += report.mistakes;
        bool record = EndlessManager.RunScore > s.highScore;
        EndlessManager.SaveStats(nightIndex + 1, true);
        string text = failText + "\n\n" +
                      Loc.T("Poziom: ", "Level: ") + (nightIndex + 1) + "\n" +
                      Loc.T("Obsłużeni pasażerowie: ", "Passengers processed: ") + EndlessManager.RunPassengers + "\n" +
                      Loc.T("Poprawne decyzje: ", "Correct decisions: ") + EndlessManager.RunCorrect + "   " + Loc.T("Błędy: ", "Mistakes: ") + EndlessManager.RunMistakes + "\n" +
                      Loc.T("Najdłuższa seria: ", "Best streak: ") + EndlessManager.RunBestStreak + "\n" +
                      Loc.T("Wynik: ", "Score: ") + EndlessManager.RunScore + "   " + Loc.T("Rekord: ", "High score: ") + s.highScore +
                      (record ? "  " + Loc.T("NOWY REKORD!", "NEW RECORD!") : "");
        yield return overlay.Show(failTitle, text, Loc.T("NOWA ZMIANA", "NEW SHIFT"), Loc.T("MENU GŁÓWNE", "MAIN MENU"), null, false);
        if (overlay.Choice == 0)
        {
            GameState.BeginEndless();
            Time.timeScale = 1f;
            SceneManager.LoadScene("Peron");
            while (true) yield return null;
        }
        BackToMenu();
    }

    // The last passenger was the conductor: the ending depends on everything the player did.
    IEnumerator EndingRoutine()
    {
        yield return new WaitForSeconds(playerAdmitted ? 3.5f : 2.5f);
        if (playerAdmitted) { sfx.Horn(); yield return train.Depart(); }
        AchievementSystem.OnShiftComplete(report, nightIndex + 1, true);

        string id = StoryManager.CalculateEnding(playerAdmitted);
        string title, text;
        StoryManager.EndingText(id, out title, out text);

        sfx.SetAutoMusic(false);
        sfx.PlayMusic("ending");
        if (id == "d") { lighting.SetDarkness(0.6f); train.FlickerFor(2f); }
        if (id == "s") { lighting.SetDarkness(0f); }

        // record the ending
        SaveData data = SaveSystem.Data;
        if (!data.endings.Contains(id)) data.endings.Add(id);
        AchievementSystem.Unlock(StoryManager.EndingAchievement(id));
        PlayerStats.StoreTo(data.story);
        data.story.flags = StoryFlags.ToList();
        data.story.finished = true;
        data.story.endingId = "abcds".IndexOf(id);
        data.story.night = 0;
        SaveSystem.Save();

        text += "\n\n<color=#8FA6C8>" + Loc.T("Sumienie ", "Conscience ") + PlayerStats.Conscience + "/10   " + Loc.T("Dyscyplina ", "Discipline ") + PlayerStats.Discipline + "/10   " +
                Loc.T("Stres ", "Stress ") + PlayerStats.Stress + "%   " + Loc.T("Zakończenia: ", "Endings: ") + data.endings.Count + "/5</color>";

        yield return overlay.Show(title, text, Loc.T("MENU GŁÓWNE", "MAIN MENU"), Loc.T("KODEKS", "CODEX"), null, true);
        while (overlay.Choice == 1)
        {
            CodexUI.Open(canvas);
            while (CodexUI.IsOpen) yield return null;
            yield return overlay.Show(title, text, Loc.T("MENU GŁÓWNE", "MAIN MENU"), Loc.T("KODEKS", "CODEX"), null, false);
        }
        BackToMenu();
        while (true) yield return null;
    }

    // ---------- helpers ----------

    void OnAsk(QA q)
    {
        if (!acceptInput || current == null) return;
        SpendMinutes(3);
        string answer = dm.Ask(q);
        dialogue.ShowAnswer(q, answer, dm.Offered());
        AchievementSystem.CheckSkills();
        sfx.Click();
        CheckFailure();
        RefreshHud();
        RefreshScore();
    }

    void OpenInspector(DocumentType type)
    {
        if (!acceptInput || current == null || Time.timeScale == 0f || DocumentInspector.IsOpen || hud.StatusOpen) return;
        inspector.Open(() => { RefreshHud(); RefreshScore(); }, type);
    }

    void SpendMinutes(int m)
    {
        clockMinutes += m;
        if (!clockStopped) clock.SetTime(clockMinutes);
    }

    IEnumerator FakeRemaining(int n, float seconds)
    {
        fakeRemaining = n;
        RefreshScore();
        yield return new WaitForSeconds(seconds);
        fakeRemaining = -1;
        RefreshScore();
    }

    void ResetBoard()
    {
        stage.ClearPassengers();
        SetPortrait(null);
        foreach (DraggableDocument d in docs) d.ResetToDesk();
        ui.ClearDocuments();
        ui.SetMessage("");
        dialogue.Clear();
        ctx.current = null;
    }

    void RefreshHud()
    {
        hud.SetMeters(PlayerStats.Conscience, PlayerStats.Discipline, PlayerStats.Stress, PlayerStats.MaxStress);
        float s = PlayerStats.Stress01;
        train.SetStress(s);
        sfx.SetStress(s);
        horror.SetStress(s);
        lighting.SetStress(s);
    }

    void RefreshScore()
    {
        if (day == null) { ui.SetScore(""); return; }
        int remaining = fakeRemaining >= 0 ? fakeRemaining : Mathf.Max(0, PassengersPerNight - passengerNumber + 1);
        if (passengerNumber == 0) remaining = PassengersPerNight;
        string mode = endless ? Loc.T("POZIOM ", "LEVEL ") + (nightIndex + 1) : Loc.T("NOC ", "NIGHT ") + (nightIndex + 1);
        string time = ((clockMinutes / 60) % 24).ToString("00") + ":" + (clockMinutes % 60).ToString("00");
        string mist = !day.judged ? Loc.T("Decyduj sam", "Decide for yourself") : Loc.T("Błędy: ", "Mistakes: ") + (report != null ? report.mistakes : 0) + "/" + day.maxMistakes;
        string t = day.timeLimit > 0f && shownSeconds >= 0 ? "   T-" + shownSeconds : "";
        ui.SetScore(mode + "   " + time + "   " + Loc.T("POZOSTAŁO: ", "REMAINING: ") + remaining + "   " + mist + t);
    }

    void SetPortrait(PassengerData p)
    {
        if (portraitRoot == null) return;
        if (p == null) { portraitRoot.SetActive(false); return; }
        DocumentData id = DocumentSystem.Get(p, DocumentType.Id);
        bool player = p.isPlayer || p.playerPhoto || (id != null && id.photoIsPlayer);
        int look = id != null && id.photoLook >= 0 ? id.photoLook : (p.idPhotoLook >= 0 ? p.idPhotoLook : p.look);
        CharFrames f = player ? CharacterArt.Conductor() : CharacterArt.Passenger(Mathf.Max(0, look));
        portraitImage.sprite = f.portrait;
        portraitRoot.SetActive(true);
    }

    void SetDecisionButtons(bool v)
    {
        if (btnAdmit != null) btnAdmit.interactable = v;
        if (btnDeny != null) btnDeny.interactable = v;
        if (btnInspect != null) btnInspect.interactable = v;
    }

    // ---------- button hooks (On Click) ----------

    public void Decide(bool admit)
    {
        if (admit) Admit(); else Deny();
    }

    public void Admit()
    {
        if (!acceptInput) return;
        decision = 1;
    }

    public void Deny()
    {
        if (!acceptInput) return;
        decision = 2;
    }

    public void BackToMenu()
    {
        Time.timeScale = 1f;
        UIState.Modal = 0;
        SceneManager.LoadScene("MainMenu");
    }
}
