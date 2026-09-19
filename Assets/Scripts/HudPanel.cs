using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

// Left column: today's rule and three meters (Conscience, Discipline, Stress).
// The full STATUS panel (reputation, authority, train systems, skills, score) opens with TAB or the STATUS button,
// so the main screen never shows everything at once.
public class HudPanel : MonoBehaviour
{
    TextMeshProUGUI ruleLabel, ruleText, hint;
    TextMeshProUGUI conLabel, disLabel, strLabel;
    Image conFill, disFill, strFill;
    TextMeshProUGUI conVal, disVal, strVal;

    // status panel
    RectTransform status;
    CanvasGroup statusGroup;
    TextMeshProUGUI stTitle, stConductor, stTrain, stSkills, stScore, stClose;
    readonly Image[] cFill = new Image[5];
    readonly TextMeshProUGUI[] cLab = new TextMeshProUGUI[5], cVal = new TextMeshProUGUI[5];
    readonly Image[] tFill = new Image[5];
    readonly TextMeshProUGUI[] tLab = new TextMeshProUGUI[5], tVal = new TextMeshProUGUI[5];
    readonly Image[] sFill = new Image[5];
    readonly TextMeshProUGUI[] sLab = new TextMeshProUGUI[5], sVal = new TextMeshProUGUI[5], sDesc = new TextMeshProUGUI[5];
    TrainSystem train;
    int shiftScore;
    float nextRefresh;

    public bool StatusOpen { get { return status != null && status.gameObject.activeSelf; } }

    public static HudPanel Create(Canvas canvas)
    {
        RectTransform rt = UIKit.NewRect("HudPanel", canvas.transform);
        HudPanel h = rt.gameObject.AddComponent<HudPanel>();
        h.Build(rt, canvas);
        return h;
    }

    void Build(RectTransform rt, Canvas canvas)
    {
        UIKit.Anchor(rt, UIKit.TL, UIKit.TL, UIKit.TL, new Vector2(40, -190), new Vector2(310, 324));
        Image bg = rt.gameObject.AddComponent<Image>();
        bg.sprite = PixelArt.Panel();
        bg.color = Color.white;
        bg.raycastTarget = false;

        ruleLabel = UIKit.Text("RuleLabel", rt, "", 22, UIKit.Amber, TextAlignmentOptions.TopLeft);
        UIKit.Anchor(ruleLabel.rectTransform, UIKit.TL, UIKit.TL, UIKit.TL, new Vector2(14, -8), new Vector2(282, 28));

        ruleText = UIKit.Text("RuleText", rt, "", 25, UIKit.Light, TextAlignmentOptions.TopLeft);
        UIKit.Anchor(ruleText.rectTransform, UIKit.TL, UIKit.TL, UIKit.TL, new Vector2(14, -36), new Vector2(282, 120));
        ruleText.enableAutoSizing = true;
        ruleText.fontSizeMin = 15;
        ruleText.fontSizeMax = 25;

        conFill = Bar(rt, "Conscience", -160, new Color(0.43f, 0.67f, 0.94f), out conVal, out conLabel);
        disFill = Bar(rt, "Discipline", -204, new Color(0.94f, 0.75f, 0.35f), out disVal, out disLabel);
        strFill = Bar(rt, "Stress", -248, new Color(0.90f, 0.31f, 0.27f), out strVal, out strLabel);

        hint = UIKit.Text("Hint", rt, "", 17, new Color(0.6f, 0.62f, 0.72f, 1f), TextAlignmentOptions.TopLeft);
        UIKit.Anchor(hint.rectTransform, UIKit.TL, UIKit.TL, UIKit.TL, new Vector2(14, -294), new Vector2(282, 24));

        BuildStatus(canvas);
        RefreshLabels();
        Loc.Changed += RefreshLabels;
    }

    void OnDestroy()
    {
        Loc.Changed -= RefreshLabels;
    }

