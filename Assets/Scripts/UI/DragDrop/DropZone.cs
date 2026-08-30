using UnityEngine;
using UnityEngine.EventSystems;

public class DropZone : MonoBehaviour, IDropHandler
{
    public void OnDrop(PointerEventData eventData)
    {
        if (eventData.pointerDrag == null) return;

        DraggableTile droppedTile = eventData.pointerDrag.GetComponent<DraggableTile>();
        if (droppedTile != null)
        {
            droppedTile.ParentToReturnTo = this.transform;
        }
    }
}
