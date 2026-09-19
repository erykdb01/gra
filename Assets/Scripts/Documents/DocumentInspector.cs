using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

// INSPECT MODE: a full-screen desk where documents can be enlarged, flipped, compared, scanned for
// microtext, checked (signature, photo) and questioned in the railway database.
public class DocumentInspector : MonoBehaviour
{
    public static DocumentInspector Instance;
    public static bool IsOpen { get { return Instance != null && Instance.gameObject.activeSelf; } }

    // ---------- one sheet of paper ----------
    class Row
    {
        public RectTransform rt;
        public Image hl;
        public Button btn;
        public TextMeshProUGUI label, value;
        public DocLine line;
    }

    class Paper
    {
        public RectTransform root;
        public Image bg;
        public TextMeshProUGUI title, number, stamp, side;
        public Image photoFrame, photoImg;
        public TextMeshProUGUI photoQ;
        public Button photoBtn;
        public readonly Row[] rows = new Row[12];
        public DocumentData doc;
        public float width;
    }

    RectTransform panel;
    CanvasGroup group;
    Paper left, right;
    readonly Button[] tabs = new Button[9];
    readonly TextMeshProUGUI[] tabLabels = new TextMeshProUGUI[9];
    readonly Image[] tabImgs = new Image[9];
    Button btnRotate, btnZoom, btnScan, btnSignature, btnPhoto, btnCompare;
    TextMeshProUGUI[] toolLabels = new TextMeshProUGUI[6];
    TextMeshProUGUI marksText, feedback, terminalTitle, log, closeLabel;
    readonly Button[] queryBtn = new Button[5];
    readonly TextMeshProUGUI[] queryLabel = new TextMeshProUGUI[5];

    PassengerData passenger;
    DayConfig day;
    Action<int> spend;
    TrainSystem train;
    Action closedCallback;

    int current = (int)DocumentType.Ticket;
    int compareDoc = (int)DocumentType.Id;
    bool compare, zoom;
    int marksLeft, marksMax;
    readonly bool[] scanned = new bool[9];
    readonly bool[] flipped = new bool[9];
    readonly bool[] queried = new bool[5];
    readonly List<string> logLines = new List<string>();
    bool animating;

    public static DocumentInspector Create(Canvas canvas)
    {
        RectTransform rt = UIKit.NewRect("DocumentInspector", canvas.transform);
        DocumentInspector d = rt.gameObject.AddComponent<DocumentInspector>();
        d.Build(rt);
        Instance = d;
        return d;
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
        Loc.Changed -= OnLanguage;
    }

    // ---------------- building the UI ----------------