    void RefreshLabels()
    {
        ruleLabel.text = Loc.T("ZASADA DNIA", "TODAY'S RULE");
        conLabel.text = Loc.T("SUMIENIE", "CONSCIENCE");
        disLabel.text = Loc.T("DYSCYPLINA", "DISCIPLINE");
        strLabel.text = Loc.T("STRES", "STRESS");
        hint.text = Loc.T("ESC pauza   TAB status", "ESC pause   TAB status");
        if (stTitle != null)
        {
            stTitle.text = Loc.T("STATUS KONDUKTORA", "CONDUCTOR STATUS");
            stConductor.text = Loc.T("KONDUKTOR", "CONDUCTOR");
            stTrain.text = Loc.T("POCIĄG", "TRAIN");
            stSkills.text = Loc.T("UMIEJĘTNOŚCI", "SKILLS");
            stClose.text = Loc.T("ZAMKNIJ (TAB)", "CLOSE (TAB)");
            string[] cn = { Loc.T("DYSCYPLINA", "DISCIPLINE"), Loc.T("SUMIENIE", "CONSCIENCE"), Loc.T("STRES", "STRESS"), Loc.T("REPUTACJA", "REPUTATION"), Loc.T("AUTORYTET", "AUTHORITY") };
            string[] tn = { Loc.T("INTEGRALNOŚĆ", "INTEGRITY"), Loc.T("TEMPERATURA", "TEMPERATURE"), Loc.T("ELEKTRYCZNOŚĆ", "ELECTRICITY"), Loc.T("CIŚNIENIE", "PRESSURE"), Loc.T("SYGNAŁ", "SIGNAL") };
            for (int i = 0; i < 5; i++)
            {
                cLab[i].text = cn[i];
                tLab[i].text = tn[i];
                sLab[i].text = SkillSystem.Name((Skill)i);
                sDesc[i].text = SkillSystem.Description((Skill)i);
            }
            RefreshStatus();
        }
    }

    Image Bar(RectTransform parent, string name, float y, Color color, out TextMeshProUGUI value, out TextMeshProUGUI label)
    {
        label = UIKit.Text(name, parent, "", 19, UIKit.Light, TextAlignmentOptions.TopLeft);
        UIKit.Anchor(label.rectTransform, UIKit.TL, UIKit.TL, UIKit.TL, new Vector2(14, y), new Vector2(220, 22));

        value = UIKit.Text(name + "Value", parent, "", 19, UIKit.Light, TextAlignmentOptions.TopRight);
        UIKit.Anchor(value.rectTransform, UIKit.TL, UIKit.TL, UIKit.TL, new Vector2(200, y), new Vector2(96, 22));

        Image back = UIKit.Img(name + "Back", parent, PixelArt.Solid(), new Color(0.08f, 0.08f, 0.12f, 1f));
        UIKit.Anchor(back.rectTransform, UIKit.TL, UIKit.TL, UIKit.TL, new Vector2(14, y - 24), new Vector2(282, 12));

        Image fill = UIKit.Img(name + "Fill", back.transform, PixelArt.Solid(), color);
        fill.rectTransform.anchorMin = Vector2.zero;
        fill.rectTransform.anchorMax = new Vector2(0.5f, 1f);
        fill.rectTransform.offsetMin = Vector2.zero;
        fill.rectTransform.offsetMax = Vector2.zero;
        return fill;
    }

    public void SetRule(string text) { ruleText.text = text; }

    static void SetBar(Image fill, TextMeshProUGUI val, int v, int max)
    {
        float k = Mathf.Clamp01(v / (float)max);
        fill.rectTransform.anchorMax = new Vector2(k, 1f);
        val.text = Mathf.Clamp(v, 0, max) + "/" + max;
    }

    public void SetMeters(int conscience, int discipline, int stress, int stressMax)
    {
        SetBar(conFill, conVal, conscience, 10);
        SetBar(disFill, disVal, discipline, 10);
        SetBar(strFill, strVal, stress, stressMax);
        if (StatusOpen) RefreshStatus();
    }

    public void SetTrain(TrainSystem t) { train = t; }
    public void SetScore(int score) { shiftScore = score; }

    // ---------------- status panel ----------------

