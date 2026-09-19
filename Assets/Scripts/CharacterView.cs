using System.Collections;
using UnityEngine;
using UnityEngine.UI;

// One animated character on the platform: idle breathing, walking, facing left/right,
// fading out, and (for the living) a puff of breath in the cold night air.
public class CharacterView : MonoBehaviour
{
    public const float Px = 5f;   // screen pixels per sprite pixel

    RectTransform rt, bodyRt;
    Image body;
    CanvasGroup group;
    CharFrames frames;
    Sprite pose;
    bool walking, breathes, talking, glitchy;
    string mood = "calm";
    float talkClock, nextGlitch, glitchUntil;
    Image shadowImg;
    float baseAlpha = 1f;
    float walkClock, idleClock, nextBreath, scaleFactor = 1f;
    int dir = 1;

    readonly Image[] puffs = new Image[3];
    readonly float[] puffAge = { -1f, -1f, -1f };

    public float FeetY { get { return rt.anchoredPosition.y; } }
    public float FeetX { get { return rt.anchoredPosition.x; } }

    public static CharacterView Create(Transform parent, CharFrames frames, string name, bool shadow, bool breathes, Color tint)
    {
        RectTransform rt = UIKit.NewRect(name, parent);
        CharacterView cv = rt.gameObject.AddComponent<CharacterView>();
        cv.Build(rt, frames, shadow, breathes, tint);
        return cv;
    }

    void Build(RectTransform root, CharFrames f, bool shadow, bool doesBreathe, Color tint)
    {
        rt = root;
        frames = f;
        breathes = doesBreathe;
        nextBreath = Random.Range(0.5f, 2f);

        // pivot sits at the feet (2 pixels above the bottom of the sprite)
        UIKit.Anchor(rt, UIKit.BC, UIKit.BC, new Vector2(0.5f, 2f / CharacterArt.H), Vector2.zero,
                     new Vector2(CharacterArt.W * Px, CharacterArt.H * Px));
        group = rt.gameObject.AddComponent<CanvasGroup>();
        group.blocksRaycasts = false;
        group.interactable = false;

        if (shadow)
        {
            Image sh = UIKit.Img("Shadow", rt, PixelArt.Shadow(), Color.white);
            shadowImg = sh;
            UIKit.Anchor(sh.rectTransform, new Vector2(0.5f, 2f / CharacterArt.H), new Vector2(0.5f, 2f / CharacterArt.H),
                         UIKit.MC, Vector2.zero, new Vector2(84, 22));
        }

        body = UIKit.Img("Body", rt, f.idle, tint);
        bodyRt = body.rectTransform;
        UIKit.Stretch(bodyRt);

        for (int i = 0; i < puffs.Length; i++)
        {
            puffs[i] = UIKit.Img("Breath", rt, PixelArt.Solid(), new Color(1f, 1f, 1f, 0f));
            UIKit.Anchor(puffs[i].rectTransform, Vector2.zero, Vector2.zero, UIKit.MC, Vector2.zero, new Vector2(10, 10));
        }
    }

    // calm, nervous, angry, scared, cold
    public void SetMood(string m) { mood = string.IsNullOrEmpty(m) ? "calm" : m; }
    public void SetTalking(bool t) { talking = t; }
    public void SetBreathes(bool b) { breathes = b; }
    public void SetGlitchy(bool g) { glitchy = g; nextGlitch = Time.time + Random.Range(1f, 3f); }
    public void SetBaseAlpha(float a) { baseAlpha = a; group.alpha = a; }
    public bool Breathes { get { return breathes; } }

    // A short flicker (used by horror events).
    public IEnumerator Flicker(float duration)
    {
        for (float e = 0f; e < duration; e += 0.05f)
        {
            group.alpha = Random.value < 0.5f ? 0.2f : baseAlpha;
            yield return new WaitForSeconds(0.05f);
        }
        group.alpha = baseAlpha;
    }

    public void SetFeet(float x, float y) { rt.anchoredPosition = new Vector2(x, y); }

    public void SetScale(float s)
    {
        scaleFactor = s;
        rt.localScale = new Vector3(dir * s, s, 1f);
    }

    // 1 = faces right, -1 = faces left.
    public void Face(int d)
    {
        dir = d >= 0 ? 1 : -1;
        rt.localScale = new Vector3(dir * scaleFactor, scaleFactor, 1f);
    }

    // Show a special pose (gesture); null returns to the idle sprite.
    public void SetPose(Sprite s) { pose = s; }

