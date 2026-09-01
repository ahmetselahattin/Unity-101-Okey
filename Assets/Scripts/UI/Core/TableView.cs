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

    [Header("Sol Taş Alma (Yandan Taş) Referanslar")]
    public GameObject leftDiscardArea;
    public TileDisplay leftDiscardTileDisplay;
    public Button leftDiscardButton;

    [Header("Masa Ortası Açılan Perler Alanı")]
    public Transform tableCenterContainer;

    private void Start()
    {
        SetLeftDiscardTile(null, false);
    }

    public void ShowGosterge(Tile gostergeTile)
    {
        if (centerStone != null) centerStone.SetActive(true);
        if (gostergeTile == null) return;

        if (gostergeTileDisplay != null)
        {
            gostergeTileDisplay.CanFlip = false;
            gostergeTileDisplay.SetTile(gostergeTile);
            return;
        }

        if (centerStoneText != null)
        {
            centerStoneText.fontStyle = FontStyles.Bold;

            if (gostergeTile.IsFakeOkey)
            {
                centerStoneText.text = "<b>SO</b>";
                centerStoneText.color = TileColorExtensions.BlackColor;
                centerStoneText.fontSize = 28;
                return;
            }

            centerStoneText.fontSize = 28;
            centerStoneText.text = gostergeTile.TileValue.ToString();
            centerStoneText.color = gostergeTile.Color.ToColor();
        }
    }

    public void SetLeftDiscardTile(Tile tile, bool isInteractable = false)
    {
        if (leftDiscardArea != null)
        {
            leftDiscardArea.SetActive(true);
        }

        if (leftDiscardTileDisplay != null)
        {
            leftDiscardTileDisplay.CanFlip = false;
            if (tile == null)
            {
                leftDiscardTileDisplay.gameObject.SetActive(false);
            }
            else
            {
                leftDiscardTileDisplay.gameObject.SetActive(true);
                leftDiscardTileDisplay.SetTile(tile);
            }
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
            leftDiscardButton.interactable = isInteractable && (leftDiscardTileDisplay != null && leftDiscardTileDisplay.gameObject.activeSelf);
        }
    }

    public void DrawOpenedMelds(List<Meld> melds)
    {
        if (tableCenterContainer == null) return;

        for (int i = tableCenterContainer.childCount - 1; i >= 0; i--)
        {
            Destroy(tableCenterContainer.GetChild(i).gameObject);
        }

        if (melds == null || melds.Count == 0) return;

        Debug.Log($"[TableView] Masanın ortasına toplam {melds.Count} adet per sergilendi.");

        for (int mIndex = 0; mIndex < melds.Count; mIndex++)
        {
            Meld meld = melds[mIndex];

            // Her per için şık bir çerçeve grubu oluştur
            GameObject meldObj = new GameObject("MeldGroup_" + mIndex);
            meldObj.transform.SetParent(tableCenterContainer, false);

            RectTransform meldRect = meldObj.AddComponent<RectTransform>();
            meldRect.sizeDelta = new Vector2((meld.Tiles.Count * 46) + 16, 74);

            Image bgImg = meldObj.AddComponent<Image>();
            bgImg.color = new Color(0.10f, 0.12f, 0.15f, 0.88f); // Koyu şık çerçeve

            HorizontalLayoutGroup layout = meldObj.AddComponent<HorizontalLayoutGroup>();
            layout.padding = new RectOffset(6, 6, 4, 4);
            layout.spacing = 3;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = false;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;

            MeldGroupUI groupUI = meldObj.AddComponent<MeldGroupUI>();
            groupUI.Initialize(mIndex, meld);

            if (TilePrefab != null)
            {
                foreach (Tile tile in meld.Tiles)
                {
                    GameObject tileObj = Instantiate(TilePrefab, meldObj.transform);
                    tileObj.name = "TableTile_" + tile.TileValue;

                    RectTransform rt = tileObj.GetComponent<RectTransform>();
                    if (rt != null)
                    {
                        rt.sizeDelta = new Vector2(42, 62);
                    }

                    DraggableTile dragComp = tileObj.GetComponent<DraggableTile>();
                    if (dragComp != null) Destroy(dragComp);

                    CanvasGroup cg = tileObj.GetComponent<CanvasGroup>();
                    if (cg != null) cg.blocksRaycasts = false;

                    TileDisplay display = tileObj.GetComponent<TileDisplay>();
                    if (display != null)
                    {
                        display.CanFlip = false;
                        display.SetTile(tile);
                    }
                }
            }
        }
    }
}
