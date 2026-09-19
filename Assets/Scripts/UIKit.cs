using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

// Tiny helpers for building UI from code.
public static class UIKit
{
    public static TMP_FontAsset Font;

    public static readonly Color Ink   = new Color(0.16f, 0.12f, 0.09f, 1f);
    public static readonly Color Amber = new Color(0.94f, 0.78f, 0.42f, 1f);
    public static readonly Color Light = new Color(0.92f, 0.90f, 0.84f, 1f);

    public static readonly Vector2 TL = new Vector2(0f, 1f);
    public static readonly Vector2 TC = new Vector2(0.5f, 1f);
    public static readonly Vector2 BC = new Vector2(0.5f, 0f);
    public static readonly Vector2 MC = new Vector2(0.5f, 0.5f);
    public static readonly Vector2 BL = new Vector2(0f, 0f);
    public static readonly Vector2 BR = new Vector2(1f, 0f);
    public static readonly Vector2 TR = new Vector2(1f, 1f);

    public static RectTransform NewRect(string name, Transform parent)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.layer = 5; // UI layer
        go.transform.SetParent(parent, false);
        return (RectTransform)go.transform;
    }

    public static void Anchor(RectTransform r, Vector2 min, Vector2 max, Vector2 pivot, Vector2 pos, Vector2 size)
    {
        r.anchorMin = min;
        r.anchorMax = max;
        r.pivot = pivot;
        r.anchoredPosition = pos;
        r.sizeDelta = size;
    }

    public static void Stretch(RectTransform r)
    {
        r.anchorMin = Vector2.zero;
        r.anchorMax = Vector2.one;
        r.offsetMin = Vector2.zero;
        r.offsetMax = Vector2.zero;
    }

    public static Image Img(string name, Transform parent, Sprite sprite, Color color)
    {
        RectTransform rt = NewRect(name, parent);
        Image img = rt.gameObject.AddComponent<Image>();
        img.sprite = sprite;
        img.color = color;
        img.raycastTarget = false;
        return img;
    }

    public static TextMeshProUGUI Text(string name, Transform parent, string text, float size, Color color, TextAlignmentOptions align)
    {
        RectTransform rt = NewRect(name, parent);
        TextMeshProUGUI t = rt.gameObject.AddComponent<TextMeshProUGUI>();
        if (Font != null) t.font = Font;
        t.text = text;
        t.fontSize = size;
        t.color = color;
        t.alignment = align;
        t.raycastTarget = false;
        t.textWrappingMode = TextWrappingModes.Normal;
        return t;
    }

    // A pixel-art button with a label. Returns the Button; label is exposed via the out parameter.
    public static Button MakeButton(string name, Transform parent, string label, Vector2 size, Color tint, float fontSize, out TextMeshProUGUI labelText)
    {
        RectTransform rt = NewRect(name, parent);
        rt.sizeDelta = size;
        Image img = rt.gameObject.AddComponent<Image>();
        img.sprite = PixelArt.ButtonSprite();
        img.color = tint;
        Button b = rt.gameObject.AddComponent<Button>();
        b.targetGraphic = img;
        ColorBlock cb = b.colors;
        cb.highlightedColor = new Color(1.12f, 1.12f, 1.12f, 1f);
        cb.pressedColor = new Color(0.75f, 0.75f, 0.75f, 1f);
        cb.disabledColor = new Color(0.45f, 0.45f, 0.45f, 0.75f);
        b.colors = cb;

        rt.gameObject.AddComponent<ButtonFX>();

        labelText = Text("Label", rt, label, fontSize, Ink, TextAlignmentOptions.Center);
        Stretch(labelText.rectTransform);
        labelText.margin = new Vector4(8, 2, 8, 2);
        return b;
    }

    // A horizontal slider (0..1) in pixel style.
    public static Slider MakeSlider(string name, Transform parent, Vector2 size, float value, UnityAction<float> onChange)
    {
        RectTransform root = NewRect(name, parent);
        root.sizeDelta = size;
        Image bg = root.gameObject.AddComponent<Image>();
        bg.sprite = PixelArt.Solid();
        bg.color = new Color(0.08f, 0.08f, 0.12f, 1f);
        bg.raycastTarget = true;

        RectTransform fillArea = NewRect("Fill Area", root);
        Stretch(fillArea);
        fillArea.offsetMin = new Vector2(4, 4);
        fillArea.offsetMax = new Vector2(-4, -4);
        Image fill = Img("Fill", fillArea, PixelArt.Solid(), Amber);
        fill.rectTransform.anchorMin = Vector2.zero;
        fill.rectTransform.anchorMax = Vector2.one;
        fill.rectTransform.offsetMin = Vector2.zero;
        fill.rectTransform.offsetMax = Vector2.zero;

        RectTransform handleArea = NewRect("Handle Slide Area", root);
        Stretch(handleArea);
        handleArea.offsetMin = new Vector2(10, 0);
        handleArea.offsetMax = new Vector2(-10, 0);
        Image handle = Img("Handle", handleArea, PixelArt.Solid(), Light);
        handle.rectTransform.sizeDelta = new Vector2(20, size.y + 10);
        handle.raycastTarget = true;

        Slider s = root.gameObject.AddComponent<Slider>();
        s.fillRect = fill.rectTransform;
        s.handleRect = handle.rectTransform;
        s.targetGraphic = handle;
        s.direction = Slider.Direction.LeftToRight;
        s.minValue = 0f;
        s.maxValue = 1f;
        s.value = value;
        if (onChange != null) s.onValueChanged.AddListener(onChange);
        return s;
    }

    // A vertically scrolling area. Fill "content" with children and set its height.
    public static RectTransform MakeScroll(string name, Transform parent, Vector2 size, out ScrollRect scroll)
    {
        RectTransform root = NewRect(name, parent);
        root.sizeDelta = size;
        Image hit = root.gameObject.AddComponent<Image>();
        hit.color = new Color(0f, 0f, 0f, 0f);
        hit.raycastTarget = true;

        RectTransform viewport = NewRect("Viewport", root);
        Stretch(viewport);
        viewport.gameObject.AddComponent<RectMask2D>();

        RectTransform content = NewRect("Content", viewport);
        content.anchorMin = new Vector2(0f, 1f);
        content.anchorMax = new Vector2(1f, 1f);
        content.pivot = new Vector2(0.5f, 1f);
        content.anchoredPosition = Vector2.zero;
        content.sizeDelta = new Vector2(0f, size.y);

        scroll = root.gameObject.AddComponent<ScrollRect>();
        scroll.viewport = viewport;
        scroll.content = content;
        scroll.horizontal = false;
        scroll.vertical = true;
        scroll.movementType = ScrollRect.MovementType.Clamped;
        scroll.scrollSensitivity = 40f;
        return content;
    }
}
