using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

// The Codex: everything the conductor has learned, sorted by category. Undiscovered entries show as "???".
public class CodexUI : MonoBehaviour
{
    static CodexUI instance;

    TextMeshProUGUI title, detailTitle, detailText, closeLabel, counter;
    readonly Button[] tabs = new Button[8];
    readonly TextMeshProUGUI[] tabLabels = new TextMeshProUGUI[8];
    RectTransform listContent;
    ScrollRect scroll;
    int category;
    string selected;
    bool open;

    public static bool IsOpen { get { return instance != null && instance.gameObject.activeSelf; } }

    public static void Open(Canvas canvas)
    {
        if (UIKit.Font == null)
        {
            TMP_Text any = FindAnyObjectByType<TMP_Text>();
            if (any != null) UIKit.Font = any.font;
        }
        if (instance == null)
        {
            RectTransform rt = UIKit.NewRect("CodexPanel", canvas.transform);
            instance = rt.gameObject.AddComponent<CodexUI>();
            instance.Build(rt);
        }
        instance.gameObject.SetActive(true);
        instance.transform.SetAsLastSibling();
        if (!instance.open) { instance.open = true; UIState.Modal++; }
        instance.RefreshAll();
    }

    public static void CloseIfOpen() { if (IsOpen) instance.Close(); }

    void Close()
    {
        if (open) { open = false; UIState.Modal = Mathf.Max(0, UIState.Modal - 1); }
        gameObject.SetActive(false);
        if (SoundFX.I != null) SoundFX.I.Click();
    }

    void OnDestroy()
    {
        if (instance == this) instance = null;
        Loc.Changed -= RefreshAll;
    }

    void Update()
    {
        Keyboard kb = Keyboard.current;
        if (kb != null && kb.escapeKey.wasPressedThisFrame)
        {
            UIState.ConsumeEsc();
            Close();
        }
    }

    static string CategoryName(int c)
    {
        switch ((CodexCategory)c)
        {
            case CodexCategory.People:    return Loc.T("LUDZIE", "PEOPLE");
            case CodexCategory.Locations: return Loc.T("MIEJSCA", "PLACES");
            case CodexCategory.Anomalies: return Loc.T("ANOMALIE", "ANOMALIES");
            case CodexCategory.Documents: return Loc.T("DOKUMENTY", "DOCUMENTS");
            case CodexCategory.Rules:     return Loc.T("ZASADY", "RULES");
            case CodexCategory.Events:    return Loc.T("ZDARZENIA", "EVENTS");
            case CodexCategory.Line13:    return Loc.T("LINIA 13", "LINE 13");
            default:                      return "???";
        }
    }

    void Build(RectTransform rt)
    {
        UIKit.Stretch(rt);
        Image bg = rt.gameObject.AddComponent<Image>();
        bg.color = new Color(0.02f, 0.02f, 0.05f, 0.98f);
        bg.raycastTarget = true;

        title = UIKit.Text("Title", rt, "", 60, UIKit.Amber, TextAlignmentOptions.Center);
        UIKit.Anchor(title.rectTransform, UIKit.TC, UIKit.TC, UIKit.TC, new Vector2(0, -24), new Vector2(1200, 80));

        counter = UIKit.Text("Counter", rt, "", 26, new Color(0.65f, 0.68f, 0.78f, 1f), TextAlignmentOptions.Center);
        UIKit.Anchor(counter.rectTransform, UIKit.TC, UIKit.TC, UIKit.TC, new Vector2(0, -100), new Vector2(1200, 36));

        // category tabs
        for (int i = 0; i < 8; i++)
        {
            int idx = i;
            TextMeshProUGUI l;
            Button b = UIKit.MakeButton("Tab" + i, rt, "", new Vector2(212, 58), new Color(0.85f, 0.85f, 0.9f), 21, out l);
            UIKit.Anchor((RectTransform)b.transform, UIKit.TL, UIKit.TL, UIKit.TL, new Vector2(40 + i * 226, -146), new Vector2(212, 58));
            l.enableAutoSizing = true; l.fontSizeMin = 13; l.fontSizeMax = 21;
            b.onClick.AddListener(() => { category = idx; selected = null; RefreshAll(); });
            tabs[i] = b; tabLabels[i] = l;
        }

        // list (left) and details (right)
        listContent = UIKit.MakeScroll("List", rt, new Vector2(620, 720), out scroll);
        UIKit.Anchor((RectTransform)listContent.parent.parent, UIKit.TL, UIKit.TL, UIKit.TL, new Vector2(40, -222), new Vector2(620, 720));

        RectTransform box = UIKit.NewRect("Detail", rt);
        UIKit.Anchor(box, UIKit.TL, UIKit.TL, UIKit.TL, new Vector2(690, -222), new Vector2(1190, 720));
        Image boxBg = box.gameObject.AddComponent<Image>();
        boxBg.sprite = PixelArt.Panel();
        boxBg.color = Color.white;
        boxBg.raycastTarget = false;

        detailTitle = UIKit.Text("DTitle", box, "", 44, UIKit.Amber, TextAlignmentOptions.TopLeft);
        UIKit.Anchor(detailTitle.rectTransform, UIKit.TL, UIKit.TL, UIKit.TL, new Vector2(30, -24), new Vector2(1130, 64));
        detailTitle.enableAutoSizing = true; detailTitle.fontSizeMin = 24; detailTitle.fontSizeMax = 44;

        detailText = UIKit.Text("DText", box, "", 30, UIKit.Light, TextAlignmentOptions.TopLeft);
        UIKit.Anchor(detailText.rectTransform, UIKit.TL, UIKit.TL, UIKit.TL, new Vector2(30, -106), new Vector2(1130, 590));
        detailText.enableAutoSizing = true; detailText.fontSizeMin = 18; detailText.fontSizeMax = 30;

        Button close = UIKit.MakeButton("Close", rt, "", new Vector2(400, 70), new Color(0.6f, 0.85f, 0.6f), 30, out closeLabel);
        UIKit.Anchor((RectTransform)close.transform, UIKit.BC, UIKit.BC, UIKit.BC, new Vector2(0, 24), new Vector2(400, 70));
        close.onClick.AddListener(Close);

        Loc.Changed += RefreshAll;
    }

