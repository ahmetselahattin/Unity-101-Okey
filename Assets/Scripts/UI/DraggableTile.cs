using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DraggableTile : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public Transform ParentToReturnTo = null; // Dýþarýdan müdahaleye açýk yeni evimiz
    GameObject placeholder = null;

    public void OnBeginDrag(PointerEventData eventData)
    {
        // Taþý ilk tuttuðumuzda varsayýlan evini þu an bulunduðu yer yapýyoruz
        ParentToReturnTo = transform.parent;

        placeholder = new GameObject("Placeholder");

        // Bütün originalParent'lar ParentToReturnTo oldu
        placeholder.transform.SetParent(ParentToReturnTo);

        LayoutElement le = placeholder.AddComponent<LayoutElement>();
        le.preferredWidth = 50;
        le.preferredHeight = 70;

        placeholder.transform.SetSiblingIndex(transform.GetSiblingIndex());

        transform.SetParent(GetComponentInParent<Canvas>().transform);
        GetComponent<CanvasGroup>().blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position = eventData.position;

        // originalParent.childCount yerine ParentToReturnTo.childCount
        int newSiblingIndex = ParentToReturnTo.childCount;

        for (int i = 0; i < ParentToReturnTo.childCount; i++)
        {
            if (transform.position.x < ParentToReturnTo.GetChild(i).position.x)
            {
                newSiblingIndex = i;

                if (placeholder.transform.GetSiblingIndex() < newSiblingIndex)
                {
                    newSiblingIndex--;
                }
                break;
            }
        }

        placeholder.transform.SetSiblingIndex(newSiblingIndex);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        transform.SetParent(ParentToReturnTo);

        if (ParentToReturnTo.GetComponent<LayoutGroup>() != null)
        {
            transform.SetSiblingIndex(placeholder.transform.GetSiblingIndex());
        }
        else
        {
            // --- KESÝN ÇÖZÜM: Inspector'ý ezip her þeyi kodla merkeze zorluyoruz ---
            RectTransform rt = GetComponent<RectTransform>();

            // Çýpalarý (Anchor) tam ortaya al
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);

            // Pivot'u tam ortaya al
            rt.pivot = new Vector2(0.5f, 0.5f);

            // Konumu sýfýrla (X:0, Y:0)
            rt.anchoredPosition = Vector2.zero;
        }

        // --- KÝLÝT MEKANÝZMASI ---
        if (ParentToReturnTo.name == "RightDiscardArea")
        {
            GetComponent<CanvasGroup>().blocksRaycasts = false;
            GetComponent<CanvasGroup>().interactable = false;

            // BÝZÝM HAMLEMÝZ BÝTTÝ! GameManager'a turu bitirmesini söylüyoruz:
            GameManager.Instance.EndTurn();
        }
        else
        {
            GetComponent<CanvasGroup>().blocksRaycasts = true;
        }

        Destroy(placeholder);
    }
}