    void BuildStatus(Canvas canvas)
    {
        status = UIKit.NewRect("StatusPanel", canvas.transform);
        UIKit.Anchor(status, UIKit.MC, UIKit.MC, UIKit.MC, Vector2.zero, new Vector2(1160, 700));
        Image bg = status.gameObject.AddComponent<Image>();
        bg.sprite = PixelArt.Panel();
        bg.color = new Color(1f, 1f, 1f, 0.98f);
        bg.raycastTarget = true;
        statusGroup = status.gameObject.AddComponent<CanvasGroup>();

        stTitle = UIKit.Text("Title", status, "", 44, UIKit.Amber, TextAlignmentOptions.Center);
        UIKit.Anchor(stTitle.rectTransform, UIKit.TC, UIKit.TC, UIKit.TC, new Vector2(0, -14), new Vector2(1000, 56));

        stConductor = UIKit.Text("H1", status, "", 26, UIKit.Amber, TextAlignmentOptions.TopLeft);
        UIKit.Anchor(stConductor.rectTransform, UIKit.TL, UIKit.TL, UIKit.TL, new Vector2(40, -84), new Vector2(500, 32));
        stTrain = UIKit.Text("H2", status, "", 26, UIKit.Amber, TextAlignmentOptions.TopLeft);
        UIKit.Anchor(stTrain.rectTransform, UIKit.TL, UIKit.TL, UIKit.TL, new Vector2(620, -84), new Vector2(500, 32));

        Color[] cc = { new Color(0.94f, 0.75f, 0.35f), new Color(0.43f, 0.67f, 0.94f), new Color(0.9f, 0.31f, 0.27f), new Color(0.6f, 0.85f, 0.6f), new Color(0.8f, 0.6f, 0.9f) };
        Color[] tc = { new Color(0.6f, 0.85f, 0.6f), new Color(0.5f, 0.8f, 0.95f), new Color(0.95f, 0.85f, 0.35f), new Color(0.9f, 0.4f, 0.3f), new Color(0.6f, 0.9f, 0.75f) };
        for (int i = 0; i < 5; i++)
        {
            float y = -122 - i * 46;
            cFill[i] = StatusRow(status, 40, y, 500, cc[i], out cLab[i], out cVal[i]);
            tFill[i] = StatusRow(status, 620, y, 500, tc[i], out tLab[i], out tVal[i]);
        }

        stSkills = UIKit.Text("H3", status, "", 26, UIKit.Amber, TextAlignmentOptions.TopLeft);
        UIKit.Anchor(stSkills.rectTransform, UIKit.TL, UIKit.TL, UIKit.TL, new Vector2(40, -366), new Vector2(500, 32));

        for (int i = 0; i < 5; i++)
        {
            float y = -404 - i * 44;
            sFill[i] = StatusRow(status, 40, y, 560, new Color(0.9f, 0.7f, 0.4f), out sLab[i], out sVal[i]);
            sDesc[i] = UIKit.Text("Desc" + i, status, "", 18, new Color(0.7f, 0.72f, 0.8f, 1f), TextAlignmentOptions.TopLeft);
            UIKit.Anchor(sDesc[i].rectTransform, UIKit.TL, UIKit.TL, UIKit.TL, new Vector2(640, y), new Vector2(480, 40));
            sDesc[i].enableAutoSizing = true;
            sDesc[i].fontSizeMin = 12;
            sDesc[i].fontSizeMax = 18;
        }

        stScore = UIKit.Text("Score", status, "", 26, UIKit.Light, TextAlignmentOptions.TopLeft);
        UIKit.Anchor(stScore.rectTransform, UIKit.BL, UIKit.BL, UIKit.BL, new Vector2(40, 18), new Vector2(700, 36));

        TextMeshProUGUI cl;
        Button close = UIKit.MakeButton("Close", status, "", new Vector2(260, 52), new Color(0.6f, 0.85f, 0.6f), 24, out cl);
        UIKit.Anchor((RectTransform)close.transform, UIKit.BR, UIKit.BR, UIKit.BR, new Vector2(-30, 14), new Vector2(260, 52));
        close.onClick.AddListener(CloseStatus);
        stClose = cl;

        status.gameObject.SetActive(false);
    }

