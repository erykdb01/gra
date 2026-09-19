using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

// Settings screen: volumes, fullscreen, resolution, language, screen shake, horror effects, text speed.
// Built from code; works in the main menu and in the pause menu. ESC or BACK closes it.
public class SettingsPanel : MonoBehaviour
{
    static SettingsPanel instance;

    TextMeshProUGUI title, backLabel, langLabel;
    TextMeshProUGUI masterLabel, musicLabel, sfxLabel;
    TextMeshProUGUI fullLabel, resLabel, shakeLabel, horrorLabel, speedLabel;
    Slider masterSlider, musicSlider, sfxSlider;
    Image polishBtn, englishBtn;
    bool open;

    public static bool IsOpen
    {
        get { return instance != null && instance.gameObject.activeSelf; }
    }

    public static void Open(Canvas canvas)
    {
        if (UIKit.Font == null)
        {
            TMP_Text any = FindAnyObjectByType<TMP_Text>();
            if (any != null) UIKit.Font = any.font;
        }

        if (instance == null)
        {
            RectTransform rt = UIKit.NewRect("SettingsPanel", canvas.transform);
            instance = rt.gameObject.AddComponent<SettingsPanel>();
            instance.Build(rt);
        }

        instance.Refresh();
        instance.gameObject.SetActive(true);
        instance.transform.SetAsLastSibling();
        if (!instance.open) { instance.open = true; UIState.Modal++; }
    }

    public static void CloseIfOpen()
    {
        if (IsOpen) instance.Close();
    }

    void Close()
    {
        if (open) { open = false; UIState.Modal = Mathf.Max(0, UIState.Modal - 1); }
        gameObject.SetActive(false);
        GameSettings.Flush();
    }

    void OnDestroy()
    {
        if (instance == this) instance = null;
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
        bg.color = new Color(0.02f, 0.02f, 0.05f, 0.97f);
        bg.raycastTarget = true;

        title = UIKit.Text("Title", rt, "", 70, UIKit.Amber, TextAlignmentOptions.Center);
        UIKit.Anchor(title.rectTransform, UIKit.TC, UIKit.TC, UIKit.TC, new Vector2(0, -60), new Vector2(1500, 100));

        // ----- left column: sound and language -----
        float lx = -470f;
        masterLabel = SliderRow(rt, lx, -220, out masterSlider, GameSettings.Master, v => { GameSettings.Master = v; RefreshLabels(); });
        musicLabel  = SliderRow(rt, lx, -340, out musicSlider, GameSettings.Music, v => { GameSettings.Music = v; RefreshLabels(); });
        sfxLabel    = SliderRow(rt, lx, -460, out sfxSlider, GameSettings.Sfx, v => { GameSettings.Sfx = v; RefreshLabels(); });

        langLabel = UIKit.Text("Language", rt, "", 34, UIKit.Light, TextAlignmentOptions.Center);
        UIKit.Anchor(langLabel.rectTransform, UIKit.TC, UIKit.TC, UIKit.TC, new Vector2(lx, -580), new Vector2(760, 50));

        TextMeshProUGUI lp, le;
        Button pl = UIKit.MakeButton("LangPl", rt, "POLSKI", new Vector2(240, 70), new Color(0.85f, 0.85f, 0.9f), 30, out lp);
        UIKit.Anchor((RectTransform)pl.transform, UIKit.TC, UIKit.TC, UIKit.TC, new Vector2(lx - 130, -640), new Vector2(240, 70));
        pl.onClick.AddListener(() => { Loc.Set(Loc.Language.Polish); Refresh(); });
        polishBtn = pl.GetComponent<Image>();

        Button en = UIKit.MakeButton("LangEn", rt, "ENGLISH", new Vector2(240, 70), new Color(0.85f, 0.85f, 0.9f), 30, out le);
        UIKit.Anchor((RectTransform)en.transform, UIKit.TC, UIKit.TC, UIKit.TC, new Vector2(lx + 130, -640), new Vector2(240, 70));
        en.onClick.AddListener(() => { Loc.Set(Loc.Language.English); Refresh(); });
        englishBtn = en.GetComponent<Image>();

        // ----- right column: display and gameplay -----
        float rx = 470f;
        fullLabel   = ToggleRow(rt, rx, -220, () => { GameSettings.Fullscreen = !GameSettings.Fullscreen; });
        resLabel    = ToggleRow(rt, rx, -320, CycleResolution);
        shakeLabel  = ToggleRow(rt, rx, -420, () => { GameSettings.ScreenShake = !GameSettings.ScreenShake; });
        horrorLabel = ToggleRow(rt, rx, -520, () => { GameSettings.HorrorEffects = !GameSettings.HorrorEffects; });
        speedLabel  = ToggleRow(rt, rx, -620, () => { GameSettings.TextSpeed = (GameSettings.TextSpeed + 1) % 4; });

        Button back = UIKit.MakeButton("Back", rt, "", new Vector2(560, 84), new Color(0.6f, 0.85f, 0.6f), 36, out backLabel);
        UIKit.Anchor((RectTransform)back.transform, UIKit.BC, UIKit.BC, UIKit.BC, new Vector2(0, 60), new Vector2(560, 84));
        back.onClick.AddListener(Close);
    }

