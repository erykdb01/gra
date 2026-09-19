using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// "SHIFT COMPLETE": the numbers of the night with animated counters, new discoveries and the night's epilogue.
public class ShiftResultsUI : MonoBehaviour
{
    public int Choice = -1;

    CanvasGroup group;
    TextMeshProUGUI title, outro, discovery;
    Button button1, button2;
    TextMeshProUGUI label1, label2;
    const int RowCount = 12;
    readonly TextMeshProUGUI[] labels = new TextMeshProUGUI[RowCount];
    readonly TextMeshProUGUI[] values = new TextMeshProUGUI[RowCount];

    public static ShiftResultsUI Create(Canvas canvas)
    {
        RectTransform rt = UIKit.NewRect("ShiftResults", canvas.transform);
        ShiftResultsUI s = rt.gameObject.AddComponent<ShiftResultsUI>();
        s.Build(rt);
        return s;
    }

    void Build(RectTransform rt)
    {
        UIKit.Stretch(rt);
        Image bg = rt.gameObject.AddComponent<Image>();
        bg.color = new Color(0.02f, 0.02f, 0.05f, 0.96f);
        bg.raycastTarget = true;
        group = rt.gameObject.AddComponent<CanvasGroup>();

        title = UIKit.Text("Title", rt, "", 68, UIKit.Amber, TextAlignmentOptions.Center);
        UIKit.Anchor(title.rectTransform, UIKit.TC, UIKit.TC, UIKit.TC, new Vector2(0, -40), new Vector2(1600, 100));
        title.enableAutoSizing = true; title.fontSizeMin = 30; title.fontSizeMax = 68;

        for (int i = 0; i < RowCount; i++)
        {
            int col = i / 6, row = i % 6;
            float x = -900f + col * 940f;
            float y = -170f - row * 58f;
            labels[i] = UIKit.Text("L" + i, rt, "", 32, UIKit.Light, TextAlignmentOptions.MidlineLeft);
            UIKit.Anchor(labels[i].rectTransform, UIKit.TC, UIKit.TC, new Vector2(0, 1), new Vector2(x, y), new Vector2(520, 50));
            values[i] = UIKit.Text("V" + i, rt, "", 34, UIKit.Amber, TextAlignmentOptions.MidlineRight);
            UIKit.Anchor(values[i].rectTransform, UIKit.TC, UIKit.TC, new Vector2(0, 1), new Vector2(x + 520f, y), new Vector2(300, 50));
        }

        discovery = UIKit.Text("Discovery", rt, "", 30, new Color(0.55f, 0.75f, 0.95f, 1f), TextAlignmentOptions.Center);
        UIKit.Anchor(discovery.rectTransform, UIKit.TC, UIKit.TC, UIKit.TC, new Vector2(0, -540), new Vector2(1500, 44));
        discovery.enableAutoSizing = true; discovery.fontSizeMin = 18; discovery.fontSizeMax = 30;

        outro = UIKit.Text("Outro", rt, "", 30, new Color(0.8f, 0.8f, 0.86f, 1f), TextAlignmentOptions.Top);
        UIKit.Anchor(outro.rectTransform, UIKit.TC, UIKit.TC, UIKit.TC, new Vector2(0, -600), new Vector2(1400, 220));
        outro.enableAutoSizing = true; outro.fontSizeMin = 18; outro.fontSizeMax = 30;
        outro.fontStyle = FontStyles.Italic;

        button1 = UIKit.MakeButton("Primary", rt, "", new Vector2(600, 80), new Color(0.6f, 0.85f, 0.6f), 32, out label1);
        UIKit.Anchor((RectTransform)button1.transform, UIKit.BC, UIKit.BC, UIKit.BC, new Vector2(0, 150), new Vector2(600, 80));
        button1.onClick.AddListener(() => Choice = 0);
        button2 = UIKit.MakeButton("Secondary", rt, "", new Vector2(600, 64), new Color(0.85f, 0.85f, 0.9f), 26, out label2);
        UIKit.Anchor((RectTransform)button2.transform, UIKit.BC, UIKit.BC, UIKit.BC, new Vector2(0, 56), new Vector2(600, 64));
        button2.onClick.AddListener(() => Choice = 1);

        gameObject.SetActive(false);
    }

    struct Row
    {
        public string label, fmt;
        public int target;
        public bool signed;
        public Color color;
    }

    static Color Good = new Color(0.55f, 0.85f, 0.55f), Bad = new Color(0.9f, 0.45f, 0.4f), Neutral = new Color(0.94f, 0.78f, 0.42f);

