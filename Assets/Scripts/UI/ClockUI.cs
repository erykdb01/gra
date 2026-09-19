using TMPro;
using UnityEngine;
using UnityEngine.UI;

// The platform clock: hands, a ticking sound, and the ability to stop at 03:13.
public class ClockUI : MonoBehaviour
{
    RectTransform hourHand, minuteHand;
    TextMeshProUGUI digital;
    int minutes = 22 * 60;
    bool stopped;
    float tickTimer;

    public static ClockUI Create(Canvas canvas)
    {
        RectTransform rt = UIKit.NewRect("Clock", canvas.transform);
        ClockUI c = rt.gameObject.AddComponent<ClockUI>();
        c.Build(rt);
        return c;
    }

    void Build(RectTransform rt)
    {
        UIKit.Anchor(rt, Vector2.one, Vector2.one, Vector2.one, new Vector2(-350f, -16f), new Vector2(150f, 176f));
        Image face = UIKit.Img("Face", rt, PixelArtExtras.ClockFace(), Color.white);
        UIKit.Anchor(face.rectTransform, UIKit.TC, UIKit.TC, UIKit.TC, Vector2.zero, new Vector2(140f, 140f));

        hourHand = MakeHand(face.transform, 34f, 5f, new Color(0.1f, 0.08f, 0.07f));
        minuteHand = MakeHand(face.transform, 50f, 3f, new Color(0.1f, 0.08f, 0.07f));

        digital = UIKit.Text("Digital", rt, "22:00", 26, UIKit.Amber, TextAlignmentOptions.Center);
        UIKit.Anchor(digital.rectTransform, UIKit.BC, UIKit.BC, UIKit.BC, new Vector2(0f, 0f), new Vector2(150f, 34f));
        SetTime(minutes);
    }

    static RectTransform MakeHand(Transform parent, float len, float width, Color c)
    {
        Image h = UIKit.Img("Hand", parent, PixelArt.Solid(), c);
        RectTransform r = h.rectTransform;
        UIKit.Anchor(r, UIKit.MC, UIKit.MC, new Vector2(0.5f, 0f), Vector2.zero, new Vector2(width, len));
        return r;
    }

    public void SetTime(int totalMinutes)
    {
        minutes = totalMinutes;
        if (stopped) return;
        Apply(totalMinutes);
    }

    void Apply(int totalMinutes)
    {
        int h = (totalMinutes / 60) % 24;
        int m = totalMinutes % 60;
        hourHand.localRotation = Quaternion.Euler(0f, 0f, -((h % 12) * 30f + m * 0.5f));
        minuteHand.localRotation = Quaternion.Euler(0f, 0f, -(m * 6f));
        digital.text = h.ToString("00") + ":" + m.ToString("00");
    }

    // The clock stops at 03:13 (an event) or runs again.
    public void Stop(bool on)
    {
        stopped = on;
        if (on) Apply(3 * 60 + 13);
        else Apply(minutes);
    }

    void Update()
    {
        if (stopped || Time.timeScale <= 0f) return;
        tickTimer += Time.deltaTime;
        if (tickTimer >= 1f)
        {
            tickTimer = 0f;
            if (SoundFX.I != null) SoundFX.I.Tick();
        }
    }
}
