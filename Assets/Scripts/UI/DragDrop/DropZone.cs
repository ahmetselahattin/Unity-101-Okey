using UnityEngine;
using UnityEngine.EventSystems;

public class DropZone : MonoBehaviour, IDropHandler
{
    [Header("Bölge Türü")]
    public bool isRightDiscardZone = true;
    public bool isCenterFinishZone = false;

    public void OnDrop(PointerEventData eventData)
    {
        if (eventData.pointerDrag == null) return;

        DraggableTile droppedTile = eventData.pointerDrag.GetComponent<DraggableTile>();
        if (droppedTile == null) return;

        TileDisplay tileDisplay = droppedTile.GetComponent<TileDisplay>();
        Tile tileData = tileDisplay != null ? tileDisplay.tileData : null;

        if (isCenterFinishZone)
        {
            // Ortaya taş atarak oyunu bitirme kontrolü
            if (GameManager.Instance != null)
            {
                if (GameManager.Instance.CanPlayerFinishToCenter())
                {
                    droppedTile.ParentToReturnTo = this.transform;
                    GameManager.Instance.OnPlayerFinishToCenter(tileData);
                    Destroy(droppedTile.gameObject);
                }
                else
                {
                    Debug.LogWarning("[DropZone] Ortaya sadece elinizde son 1 taş kaldığında ve elinizi açtığınızda oyunu bitirmek için taş atabilirsiniz!");
                }
            }
            return;
        }

        if (isRightDiscardZone)
        {
            // GameManager üzerinden taş atma izni kontrolü
            if (GameManager.Instance != null && GameManager.Instance.CanPlayerDiscard())
            {
                // Önceki atılan taşları temizle veya arkaya al
                for (int i = transform.childCount - 1; i >= 0; i--)
                {
                    Transform child = transform.GetChild(i);
                    if (child.name != "Placeholder")
                    {
                        Destroy(child.gameObject);
                    }
                }

                droppedTile.ParentToReturnTo = this.transform;

                // Taş atıldığında oyuncunun elinden düşür ve turu bitir
                GameManager.Instance.OnPlayerDiscardTile(tileData);
            }
            else
            {
                Debug.LogWarning("[DropZone] Şu an taş atamazsınız! (Önce taş çekmeli veya yandan taş aldıysanız elinizi açmalısınız).");
                // ParentToReturnTo değiştirilmez, taş eski yuvasına geri döner!
            }
        }
        else
        {
            droppedTile.ParentToReturnTo = this.transform;
        }
    }
}
