using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasGroup))]
public class DraggableTile : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [HideInInspector]
    public Transform ParentToReturnTo = null;
    private GameObject placeholder = null;
    private CanvasGroup canvasGroup;
    private Canvas rootCanvas;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        rootCanvas = GetComponentInParent<Canvas>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        ParentToReturnTo = transform.parent;

        placeholder = new GameObject("Placeholder");
        placeholder.transform.SetParent(ParentToReturnTo);

        LayoutElement le = placeholder.AddComponent<LayoutElement>();
        le.preferredWidth = 50;
        le.preferredHeight = 70;

        placeholder.transform.SetSiblingIndex(transform.GetSiblingIndex());

        if (rootCanvas == null) rootCanvas = GetComponentInParent<Canvas>();
        if (rootCanvas != null) transform.SetParent(rootCanvas.transform);

        if (canvasGroup != null) canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position = eventData.position;

        if (ParentToReturnTo != null)
        {
            int newSiblingIndex = ParentToReturnTo.childCount;

            for (int i = 0; i < ParentToReturnTo.childCount; i++)
            {
                if (transform.position.x < ParentToReturnTo.GetChild(i).position.x)
                {
                    newSiblingIndex = i;

                    if (placeholder != null && placeholder.transform.GetSiblingIndex() < newSiblingIndex)
                    {
                        newSiblingIndex--;
                    }
                    break;
                }
            }

            if (placeholder != null)
            {
                placeholder.transform.SetSiblingIndex(newSiblingIndex);
            }
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        transform.SetParent(ParentToReturnTo);

        if (ParentToReturnTo != null && ParentToReturnTo.GetComponent<LayoutGroup>() != null)
        {
            if (placeholder != null)
            {
                transform.SetSiblingIndex(placeholder.transform.GetSiblingIndex());
            }
        }
        else
        {
            RectTransform rt = GetComponent<RectTransform>();
            if (rt != null)
            {
                rt.anchorMin = new Vector2(0.5f, 0.5f);
                rt.anchorMax = new Vector2(0.5f, 0.5f);
                rt.pivot = new Vector2(0.5f, 0.5f);
                rt.anchoredPosition = Vector2.zero;
            }
        }

        if (placeholder != null)
        {
            Destroy(placeholder);
        }

        // Eğer sağ taş atma bölgesine bırakıldıysa taşı oraya kilitle
        bool isDiscardArea = (ParentToReturnTo != null && (ParentToReturnTo.name == "RightDiscardArea" || (ParentToReturnTo.GetComponent<DropZone>() != null && ParentToReturnTo.GetComponent<DropZone>().isRightDiscardZone)));

        if (isDiscardArea)
        {
            if (canvasGroup != null)
            {
                canvasGroup.blocksRaycasts = false;
                canvasGroup.interactable = false;
            }

            // Atılan taşın tekrar sürüklenmesini engelle
            Destroy(this);
        }
        else
        {
            if (canvasGroup != null)
            {
                canvasGroup.blocksRaycasts = true;
            }
        }
    }
}