    // A labelled bar: the label above-left, the value above-right, the bar under them.
    Image StatusRow(RectTransform parent, float x, float y, float width, Color color, out TextMeshProUGUI label, out TextMeshProUGUI value)
    {
        label = UIKit.Text("L", parent, "", 20, UIKit.Light, TextAlignmentOptions.TopLeft);
        UIKit.Anchor(label.rectTransform, UIKit.TL, UIKit.TL, UIKit.TL, new Vector2(x, y), new Vector2(width * 0.6f, 24));
        value = UIKit.Text("V", parent, "", 20, UIKit.Light, TextAlignmentOptions.TopRight);
        UIKit.Anchor(value.rectTransform, UIKit.TL, UIKit.TL, UIKit.TL, new Vector2(x + width * 0.5f, y), new Vector2(width * 0.5f, 24));
        Image back = UIKit.Img("B", parent, PixelArt.Solid(), new Color(0.08f, 0.08f, 0.12f, 1f));
        UIKit.Anchor(back.rectTransform, UIKit.TL, UIKit.TL, UIKit.TL, new Vector2(x, y - 26), new Vector2(width, 10));
        Image fill = UIKit.Img("F", back.transform, PixelArt.Solid(), color);
        fill.rectTransform.anchorMin = Vector2.zero;
        fill.rectTransform.anchorMax = new Vector2(0.5f, 1f);
        fill.rectTransform.offsetMin = Vector2.zero;
        fill.rectTransform.offsetMax = Vector2.zero;
        return fill;
    }

    static void Fill(Image f, float k) { f.rectTransform.anchorMax = new Vector2(Mathf.Clamp01(k), 1f); }

    void RefreshStatus()
    {
        if (stTitle == null) return;
        Fill(cFill[0], PlayerStats.Discipline / 10f); cVal[0].text = PlayerStats.Discipline + "/10";
        Fill(cFill[1], PlayerStats.Conscience / 10f); cVal[1].text = PlayerStats.Conscience + "/10";
        Fill(cFill[2], PlayerStats.Stress / 100f);    cVal[2].text = PlayerStats.Stress + "/100";
        Fill(cFill[3], PlayerStats.Reputation / 100f); cVal[3].text = PlayerStats.Reputation + "/100";
        Fill(cFill[4], PlayerStats.Authority / 100f); cVal[4].text = PlayerStats.Authority + "/100";

        for (int i = 0; i < 5; i++)
        {
            float v = train != null ? train.Get01(i) : 1f;
            Fill(tFill[i], v);
            tVal[i].text = Mathf.RoundToInt(v * 100f) + "%";
        }

        for (int i = 0; i < 5; i++)
        {
            Skill s = (Skill)i;
            int lvl = SkillSystem.Level(s);
            Fill(sFill[i], lvl >= SkillSystem.MaxLevel ? 1f : SkillSystem.Progress01(s));
            sVal[i].text = Loc.T("Poz. ", "Lv ") + lvl + (lvl >= SkillSystem.MaxLevel ? " MAX" : "");
        }
        stScore.text = Loc.T("WYNIK ZMIANY: ", "SHIFT SCORE: ") + shiftScore;
    }

    public void ToggleStatus()
    {
        if (StatusOpen) CloseStatus(); else OpenStatus();
    }

    public void OpenStatus()
    {
        if (StatusOpen || DocumentInspector.IsOpen || Time.timeScale == 0f) return;
        status.gameObject.SetActive(true);
        status.SetAsLastSibling();
        UIState.Modal++;
        RefreshLabels();
        RefreshStatus();
        StartCoroutine(UIAnim.Fade(statusGroup, 0f, 1f, 0.15f));
        if (SoundFX.I != null) SoundFX.I.Paper();
    }

    public void CloseStatus()
    {
        if (!StatusOpen) return;
        status.gameObject.SetActive(false);
        UIState.Modal = Mathf.Max(0, UIState.Modal - 1);
        if (SoundFX.I != null) SoundFX.I.Click();
    }

    void Update()
    {
        Keyboard kb = Keyboard.current;
        if (kb == null) return;
        if (kb.tabKey.wasPressedThisFrame && Time.timeScale > 0f && !DocumentInspector.IsOpen) ToggleStatus();
        if (StatusOpen)
        {
            if (kb.escapeKey.wasPressedThisFrame) { UIState.ConsumeEsc(); CloseStatus(); }
            if (Time.unscaledTime > nextRefresh) { nextRefresh = Time.unscaledTime + 0.3f; RefreshStatus(); }
        }
    }
}