    public IEnumerator WalkTo(float x, float y, float scale, float speed)
    {
        Vector2 from = rt.anchoredPosition;
        Vector2 to = new Vector2(x, y);
        float s0 = scaleFactor;
        float dur = Mathf.Max(0.01f, Vector2.Distance(from, to) / speed);

        walking = true;
        for (float e = 0f; e < dur; e += Time.deltaTime)
        {
            float k = e / dur;
            rt.anchoredPosition = Vector2.Lerp(from, to, k);
            SetScale(Mathf.Lerp(s0, scale, k));
            yield return null;
        }
        rt.anchoredPosition = to;
        SetScale(scale);
        walking = false;
    }

    public IEnumerator FadeTo(float alpha, float duration)
    {
        float a0 = group.alpha;
        for (float e = 0f; e < duration; e += Time.deltaTime)
        {
            group.alpha = Mathf.Lerp(a0, alpha, e / duration);
            yield return null;
        }
        group.alpha = alpha;
    }

    // Glitch effect for people who never existed: flicker, then vanish.
    public IEnumerator Glitch(float duration)
    {
        for (float e = 0f; e < duration; e += 0.06f)
        {
            group.alpha = Random.value < 0.5f ? 0.15f : 1f;
            rt.anchoredPosition += new Vector2(Random.Range(-6f, 6f), 0f);
            yield return new WaitForSeconds(0.06f);
        }
        group.alpha = 0f;
    }

    void Update()
    {
        if (body == null) return;

        if (walking)
        {
            walkClock += Time.deltaTime * 7f;
            body.sprite = ((int)walkClock % 2 == 0) ? frames.walkA : frames.walkB;
            bodyRt.anchoredPosition = new Vector2(0f, Mathf.Abs(Mathf.Sin(walkClock * Mathf.PI)) * 4f);
        }
        else
        {
            idleClock += Time.deltaTime;
            Sprite idleSprite = frames.idle;
            if (talking && frames.talk != null)
            {
                talkClock += Time.deltaTime;
                if (((int)(talkClock * 7f)) % 2 == 1) idleSprite = frames.talk;
            }
            body.sprite = pose != null ? pose : idleSprite;

            float speed = 2f, amp = 1.2f, jx = 0f;
            switch (mood)
            {
                case "nervous": speed = 3.4f; jx = Mathf.Round(Mathf.Sin(idleClock * 23f)); break;
                case "angry":   speed = 4.2f; amp = 2f; break;
                case "scared":  speed = 5f; jx = Mathf.Round(Mathf.Sin(idleClock * 31f) * 2f); break;
                case "cold":    speed = 0.9f; amp = 0.4f; break;
            }
            bodyRt.anchoredPosition = new Vector2(jx, Mathf.Round(Mathf.Sin(idleClock * speed) * amp));
        }

        // people the system does not fully know glitch now and then
        if (glitchy && Time.time > nextGlitch)
        {
            nextGlitch = Time.time + Random.Range(2.5f, 6f);
            StartCoroutine(Flicker(0.18f));
            bodyRt.anchoredPosition += new Vector2(Random.Range(-8f, 8f), 0f);
        }
        if (shadowImg != null && glitchy)
            shadowImg.rectTransform.anchoredPosition = new Vector2(Mathf.Sin(Time.time * 1.7f) * 18f, 0f);

        if (breathes) UpdateBreath();
    }

    // Visible breath: only living people have it.
    void UpdateBreath()
    {
        nextBreath -= Time.deltaTime;
        if (nextBreath <= 0f)
        {
            nextBreath = Random.Range(2.0f, 2.8f);
            for (int i = 0; i < puffs.Length; i++)
            {
                if (puffAge[i] < 0f) { puffAge[i] = 0f; break; }
            }
        }

        for (int i = 0; i < puffs.Length; i++)
        {
            if (puffAge[i] < 0f) continue;
            puffAge[i] += Time.deltaTime;
            float a = puffAge[i];
            if (a > 1.4f)
            {
                puffAge[i] = -1f;
                puffs[i].color = new Color(1f, 1f, 1f, 0f);
                continue;
            }
            float size = 10f + a * 16f;
            puffs[i].rectTransform.sizeDelta = new Vector2(size, size);
            // mouth is on the facing side, at about 28 sprite pixels up
            puffs[i].rectTransform.anchoredPosition = new Vector2(66f + a * 34f, 29f * Px + a * 22f);
            puffs[i].color = new Color(0.9f, 0.95f, 1f, 0.5f * (1f - a / 1.4f));
        }
    }
}
