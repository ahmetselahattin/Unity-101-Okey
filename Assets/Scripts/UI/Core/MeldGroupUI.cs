using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class MeldGroupUI : MonoBehaviour, IDropHandler, IPointerClickHandler
{
    public int MeldIndex { get; private set; }
    public Meld MeldData { get; private set; }

    public void Initialize(int index, Meld meld)
    {
        MeldIndex = index;
        MeldData = meld;
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (eventData.pointerDrag == null) return;

        DraggableTile draggable = eventData.pointerDrag.GetComponent<DraggableTile>();
        if (draggable == null) return;

        TileDisplay display = draggable.GetComponent<TileDisplay>();
        if (display != null && display.tileData != null)
        {
            // Taşı bu pere işlemeyi dene
            if (GameManager.Instance != null)
            {
                bool success = GameManager.Instance.ProcessTileToTable(display.tileData, MeldIndex);
                if (success)
                {
                    // Başarılıysa sürüklenen nesneyi yok et (çünkü elden çıktı ve masaya eklendi)
                    Destroy(draggable.gameObject);
                }
            }
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log($"[MeldGroupUI] Per {MeldIndex} tıklandı. Türü: {MeldData?.Type}, Taş Sayısı: {MeldData?.Tiles?.Count}");
    }
}