    void Build(RectTransform rt)
    {
        panel = rt;
        UIKit.Stretch(rt);
        Image bg = rt.gameObject.AddComponent<Image>();
        bg.color = new Color(0.03f, 0.03f, 0.06f, 0.93f);
        bg.raycastTarget = true;
        group = rt.gameObject.AddComponent<CanvasGroup>();

        // tabs
        for (int i = 0; i < 9; i++)
        {
            int idx = i;
            TextMeshProUGUI lab;
            Button b = UIKit.MakeButton("Tab" + i, rt, "", new Vector2(196, 54), new Color(0.82f, 0.82f, 0.88f), 20, out lab);
            UIKit.Anchor((RectTransform)b.transform, UIKit.TL, UIKit.TL, UIKit.TL, new Vector2(60 + i * 200, -24), new Vector2(196, 54));
            lab.enableAutoSizing = true; lab.fontSizeMin = 13; lab.fontSizeMax = 22;
            b.onClick.AddListener(() => OnTab(idx));
            tabs[i] = b; tabLabels[i] = lab; tabImgs[i] = b.GetComponent<Image>();
        }

        left = MakePaper("PaperLeft", rt);
        right = MakePaper("PaperRight", rt);

        // toolbox
        string[] names = { "", "", "", "", "", "" };
        btnRotate = Tool(rt, 0, out toolLabels[0], Rotate);
        btnZoom = Tool(rt, 1, out toolLabels[1], ToggleZoom);
        btnScan = Tool(rt, 2, out toolLabels[2], Scan);
        btnSignature = Tool(rt, 3, out toolLabels[3], CheckSignature);
        btnPhoto = Tool(rt, 4, out toolLabels[4], CheckPhoto);
        btnCompare = Tool(rt, 5, out toolLabels[5], ToggleCompare);

        marksText = UIKit.Text("Marks", rt, "", 24, UIKit.Amber, TextAlignmentOptions.Left);
        UIKit.Anchor(marksText.rectTransform, UIKit.TL, UIKit.TL, UIKit.TL, new Vector2(1500, -540), new Vector2(380, 36));
        feedback = UIKit.Text("Feedback", rt, "", 22, UIKit.Light, TextAlignmentOptions.TopLeft);
        UIKit.Anchor(feedback.rectTransform, UIKit.TL, UIKit.TL, UIKit.TL, new Vector2(1500, -582), new Vector2(380, 190));

        // terminal
        RectTransform term = UIKit.NewRect("Terminal", rt);
        UIKit.Anchor(term, UIKit.BL, UIKit.BL, UIKit.BL, new Vector2(40, 20), new Vector2(1440, 250));
        Image tbg = term.gameObject.AddComponent<Image>();
        tbg.sprite = PixelArt.Panel();
        tbg.color = Color.white;
        tbg.raycastTarget = false;
        terminalTitle = UIKit.Text("Title", term, "", 22, new Color(0.45f, 0.95f, 0.6f), TextAlignmentOptions.TopLeft);
        UIKit.Anchor(terminalTitle.rectTransform, UIKit.TL, UIKit.TL, UIKit.TL, new Vector2(16, -8), new Vector2(600, 28));
        for (int i = 0; i < 5; i++)
        {
            int q = i;
            TextMeshProUGUI lab;
            Button b = UIKit.MakeButton("Query" + i, term, "", new Vector2(300, 40), new Color(0.6f, 0.9f, 0.7f), 20, out lab);
            UIKit.Anchor((RectTransform)b.transform, UIKit.TL, UIKit.TL, UIKit.TL, new Vector2(16, -40 - i * 41), new Vector2(300, 38));
            lab.enableAutoSizing = true; lab.fontSizeMin = 13; lab.fontSizeMax = 20;
            b.onClick.AddListener(() => Query(q));
            queryBtn[i] = b; queryLabel[i] = lab;
        }
        log = UIKit.Text("Log", term, "", 22, new Color(0.6f, 0.95f, 0.7f), TextAlignmentOptions.TopLeft);
        UIKit.Anchor(log.rectTransform, UIKit.TL, UIKit.TL, UIKit.TL, new Vector2(340, -40), new Vector2(1080, 200));

        TextMeshProUGUI cl;
        Button close = UIKit.MakeButton("Close", rt, "", new Vector2(380, 64), new Color(0.9f, 0.55f, 0.5f), 28, out cl);
        UIKit.Anchor((RectTransform)close.transform, Vector2.right, Vector2.right, Vector2.right, new Vector2(-40, 20), new Vector2(380, 64));
        close.onClick.AddListener(Close);
        closeLabel = cl;

        Loc.Changed += OnLanguage;
        RefreshStaticTexts();
        gameObject.SetActive(false);
    }

    Button Tool(RectTransform parent, int index, out TextMeshProUGUI label, UnityEngine.Events.UnityAction action)
    {
        TextMeshProUGUI l;
        Button b = UIKit.MakeButton("Tool" + index, parent, "", new Vector2(380, 60), new Color(0.85f, 0.85f, 0.9f), 24, out l);
        UIKit.Anchor((RectTransform)b.transform, UIKit.TL, UIKit.TL, UIKit.TL, new Vector2(1500, -100 - index * 70), new Vector2(380, 60));
        l.enableAutoSizing = true; l.fontSizeMin = 14; l.fontSizeMax = 24;
        b.onClick.AddListener(action);
        label = l;
        return b;
    }

