using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// The big rubber stamp that slams onto the desk after each decision.
public class StampEffect : MonoBehaviour
{
    RectTransform root;
    Image frame;
    TextMeshProUGUI label;
    CanvasGroup group;

    public static StampEffect Create(Canvas canvas)
    {
        RectTransform rt = UIKit.NewRect("Stamp", canvas.transform);
        StampEffect s = rt.gameObject.AddComponent<StampEffect>();
        s.Build(rt);
        return s;
    }

    void Build(RectTransform rt)
    {
        root = rt;
        UIKit.Anchor(rt, UIKit.MC, UIKit.MC, UIKit.MC, new Vector2(0, -260), new Vector2(600, 200));
        group = rt.gameObject.AddComponent<CanvasGroup>();
        group.alpha = 0f;
        group.blocksRaycasts = false;
        group.interactable = false;

        frame = UIKit.Img("Frame", rt, PixelArt.Frame(60, 20, 2), Color.white);
        UIKit.Stretch(frame.rectTransform);

        label = UIKit.Text("Label", rt, "", 72, Color.white, TextAlignmentOptions.Center);
        UIKit.Stretch(label.rectTransform);
        label.fontStyle = FontStyles.Bold;
    }

    public IEnumerator Play(bool admit, TrainView train)
    {
        Color c = admit ? new Color(0.25f, 0.72f, 0.38f, 1f) : new Color(0.82f, 0.24f, 0.22f, 1f);
        frame.color = c;
        label.color = c;
        label.text = admit ? Loc.T("WPUSZCZONO", "ADMITTED") : Loc.T("ODMÓWIONO", "DENIED");
        root.localRotation = Quaternion.Euler(0f, 0f, admit ? -8f : 7f);
        transform.SetAsLastSibling();

        // slam down
        for (float e = 0f; e < 0.12f; e += Time.deltaTime)
        {
            float k = e / 0.12f;
            root.localScale = Vector3.one * Mathf.Lerp(2.4f, 1f, k);
            group.alpha = k;
            yield return null;
        }
        root.localScale = Vector3.one;
        group.alpha = 1f;

        if (SoundFX.I != null) SoundFX.I.Stamp();
        if (train != null) train.Bump(0.35f);

        yield return new WaitForSeconds(0.5f);

        for (float e = 0f; e < 0.25f; e += Time.deltaTime)
        {
            group.alpha = 1f - e / 0.25f;
            yield return null;
        }
        group.alpha = 0f;
    }
}
