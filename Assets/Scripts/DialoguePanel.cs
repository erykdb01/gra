using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

// Conversation box with the passenger: a speech area (typewriter) and up to six questions to ask.
// The number of visible questions grows with the INTERROGATION skill (see DialogueManager.SlotCount).
public class DialoguePanel : MonoBehaviour
{
    public const int MaxSlots = 6;

    public Action<QA> OnAsk;
    public Action<bool> OnTalking;      // true while the passenger "speaks"

    TextMeshProUGUI speech, header;
    readonly Button[] options = new Button[MaxSlots];
    readonly TextMeshProUGUI[] labels = new TextMeshProUGUI[MaxSlots];
    readonly Image[] optionImages = new Image[MaxSlots];
    readonly QA[] shown = new QA[MaxSlots];
    bool interactable;
    string speaker = "";
    Coroutine typing;
    string fullText = "";

    public static DialoguePanel Create(Canvas canvas)
    {
        RectTransform rt = UIKit.NewRect("DialoguePanel", canvas.transform);
        DialoguePanel d = rt.gameObject.AddComponent<DialoguePanel>();
        d.Build(rt);
        return d;
    }

    void Build(RectTransform rt)
    {
        UIKit.Anchor(rt, UIKit.TC, UIKit.TC, UIKit.TC, new Vector2(0, -190), new Vector2(1100, 286));
        Image bg = rt.gameObject.AddComponent<Image>();
        bg.sprite = PixelArt.Panel();
        bg.color = Color.white;
        bg.raycastTarget = true;   // clicking the panel skips the typewriter

        header = UIKit.Text("Header", rt, "", 22, UIKit.Amber, TextAlignmentOptions.TopLeft);
        UIKit.Anchor(header.rectTransform, UIKit.TL, UIKit.TL, UIKit.TL, new Vector2(20, -8), new Vector2(900, 28));

        speech = UIKit.Text("Speech", rt, "", 27, UIKit.Light, TextAlignmentOptions.TopLeft);
        UIKit.Anchor(speech.rectTransform, UIKit.TL, UIKit.TL, UIKit.TL, new Vector2(20, -36), new Vector2(1060, 100));
        speech.enableAutoSizing = true;
        speech.fontSizeMin = 18;
        speech.fontSizeMax = 27;

        for (int i = 0; i < MaxSlots; i++)
        {
            int idx = i;
            TextMeshProUGUI lab;
            Button b = UIKit.MakeButton("Question" + i, rt, "", new Vector2(520, 40), new Color(0.85f, 0.85f, 0.9f), 21, out lab);
            float x = 20 + (i % 2) * 540;
            float y = -140 - (i / 2) * 46;
            UIKit.Anchor((RectTransform)b.transform, UIKit.TL, UIKit.TL, UIKit.TL, new Vector2(x, y), new Vector2(520, 40));
            lab.enableAutoSizing = true;
            lab.fontSizeMin = 14;
            lab.fontSizeMax = 21;
            lab.textWrappingMode = TextWrappingModes.Normal;
            lab.margin = new Vector4(6, 0, 6, 0);
            b.onClick.AddListener(() => Clicked(idx));
            options[i] = b;
            labels[i] = lab;
            optionImages[i] = b.GetComponent<Image>();
            b.gameObject.SetActive(false);
        }

        RefreshHeader();
        Loc.Changed += RefreshHeader;
    }

    void OnDestroy()
    {
        Loc.Changed -= RefreshHeader;
    }

    void RefreshHeader()
    {
        header.text = Loc.T("ROZMOWA", "TALK") + (string.IsNullOrEmpty(speaker) ? "" : "  -  " + speaker);
    }

    public void SetSpeaker(string name)
    {
        speaker = name ?? "";
        RefreshHeader();
    }

    void Clicked(int i)
    {
        if (!interactable || shown[i] == null) return;
        if (OnAsk != null) OnAsk(shown[i]);
    }

    // Starts a conversation: the greeting and the first set of questions.
    public void Open(string greeting, List<QA> offered)
    {
        SetOptions(offered);
        Say(greeting);
    }

    // The answer to a question: shows the question in blue, then the reply, then the new set of questions.
    public void ShowAnswer(QA q, string answer, List<QA> offered)
    {
        SetOptions(offered);
        Say("<color=#8FA6C8>" + Loc.T("Ty: ", "You: ") + q.question + "</color>\n" + answer);
    }

    // Shows a line without changing the questions (events, reactions).
    public void Interject(string text)
    {
        Say(text);
    }

    public void Clear()
    {
        StopTyping();
        fullText = "";
        speech.text = "";
        speech.maxVisibleCharacters = 99999;
        SetOptions(null);
    }

    public void SetInteractable(bool v)
    {
        interactable = v;
        RefreshButtons();
    }

    public void SetOptions(List<QA> offered)
    {
        for (int i = 0; i < MaxSlots; i++)
        {
            QA q = (offered != null && i < offered.Count) ? offered[i] : null;
            shown[i] = q;
            options[i].gameObject.SetActive(q != null);
            if (q == null) continue;
            string prefix = q.minAuthority > 0 ? "<color=#8A5A10>[!]</color> " : "";
            labels[i].text = prefix + q.question;
            optionImages[i].color = q.contextual ? new Color(0.95f, 0.88f, 0.7f) : new Color(0.85f, 0.85f, 0.9f);
        }
        RefreshButtons();
    }

    void RefreshButtons()
    {
        for (int i = 0; i < MaxSlots; i++)
            options[i].interactable = interactable && shown[i] != null;
    }

    // ---------- typewriter ----------

    void StopTyping()
    {
        if (typing != null)
        {
            StopCoroutine(typing);
            typing = null;
            if (OnTalking != null) OnTalking(false);
        }
    }

    void Say(string text)
    {
        StopTyping();
        fullText = text ?? "";
        speech.text = fullText;
        int speed = SaveSystem.Data.settings.textSpeed;
        if (speed >= 3 || string.IsNullOrEmpty(fullText) || !isActiveAndEnabled)
        {
            speech.maxVisibleCharacters = 99999;
            return;
        }
        typing = StartCoroutine(Type(speed));
    }

    IEnumerator Type(int speed)
    {
        float cps = speed == 0 ? 28f : (speed == 1 ? 60f : 130f);
        speech.ForceMeshUpdate();
        int total = speech.textInfo.characterCount;
        speech.maxVisibleCharacters = 0;
        if (OnTalking != null) OnTalking(true);
        float shownChars = 0f;
        int last = 0;
        while (last < total)
        {
            shownChars += Time.unscaledDeltaTime * cps;
            int n = Mathf.Min(total, Mathf.FloorToInt(shownChars));
            if (n != last)
            {
                last = n;
                speech.maxVisibleCharacters = n;
                if (n % 4 == 0 && SoundFX.I != null) SoundFX.I.Tick();
            }
            yield return null;
        }
        speech.maxVisibleCharacters = 99999;
        typing = null;
        if (OnTalking != null) OnTalking(false);
    }

    void Update()
    {
        // click or SPACE skips the typewriter
        if (typing == null) return;
        Keyboard kb = Keyboard.current;
        Mouse mouse = Mouse.current;
        bool skip = kb != null && kb.spaceKey.wasPressedThisFrame;
        if (!skip && mouse != null && mouse.leftButton.wasPressedThisFrame)
            skip = RectTransformUtility.RectangleContainsScreenPoint((RectTransform)transform, mouse.position.ReadValue());
        if (skip)
        {
            StopTyping();
            speech.maxVisibleCharacters = 99999;
        }
    }
}