    Paper MakePaper(string name, RectTransform parent)
    {
        var p = new Paper();
        p.root = UIKit.NewRect(name, parent);
        UIKit.Anchor(p.root, UIKit.TL, UIKit.TL, UIKit.TL, new Vector2(290, -100), new Vector2(900, 660));
        p.bg = p.root.gameObject.AddComponent<Image>();
        p.bg.sprite = PixelArt.Paper(72, 66, name == "PaperLeft" ? 3 : 5);
        p.bg.color = Color.white;
        p.bg.raycastTarget = false;

        p.title = UIKit.Text("Title", p.root, "", 32, UIKit.Ink, TextAlignmentOptions.TopLeft);
        UIKit.Anchor(p.title.rectTransform, UIKit.TL, UIKit.TL, UIKit.TL, new Vector2(28, -14), new Vector2(560, 44));
        p.title.fontStyle = FontStyles.Bold;
        p.number = UIKit.Text("Number", p.root, "", 18, new Color(0.35f, 0.28f, 0.22f), TextAlignmentOptions.TopLeft);
        UIKit.Anchor(p.number.rectTransform, UIKit.BL, UIKit.BL, UIKit.BL, new Vector2(28, 12), new Vector2(320, 26));
        p.side = UIKit.Text("Side", p.root, "", 18, new Color(0.35f, 0.28f, 0.22f), TextAlignmentOptions.TopRight);
        UIKit.Anchor(p.side.rectTransform, Vector2.right, Vector2.right, Vector2.right, new Vector2(-28, 12), new Vector2(240, 26));
        p.stamp = UIKit.Text("Stamp", p.root, "", 24, new Color(0.7f, 0.15f, 0.15f, 0.8f), TextAlignmentOptions.Center);
        UIKit.Anchor(p.stamp.rectTransform, Vector2.one, Vector2.one, Vector2.one, new Vector2(-40, -20), new Vector2(300, 54));
        p.stamp.rectTransform.localRotation = Quaternion.Euler(0, 0, -9f);
        p.stamp.fontStyle = FontStyles.Bold;

        // ID photo slot (top right)
        p.photoFrame = UIKit.Img("PhotoFrame", p.root, PixelArt.Solid(), new Color(0.15f, 0.12f, 0.10f, 1f));
        UIKit.Anchor(p.photoFrame.rectTransform, Vector2.one, Vector2.one, Vector2.one, new Vector2(-30, -84), new Vector2(150, 168));
        Image inner = UIKit.Img("PhotoBg", p.photoFrame.transform, PixelArt.Solid(), new Color(0.27f, 0.34f, 0.50f, 1f));
        UIKit.Stretch(inner.rectTransform);
        inner.rectTransform.offsetMin = new Vector2(5, 5); inner.rectTransform.offsetMax = new Vector2(-5, -5);
        p.photoImg = UIKit.Img("Photo", inner.transform, null, Color.white);
        UIKit.Stretch(p.photoImg.rectTransform);
        p.photoQ = UIKit.Text("Q", inner.transform, "?", 90, new Color(0.7f, 0.75f, 0.85f, 0.7f), TextAlignmentOptions.Center);
        UIKit.Stretch(p.photoQ.rectTransform);
        p.photoFrame.raycastTarget = true;
        p.photoBtn = p.photoFrame.gameObject.AddComponent<Button>();
        p.photoBtn.targetGraphic = p.photoFrame;

        for (int i = 0; i < p.rows.Length; i++)
        {
            var r = new Row();
            r.rt = UIKit.NewRect("Row" + i, p.root);
            UIKit.Anchor(r.rt, UIKit.TL, UIKit.TL, UIKit.TL, new Vector2(20, -78 - i * 46), new Vector2(860, 44));
            r.hl = r.rt.gameObject.AddComponent<Image>();
            r.hl.color = new Color(1f, 0.85f, 0.2f, 0f);
            r.hl.raycastTarget = true;
            r.btn = r.rt.gameObject.AddComponent<Button>();
            r.btn.targetGraphic = r.hl;
            ColorBlock cb = r.btn.colors;
            cb.normalColor = Color.white; cb.highlightedColor = new Color(1f, 0.9f, 0.5f, 1f); cb.pressedColor = new Color(1f, 0.8f, 0.3f, 1f);
            r.btn.colors = cb;
            r.label = UIKit.Text("Label", r.rt, "", 19, new Color(0.38f, 0.30f, 0.24f), TextAlignmentOptions.MidlineLeft);
            UIKit.Anchor(r.label.rectTransform, Vector2.zero, new Vector2(0, 1), new Vector2(0, 0.5f), new Vector2(8, 0), new Vector2(230, 0));
            r.label.textWrappingMode = TextWrappingModes.NoWrap;
            r.value = UIKit.Text("Value", r.rt, "", 26, UIKit.Ink, TextAlignmentOptions.MidlineLeft);
            UIKit.Anchor(r.value.rectTransform, Vector2.zero, Vector2.one, new Vector2(0, 0.5f), new Vector2(245, 0), new Vector2(-260, 0));
            r.value.enableAutoSizing = true; r.value.fontSizeMin = 14; r.value.fontSizeMax = 26;
            r.value.textWrappingMode = TextWrappingModes.NoWrap;
            int ri = i;
            Paper pp = p;
            r.btn.onClick.AddListener(() => OnRow(pp, ri));
            p.rows[i] = r;
        }
        p.photoBtn.onClick.AddListener(() => OnPhotoClick(p));
        return p;
    }

