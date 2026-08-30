using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TableView : MonoBehaviour
{
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
        if (melds == null) return;
        Debug.Log($"[TableView] Masanın ortasına toplam {melds.Count} adet per sergilendi.");
    }
}
