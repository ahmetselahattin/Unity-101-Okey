using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TableView : MonoBehaviour
{
    [Header("Tile Prefab")]
    public GameObject TilePrefab;

    [Header("Gösterge Taş UI Referansları")]
    public GameObject centerStone;
    public TileDisplay gostergeTileDisplay;
    public TextMeshProUGUI centerStoneText;

    [Header("Sol Taş Alma (Yandan Taş) Referansları")]
    public GameObject leftDiscardArea;
    public TileDisplay leftDiscardTileDisplay;
    public Button leftDiscardButton;

    [Header("Masa Ortası Açılan Perler Alanı")]
    public Transform tableCenterContainer;

    public void ShowGosterge(Tile gostergeTile)
    {
        if (centerStone != null) centerStone.SetActive(true);
        if (gostergeTile == null) return;

        if (gostergeTileDisplay != null)
        {
            gostergeTileDisplay.SetTile(gostergeTile);
            return;
        }

        if (centerStoneText != null)
        {
            if (gostergeTile.IsFakeOkey)
            {
                centerStoneText.text = "<b>SO</b>";
                centerStoneText.color = new Color(0.15f, 0.15f, 0.15f, 1f);
                centerStoneText.fontSize = 28;
                return;
            }

            centerStoneText.fontSize = 28;
            centerStoneText.text = gostergeTile.TileValue.ToString();

            switch (gostergeTile.Color)
            {
                case TileColor.Red:
                    centerStoneText.color = new Color(0.9f, 0.1f, 0.1f, 1f);
                    break;
                case TileColor.Black:
                    centerStoneText.color = new Color(0.15f, 0.15f, 0.15f, 1f);
                    break;
                case TileColor.Blue:
                    centerStoneText.color = new Color(0.1f, 0.45f, 0.95f, 1f);
                    break;
                case TileColor.Yellow:
                    centerStoneText.color = new Color(0.95f, 0.65f, 0f, 1f);
                    break;
            }
        }
    }

    public void SetLeftDiscardTile(Tile tile, bool isInteractable = false)
    {
        if (leftDiscardArea != null)
        {
            leftDiscardArea.SetActive(tile != null);
        }

        if (leftDiscardTileDisplay != null && tile != null)
        {
            leftDiscardTileDisplay.SetTile(tile);
        }

        if (leftDiscardButton != null)
        {
            leftDiscardButton.interactable = isInteractable && (tile != null);
        }
    }

    public void SetLeftDiscardButtonInteractable(bool isInteractable)
    {
        if (leftDiscardButton != null)
        {
            leftDiscardButton.interactable = isInteractable;
        }
    }

    public void DrawOpenedMelds(List<Meld> melds)
    {
        if (tableCenterContainer == null) return;

        // 1. Önceki açılan perleri temizle
        for (int i = tableCenterContainer.childCount - 1; i >= 0; i--)
        {
            Destroy(tableCenterContainer.GetChild(i).gameObject);
        }

        if (melds == null || melds.Count == 0) return;

        Debug.Log($"[TableView] Masanın ortasına toplam {melds.Count} adet per sergilendi.");

        // 2. Her bir Meld için bir grup oluştur ve taşlarını yerleştir
        for (int mIndex = 0; mIndex < melds.Count; mIndex++)
        {
            Meld meld = melds[mIndex];

            GameObject meldObj = new GameObject("MeldGroup_" + mIndex);
            meldObj.transform.SetParent(tableCenterContainer, false);

            RectTransform meldRect = meldObj.AddComponent<RectTransform>();
            meldRect.sizeDelta = new Vector2((meld.Tiles.Count * 45) + 10, 65);

            HorizontalLayoutGroup layout = meldObj.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 2;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = false;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;

            // Taş işleme ve tıklama dinleyicisi
            MeldGroupUI groupUI = meldObj.AddComponent<MeldGroupUI>();
            groupUI.Initialize(mIndex, meld);

            // Per içindeki taşları oluştur
            if (TilePrefab != null)
            {
                foreach (Tile tile in meld.Tiles)
                {
                    GameObject tileObj = Instantiate(TilePrefab, meldObj.transform);
                    tileObj.name = "TableTile_" + tile.TileValue;
                    tileObj.transform.localScale = new Vector3(0.85f, 0.85f, 1f);

                    // Masadaki taşların sürüklenmesini engelle
                    DraggableTile dragComp = tileObj.GetComponent<DraggableTile>();
                    if (dragComp != null) Destroy(dragComp);

                    CanvasGroup cg = tileObj.GetComponent<CanvasGroup>();
                    if (cg != null) cg.blocksRaycasts = false; // Tıklamalar üstteki MeldGroupUI'a geçsin

                    TileDisplay display = tileObj.GetComponent<TileDisplay>();
                    if (display != null)
                    {
                        display.SetTile(tile);
                    }
                }
            }
        }
    }
}