    void OnLanguage() { if (gameObject.activeSelf) { RefreshStaticTexts(); Redraw(); } }

    void RefreshStaticTexts()
    {
        toolLabels[0].text = Loc.T("OBRÓĆ (druga strona)", "ROTATE (other side)");
        toolLabels[1].text = zoom ? Loc.T("POMNIEJSZ", "ZOOM OUT") : Loc.T("POWIĘKSZ", "ZOOM IN");
        toolLabels[2].text = Loc.T("SKAN MIKROTEKSTU (1 min)", "SCAN MICROTEXT (1 min)");
        toolLabels[3].text = Loc.T("SPRAWDŹ PODPIS (2 min)", "CHECK SIGNATURE (2 min)");
        toolLabels[4].text = Loc.T("SPRAWDŹ ZDJĘCIE", "CHECK PHOTO");
        toolLabels[5].text = compare ? Loc.T("KONIEC PORÓWNANIA", "STOP COMPARING") : Loc.T("PORÓWNAJ DOKUMENTY", "COMPARE DOCUMENTS");
        terminalTitle.text = Loc.T("TERMINAL KOLEI - zapytania (3 min, zużywają prąd)", "RAILWAY TERMINAL - queries (3 min, they draw power)");
        for (int i = 0; i < 5; i++) queryLabel[i].text = DocumentSystem.QueryName(i);
        closeLabel.text = Loc.T("ZAMKNIJ (ESC)", "CLOSE (ESC)");
        string[] shortEn = { "TICKET", "ID CARD", "RAIL DB", "REGISTRY", "MANIFEST", "MEDICAL", "DEATH REC.", "TRAVEL", "ORDERS" };
        string[] shortPl = { "BILET", "DOWÓD", "BAZA KOL.", "REJESTR", "MANIFEST", "MEDYCZNA", "AKT ZGONU", "PODRÓŻE", "ROZKAZY" };
        for (int i = 0; i < 9; i++) tabLabels[i].text = Loc.T(shortPl[i], shortEn[i]);
    }

    // ---------------- opening / closing ----------------

    public void Setup(TrainSystem trainSystem, Action<int> spendMinutes)
    {
        train = trainSystem;
        spend = spendMinutes;
    }

    // New passenger: forget everything about the last one.
    public void ResetFor(PassengerData p, DayConfig d)
    {
        passenger = p;
        day = d;
        for (int i = 0; i < 9; i++) { scanned[i] = false; flipped[i] = false; }
        for (int i = 0; i < 5; i++) queried[i] = false;
        marksMax = 2 + SkillSystem.Level(Skill.Observation) / 2;
        marksLeft = marksMax;
        compare = false; zoom = false;
        current = (int)DocumentType.Id;
        logLines.Clear();
        AnomalySystem.BeginPassenger();
        if (p != null)
        {
            if (SkillSystem.Level(Skill.Observation) >= 2) logLines.Add(Loc.T("OBSERWACJA: ", "OBSERVATION: ") + p.traits);
            if (SkillSystem.Level(Skill.Observation) >= 4 && p.anomalies.Count > 0 && p.truth != PassengerTruth.Alive)
                logLines.Add(Loc.T("Masz przeczucie, że coś jest nie tak z tą osobą.", "You have a feeling that something is wrong with this person."));
        }
    }

