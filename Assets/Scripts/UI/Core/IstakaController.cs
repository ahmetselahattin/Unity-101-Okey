using System.Collections.Generic;
using UnityEngine;

public class IstakaController : MonoBehaviour
{
    [Header("Prefabs & Referanslar")]
    public GameObject TilePrefab;
    public GameObject SlotPrefab;
    public Transform HandPanel;

    private readonly List<Transform> istakaSlots = new List<Transform>();
    public const int TotalSlotCount = 42;

    public void Initialize()
    {
        if (HandPanel == null) return;

        // Eer nceden oluturulmu slotlar varsa temizle veya kaydet
        if (istakaSlots.Count == 0)
        {
            if (HandPanel.childCount == 0 && SlotPrefab != null)
            {
                for (int i = 0; i < TotalSlotCount; i++)
                {
                    GameObject newSlot = Instantiate(SlotPrefab, HandPanel);
                    newSlot.name = "Slot_" + i;
                    istakaSlots.Add(newSlot.transform);
                }
            }
            else
            {
                foreach (Transform child in HandPanel)
                {
                    istakaSlots.Add(child);
                }
            }
        }
    }

    public void DrawHand(List<Tile> playerHand)
    {
        ClearTilesOnly();

        if (playerHand == null) return;

        for (int i = 0; i < playerHand.Count && i < istakaSlots.Count; i++)
        {
            if (TilePrefab == null) break;
            GameObject tileObj = Instantiate(TilePrefab, istakaSlots[i]);
            TileDisplay display = tileObj.GetComponent<TileDisplay>();
            if (display != null)
            {
                display.SetTile(playerHand[i]);
            }
        }
    }

    public void AddSingleTile(Tile tileData)
    {
        if (tileData == null || TilePrefab == null) return;

        // lk bo yuvay bul
        Transform targetSlot = null;
        foreach (Transform slot in istakaSlots)
        {
            if (slot.childCount == 0)
            {
                targetSlot = slot;
                break;
            }
        }

        Transform parent = targetSlot != null ? targetSlot : HandPanel;
        GameObject tileObj = Instantiate(TilePrefab, parent);
        TileDisplay display = tileObj.GetComponent<TileDisplay>();
        if (display != null)
        {
            display.SetTile(tileData);
        }
    }

    public void ClearTilesOnly()
    {
        foreach (Transform slot in istakaSlots)
        {
            for (int i = slot.childCount - 1; i >= 0; i--)
            {
                Destroy(slot.GetChild(i).gameObject);
            }
        }
    }

    public List<List<Tile>> GetMeldsFromIstaka()
    {
        List<List<Tile>> detectedMelds = new List<List<Tile>>();
        List<Tile> currentGroup = new List<Tile>();

        foreach (Transform slot in istakaSlots)
        {
            TileDisplay tileDisp = slot.GetComponentInChildren<TileDisplay>();

            if (tileDisp != null && tileDisp.tileData != null)
            {
                currentGroup.Add(tileDisp.tileData);
            }
            else
            {
                if (currentGroup.Count >= 3)
                {
                    detectedMelds.Add(new List<Tile>(currentGroup));
                }
                currentGroup.Clear();
            }
        }

        if (currentGroup.Count >= 3)
        {
            detectedMelds.Add(new List<Tile>(currentGroup));
        }

        return detectedMelds;
    }

    public void RemoveTiles(List<Tile> tilesToRemove)
    {
        if (tilesToRemove == null) return;

        foreach (Tile tile in tilesToRemove)
        {
            foreach (Transform slot in istakaSlots)
            {
                TileDisplay tileDisp = slot.GetComponentInChildren<TileDisplay>();
                if (tileDisp != null && tileDisp.tileData != null && tileDisp.tileData.IsSame(tile))
                {
                    Destroy(tileDisp.gameObject);
                    break;
                }
            }
        }
    }
}
