using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Small popup in the corner: achievements, codex entries, skill level-ups, warnings.
public class ToastUI : MonoBehaviour
{
    struct Item { public string title, body; public Color color; }

    static ToastUI instance;
    static readonly Queue<Item> pending = new Queue<Item>();

    RectTransform panel;
    TextMeshProUGUI titleText, bodyText;
    Image stripe;
    bool running;

    public static void Show(string title, string body, Color color)
    {
        pending.Enqueue(new Item { title = title, body = body, color = color });
        if (instance == null) Create();
        if (instance != null) instance.Kick();
    }

    static void Create()
    {
        var go = new GameObject("Toasts");
        Canvas c = go.AddComponent<Canvas>();
        c.renderMode = RenderMode.ScreenSpaceOverlay;
        c.sortingOrder = 500;
        CanvasScaler cs = go.AddComponent<CanvasScaler>();
        cs.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        cs.referenceResolution = new Vector2(1920, 1080);
        cs.matchWidthOrHeight = 0.5f;
        instance = go.AddComponent<ToastUI>();
        instance.Build();
    }

    void Build()
    {
        panel = UIKit.NewRect("Toast", transform);
        UIKit.Anchor(panel, new Vector2(1, 0), new Vector2(1, 0), new Vector2(1, 0), new Vector2(460, 40), new Vector2(430, 92));
        Image bg = panel.gameObject.AddComponent<Image>();
        bg.sprite = PixelArt.Panel();
        bg.color = Color.white;
        bg.raycastTarget = false;

        stripe = UIKit.Img("Stripe", panel, PixelArt.Solid(), Color.white);
        UIKit.Anchor(stripe.rectTransform, new Vector2(0, 0), new Vector2(0, 1), new Vector2(0, 0.5f), new Vector2(6, 0), new Vector2(6, -16));

        titleText = UIKit.Text("Title", panel, "", 20, UIKit.Amber, TextAlignmentOptions.TopLeft);
        UIKit.Anchor(titleText.rectTransform, UIKit.TL, UIKit.TL, UIKit.TL, new Vector2(24, -10), new Vector2(390, 26));
        bodyText = UIKit.Text("Body", panel, "", 26, UIKit.Light, TextAlignmentOptions.TopLeft);
        UIKit.Anchor(bodyText.rectTransform, UIKit.TL, UIKit.TL, UIKit.TL, new Vector2(24, -36), new Vector2(390, 50));
        bodyText.textWrappingMode = TextWrappingModes.NoWrap;
        bodyText.overflowMode = TextOverflowModes.Ellipsis;
    }

    void Kick()
    {
        if (!running) StartCoroutine(Run());
    }

    void OnDestroy()
    {
        if (instance == this) instance = null;
    }

    IEnumerator Run()
    {
        running = true;
        while (pending.Count > 0)
        {
            Item it = pending.Dequeue();
            titleText.text = it.title;
            bodyText.text = it.body;
            stripe.color = it.color;

            // slide in (unscaled time: works while the game is paused)
            for (float e = 0f; e < 0.3f; e += Time.unscaledDeltaTime)
            {
                float k = 1f - Mathf.Pow(1f - e / 0.3f, 3f);
                panel.anchoredPosition = new Vector2(Mathf.Lerp(460f, -30f, k), 40f);
                yield return null;
            }
            panel.anchoredPosition = new Vector2(-30f, 40f);
            for (float e = 0f; e < 3.2f; e += Time.unscaledDeltaTime) yield return null;
            for (float e = 0f; e < 0.3f; e += Time.unscaledDeltaTime)
            {
                panel.anchoredPosition = new Vector2(Mathf.Lerp(-30f, 460f, e / 0.3f), 40f);
                yield return null;
            }
            panel.anchoredPosition = new Vector2(460f, 40f);
        }
        running = false;
    }
}
