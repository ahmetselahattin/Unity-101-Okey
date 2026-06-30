using UnityEngine;
using UnityEngine.EventSystems;

public class DropZone : MonoBehaviour, IDropHandler
{
    // Üzerine bir obje býrakýldýðýnda bu fonksiyon otomatik çalýþýr
    public void OnDrop(PointerEventData eventData)
    {
        // Býrakýlan objede DraggableTile kodu var mý diye bakýyoruz
        DraggableTile droppedTile = eventData.pointerDrag.GetComponent<DraggableTile>();

        if (droppedTile != null)
        {
            // Eðer varsa, taþýn "geri döneceði evi" artýk BU PANEL (DiscardPanel) yapýyoruz!
            droppedTile.ParentToReturnTo = this.transform;
        }
    }
}