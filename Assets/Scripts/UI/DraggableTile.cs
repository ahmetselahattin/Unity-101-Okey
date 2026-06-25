using UnityEngine;
using UnityEngine.EventSystems; 

// Sýnýfýmýzýn yanýna bu 3 arayüzü (interface) ekliyoruz
public class DraggableTile : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    Transform originalParent; // Taþýn ilk bulunduðu yeri hatýrlamak için

    public void OnBeginDrag(PointerEventData eventData)
    {
        // Taþý tuttuðumuzda eski yerini hafýzaya alýyoruz
        originalParent = transform.parent;

        // Taþý Grid'den koparýp en üst katmana (Canvas'a) alýyoruz ki diðer taþlarýn altýnda kalmasýn
        transform.SetParent(transform.root);

        // Farenin taþýn içinden geçip altýndaki zemini (veya diðer taþý) algýlayabilmesi için
        GetComponent<CanvasGroup>().blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        // Input.mousePosition yerine, modern UI sistemi olan eventData'nýn pozisyonunu kullanýyoruz.
        // Bu kod hem mobilde hem bilgisayarda kusursuz çalýþýr.
        transform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        // Taþý býraktýðýmýzda þimdilik eski yerine geri dönsün 
        // (Ýleride burayý ýstakada yeni bir yere oturmasý için deðiþtireceðiz)
        transform.SetParent(originalParent);

        // Týklama engelini tekrar açýyoruz
        GetComponent<CanvasGroup>().blocksRaycasts = true;
    }
}