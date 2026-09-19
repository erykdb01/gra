using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// ESC opens the pause menu: Resume / Codex / Settings / Restart Night / Main Menu.
// The game world is frozen (Time.timeScale = 0). Restart and Main Menu ask for a second click.
public class PauseMenu : MonoBehaviour
{
    public static bool IsPaused;

    Canvas canvas;
    GameObject panel;
    TextMeshProUGUI title, info;
    readonly List<TextMeshProUGUI> labels = new List<TextMeshProUGUI>();
    readonly List<string> textsPl = new List<string>();
    readonly List<string> textsEn = new List<string>();
    int confirming = -1;
    string infoText = "";

    public static PauseMenu Create(Canvas canvas)
    {
        var host = new GameObject("PauseController");
        PauseMenu pm = host.AddComponent<PauseMenu>();
        pm.canvas = canvas;
        pm.Build();
        return pm;
    }

    public void SetInfo(string text)
    {
        infoText = text ?? "";
        if (info != null) info.text = infoText;
    }

    void Build()
    {
        RectTransform rt = UIKit.NewRect("PausePanel", canvas.transform);
        UIKit.Stretch(rt);
        panel = rt.gameObject;

        Image bg = panel.AddComponent<Image>();
        bg.color = new Color(0.02f, 0.02f, 0.05f, 0.88f);
        bg.raycastTarget = true;

        title = UIKit.Text("Title", rt, "", 78, UIKit.Amber, TextAlignmentOptions.Center);
        UIKit.Anchor(title.rectTransform, UIKit.TC, UIKit.TC, UIKit.TC, new Vector2(0, -90), new Vector2(1500, 110));

        info = UIKit.Text("Info", rt, "", 28, new Color(0.65f, 0.68f, 0.78f, 1f), TextAlignmentOptions.Center);
        UIKit.Anchor(info.rectTransform, UIKit.TC, UIKit.TC, UIKit.TC, new Vector2(0, -200), new Vector2(1500, 44));

        AddButton(rt, "KONTYNUUJ", "RESUME", -290, new Color(0.6f, 0.85f, 0.6f), Resume);
        AddButton(rt, "KODEKS", "CODEX", -390, new Color(0.85f, 0.85f, 0.9f), () => CodexUI.Open(canvas));
        AddButton(rt, "USTAWIENIA", "SETTINGS", -490, new Color(0.85f, 0.85f, 0.9f), () => SettingsPanel.Open(canvas));
        AddButton(rt, "POWTÓRZ NOC", "RESTART NIGHT", -590, new Color(0.9f, 0.8f, 0.55f), RestartNight);
        AddButton(rt, "MENU GŁÓWNE", "MAIN MENU", -690, new Color(0.9f, 0.5f, 0.45f), MainMenuClicked);

        RefreshLabels();
        Loc.Changed += RefreshLabels;
        panel.SetActive(false);
    }

    void AddButton(RectTransform parent, string pl, string en, float y, Color tint, UnityEngine.Events.UnityAction action)
    {
        TextMeshProUGUI l;
        Button b = UIKit.MakeButton(en, parent, "", new Vector2(600, 80), tint, 34, out l);
        UIKit.Anchor((RectTransform)b.transform, UIKit.TC, UIKit.TC, UIKit.TC, new Vector2(0, y), new Vector2(600, 80));
        b.onClick.AddListener(action);
        labels.Add(l);
        textsPl.Add(pl);
        textsEn.Add(en);
    }

    void RefreshLabels()
    {
        title.text = Loc.T("PAUZA", "PAUSED");
        info.text = infoText;
        for (int i = 0; i < labels.Count; i++)
        {
            string t = Loc.T(textsPl[i], textsEn[i]);
            if (i == confirming) t = Loc.T("NA PEWNO? KLIKNIJ JESZCZE RAZ", "SURE? CLICK AGAIN");
            labels[i].text = t;
        }
    }

    void RestartNight()
    {
        if (confirming != 3) { confirming = 3; RefreshLabels(); return; }
        Time.timeScale = 1f;
        IsPaused = false;
        UIState.Modal = 0;
        GameState.ForcedNight = GameManager.CurrentNight;
        SceneManager.LoadScene("Peron");
    }

    void MainMenuClicked()
    {
        if (confirming != 4) { confirming = 4; RefreshLabels(); return; }
        Time.timeScale = 1f;
        IsPaused = false;
        UIState.Modal = 0;
        SceneManager.LoadScene("MainMenu");
    }

    // ESC is handled after every other panel had its chance to consume it this frame.
    void LateUpdate()
    {
        Keyboard kb = Keyboard.current;
        if (kb == null || !kb.escapeKey.wasPressedThisFrame) return;
        if (UIState.EscConsumed) return;
        if (SettingsPanel.IsOpen || CodexUI.IsOpen || AchievementUI.IsOpen) return;

        if (panel.activeSelf) Resume();
        else if (UIState.Modal == 0) Pause();
    }

    void Pause()
    {
        confirming = -1;
        RefreshLabels();
        panel.SetActive(true);
        panel.transform.SetAsLastSibling();
        Time.timeScale = 0f;
        IsPaused = true;
        UIState.Modal++;
    }

    void Resume()
    {
        confirming = -1;
        panel.SetActive(false);
        Time.timeScale = 1f;
        IsPaused = false;
        UIState.Modal = Mathf.Max(0, UIState.Modal - 1);
    }

    void OnDestroy()
    {
        Loc.Changed -= RefreshLabels;
        Time.timeScale = 1f;
        IsPaused = false;
    }
}