    public void Open(Action onClosed, DocumentType start)
    {
        if (passenger == null) return;
        closedCallback = onClosed;
        current = (int)start;
        if (!IsAvailable(current)) current = FirstAvailable();
        if (compare && (compareDoc == current || !IsAvailable(compareDoc))) compareDoc = NextAvailable(current);

        gameObject.SetActive(true);
        transform.SetAsLastSibling();
        UIState.Modal++;
        RefreshStaticTexts();
        Redraw();
        StartCoroutine(UIAnim.Fade(group, 0f, 1f, 0.18f));
        if (SoundFX.I != null) SoundFX.I.Paper();
        if (!openedOnce(passenger)) { SkillSystem.Give(Skill.Documents, 3); SkillSystem.Give(Skill.Observation, 2); }
    }

    PassengerData lastRewarded;
    bool openedOnce(PassengerData p)
    {
        if (lastRewarded == p) return true;
        lastRewarded = p;
        return false;
    }

    public void Close()
    {
        if (!gameObject.activeSelf) return;
        gameObject.SetActive(false);
        UIState.Modal = Mathf.Max(0, UIState.Modal - 1);
        if (SoundFX.I != null) SoundFX.I.Paper();
        if (closedCallback != null) closedCallback();
    }

    void Update()
    {
        if (!gameObject.activeSelf) return;
        Keyboard kb = Keyboard.current;
        if (kb != null && kb.escapeKey.wasPressedThisFrame)
        {
            UIState.ConsumeEsc();
            Close();
        }
    }

    // ---------------- helpers ----------------

    bool IsAvailable(int type)
    {
        return day != null && (day.docMask & (1 << type)) != 0 && DocumentSystem.Get(passenger, (DocumentType)type) != null;
    }

    int FirstAvailable()
    {
        for (int i = 0; i < 9; i++) if (IsAvailable(i)) return i;
        return 0;
    }

    int NextAvailable(int from)
    {
        for (int k = 1; k <= 9; k++)
        {
            int i = (from + k) % 9;
            if (IsAvailable(i)) return i;
        }
        return from;
    }

    void Say(string text) { feedback.text = text; }

    void Spend(int minutes) { if (spend != null) spend(minutes); }

    // ---------------- drawing ----------------

    void Redraw()
    {
        if (passenger == null) return;

        for (int i = 0; i < 9; i++)
        {
            bool av = IsAvailable(i);
            tabs[i].gameObject.SetActive(av);
            tabImgs[i].color = (i == current) ? new Color(1f, 0.9f, 0.55f) : (compare && i == compareDoc ? new Color(0.65f, 0.85f, 1f) : new Color(0.82f, 0.82f, 0.88f));
        }

        // layout: single big paper or two side by side
        float w = compare ? 700f : 900f;
        UIKit.Anchor(left.root, UIKit.TL, UIKit.TL, UIKit.TL, compare ? new Vector2(40, -100) : new Vector2(290, -100), new Vector2(w, 660));
        right.root.gameObject.SetActive(compare);
        if (compare) UIKit.Anchor(right.root, UIKit.TL, UIKit.TL, UIKit.TL, new Vector2(760, -100), new Vector2(w, 660));
        float zs = (zoom && !compare) ? 1.12f : 1f;
        left.root.localScale = new Vector3(zs, zs, 1f);

        FillPaper(left, current, w);
        if (compare) FillPaper(right, compareDoc, w);

        RefreshStaticTexts();
        marksText.text = Loc.T("Oznaczenia: ", "Marks: ") + marksLeft + "/" + marksMax + Loc.T("  (kliknij linię)", "  (click a line)");
        for (int i = 0; i < 5; i++) queryBtn[i].interactable = !queried[i];

        var sb = new System.Text.StringBuilder();
        int from = Mathf.Max(0, logLines.Count - 7);
        for (int i = from; i < logLines.Count; i++) sb.Append("> ").Append(logLines[i]).Append("\n");
        log.text = sb.ToString();

        btnScan.interactable = HasHidden(current) && !scanned[current];
        btnPhoto.interactable = true;
    }

