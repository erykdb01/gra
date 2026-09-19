using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Makes a document panel draggable with the mouse so the player can lay documents side by side.
// While dragged the panel leaves the layout group (a placeholder keeps its slot).
public class DraggableDocument : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
{
    RectTransform rt;
    RectTransform home;
    int homeIndex;
    RectTransform canvasRect;
    GameObject placeholder;
    bool free;
    public System.Action OnClicked;     // click (without dragging): open the inspector
    Vector2 grabOffset;

    public void Init(RectTransform homeContainer)
    {
        rt = (RectTransform)transform;
        home = homeContainer;
        homeIndex = rt.GetSiblingIndex();
        canvasRect = (RectTransform)GetComponentInParent<Canvas>().rootCanvas.transform;

        LayoutElement le = gameObject.GetComponent<LayoutElement>();
        if (le == null) le = gameObject.AddComponent<LayoutElement>();
        le.flexibleWidth = 1f;
        le.flexibleHeight = 1f;
        le.preferredWidth = 0f;
        le.preferredHeight = 0f;

        Tilt();
    }

    void Tilt()
    {
        rt.localRotation = Quaternion.Euler(0f, 0f, Random.Range(-1.2f, 1.2f));
    }

    Vector2 PointerToCanvas(PointerEventData e)
    {
        Vector2 local;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, e.position, e.pressEventCamera, out local);
        return local - canvasRect.rect.center;
    }

    public void OnBeginDrag(PointerEventData e)
    {
        if (!free)
        {
            Vector2 size = rt.rect.size;
            Vector3 worldPos = rt.position;

            placeholder = new GameObject("Placeholder", typeof(RectTransform), typeof(LayoutElement));
            placeholder.layer = 5;
            placeholder.transform.SetParent(home, false);
            placeholder.transform.SetSiblingIndex(homeIndex);
            LayoutElement pl = placeholder.GetComponent<LayoutElement>();
            pl.flexibleWidth = 1f;
            pl.flexibleHeight = 1f;
            pl.preferredWidth = 0f;
            pl.preferredHeight = 0f;

            rt.SetParent(canvasRect, false);
            rt.anchorMin = UIKit.MC;
            rt.anchorMax = UIKit.MC;
            rt.pivot = UIKit.MC;
            rt.sizeDelta = size;
            rt.position = worldPos;
            free = true;
        }

        rt.SetAsLastSibling();
        grabOffset = rt.anchoredPosition - PointerToCanvas(e);
        if (SoundFX.I != null) SoundFX.I.Paper();
    }

    public void OnDrag(PointerEventData e)
    {
        Vector2 pos = PointerToCanvas(e) + grabOffset;
        Vector2 half = canvasRect.rect.size * 0.5f;
        pos.x = Mathf.Clamp(pos.x, -half.x, half.x);
        pos.y = Mathf.Clamp(pos.y, -half.y, half.y);
        rt.anchoredPosition = pos;
    }

    public void OnEndDrag(PointerEventData e) { }

    public void OnPointerClick(PointerEventData e)
    {
        if (e.dragging) return;
        if (OnClicked != null) OnClicked();
    }

    // Put the document back into its slot on the desk (called for every new passenger).
    public void ResetToDesk()
    {
        if (free)
        {
            if (placeholder != null) { placeholder.SetActive(false); Destroy(placeholder); placeholder = null; }
            rt.SetParent(home, false);
            rt.SetSiblingIndex(homeIndex);
            free = false;
        }
        Tilt();
        LayoutRebuilder.ForceRebuildLayoutImmediate(home);
    }
}
