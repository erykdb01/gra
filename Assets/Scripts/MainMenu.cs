using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// The cinematic main menu: black screen, rain and a distant train, "LINIA 13", the logo, then the menu.
// The scene only needs a Canvas (and optionally the old Title / BtnPlay / BtnOptions / BtnQuit, which are hidden).
public class MainMenu : MonoBehaviour
{
    Canvas canvas;
    RectTransform root;
    CanvasGroup cover;
    TrainView train;
    LightingController lighting;
    CharacterView conductor, ghost;
    Image logoImg, subtitleBg;
    TextMeshProUGUI introText, counterText, hintText;
    Image polishBtn, englishBtn;
    RectTransform menuRoot;
    readonly List<MenuItemFX> items = new List<MenuItemFX>();
    Sprite logoPl, logoEn;

    bool introRunning = true, skipIntro, starting;
    float idleTime, nextSecret, nextCounter, menuTime, lastMouseMove;
    Vector2 lastMouse;
    int confirmIndex = -1;
    float confirmUntil;
    bool insomniaGiven;

    // ------------------------------------------------------------------ setup

    void Awake()
    {
        Time.timeScale = 1f;
        UIState.Modal = 0;
        UIState.EscFrame = -1;
        canvas = FindAnyObjectByType<Canvas>();
        SaveSystem.Load();
        AudioListener.volume = GameSettings.Master;

        if (canvas == null) return;
        root = (RectTransform)canvas.transform;

        // take the font from the old title text, then hide the old scene UI
        TMP_Text oldTitle = LabelOf("Title");
        if (oldTitle != null) UIKit.Font = oldTitle.font;
        else
        {
            TMP_Text any = FindAnyObjectByType<TMP_Text>();
            if (any != null) UIKit.Font = any.font;
        }
        HideOld("Title"); HideOld("BtnPlay"); HideOld("BtnOptions"); HideOld("BtnQuit");

        BuildWorld();
        BuildUI();

        SoundFX fx = SoundFX.Create(gameObject);
        fx.StartAmbience(false);
        fx.RadioOn(true);
        fx.SetAutoMusic(false);

        Loc.Changed += Refresh;
        nextSecret = 20f;
        nextCounter = UnityEngine.Random.Range(14f, 30f);
    }

    void Start()
    {
        if (canvas != null) StartCoroutine(Intro());
    }

    void OnDestroy()
    {
        Loc.Changed -= Refresh;
    }

    TMP_Text LabelOf(string objectName)
    {
        Transform t = FindDeep(root != null ? root : canvas.transform, objectName);
        if (t == null) return null;
        TMP_Text txt = t.GetComponent<TMP_Text>();
        if (txt == null) txt = t.GetComponentInChildren<TMP_Text>();
        return txt;
    }

    void HideOld(string name)
    {
        Transform t = FindDeep(canvas.transform, name);
        if (t != null) t.gameObject.SetActive(false);
    }

    static Transform FindDeep(Transform parent, string name)
    {
        foreach (Transform child in parent)
        {
            if (child.name == name) return child;
            Transform found = FindDeep(child, name);
            if (found != null) return found;
        }
        return null;
    }