    bool HasHidden(int type)
    {
        DocumentData d = DocumentSystem.Get(passenger, (DocumentType)type);
        if (d == null) return false;
        List<DocLine> lines = flipped[type] ? d.back : d.front;
        for (int i = 0; i < lines.Count; i++) if (lines[i].hidden) return true;
        return false;
    }

    void FillPaper(Paper p, int type, float width)
    {
        DocumentData d = DocumentSystem.Get(passenger, (DocumentType)type);
        p.doc = d;
        p.width = width;
        if (d == null) return;

        bool back = flipped[type];
        p.title.text = DocumentSystem.TitleOf(d.type);
        p.number.text = Loc.T("Nr ", "No. ") + d.number;
        p.stamp.text = d.stamp;
        p.side.text = back ? Loc.T("strona B (rewers)", "side B (reverse)") : Loc.T("strona A (awers)", "side A (front)");
        p.stamp.gameObject.SetActive(!back);

        // photo slot (only on the ID front)
        bool showPhoto = (d.type == DocumentType.Id) && !back;
        p.photoFrame.gameObject.SetActive(showPhoto);
        if (showPhoto)
        {
            Sprite sp = null;
            if (d.photoIsPlayer) sp = CharacterArt.Conductor().portrait;
            else if (d.photoLook >= 0) sp = CharacterArt.Passenger(d.photoLook).portrait;
            p.photoImg.sprite = sp;
            p.photoImg.enabled = sp != null;
            p.photoQ.gameObject.SetActive(sp == null);
            DocLine photoLine = FindPhotoLine(d);
            p.photoFrame.color = (photoLine != null && photoLine.marked) ? new Color(1f, 0.85f, 0.2f) : new Color(0.15f, 0.12f, 0.10f, 1f);
        }

        List<DocLine> lines = back ? d.back : d.front;
        int row = 0;
        float rowW = width - 40f - (showPhoto ? 170f : 0f);
        for (int i = 0; i < lines.Count && row < p.rows.Length; i++)
        {
            DocLine l = lines[i];
            if (l.isPhoto) continue;
            Row r = p.rows[row++];
            r.line = l;
            r.rt.gameObject.SetActive(true);
            r.rt.sizeDelta = new Vector2(rowW, 44);
            UIKit.Anchor(r.rt, UIKit.TL, UIKit.TL, UIKit.TL, new Vector2(20, -78 - (row - 1) * 46), new Vector2(rowW, 44));

            if (l.hidden)
            {
                bool readable = scanned[type] && SkillSystem.Level(Skill.Documents) >= l.minSkill;
                if (readable)
                {
                    r.label.text = l.label;
                    r.value.text = "<i>" + l.value + "</i>";
                    r.value.color = new Color(0.55f, 0.1f, 0.5f);
                }
                else
                {
                    r.label.text = "░░░░░░";
                    r.value.text = scanned[type] ? "░░░░░░░░ (" + Loc.T("DOKUMENTY ", "DOCUMENTS ") + l.minSkill + ")" : "░░░░░░░░░░";
                    r.value.color = new Color(0.45f, 0.4f, 0.36f, 0.6f);
                }
            }
            else
            {
                r.label.text = l.label;
                r.value.text = l.value;
                r.value.color = UIKit.Ink;
            }
            r.hl.color = l.marked ? new Color(1f, 0.85f, 0.2f, 0.45f) : new Color(1f, 0.85f, 0.2f, 0f);
        }
        for (int i = row; i < p.rows.Length; i++) { p.rows[i].rt.gameObject.SetActive(false); p.rows[i].line = null; }
    }

    static DocLine FindPhotoLine(DocumentData d)
    {
        for (int i = 0; i < d.front.Count; i++) if (d.front[i].isPhoto) return d.front[i];
        return null;
    }

    // ---------------- interaction ----------------

    void OnTab(int idx)
    {
        if (!IsAvailable(idx)) return;
        if (compare)
        {
            if (idx == current) return;
            compareDoc = idx;
        }
        else current = idx;
        if (SoundFX.I != null) SoundFX.I.Paper();
        Redraw();
    }