    void RefreshAll()
    {
        if (this == null || !gameObject.activeInHierarchy) return;
        title.text = Loc.T("KODEKS", "CODEX");
        closeLabel.text = Loc.T("ZAMKNIJ", "CLOSE");
        counter.text = Loc.T("Odkryte wpisy: ", "Entries found: ") + Codex.UnlockedCount + " / " + Codex.All.Count;

        for (int i = 0; i < 8; i++)
        {
            int total = 0, found = 0;
            foreach (CodexEntry e in Codex.All)
                if ((int)e.category == i) { total++; if (Codex.IsUnlocked(e.id)) found++; }
            tabLabels[i].text = CategoryName(i) + " " + found + "/" + total;
            tabs[i].GetComponent<Image>().color = i == category ? new Color(0.94f, 0.78f, 0.42f) : new Color(0.85f, 0.85f, 0.9f);
        }

        for (int i = listContent.childCount - 1; i >= 0; i--) Destroy(listContent.GetChild(i).gameObject);

        var entries = new List<CodexEntry>();
        foreach (CodexEntry e in Codex.All) if ((int)e.category == category) entries.Add(e);

        for (int i = 0; i < entries.Count; i++)
        {
            CodexEntry e = entries[i];
            bool known = Codex.IsUnlocked(e.id);
            TextMeshProUGUI l;
            Button b = UIKit.MakeButton("Entry", listContent, "", new Vector2(0, 56), known ? new Color(0.85f, 0.85f, 0.9f) : new Color(0.5f, 0.5f, 0.55f), 24, out l);
            RectTransform brt = (RectTransform)b.transform;
            brt.anchorMin = new Vector2(0, 1); brt.anchorMax = new Vector2(1, 1); brt.pivot = new Vector2(0.5f, 1f);
            brt.anchoredPosition = new Vector2(0, -i * 62);
            brt.sizeDelta = new Vector2(-10, 56);
            l.text = known ? e.title : "???";
            l.enableAutoSizing = true; l.fontSizeMin = 14; l.fontSizeMax = 24;
            string id = e.id;
            b.onClick.AddListener(() => { selected = id; ShowDetail(); });
            if (selected == null && known) selected = id;
        }
        listContent.sizeDelta = new Vector2(0, Mathf.Max(720, entries.Count * 62));
        scroll.verticalNormalizedPosition = 1f;
        ShowDetail();
    }

    void ShowDetail()
    {
        CodexEntry e = selected != null ? Codex.Get(selected) : null;
        if (e == null || !Codex.IsUnlocked(e.id))
        {
            detailTitle.text = "???";
            detailText.text = Loc.T("Nie odkryto jeszcze tego wpisu. Obserwuj, pytaj i sprawdzaj dokumenty.", "You have not discovered this entry yet. Observe, ask and check the documents.");
            return;
        }
        detailTitle.text = e.title;
        detailText.text = e.text;
    }
}
