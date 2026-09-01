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
    public const int SlotsPerRow = 21;

    public void Initialize()
    {
        if (HandPanel == null) return;

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
                display.CanFlip = true;
                display.SetTile(playerHand[i]);
            }
        }
    }

    public void DrawArrangedHand(Tile[] slotLayout)
    {
        ClearTilesOnly();

        if (slotLayout == null) return;

        for (int i = 0; i < slotLayout.Length && i < istakaSlots.Count; i++)
        {
            if (slotLayout[i] != null && TilePrefab != null)
            {
                GameObject tileObj = Instantiate(TilePrefab, istakaSlots[i]);
                TileDisplay display = tileObj.GetComponent<TileDisplay>();
                if (display != null)
                {
                    display.CanFlip = true;
                    display.SetTile(slotLayout[i]);
                }
            }
        }
    }

    public void AddSingleTile(Tile tileData)
    {
        if (tileData == null || TilePrefab == null) return;

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
            display.CanFlip = true;
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

    /// <summary>
    /// Istakadaki taşları fiziksel sırasına göre okur. İster aralarında boşluk olsun ister bitişik
    /// dizilmiş olsun, oyuncunun soldan sağa dizdiği perleri tam olarak ayrıştırır.
    /// </summary>
    public List<List<Tile>> GetMeldsFromIstaka(Tile okeyTile = null)
    {
        List<List<Tile>> detectedMelds = new List<List<Tile>>();
        List<Tile> currentBlock = new List<Tile>();

        for (int i = 0; i < istakaSlots.Count; i++)
        {
            Transform slot = istakaSlots[i];
            TileDisplay tileDisp = slot.GetComponentInChildren<TileDisplay>();

            if (tileDisp != null && tileDisp.tileData != null)
            {
                currentBlock.Add(tileDisp.tileData);
            }
            else
            {
                if (currentBlock.Count > 0)
                {
                    ProcessContiguousBlock(currentBlock, okeyTile, detectedMelds);
                    currentBlock.Clear();
                }
            }

            // Satır sonu kontrolü (Slot 20 üst satırın sonudur)
            if (i == SlotsPerRow - 1)
            {
                if (currentBlock.Count > 0)
                {
                    ProcessContiguousBlock(currentBlock, okeyTile, detectedMelds);
                    currentBlock.Clear();
                }
            }
        }

        if (currentBlock.Count > 0)
        {
            ProcessContiguousBlock(currentBlock, okeyTile, detectedMelds);
        }

        return detectedMelds;
    }

    private void ProcessContiguousBlock(List<Tile> block, Tile okeyTile, List<List<Tile>> outputMelds)
    {
        if (block == null || block.Count < 3) return;

        // 1. Blok tek başına geçerli bir per mi?
        if (OkeyRuleEngine.CheckGroupPer(block, okeyTile) || OkeyRuleEngine.CheckSequencePer(block, okeyTile))
        {
            outputMelds.Add(new List<Tile>(block));
            return;
        }

        // 2. Blok bitişik dizilmiş birden fazla per içeriyorsa soldan sağa parçala
        int i = 0;
        while (i < block.Count)
        {
            bool found = false;
            // 5'li, 4'lü, 3'lü per parçalarını dene
            for (int len = 5; len >= 3; len--)
            {
                if (i + len <= block.Count)
                {
                    List<Tile> subChunk = block.GetRange(i, len);
                    if (OkeyRuleEngine.CheckGroupPer(subChunk, okeyTile) || OkeyRuleEngine.CheckSequencePer(subChunk, okeyTile))
                    {
                        outputMelds.Add(subChunk);
                        i += len;
                        found = true;
                        break;
                    }
                }
            }

            if (!found)
            {
                i++; // Geçersiz tekil taşı atla
            }
        }
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
