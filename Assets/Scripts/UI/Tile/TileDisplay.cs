using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class TileDisplay : MonoBehaviour, IPointerClickHandler
{
    [Header("UI Referansları")]
    public TextMeshProUGUI NumberText;
    public Image BackgroundImage;

    [Header("Görsel Ayarlar")]
    public Color NormalTileBgColor = new Color(0.98f, 0.97f, 0.94f, 1f); // Krem taş rengi
    public Color FlippedTileBgColor = new Color(0.38f, 0.24f, 0.14f, 1f); // Ahşap taş sırtı rengi

    public Tile tileData { get; private set; }
    public bool IsFlipped { get; private set; } = false;

    public void SetTile(Tile data)
    {
        this.tileData = data;
        this.IsFlipped = false;

        UpdateVisuals();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // Sol tık ile çift tıklama (Double Click) kontrolü
        if (eventData.button == PointerEventData.InputButton.Left && eventData.clickCount == 2)
        {
            ToggleFlip();
        }
    }

    public void ToggleFlip()
    {
        IsFlipped = !IsFlipped;
        UpdateVisuals();
    }

    private void UpdateVisuals()
    {
        if (tileData == null) return;

        if (BackgroundImage != null)
        {
            BackgroundImage.color = IsFlipped ? FlippedTileBgColor : NormalTileBgColor;
        }

        if (NumberText != null)
        {
            if (IsFlipped)
            {
                NumberText.gameObject.SetActive(false);
            }
            else
            {
                NumberText.gameObject.SetActive(true);
                NumberText.enableAutoSizing = true;
                NumberText.fontSizeMin = 12;
                NumberText.fontSizeMax = 32;
                NumberText.alignment = TextAlignmentOptions.Center;
                NumberText.fontStyle = FontStyles.Bold;

                if (tileData.IsFakeOkey)
                {
                    NumberText.text = "SO";
                    NumberText.color = TileColorExtensions.BlackColor;
                }
                else
                {
                    NumberText.text = tileData.TileValue.ToString();
                    NumberText.color = tileData.Color.ToColor();
                }
            }
        }
    }
}
