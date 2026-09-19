using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

// Hover / click feedback for buttons: a slight scale, a soft tick, a press sound and an optional tooltip.
public class ButtonFX : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler
{
    public string tooltip;
    public bool silent;

    RectTransform rt;
    Selectable sel;
    float target = 1f, current = 1f;

    void Awake()
    {
        rt = (RectTransform)transform;
        sel = GetComponent<Selectable>();
    }

    bool Active { get { return sel == null || sel.IsInteractable(); } }

    public void OnPointerEnter(PointerEventData e)
    {
        if (!Active) return;
        target = 1.05f;
        if (!silent && SoundFX.I != null) SoundFX.I.Tick();
        if (!string.IsNullOrEmpty(tooltip)) TooltipUI.Show(tooltip);
    }

    public void OnPointerExit(PointerEventData e)
    {
        target = 1f;
        if (!string.IsNullOrEmpty(tooltip)) TooltipUI.Hide();
    }

    public void OnPointerDown(PointerEventData e)
    {
        if (!Active) return;
        target = 0.96f;
        if (!silent && SoundFX.I != null) SoundFX.I.Click();
    }

    void OnDisable()
    {
        target = 1f; current = 1f;
        if (rt != null) rt.localScale = Vector3.one;
        if (!string.IsNullOrEmpty(tooltip)) TooltipUI.Hide();
    }

    void Update()
    {
        if (Mathf.Abs(current - target) < 0.001f) { if (target == 0.96f) target = 1.05f; return; }
        current = Mathf.MoveTowards(current, target, Time.unscaledDeltaTime * 3f);
        rt.localScale = new Vector3(current, current, 1f);
    }
}

// A tooltip that follows the mouse. Has its own overlay canvas, so it works in every scene.
public class TooltipUI : MonoBehaviour
{
    static TooltipUI instance;
    RectTransform panel;
    TextMeshProUGUI label;
    RectTransform canvasRt;

    public static void Show(string text)
    {
        if (instance == null) Create();
        if (instance == null) return;
        instance.label.text = text;
        instance.panel.gameObject.SetActive(true);
        instance.Follow();
    }

    public static void Hide()
    {
        if (instance != null) instance.panel.gameObject.SetActive(false);
    }

    static void Create()
    {
        var go = new GameObject("Tooltip");
        Canvas c = go.AddComponent<Canvas>();
        c.renderMode = RenderMode.ScreenSpaceOverlay;
        c.sortingOrder = 600;
        CanvasScaler cs = go.AddComponent<CanvasScaler>();
        cs.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        cs.referenceResolution = new Vector2(1920, 1080);
        cs.matchWidthOrHeight = 0.5f;
        instance = go.AddComponent<TooltipUI>();
        instance.canvasRt = (RectTransform)go.transform;
        instance.panel = UIKit.NewRect("Panel", go.transform);
        instance.panel.sizeDelta = new Vector2(420f, 70f);
        instance.panel.pivot = new Vector2(0f, 1f);
        instance.panel.anchorMin = instance.panel.anchorMax = new Vector2(0f, 0f);
        Image bg = instance.panel.gameObject.AddComponent<Image>();
        bg.sprite = PixelArt.Panel();
        bg.color = Color.white;
        bg.raycastTarget = false;
        instance.label = UIKit.Text("Text", instance.panel, "", 22, UIKit.Light, TextAlignmentOptions.TopLeft);
        UIKit.Stretch(instance.label.rectTransform);
        instance.label.margin = new Vector4(12, 8, 12, 8);
        instance.panel.gameObject.SetActive(false);
    }

    void OnDestroy() { if (instance == this) instance = null; }

    void Follow()
    {
        Mouse m = Mouse.current;
        if (m == null || canvasRt == null) return;
        Vector2 sp = m.position.ReadValue();
        Vector2 local;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRt, sp, null, out local);
        Vector2 size = canvasRt.rect.size;
        Vector2 pos = local + size * 0.5f + new Vector2(18f, -12f);
        pos.x = Mathf.Clamp(pos.x, 0f, size.x - panel.sizeDelta.x);
        pos.y = Mathf.Clamp(pos.y, panel.sizeDelta.y, size.y);
        panel.anchoredPosition = pos;
    }

    void Update()
    {
        if (panel != null && panel.gameObject.activeSelf) Follow();
    }
}

// Small animation helpers for panels (they use unscaled time so they also work while the game is paused).
public static class UIAnim
{
    public static IEnumerator Fade(CanvasGroup g, float from, float to, float seconds)
    {
        g.alpha = from;
        for (float e = 0f; e < seconds; e += Time.unscaledDeltaTime)
        {
            g.alpha = Mathf.Lerp(from, to, e / seconds);
            yield return null;
        }
        g.alpha = to;
    }

    public static IEnumerator Slide(RectTransform rt, Vector2 from, Vector2 to, float seconds)
    {
        rt.anchoredPosition = from;
        for (float e = 0f; e < seconds; e += Time.unscaledDeltaTime)
        {
            float k = 1f - Mathf.Pow(1f - Mathf.Clamp01(e / seconds), 3f);
            rt.anchoredPosition = Vector2.LerpUnclamped(from, to, k);
            yield return null;
        }
        rt.anchoredPosition = to;
    }

    public static IEnumerator Pop(RectTransform rt, float seconds)
    {
        for (float e = 0f; e < seconds; e += Time.unscaledDeltaTime)
        {
            float k = e / seconds;
            float s = 0.85f + 0.15f * (1f - Mathf.Pow(1f - k, 3f));
            rt.localScale = new Vector3(s, s, 1f);
            yield return null;
        }
        rt.localScale = Vector3.one;
    }
}

// Shared UI state: which full-screen panels are open, and whether ESC was already used this frame.
public static class UIState
{
    public static int Modal;            // number of open full-screen panels (inspector, codex, settings...)
    public static int EscFrame = -1;    // frame in which ESC was consumed by a panel

    // Panels destroyed with their scene cannot decrement the counter, so every new scene starts from zero.
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void Init()
    {
        Modal = 0;
        EscFrame = -1;
        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
    }

    static void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
    {
        if (mode == UnityEngine.SceneManagement.LoadSceneMode.Single) { Modal = 0; EscFrame = -1; }
    }

    public static void ConsumeEsc() { EscFrame = Time.frameCount; }
    public static bool EscConsumed { get { return EscFrame == Time.frameCount; } }
}
