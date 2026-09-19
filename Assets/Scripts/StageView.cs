using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// The platform: the conductor (you), the passenger being checked, the queue waiting behind,
// and the movement of people: walking in, boarding the train or walking away.
public class StageView : MonoBehaviour
{
    // positions are relative to the centre of the screen (x) and the bottom of the screen (y)
    const float FrontY = 452f;       // front lane, where the passenger stands to be checked
    const float BackY = 498f;        // back lane, used when leaving
    const float ConductorY = 448f;
    const float CheckX = -270f;
    const float ConductorX = 60f;

    RectTransform stage, queueRoot, actorsRoot;
    TrainView train;

    CharacterView conductor;
    CharFrames conductorFrames;
    CharacterView passenger;                                   // the person at the booth
    readonly List<CharacterView> actors = new List<CharacterView>();
    readonly List<Image> queue = new List<Image>();
    readonly int[] queueLooks = new int[5];
    int exiting;

    public bool Busy { get { return exiting > 0; } }

    float HalfWidth
    {
        get { return ((RectTransform)stage.parent).rect.width * 0.5f; }
    }

    public static StageView Create(Canvas canvas, TrainView train)
    {
        RectTransform rt = UIKit.NewRect("Stage", canvas.transform);
        StageView sv = rt.gameObject.AddComponent<StageView>();
        sv.Build(rt, train);
        return sv;
    }

    void Build(RectTransform rt, TrainView t)
    {
        stage = rt;
        train = t;
        UIKit.Stretch(stage);

        // platform floor in front of the train
        Image floor = UIKit.Img("Platform", stage, PixelArt.Platform(), Color.white);
        UIKit.Anchor(floor.rectTransform, new Vector2(0, 0), new Vector2(1, 0), new Vector2(0.5f, 0), new Vector2(0, 450), new Vector2(0, 74));

        queueRoot = UIKit.NewRect("Queue", stage);
        UIKit.Stretch(queueRoot);
        actorsRoot = UIKit.NewRect("Actors", stage);
        UIKit.Stretch(actorsRoot);

        for (int i = 0; i < queueLooks.Length; i++) queueLooks[i] = Random.Range(0, CharacterArt.LookCount);

        // the conductor: you
        conductorFrames = CharacterArt.Conductor();
        conductor = CharacterView.Create(actorsRoot, conductorFrames, "Conductor", true, true, Color.white);
        conductor.Face(-1);
        conductor.SetScale(1f);
        conductor.SetFeet(ConductorX, ConductorY);
        actors.Add(conductor);
    }

    // ---------- queue of waiting passengers (dark silhouettes) ----------

    public void SetQueue(int count)
    {
        count = Mathf.Clamp(count, 0, queueLooks.Length);
        for (int i = 0; i < queue.Count; i++)
            if (queue[i] != null) Destroy(queue[i].gameObject);
        queue.Clear();

        for (int i = count - 1; i >= 0; i--)
        {
            CharFrames f = CharacterArt.Passenger(queueLooks[i]);
            Image im = UIKit.Img("Waiting" + i, queueRoot, f.idle, new Color(0.10f, 0.10f, 0.18f, 0.88f));
            UIKit.Anchor(im.rectTransform, UIKit.BC, UIKit.BC, new Vector2(0.5f, 2f / CharacterArt.H),
                         new Vector2(-620f + i * 62f, BackY + 8f), new Vector2(CharacterArt.W * CharacterView.Px, CharacterArt.H * CharacterView.Px));
            im.rectTransform.localScale = new Vector3(0.7f, 0.7f, 1f);
            queue.Add(im);
        }
    }

    // ---------- passenger walks up to the booth ----------

    public CharacterView Current { get { return passenger; } }
    public CharacterView Conductor { get { return conductor; } }
    public Vector2 LanternPos { get { return new Vector2(ConductorX - 30f, ConductorY + 110f); } }

    public IEnumerator Enter(PassengerData p)
    {
        CharFrames f = p.isPlayer ? CharacterArt.Conductor() : CharacterArt.Passenger(p.look);

        Color tint = Color.white;
        if (p.isPlayer) tint = new Color(0.72f, 0.85f, 1f, 1f);           // your ghostly reflection
        else if (PassengerTraits.IsColdTint(p)) tint = new Color(0.82f, 0.9f, 1f, 1f);

        passenger = CharacterView.Create(actorsRoot, f, "Passenger", PassengerTraits.HasShadow(p), PassengerTraits.HasBreath(p), tint);
        passenger.Face(1);
        float sc = p.scale <= 0f ? 1f : p.scale;
        passenger.SetScale(sc);
        passenger.SetMood(p.behaviour);
        passenger.SetGlitchy(PassengerTraits.Glitches(p));
        passenger.SetBaseAlpha(PassengerTraits.Alpha(p));
        passenger.SetFeet(-HalfWidth - 140f, FrontY);
        actors.Add(passenger);

        yield return passenger.WalkTo(CheckX, FrontY, sc, 430f);
    }

    public void SetTalking(bool t) { if (passenger != null) passenger.SetTalking(t); }

    // ---------- events on the platform ----------

    // Somebody in the queue disappears.
    public void VanishFromQueue()
    {
        for (int i = queue.Count - 1; i >= 0; i--)
        {
            if (queue[i] != null && queue[i].gameObject.activeSelf)
            {
                StartCoroutine(FadeImage(queue[i], 1.4f));
                return;
            }
        }
    }