    public IEnumerator Show(ShiftReport r, string heading, string epilogue, string primary, string secondary, int perfectMax)
    {
        var rows = new List<Row>();
        rows.Add(new Row { label = Loc.T("Pasażerowie", "Passengers"), fmt = "{0} / " + perfectMax, target = r.passengers, color = Neutral });
        rows.Add(new Row { label = Loc.T("Poprawne decyzje", "Correct decisions"), fmt = "{0}", target = r.correct, color = Good });
        rows.Add(new Row { label = Loc.T("Błędy", "Mistakes"), fmt = "{0}", target = r.mistakes, color = r.mistakes == 0 ? Good : Bad });
        rows.Add(new Row { label = Loc.T("Odmówiono", "Denied"), fmt = "{0}", target = r.denied, color = Neutral });
        rows.Add(new Row { label = Loc.T("Wpuszczono", "Admitted"), fmt = "{0}", target = r.admitted, color = Neutral });
        rows.Add(new Row { label = Loc.T("Najdłuższa seria", "Best streak"), fmt = "{0}", target = r.bestStreak, color = Neutral });
        rows.Add(new Row { label = Loc.T("Dyscyplina", "Discipline"), fmt = "{0}", target = r.discipline, signed = true, color = r.discipline >= 0 ? Good : Bad });
        rows.Add(new Row { label = Loc.T("Sumienie", "Conscience"), fmt = "{0}", target = r.conscience, signed = true, color = r.conscience >= 0 ? Good : Bad });
        rows.Add(new Row { label = Loc.T("Stres", "Stress"), fmt = "{0}%", target = r.stress, color = r.stress > 70 ? Bad : Neutral });
        rows.Add(new Row { label = "XP", fmt = "{0}", target = r.xp, signed = true, color = Good });
        rows.Add(new Row { label = Loc.T("Wynik zmiany", "Shift score"), fmt = "{0}", target = r.score, color = Neutral });
        rows.Add(new Row { label = Loc.T("Złamane zasady", "Rules broken"), fmt = "{0}", target = r.rulesBroken, color = r.rulesBroken == 0 ? Good : Bad });

        title.text = heading;
        outro.text = epilogue;
        outro.maxVisibleCharacters = 0;
        discovery.text = "";
        for (int i = 0; i < RowCount; i++) { labels[i].text = ""; values[i].text = ""; }
        button1.gameObject.SetActive(false);
        button2.gameObject.SetActive(false);
        label1.text = primary;
        if (!string.IsNullOrEmpty(secondary)) label2.text = secondary;

        Choice = -1;
        gameObject.SetActive(true);
        transform.SetAsLastSibling();
        group.blocksRaycasts = true;
        yield return UIAnim.Fade(group, 0f, 1f, 0.4f);

        // rows count up one after another
        for (int i = 0; i < rows.Count && i < RowCount; i++)
        {
            Row row = rows[i];
            labels[i].text = row.label;
            values[i].color = row.color;
            float dur = Mathf.Clamp(0.25f + Mathf.Abs(row.target) * 0.01f, 0.25f, 0.9f);
            for (float e = 0f; e < dur; e += Time.unscaledDeltaTime)
            {
                int v = Mathf.RoundToInt(Mathf.Lerp(0, row.target, e / dur));
                values[i].text = Format(row, v);
                if (SoundFX.I != null && Time.frameCount % 3 == 0) SoundFX.I.Tick();
                yield return null;
            }
            values[i].text = Format(row, row.target);
            if (SoundFX.I != null) SoundFX.I.Click();
            yield return new WaitForSecondsRealtime(0.08f);
        }

        // discovery
        if (!string.IsNullOrEmpty(r.discovery))
        {
            discovery.text = Loc.T("Nowe odkrycie: ", "New discovery: ") + r.discovery;
            if (SoundFX.I != null) SoundFX.I.Stinger(0.25f);
            yield return new WaitForSecondsRealtime(0.5f);
        }

        // the epilogue is typed
        if (!string.IsNullOrEmpty(epilogue))
        {
            outro.ForceMeshUpdate();
            int total = outro.textInfo.characterCount;
            int speed = SaveSystem.Data.settings.textSpeed;
            float cps = speed == 0 ? 30f : (speed == 1 ? 60f : (speed == 2 ? 130f : 9999f));
            float shown = 0f;
            while (outro.maxVisibleCharacters < total)
            {
                shown += Time.unscaledDeltaTime * cps;
                outro.maxVisibleCharacters = Mathf.Min(total, Mathf.FloorToInt(shown));
                yield return null;
            }
        }
        outro.maxVisibleCharacters = 99999;

        button1.gameObject.SetActive(true);
        button2.gameObject.SetActive(!string.IsNullOrEmpty(secondary));

        while (Choice < 0) yield return null;

        if (SoundFX.I != null) SoundFX.I.Click();
        yield return UIAnim.Fade(group, 1f, 0f, 0.25f);
        group.blocksRaycasts = false;
        gameObject.SetActive(false);
    }

    static string Format(Row row, int v)
    {
        string s = string.Format(row.fmt, Mathf.Abs(v));
        if (row.signed) return (v >= 0 ? "+" : "-") + s;
        return v < 0 ? "-" + s : s;
    }
}
