using UnityEngine;
using UnityEngine.EventSystems;

public class IstakaSlot : MonoBehaviour, IDropHandler
{
    public void OnDrop(PointerEventData eventData)
    {
        if (eventData.pointerDrag == null) return;

        DraggableTile draggedTile = eventData.pointerDrag.GetComponent<DraggableTile>();
        if (draggedTile == null) return;

        // Yuva bosa direkt bu slota al
        if (transform.childCount == 0 || (transform.childCount == 1 && transform.GetChild(0).name == "Placeholder"))
        {
            draggedTile.ParentToReturnTo = this.transform;
        }
        else
        {
            // Yuvada baka bir ta varsa Swap (yer deitir)
            Transform existingTile = transform.GetChild(0);
            if (existingTile.name == "Placeholder" && transform.childCount > 1)
            {
                existingTile = transform.GetChild(1);
            }

            if (existingTile.name != "Placeholder")
            {
                existingTile.SetParent(draggedTile.ParentToReturnTo);
                existingTile.localPosition = Vector3.zero;
            }

            draggedTile.ParentToReturnTo = this.transform;
        }
    }
}