    TextMeshProUGUI SliderRow(RectTransform parent, float x, float y, out Slider slider, float value, UnityEngine.Events.UnityAction<float> onChange)
    {
        TextMeshProUGUI label = UIKit.Text("SliderLabel", parent, "", 34, UIKit.Light, TextAlignmentOptions.Center);
        UIKit.Anchor(label.rectTransform, UIKit.TC, UIKit.TC, UIKit.TC, new Vector2(x, y), new Vector2(760, 50));
        slider = UIKit.MakeSlider("Slider", parent, new Vector2(620, 36), value, onChange);
        UIKit.Anchor((RectTransform)slider.transform, UIKit.TC, UIKit.TC, UIKit.TC, new Vector2(x, y - 66), new Vector2(620, 36));
        return label;
    }

    TextMeshProUGUI ToggleRow(RectTransform parent, float x, float y, UnityEngine.Events.UnityAction action)
    {
        TextMeshProUGUI l;
        Button b = UIKit.MakeButton("Toggle", parent, "", new Vector2(720, 76), new Color(0.85f, 0.85f, 0.9f), 30, out l);
        UIKit.Anchor((RectTransform)b.transform, UIKit.TC, UIKit.TC, UIKit.TC, new Vector2(x, y), new Vector2(720, 76));
        b.onClick.AddListener(() => { action(); Refresh(); });
        return l;
    }

    void CycleResolution()
    {
        int n = GameSettings.Resolutions.Count;
        int i = GameSettings.ResolutionIndex + 1;
        if (i >= n) i = -1;
        GameSettings.ResolutionIndex = i;
    }

    static string OnOff(bool v) { return v ? Loc.T("WŁ.", "ON") : Loc.T("WYŁ.", "OFF"); }

    void RefreshLabels()
    {
        masterLabel.text = Loc.T("GŁOŚNOŚĆ GŁÓWNA: ", "MASTER VOLUME: ") + Mathf.RoundToInt(GameSettings.Master * 100f) + "%";
        musicLabel.text = Loc.T("MUZYKA: ", "MUSIC: ") + Mathf.RoundToInt(GameSettings.Music * 100f) + "%";
        sfxLabel.text = Loc.T("EFEKTY: ", "EFFECTS: ") + Mathf.RoundToInt(GameSettings.Sfx * 100f) + "%";
    }

    void Refresh()
    {
        title.text = Loc.T("USTAWIENIA", "SETTINGS");
        RefreshLabels();
        masterSlider.SetValueWithoutNotify(GameSettings.Master);
        musicSlider.SetValueWithoutNotify(GameSettings.Music);
        sfxSlider.SetValueWithoutNotify(GameSettings.Sfx);

        fullLabel.text = Loc.T("EKRAN: ", "SCREEN: ") + (GameSettings.Fullscreen ? Loc.T("PEŁNY EKRAN", "FULLSCREEN") : Loc.T("OKNO", "WINDOWED"));
        resLabel.text = Loc.T("ROZDZIELCZOŚĆ: ", "RESOLUTION: ") + GameSettings.ResolutionLabel();
        shakeLabel.text = Loc.T("TRZĘSIENIE EKRANU: ", "SCREEN SHAKE: ") + OnOff(GameSettings.ScreenShake);
        horrorLabel.text = Loc.T("EFEKTY GROZY: ", "HORROR EFFECTS: ") + OnOff(GameSettings.HorrorEffects);
        string[] speeds = { Loc.T("WOLNY", "SLOW"), Loc.T("NORMALNY", "NORMAL"), Loc.T("SZYBKI", "FAST"), Loc.T("NATYCHMIAST", "INSTANT") };
        speedLabel.text = Loc.T("TEMPO TEKSTU: ", "TEXT SPEED: ") + speeds[Mathf.Clamp(GameSettings.TextSpeed, 0, 3)];

        langLabel.text = Loc.T("JĘZYK / LANGUAGE", "LANGUAGE / JĘZYK");
        backLabel.text = Loc.T("WRÓĆ", "BACK");

        Color on = new Color(0.6f, 0.85f, 0.6f);
        Color off = new Color(0.85f, 0.85f, 0.9f);
        polishBtn.color = Loc.IsPolish ? on : off;
        englishBtn.color = Loc.IsPolish ? off : on;
    }
}