    void OnRow(Paper p, int rowIndex)
    {
        Row r = p.rows[rowIndex];
        if (r.line == null) return;
        Mark(r.line, p.doc);
    }

    void OnPhotoClick(Paper p)
    {
        if (p.doc == null) return;
        DocLine l = FindPhotoLine(p.doc);
        if (l != null) Mark(l, p.doc);
    }

    // Marking a line as suspicious: reveals whether something is really wrong there.
    void Mark(DocLine l, DocumentData d)
    {
        if (animating) return;
        if (l.hidden && !(scanned[(int)d.type] && SkillSystem.Level(Skill.Documents) >= l.minSkill))
        {
            Say(Loc.T("Za drobne, żeby przeczytać. Użyj skanu mikrotekstu (potrzebny odpowiedni poziom DOKUMENTÓW).", "Too small to read. Use the microtext scan (needs enough DOCUMENTS skill)."));
            return;
        }
        if (l.marked)
        {
            l.marked = false;
            marksLeft = Mathf.Min(marksMax, marksLeft + 0);   // no refund: a mark is a decision
            Redraw();
            return;
        }
        if (marksLeft <= 0)
        {
            Say(Loc.T("Nie masz już oznaczeń dla tego pasażera. Rozwijaj SPOSTRZEGAWCZOŚĆ.", "No marks left for this passenger. Raise OBSERVATION."));
            return;
        }
        marksLeft--;
        l.marked = true;
        Spend(1);
        if (!string.IsNullOrEmpty(l.anomaly))
        {
            bool fresh = AnomalySystem.Discover(l.anomaly, Skill.Documents);
            Say("<color=#7FD08A>" + Loc.T("Tu coś jest nie tak: ", "Something is wrong here: ") + AnomalySystem.Title(l.anomaly) + "</color>");
            if (fresh && SoundFX.I != null) SoundFX.I.Good();
            if (d.type == DocumentType.Id && d.photoIsPlayer && l.isPhoto) OnOwnPhoto();
        }
        else
        {
            Say(Loc.T("Nic niezwykłego w tym miejscu.", "Nothing unusual there."));
            SkillSystem.Give(Skill.Documents, 1);
        }
        Redraw();
    }

    void OnOwnPhoto()
    {
        StoryFlags.Set(Flag.SawOwnPhoto);
        AchievementSystem.Unlock("mirror");
        Codex.Unlock("evt_photo");
    }

    void Rotate()
    {
        if (animating || passenger == null) return;
        StartCoroutine(RotateRoutine());
    }

    IEnumerator RotateRoutine()
    {
        animating = true;
        RectTransform rt = left.root;
        float zs = (zoom && !compare) ? 1.12f : 1f;
        for (float e = 0f; e < 0.14f; e += Time.unscaledDeltaTime)
        {
            rt.localScale = new Vector3(zs * (1f - e / 0.14f), zs, 1f);
            yield return null;
        }
        flipped[current] = !flipped[current];
        if (SoundFX.I != null) SoundFX.I.Paper();
        Redraw();
        for (float e = 0f; e < 0.14f; e += Time.unscaledDeltaTime)
        {
            rt.localScale = new Vector3(zs * (e / 0.14f), zs, 1f);
            yield return null;
        }
        rt.localScale = new Vector3(zs, zs, 1f);
        animating = false;
    }

    void ToggleZoom()
    {
        if (compare) { Say(Loc.T("Nie można powiększać podczas porównywania.", "You cannot zoom while comparing.")); return; }
        zoom = !zoom;
        Redraw();
    }

    void ToggleCompare()
    {
        compare = !compare;
        if (compare)
        {
            zoom = false;
            compareDoc = NextAvailable(current);
            Say(Loc.T("Wybierz drugą kartkę zakładką u góry.", "Pick the second sheet with the tabs above."));
        }
        Redraw();
    }

    void Scan()
    {
        if (passenger == null || scanned[current]) return;
        scanned[current] = true;
        Spend(1);
        if (SoundFX.I != null) SoundFX.I.Paper();
        SkillSystem.Give(Skill.Documents, 3);
        Say(Loc.T("Skanujesz papier pod światło. Ukryty tekst: ", "You hold the paper against the light. Hidden text: ") +
            (HasHidden(current) ? Loc.T("jest.", "present.") : Loc.T("brak.", "none.")));
        Redraw();
    }