    static Sprite GradientSprite(bool horizontal, Color a, Color b)
    {
        const int n = 64;
        var tex = new Texture2D(horizontal ? n : 1, horizontal ? 1 : n, TextureFormat.RGBA32, false);
        tex.wrapMode = TextureWrapMode.Clamp;
        tex.filterMode = FilterMode.Bilinear;
        for (int i = 0; i < n; i++)
        {
            Color c = Color.Lerp(a, b, i / (float)(n - 1));
            if (horizontal) tex.SetPixel(i, 0, c); else tex.SetPixel(0, i, c);
        }
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100f);
    }

    // The night platform: sky, distant train, platform floor, the conductor with the lantern, rain and fog.
    void BuildWorld()
    {
        Image backdrop = PixelArt.CreateBackdrop(canvas.transform);

        train = TrainView.Create(canvas);
        train.transform.localScale = new Vector3(0.62f, 0.62f, 1f);
        train.ParkOffscreen();

        // the ground under the platform
        Image ground = UIKit.Img("Ground", canvas.transform, GradientSprite(false, new Color(0.02f, 0.02f, 0.035f, 1f), new Color(0.06f, 0.06f, 0.09f, 1f)), Color.white);
        UIKit.Anchor(ground.rectTransform, UIKit.BC, UIKit.BC, UIKit.BC, Vector2.zero, new Vector2(2400f, 452f));

        Image strip = UIKit.Img("PlatformStrip", canvas.transform, PixelArt.Platform(), Color.white);
        UIKit.Anchor(strip.rectTransform, UIKit.BC, UIKit.BC, UIKit.BC, new Vector2(0, 440f), new Vector2(2400f, 90f));

        lighting = LightingController.Create(canvas, null);
        lighting.SetDarkness(0.92f);

        conductor = CharacterView.Create(canvas.transform, CharacterArt.Conductor(), "MenuConductor", true, true, Color.white);
        conductor.SetFeet(560f, 452f);
        conductor.Face(-1);
        conductor.SetMood("calm");
        lighting.SetLantern(new Vector2(500f, 560f));
        conductor.SetBaseAlpha(0f);

        ghost = CharacterView.Create(canvas.transform, CharacterArt.Passenger(12), "MenuGhost", false, false, new Color(0.02f, 0.02f, 0.03f, 1f));
        ghost.SetFeet(-260f, 452f);
        ghost.SetBaseAlpha(0f);

        // order: backdrop, train, ground, strip, back lights, people, front lights
        int i = 0;
        backdrop.transform.SetSiblingIndex(i++);
        train.transform.SetSiblingIndex(i++);
        ground.transform.SetSiblingIndex(i++);
        strip.transform.SetSiblingIndex(i++);
        lighting.BackLayer.SetSiblingIndex(i++);
        conductor.transform.SetSiblingIndex(i++);
        ghost.transform.SetSiblingIndex(i++);
        lighting.FrontLayer.SetSiblingIndex(i++);
    }

    void BuildUI()
    {
        // darker strip on the left so the menu is readable
        Image strip = UIKit.Img("MenuShade", canvas.transform, GradientSprite(true, new Color(0f, 0f, 0.01f, 0.86f), new Color(0f, 0f, 0.01f, 0f)), Color.white);
        UIKit.Anchor(strip.rectTransform, new Vector2(0, 0), new Vector2(0, 1), new Vector2(0, 0.5f), Vector2.zero, new Vector2(760f, 0f));
        subtitleBg = strip;
        strip.color = new Color(1, 1, 1, 0f);

        // logo
        logoImg = UIKit.Img("Logo", canvas.transform, null, new Color(1, 1, 1, 0));
        UIKit.Anchor(logoImg.rectTransform, UIKit.TC, UIKit.TC, UIKit.TC, new Vector2(0, -70), new Vector2(900, 150));

        // menu items
        menuRoot = UIKit.NewRect("MenuItems", canvas.transform);
        UIKit.Anchor(menuRoot, UIKit.TL, UIKit.TL, UIKit.TL, new Vector2(110, -350), new Vector2(560, 640));
        AddItem("STORY MODE", "TRYB FABULARNY", 0, () => StartStory());
        AddItem("NIGHT SHIFT", "NIESKOŃCZONA ZMIANA", 1, () => StartEndless());
        AddItem("CONTINUE", "KONTYNUUJ", 2, () => ContinueStory());
        AddItem("CODEX", "KODEKS", 3.6f, () => CodexUI.Open(canvas));
        AddItem("ACHIEVEMENTS", "OSIĄGNIĘCIA", 4.6f, () => AchievementUI.Open(canvas));
        AddItem("SETTINGS", "USTAWIENIA", 5.9f, () => SettingsPanel.Open(canvas));
        AddItem("QUIT", "WYJDŹ", 7.2f, () => Quit());

        // passenger counter (bottom right) and hint
        counterText = UIKit.Text("Counter", canvas.transform, "PASSENGERS: 13", 30, new Color(0.7f, 0.72f, 0.8f, 0f), TextAlignmentOptions.BottomRight);
        UIKit.Anchor(counterText.rectTransform, UIKit.BR, UIKit.BR, UIKit.BR, new Vector2(-40, 30), new Vector2(600, 44));

        hintText = UIKit.Text("Hint", canvas.transform, "", 22, new Color(0.6f, 0.62f, 0.72f, 0f), TextAlignmentOptions.BottomLeft);
        UIKit.Anchor(hintText.rectTransform, UIKit.BL, UIKit.BL, UIKit.BL, new Vector2(40, 30), new Vector2(900, 34));

        // language buttons
        TextMeshProUGUI l;
        Button pl = UIKit.MakeButton("LangPl", canvas.transform, "PL", new Vector2(90, 56), new Color(0.85f, 0.85f, 0.9f), 28, out l);
        UIKit.Anchor((RectTransform)pl.transform, UIKit.TR, UIKit.TR, UIKit.TR, new Vector2(-140, -24), new Vector2(90, 56));
        pl.onClick.AddListener(() => Loc.Set(Loc.Language.Polish));
        polishBtn = pl.GetComponent<Image>();
        Button en = UIKit.MakeButton("LangEn", canvas.transform, "EN", new Vector2(90, 56), new Color(0.85f, 0.85f, 0.9f), 28, out l);
        UIKit.Anchor((RectTransform)en.transform, UIKit.TR, UIKit.TR, UIKit.TR, new Vector2(-30, -24), new Vector2(90, 56));
        en.onClick.AddListener(() => Loc.Set(Loc.Language.English));
        englishBtn = en.GetComponent<Image>();
        polishBtn.gameObject.SetActive(false);
        englishBtn.gameObject.SetActive(false);

        // intro text in the middle
        introText = UIKit.Text("Intro", canvas.transform, "", 120, new Color(0.9f, 0.88f, 0.8f, 0f), TextAlignmentOptions.Center);
        UIKit.Anchor(introText.rectTransform, UIKit.MC, UIKit.MC, UIKit.MC, new Vector2(0, 60), new Vector2(1600, 200));
        introText.characterSpacing = 30f;

        // the black cover that fades away
        RectTransform cv = UIKit.NewRect("Cover", canvas.transform);
        UIKit.Stretch(cv);
        Image cvImg = cv.gameObject.AddComponent<Image>();
        cvImg.color = Color.black;
        cvImg.raycastTarget = true;
        cover = cv.gameObject.AddComponent<CanvasGroup>();
        cover.alpha = 1f;

        // menu items start hidden
        foreach (MenuItemFX it in items) it.SetVisible(0f);
        Refresh();
    }

    void AddItem(string en, string pl, float slot, Action onClick)
    {
        RectTransform rt = UIKit.NewRect("Item_" + en, menuRoot);
        UIKit.Anchor(rt, UIKit.TL, UIKit.TL, UIKit.TL, new Vector2(0, -slot * 76f), new Vector2(560, 64));
        Image hit = rt.gameObject.AddComponent<Image>();
        hit.color = new Color(0, 0, 0, 0);
        hit.raycastTarget = true;
        TextMeshProUGUI t = UIKit.Text("Label", rt, en, 44, new Color(0.82f, 0.8f, 0.74f, 1f), TextAlignmentOptions.MidlineLeft);
        UIKit.Stretch(t.rectTransform);
        t.textWrappingMode = TextWrappingModes.NoWrap;
        MenuItemFX fx = rt.gameObject.AddComponent<MenuItemFX>();
        fx.Init(this, items.Count, t, en, pl, onClick);
        items.Add(fx);
    }

    // ------------------------------------------------------------------ text

    void Refresh()
    {
        if (canvas == null) return;
        foreach (MenuItemFX it in items) it.Relabel();
        UpdateContinue();
        if (logoImg != null && !introRunning) SetLogo();
        if (hintText != null)
            hintText.text = Loc.T("Wersja 2.0   Trzynaście osób. Zawsze trzynaście.", "Version 2.0   Thirteen people. Always thirteen.");
        if (polishBtn != null)
        {
            Color on = new Color(0.6f, 0.85f, 0.6f);
            Color off = new Color(0.85f, 0.85f, 0.9f);
            polishBtn.color = Loc.IsPolish ? on : off;
            englishBtn.color = Loc.IsPolish ? off : on;
        }
    }

    void UpdateContinue()
    {
        MenuItemFX c = items.Count > 2 ? items[2] : null;
        if (c == null) return;
        bool has = SaveSystem.HasStorySave;
        c.SetEnabled(has);
        if (has)
        {
            int n = SaveSystem.Data.story.night + 1;
            c.extraPl = "  (NOC " + n + ")";
            c.extraEn = "  (NIGHT " + n + ")";
        }
        else { c.extraPl = ""; c.extraEn = ""; }
        c.Relabel();
    }

    void SetLogo()
    {
        bool pl = Loc.IsPolish;
        if (pl && logoPl == null) logoPl = PixelArtExtras.Logo("OSTATNI PERON", new Color32(220, 214, 198, 255), new Color32(150, 20, 20, 255), 13);
        if (!pl && logoEn == null) logoEn = PixelArtExtras.Logo("THE LAST PLATFORM", new Color32(220, 214, 198, 255), new Color32(150, 20, 20, 255), 13);
        Sprite s = pl ? logoPl : logoEn;
        logoImg.sprite = s;
        float k = pl ? 9f : 8f;
        logoImg.rectTransform.sizeDelta = new Vector2(s.rect.width * k, s.rect.height * k);
    }

    // ------------------------------------------------------------------ intro

    bool SkipRequested()
    {
        Keyboard kb = Keyboard.current;
        Mouse m = Mouse.current;
        return skipIntro || (kb != null && (kb.spaceKey.wasPressedThisFrame || kb.enterKey.wasPressedThisFrame || kb.escapeKey.wasPressedThisFrame)) ||
               (m != null && m.leftButton.wasPressedThisFrame);
    }

    IEnumerator Wait(float seconds)
    {
        for (float e = 0f; e < seconds && !skipIntro; e += Time.deltaTime)
        {
            if (SkipRequested()) skipIntro = true;
            yield return null;
        }
    }

    IEnumerator FadeGroup(CanvasGroup g, float to, float seconds)
    {
        float from = g.alpha;
        for (float e = 0f; e < seconds && !skipIntro; e += Time.deltaTime)
        {
            if (SkipRequested()) skipIntro = true;
            g.alpha = Mathf.Lerp(from, to, e / seconds);
            yield return null;
        }
        g.alpha = to;
    }

    IEnumerator FadeText(TMP_Text t, float to, float seconds)
    {
        Color c = t.color;
        float from = c.a;
        for (float e = 0f; e < seconds && !skipIntro; e += Time.deltaTime)
        {
            if (SkipRequested()) skipIntro = true;
            c.a = Mathf.Lerp(from, to, e / seconds);
            t.color = c;
            yield return null;
        }
        c.a = to;
        t.color = c;
    }

    IEnumerator Intro()
    {
        // 1. black: only sound (rain, hum, radio, a far train)
        yield return Wait(2.6f);
        if (!skipIntro && SoundFX.I != null) SoundFX.I.Whistle();

        // 2. the night appears, the train rolls in
        StartCoroutine(train.Arrive());
        yield return FadeGroup(cover, 0f, 3.2f);
        cover.blocksRaycasts = false;
        StartCoroutine(FadeDarkness(0.92f, 0.28f, 4f));
        StartCoroutine(FadeConductor());
        yield return Wait(1.6f);

        // 3. "LINIA 13"
        if (!skipIntro)
        {
            introText.text = Loc.T("LINIA 13", "LINE 13");
            yield return FadeText(introText, 1f, 1.6f);
            if (SoundFX.I != null && !skipIntro) SoundFX.I.Knock();
            yield return Wait(1.8f);
            yield return FadeText(introText, 0f, 1.0f);
        }
        introText.text = "";

        // 4. the logo
        introRunning = false;
        SetLogo();
        if (SoundFX.I != null) { SoundFX.I.PlayMusic("menu"); SoundFX.I.RadioOn(false); }
        if (!skipIntro && SoundFX.I != null) SoundFX.I.Stinger(0.35f);
        StartCoroutine(FadeLogo());
        lighting.SetDarkness(0.25f);
        yield return Wait(1.6f);

        // 5. the menu
        polishBtn.gameObject.SetActive(true);
        englishBtn.gameObject.SetActive(true);
        StartCoroutine(FadeImage(subtitleBg, 1f, 1.2f));
        for (int i = 0; i < items.Count; i++)
        {
            items[i].SetVisible(1f);
            if (!skipIntro) yield return new WaitForSeconds(0.09f);
        }
        StartCoroutine(FadeTextRoutine(counterText, 0.9f, 1.5f));
        StartCoroutine(FadeTextRoutine(hintText, 0.8f, 1.5f));
        cover.alpha = 0f;
        cover.blocksRaycasts = false;
        introText.text = "";
        Refresh();
        skipIntro = false;
        lastMouseMove = Time.unscaledTime;
        lighting.SetDarkness(0.25f);
        foreach (MenuItemFX it in items) it.SetVisible(1f);
        conductor.SetBaseAlpha(1f);
        subtitleBg.color = Color.white;
    }

    IEnumerator FadeDarkness(float from, float to, float seconds)
    {
        for (float e = 0f; e < seconds && !skipIntro; e += Time.deltaTime)
        {
            lighting.SetDarkness(Mathf.Lerp(from, to, e / seconds));
            yield return null;
        }
        lighting.SetDarkness(to);
    }

    IEnumerator FadeConductor()
    {
        yield return new WaitForSeconds(1.5f);
        for (float e = 0f; e < 2f; e += Time.deltaTime)
        {
            conductor.SetBaseAlpha(Mathf.Clamp01(e / 2f));
            yield return null;
        }
        conductor.SetBaseAlpha(1f);
    }

    IEnumerator FadeLogo()
    {
        RectTransform r = logoImg.rectTransform;
        for (float e = 0f; e < 1.8f; e += Time.deltaTime)
        {
            float k = 1f - Mathf.Pow(1f - Mathf.Clamp01(e / 1.8f), 3f);
            logoImg.color = new Color(1, 1, 1, k);
            r.localScale = Vector3.one * Mathf.Lerp(1.06f, 1f, k);
            yield return null;
        }
        logoImg.color = Color.white;
        r.localScale = Vector3.one;
    }

    IEnumerator FadeImage(Image img, float to, float seconds)
    {
        Color c = img.color;
        float from = c.a;
        for (float e = 0f; e < seconds; e += Time.deltaTime)
        {
            c.a = Mathf.Lerp(from, to, e / seconds);
            img.color = c;
            yield return null;
        }
        c.a = to;
        img.color = c;
    }

    IEnumerator FadeTextRoutine(TMP_Text t, float to, float seconds)
    {
        Color c = t.color;
        float from = c.a;
        for (float e = 0f; e < seconds; e += Time.deltaTime)
        {
            c.a = Mathf.Lerp(from, to, e / seconds);
            t.color = c;
            yield return null;
        }
        c.a = to;
        t.color = c;
    }

    // ------------------------------------------------------------------ update: secrets, idle, ESC

    void Update()
    {
        if (canvas == null) return;

        Keyboard kb = Keyboard.current;
        Mouse m = Mouse.current;

        if (introRunning)
        {
            if (SkipRequested()) skipIntro = true;
            return;
        }

        // ESC closes panels (they consume it themselves); nothing else to do in the menu

        // idle detection
        Vector2 mp = m != null ? m.position.ReadValue() : Vector2.zero;
        if ((mp - lastMouse).sqrMagnitude > 4f || (kb != null && kb.anyKey.wasPressedThisFrame) || (m != null && m.leftButton.wasPressedThisFrame))
            lastMouseMove = Time.unscaledTime;
        lastMouse = mp;
        idleTime = Time.unscaledTime - lastMouseMove;

        menuTime += Time.unscaledDeltaTime;
        if (!insomniaGiven && menuTime > 120f && !starting)
        {
            insomniaGiven = true;
            AchievementSystem.Unlock("insomnia");
        }

        if (confirmIndex >= 0 && Time.unscaledTime > confirmUntil)
        {
            confirmIndex = -1;
            foreach (MenuItemFX it in items) it.Relabel();
        }

        if (starting || SettingsPanel.IsOpen || CodexUI.IsOpen || AchievementUI.IsOpen) return;

        // ambient secrets after some idle time
        if (idleTime > 8f && Time.unscaledTime > nextSecret)
        {
            nextSecret = Time.unscaledTime + UnityEngine.Random.Range(28f, 70f);
            StartCoroutine(Secret(UnityEngine.Random.Range(0, 3)));
        }
        if (Time.unscaledTime > nextCounter)
        {
            nextCounter = Time.unscaledTime + (SaveSystem.Data.endings.Count > 0 ? UnityEngine.Random.Range(20f, 45f) : UnityEngine.Random.Range(50f, 110f));
            StartCoroutine(CounterSecret());
        }
    }

    IEnumerator Secret(int kind)
    {
        if (!SaveSystem.Data.settings.horrorEffects && kind != 2) kind = 2;
        switch (kind)
        {
            case 0:   // a silhouette on the platform
            {
                float x = UnityEngine.Random.value < 0.5f ? -300f : 120f;
                ghost.SetFeet(x, 452f);
                yield return ghost.FadeTo(0.9f, 2.4f);
                yield return new WaitForSeconds(2.2f);
                if (SoundFX.I != null) SoundFX.I.Breath();
                yield return ghost.FadeTo(0f, 0.25f);
                break;
            }
            case 1:   // an open door in the train
                train.SetDoorOpen(true);
                if (SoundFX.I != null) SoundFX.I.Door();
                yield return new WaitForSeconds(5f);
                train.SetDoorOpen(false);
                break;
            default:  // the lights flicker
                train.FlickerFor(1.4f);
                lighting.FlickerLantern(1.2f);
                if (SoundFX.I != null) SoundFX.I.PowerDown();
                break;
        }
    }

    IEnumerator CounterSecret()
    {
        if (counterText == null) yield break;
        counterText.text = "PASSENGERS: 14";
        counterText.color = new Color(0.85f, 0.3f, 0.28f, 1f);
        train.FlickerFor(0.7f);
        AchievementSystem.Unlock("fourteen");
        yield return new WaitForSeconds(1.1f);
        counterText.text = Loc.T("PASAŻEROWIE: 13", "PASSENGERS: 13");
        counterText.color = new Color(0.7f, 0.72f, 0.8f, 0.9f);
    }

    // ------------------------------------------------------------------ menu actions (called by items)

    public void OnHover(int index)
    {
        if (lighting != null) lighting.FlickerLantern(0.12f);
        if (conductor != null) conductor.SetMood(index == 6 ? "cold" : "calm");
        lastMouseMove = Time.unscaledTime;
    }

    bool Confirm(int index)
    {
        if (confirmIndex == index && Time.unscaledTime < confirmUntil) { confirmIndex = -1; return true; }
        confirmIndex = index;
        confirmUntil = Time.unscaledTime + 4f;
        foreach (MenuItemFX it in items) it.Relabel();
        return false;
    }

    public bool IsConfirming(int index) { return confirmIndex == index && Time.unscaledTime < confirmUntil; }

    void StartStory()
    {
        if (starting) return;
        if (SaveSystem.HasStorySave && !Confirm(0)) return;
        GameState.BeginStory(false);
        StartCoroutine(LoadGame());
    }

    void StartEndless()
    {
        if (starting) return;
        GameState.BeginEndless();
        StartCoroutine(LoadGame());
    }

    void ContinueStory()
    {
        if (starting || !SaveSystem.HasStorySave) return;
        GameState.BeginStory(true);
        StartCoroutine(LoadGame());
    }

    IEnumerator LoadGame()
    {
        starting = true;
        cover.blocksRaycasts = true;
        if (SoundFX.I != null) { SoundFX.I.Horn(); SoundFX.I.PlayMusic(null); }
        for (float e = 0f; e < 0.9f; e += Time.deltaTime)
        {
            cover.alpha = e / 0.9f;
            yield return null;
        }
        cover.alpha = 1f;
        SceneManager.LoadScene("Peron");
    }

    // ------------------------------------------------------------------ methods for the scene's old OnClick events

    public void Play() { StartStory(); }

    public void Options()
    {
        if (canvas == null) canvas = FindAnyObjectByType<Canvas>();
        if (canvas != null) SettingsPanel.Open(canvas);
    }

    public void Quit()
    {
        GameSettings.QuitGame();
    }
}

