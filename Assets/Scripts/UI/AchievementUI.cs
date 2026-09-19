using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

// List of achievements. Secret ones stay hidden ("???") until unlocked.
public class AchievementUI : MonoBehaviour
{
    static AchievementUI instance;

    TextMeshProUGUI title, counter, closeLabel;
    RectTransform content;
    ScrollRect scroll;
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
            RectTransform rt = UIKit.NewRect("AchievementPanel", canvas.transform);
            instance = rt.gameObject.AddComponent<AchievementUI>();
            instance.Build(rt);
        }
        instance.gameObject.SetActive(true);
        instance.transform.SetAsLastSibling();
        if (!instance.open) { instance.open = true; UIState.Modal++; }
        instance.Refresh();
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
        Loc.Changed -= Refresh;
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

    void Build(RectTransform rt)
    {
        UIKit.Stretch(rt);
        Image bg = rt.gameObject.AddComponent<Image>();
        bg.color = new Color(0.02f, 0.02f, 0.05f, 0.98f);
        bg.raycastTarget = true;

        title = UIKit.Text("Title", rt, "", 60, UIKit.Amber, TextAlignmentOptions.Center);
        UIKit.Anchor(title.rectTransform, UIKit.TC, UIKit.TC, UIKit.TC, new Vector2(0, -24), new Vector2(1200, 80));
        counter = UIKit.Text("Counter", rt, "", 26, new Color(0.65f, 0.68f, 0.78f, 1f), TextAlignmentOptions.Center);
        UIKit.Anchor(counter.rectTransform, UIKit.TC, UIKit.TC, UIKit.TC, new Vector2(0, -104), new Vector2(1200, 36));

        content = UIKit.MakeScroll("List", rt, new Vector2(1500, 780), out scroll);
        UIKit.Anchor((RectTransform)content.parent.parent, UIKit.TC, UIKit.TC, UIKit.TC, new Vector2(0, -150), new Vector2(1500, 780));

        Button close = UIKit.MakeButton("Close", rt, "", new Vector2(400, 70), new Color(0.6f, 0.85f, 0.6f), 30, out closeLabel);
        UIKit.Anchor((RectTransform)close.transform, UIKit.BC, UIKit.BC, UIKit.BC, new Vector2(0, 24), new Vector2(400, 70));
        close.onClick.AddListener(Close);

        Loc.Changed += Refresh;
    }

    void Refresh()
    {
        if (this == null || !gameObject.activeInHierarchy) return;
        title.text = Loc.T("OSIĄGNIĘCIA", "ACHIEVEMENTS");
        closeLabel.text = Loc.T("ZAMKNIJ", "CLOSE");
        counter.text = AchievementSystem.UnlockedCount + " / " + AchievementSystem.All.Count;

        for (int i = content.childCount - 1; i >= 0; i--) Destroy(content.GetChild(i).gameObject);

        int n = 0;
        foreach (AchievementDef a in AchievementSystem.All)
        {
            bool got = AchievementSystem.IsUnlocked(a.id);
            RectTransform row = UIKit.NewRect("Row", content);
            row.anchorMin = new Vector2(0, 1); row.anchorMax = new Vector2(1, 1); row.pivot = new Vector2(0.5f, 1f);
            row.anchoredPosition = new Vector2(0, -n * 96);
            row.sizeDelta = new Vector2(-20, 88);
            Image img = row.gameObject.AddComponent<Image>();
            img.sprite = PixelArt.Panel();
            img.color = got ? Color.white : new Color(0.55f, 0.55f, 0.6f, 1f);
            img.raycastTarget = false;

            string t = got || !a.secret ? a.title : "???";
            string d = got || !a.secret ? a.desc : Loc.T("Sekret. Odkryj go w grze.", "A secret. Find it in the game.");
            TextMeshProUGUI tt = UIKit.Text("T", row, (got ? "[X] " : "[ ] ") + t, 30, got ? UIKit.Amber : UIKit.Light, TextAlignmentOptions.TopLeft);
            UIKit.Anchor(tt.rectTransform, UIKit.TL, UIKit.TL, UIKit.TL, new Vector2(24, -8), new Vector2(1400, 38));
            TextMeshProUGUI dd = UIKit.Text("D", row, d, 24, new Color(0.75f, 0.77f, 0.85f, 1f), TextAlignmentOptions.TopLeft);
            UIKit.Anchor(dd.rectTransform, UIKit.TL, UIKit.TL, UIKit.TL, new Vector2(24, -48), new Vector2(1400, 34));
            n++;
        }
        content.sizeDelta = new Vector2(0, Mathf.Max(780, n * 96));
        scroll.verticalNormalizedPosition = 1f;
    }
}