    IEnumerator FadeImage(Image im, float dur)
    {
        Color c0 = im.color;
        for (float e = 0f; e < dur; e += Time.deltaTime)
        {
            if (im == null) yield break;
            im.color = new Color(c0.r, c0.g, c0.b, c0.a * (1f - e / dur));
            yield return null;
        }
        if (im != null) im.gameObject.SetActive(false);
    }

    // An extra dark figure appears at the back of the platform (does not count as a passenger).
    public void ShowExtraFigure()
    {
        CharFrames f = CharacterArt.Passenger(12);
        Image im = UIKit.Img("Extra", queueRoot, f.idle, new Color(0.05f, 0.05f, 0.09f, 0.95f));
        UIKit.Anchor(im.rectTransform, UIKit.BC, UIKit.BC, new Vector2(0.5f, 2f / CharacterArt.H),
                     new Vector2(-780f, BackY + 4f), new Vector2(CharacterArt.W * CharacterView.Px, CharacterArt.H * CharacterView.Px));
        im.rectTransform.localScale = new Vector3(0.75f, 0.75f, 1f);
        queue.Add(im);
    }

    // ---------- conductor's reaction to your decision ----------

    public void PlayDecision(bool admit)
    {
        StartCoroutine(DecisionRoutine(admit));
    }

    IEnumerator DecisionRoutine(bool admit)
    {
        conductor.SetPose(conductorFrames.stamp);
        yield return new WaitForSeconds(0.3f);

        if (admit)
        {
            conductor.Face(1);                       // turns to the train
            conductor.SetPose(conductorFrames.point);
        }
        else
        {
            conductor.SetPose(conductorFrames.palm); // "no entry"
        }
        yield return new WaitForSeconds(1.3f);

        conductor.SetPose(null);
        conductor.Face(-1);
    }

    // ---------- the passenger leaves ----------

    public void ExitCurrent(PassengerData p, bool admitted)
    {
        if (passenger == null) return;
        CharacterView cv = passenger;
        passenger = null;
        StartCoroutine(ExitRoutine(cv, p, admitted));
    }

    IEnumerator ExitRoutine(CharacterView cv, PassengerData p, bool admitted)
    {
        exiting++;
        float sc = p.scale <= 0f ? 1f : p.scale;

        if (admitted)
        {
            // walks behind the conductor to the open door and steps into the train
            cv.Face(1);
            yield return cv.WalkTo(-40f, BackY, 0.9f * sc, 380f);
            train.SetDoorOpen(true);
            yield return cv.WalkTo(train.DoorX - 6f, train.DoorY, 0.3f * sc, 300f);
            yield return cv.FadeTo(0f, 0.2f);
            yield return new WaitForSeconds(0.25f);
            train.SetDoorOpen(false);

            if (p.isPlayer)
            {
                // you let yourself go: the conductor fades away
                yield return conductor.FadeTo(0f, 1.6f);
            }
        }
        else
        {
            // turns around and walks away along the back lane
            cv.Face(-1);
            float leftEdge = -HalfWidth - 200f;

            if (p.sympathetic && !p.isPlayer)
            {
                // stops, looks back once, then goes on
                yield return cv.WalkTo(-430f, BackY, 0.95f * sc, 230f);
                cv.Face(1);
                yield return new WaitForSeconds(0.8f);
                cv.Face(-1);
            }

            if ((p.truth == PassengerTruth.Dead || p.truth == PassengerTruth.Echo) && !p.isPlayer)
            {
                StartCoroutine(cv.FadeTo(0f, 2.2f));                       // fades into the mist
                yield return cv.WalkTo(leftEdge, BackY, 0.9f * sc, 240f);
            }
            else if ((p.truth == PassengerTruth.NonExistent || p.truth == PassengerTruth.Unknown || p.truth == PassengerTruth.Loop) && !p.isPlayer)
            {
                yield return cv.WalkTo(-620f, BackY, 0.92f * sc, 260f);
                yield return cv.Glitch(0.5f);                              // glitches out of existence
            }
            else if (p.isPlayer)
            {
                StartCoroutine(cv.FadeTo(0f, 2.4f));
                yield return cv.WalkTo(leftEdge, BackY, 0.9f * sc, 200f);
            }
            else
            {
                yield return cv.WalkTo(leftEdge, BackY, 0.9f * sc, 250f);
            }
        }

        actors.Remove(cv);
        if (cv != null) Destroy(cv.gameObject);
        exiting--;
    }

    // Removes the passenger at the booth (e.g. when a night is retried).
    public void ClearPassengers()
    {
        StopAllCoroutines();
        exiting = 0;
        for (int i = actors.Count - 1; i >= 0; i--)
        {
            if (actors[i] != conductor)
            {
                if (actors[i] != null) Destroy(actors[i].gameObject);
                actors.RemoveAt(i);
            }
        }
        passenger = null;
        conductor.SetPose(null);
        conductor.Face(-1);
        train.SetDoorOpen(false);
        SetQueue(0);
    }

    // Draw people in depth order: the farther (higher on screen) the earlier.
    void LateUpdate()
    {
        actors.Sort((a, b) =>
        {
            if (a == null || b == null) return 0;
            return b.FeetY.CompareTo(a.FeetY);
        });
        for (int i = 0; i < actors.Count; i++)
            if (actors[i] != null) actors[i].transform.SetSiblingIndex(i);
    }
}