// One text entry of the main menu: hover slides it to the right and lights it up, click plays a sound.
public class MenuItemFX : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public string extraPl = "", extraEn = "";

    MainMenu menu;
    int index;
    TextMeshProUGUI label;
    string en, pl;
    Action onClick;
    bool hover, enabledItem = true;
    float slide, visible, shown;
    RectTransform rt;
    Vector2 basePos;
    bool baseSet;

    static readonly Color Normal = new Color(0.82f, 0.8f, 0.74f, 1f);
    static readonly Color Hot = new Color(0.98f, 0.82f, 0.45f, 1f);
    static readonly Color Dead = new Color(0.4f, 0.4f, 0.42f, 1f);

    public void Init(MainMenu m, int i, TextMeshProUGUI l, string en, string pl, Action click)
    {
        menu = m; index = i; label = l; this.en = en; this.pl = pl; onClick = click;
        rt = (RectTransform)transform;
    }

    public void SetEnabled(bool e) { enabledItem = e; }

    public void SetVisible(float v) { visible = v; }

    public void Relabel()
    {
        string text = Loc.T(pl, en) + Loc.T(extraPl, extraEn);
        if (menu != null && menu.IsConfirming(index))
            text = Loc.T("NOWA GRA? KLIKNIJ JESZCZE RAZ", "NEW GAME? CLICK AGAIN");
        label.text = text;
    }

    public void OnPointerEnter(PointerEventData e)
    {
        if (!enabledItem) return;
        hover = true;
        if (SoundFX.I != null) SoundFX.I.Tick();
        menu.OnHover(index);
    }

    public void OnPointerExit(PointerEventData e) { hover = false; }

    public void OnPointerClick(PointerEventData e)
    {
        if (!enabledItem) return;
        if (SoundFX.I != null) SoundFX.I.Click();
        onClick();
    }

    void Update()
    {
        if (!baseSet) { basePos = rt.anchoredPosition; baseSet = true; }
        float target = hover && enabledItem ? 1f : 0f;
        slide = Mathf.MoveTowards(slide, target, Time.unscaledDeltaTime * 6f);
        float s = slide * slide * (3f - 2f * slide);
        shown = Mathf.MoveTowards(shown, visible, Time.unscaledDeltaTime * 3f);
        float a = Mathf.Clamp01(shown);
        label.color = Color.Lerp(enabledItem ? Normal : Dead, Hot, s) * new Color(1, 1, 1, a);
        rt.anchoredPosition = basePos + new Vector2(s * 22f - (1f - a) * 40f, 0f);
    }
}
