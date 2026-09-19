using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

// Full-screen panel for night intros, game over and endings. Up to three buttons; the result is in Choice.
public class Overlay : MonoBehaviour
{
    public int Choice = -1;

    CanvasGroup group;
    TextMeshProUGUI title, body, label1, label2, label3;
    Button button1, button2, button3;
    Image bg;

    public static Overlay Create(Canvas canvas)
    {
        RectTransform rt = UIKit.NewRect("Overlay", canvas.transform);
        Overlay o = rt.gameObject.AddComponent<Overlay>();
        o.Build(rt);
        return o;
    }

    void Build(RectTransform rt)
    {
        UIKit.Stretch(rt);
        bg = rt.gameObject.AddComponent<Image>();
        bg.color = new Color(0.02f, 0.02f, 0.05f, 0.95f);
        bg.raycastTarget = true;
        group = rt.gameObject.AddComponent<CanvasGroup>();

        title = UIKit.Text("Title", rt, "", 74, UIKit.Amber, TextAlignmentOptions.Center);
        UIKit.Anchor(title.rectTransform, UIKit.TC, UIKit.TC, UIKit.TC, new Vector2(0, -90), new Vector2(1600, 120));
        title.enableAutoSizing = true;
        title.fontSizeMin = 34;
        title.fontSizeMax = 74;

        body = UIKit.Text("Body", rt, "", 34, UIKit.Light, TextAlignmentOptions.Top);
        UIKit.Anchor(body.rectTransform, UIKit.TC, UIKit.TC, UIKit.TC, new Vector2(0, -230), new Vector2(1400, 580));
        body.enableAutoSizing = true;
        body.fontSizeMin = 20;
        body.fontSizeMax = 34;

        button1 = UIKit.MakeButton("Primary", rt, "", new Vector2(600, 80), new Color(0.6f, 0.85f, 0.6f), 32, out label1);
        UIKit.Anchor((RectTransform)button1.transform, UIKit.BC, UIKit.BC, UIKit.BC, new Vector2(0, 230), new Vector2(600, 80));
        button1.onClick.AddListener(() => Choice = 0);

        button2 = UIKit.MakeButton("Secondary", rt, "", new Vector2(600, 68), new Color(0.85f, 0.85f, 0.9f), 28, out label2);
        UIKit.Anchor((RectTransform)button2.transform, UIKit.BC, UIKit.BC, UIKit.BC, new Vector2(0, 140), new Vector2(600, 68));
        button2.onClick.AddListener(() => Choice = 1);

        button3 = UIKit.MakeButton("Tertiary", rt, "", new Vector2(600, 68), new Color(0.85f, 0.85f, 0.9f), 28, out label3);
        UIKit.Anchor((RectTransform)button3.transform, UIKit.BC, UIKit.BC, UIKit.BC, new Vector2(0, 52), new Vector2(600, 68));
        button3.onClick.AddListener(() => Choice = 2);

        gameObject.SetActive(false);
    }

    // The background is normally almost black; a scene may want it a little transparent.
    public void SetBackground(Color c) { if (bg != null) bg.color = c; }

    // Shows the overlay and waits until the player picks a button. Result is in Choice (0, 1 or 2).
    // typed = the body appears letter by letter (endings).
    public IEnumerator ShowRoutine(string t, string b, string primary, string secondary)
    {
        yield return Show(t, b, primary, secondary, null, false);
    }

    public IEnumerator Show(string t, string b, string primary, string secondary, string tertiary, bool typed)
    {
        title.text = t;
        body.text = b;
        label1.text = primary;
        button1.gameObject.SetActive(!string.IsNullOrEmpty(primary));
        button2.gameObject.SetActive(!string.IsNullOrEmpty(secondary));
        button3.gameObject.SetActive(!string.IsNullOrEmpty(tertiary));
        if (!string.IsNullOrEmpty(secondary)) label2.text = secondary;
        if (!string.IsNullOrEmpty(tertiary)) label3.text = tertiary;

        Choice = -1;
        gameObject.SetActive(true);
        transform.SetAsLastSibling();
        group.blocksRaycasts = true;
        group.alpha = 0f;

        // the buttons wait for the text
        bool hideButtons = typed;
        if (hideButtons) { button1.gameObject.SetActive(false); button2.gameObject.SetActive(false); button3.gameObject.SetActive(false); }

        for (float e = 0f; e < 0.4f; e += Time.unscaledDeltaTime)
        {
            group.alpha = e / 0.4f;
            yield return null;
        }
        group.alpha = 1f;

        if (typed && SaveSystem.Data.settings.textSpeed < 3)
        {
            body.ForceMeshUpdate();
            int total = body.textInfo.characterCount;
            body.maxVisibleCharacters = 0;
            float cps = new float[] { 30f, 55f, 120f, 9999f }[Mathf.Clamp(SaveSystem.Data.settings.textSpeed, 0, 3)];
            float shown = 0f;
            while (body.maxVisibleCharacters < total)
            {
                shown += Time.unscaledDeltaTime * cps;
                body.maxVisibleCharacters = Mathf.Min(total, Mathf.FloorToInt(shown));
                Keyboard kb = Keyboard.current;
                Mouse m = Mouse.current;
                if ((kb != null && kb.spaceKey.wasPressedThisFrame) || (m != null && m.leftButton.wasPressedThisFrame)) break;
                yield return null;
            }
        }
        body.maxVisibleCharacters = 99999;
        if (hideButtons)
        {
            button1.gameObject.SetActive(!string.IsNullOrEmpty(primary));
            button2.gameObject.SetActive(!string.IsNullOrEmpty(secondary));
            button3.gameObject.SetActive(!string.IsNullOrEmpty(tertiary));
        }

        while (Choice < 0) yield return null;

        if (SoundFX.I != null) SoundFX.I.Click();
        for (float e = 0f; e < 0.25f; e += Time.unscaledDeltaTime)
        {
            group.alpha = 1f - e / 0.25f;
            yield return null;
        }
        group.alpha = 0f;
        group.blocksRaycasts = false;
        gameObject.SetActive(false);
    }
}