    void CheckSignature()
    {
        if (passenger == null) return;
        if (SkillSystem.Level(Skill.Documents) < 2)
        {
            Say(Loc.T("Nie umiesz jeszcze weryfikować podpisów (DOKUMENTY 2).", "You cannot verify signatures yet (DOCUMENTS 2)."));
            return;
        }
        DocumentData d = DocumentSystem.Get(passenger, (DocumentType)current);
        if (d == null) return;
        bool any = false; string found = null;
        List<DocLine> lines = flipped[current] ? d.back : d.front;
        for (int i = 0; i < lines.Count; i++)
        {
            if (!lines[i].isSignature) continue;
            any = true;
            if (lines[i].anomaly == "a_signature") found = lines[i].anomaly;
        }
        Spend(2);
        if (!any) { Say(Loc.T("Na tej stronie nie ma podpisu.", "There is no signature on this side.")); return; }
        SkillSystem.Give(Skill.Documents, 4);
        if (found != null)
        {
            AnomalySystem.Discover(found, Skill.Documents);
            Say("<color=#E07070>" + Loc.T("Podpis jest sfałszowany: zbyt równy, bez drżenia pióra.", "The signature is forged: too even, no tremor of the pen.") + "</color>");
        }
        else Say("<color=#7FD08A>" + Loc.T("Podpis wygląda autentycznie.", "The signature looks genuine.") + "</color>");
        Redraw();
    }

    void CheckPhoto()
    {
        if (passenger == null) return;
        DocumentData id = DocumentSystem.Get(passenger, DocumentType.Id);
        if (id == null || !IsAvailable((int)DocumentType.Id)) { Say(Loc.T("Ten pasażer nie ma dowodu ze zdjęciem.", "This passenger has no photo ID.")); return; }
        current = (int)DocumentType.Id;
        flipped[current] = false;
        Spend(1);
        SkillSystem.Give(Skill.Observation, 3);

        if (id.photoLook == -2)
        {
            AnomalySystem.Discover("a_unknown", Skill.Observation);
            Say("<color=#E07070>" + Loc.T("Zdjęcie jest puste. Papier nie zna twarzy tej osoby.", "The photo is blank. The paper does not know this person's face.") + "</color>");
        }
        else if (passenger.isPlayer)
        {
            Say("<color=#E0B070>" + Loc.T("Zdjęcie przedstawia ciebie. To twoja twarz. Nawet mundur się zgadza.", "The photo shows you. It is your face. Even the uniform matches.") + "</color>");
        }
        else if (id.photoIsPlayer)
        {
            AnomalySystem.Discover("a_photo", Skill.Observation);
            OnOwnPhoto();
            Say("<color=#E07070>" + Loc.T("To ty. Na cudzym dowodzie jest twoja twarz.", "It is you. Someone else's ID carries your face.") + "</color>");
        }
        else if (id.photoLook != passenger.look)
        {
            AnomalySystem.Discover("a_photo", Skill.Observation);
            Say("<color=#E07070>" + Loc.T("Zdjęcie przedstawia kogoś innego niż osoba przed tobą.", "The photo shows someone other than the person in front of you.") + "</color>");
        }
        else Say("<color=#7FD08A>" + Loc.T("Zdjęcie zgadza się z twarzą pasażera.", "The photo matches the passenger's face.") + "</color>");
        Redraw();
    }

    void Query(int q)
    {
        if (passenger == null || queried[q]) return;
        queried[q] = true;
        string anomaly;
        string res = DocumentSystem.Query(passenger, q, out anomaly);
        logLines.Add(DocumentSystem.QueryName(q) + ": " + res);
        Spend(3);
        if (train != null) train.AddElectricity(-2f);
        SkillSystem.Give(Skill.Documents, 4);
        if (SoundFX.I != null) SoundFX.I.Click();
        if (anomaly != null) AnomalySystem.Discover(anomaly, Skill.Documents);
        Redraw();
    }
}
