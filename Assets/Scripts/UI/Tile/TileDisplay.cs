using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class TileDisplay : MonoBehaviour, IPointerClickHandler
{
    [Header("UI Referansları")]
    public TextMeshProUGUI NumberText;
    public Image BackgroundImage;

    [Header("Görsel Renk Ayarları")]
    public Color NormalTileBgColor = new Color(0.98f, 0.97f, 0.94f, 1f); // Doğal kemik/krem rengi
    public Color FlippedTileBgColor = new Color(1f, 1f, 1f, 1f);          // Bembeyaz sayısız taş sırtı

    [Header("Çevrilebilirlik")]
    public bool CanFlip = true; // Yalnızca oyuncunun ıstakasındaki taşlar için true'dur

    public Tile tileData { get; private set; }
    public bool IsFlipped { get; private set; } = false;

    private void Awake()
    {
        if (BackgroundImage == null) BackgroundImage = GetComponent<Image>();
        if (NumberText == null) NumberText = GetComponentInChildren<TextMeshProUGUI>();
    }

    public void SetTile(Tile data)
    {
        this.tileData = data;
        this.IsFlipped = false;

        UpdateVisuals();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!CanFlip) return; // Gösterge, masa perleri ve atılan taşlar döndürülemez!

        // Sol tık ile çift tıklama (Double Click) kontrolü
        if (eventData.button == PointerEventData.InputButton.Left && eventData.clickCount == 2)
        {
            ToggleFlip();
        }
    }

    public void ToggleFlip()
    {
        if (!CanFlip) return;

        IsFlipped = !IsFlipped;
        UpdateVisuals();
        Debug.Log($"[TileDisplay] Taş ters çevrildi. Durum: {(IsFlipped ? "Ters (Bembeyaz/Boş)" : "Düz")}");
    }

    private void UpdateVisuals()
    {
        if (tileData == null) return;

        if (BackgroundImage == null) BackgroundImage = GetComponent<Image>();
        if (NumberText == null) NumberText = GetComponentInChildren<TextMeshProUGUI>();

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
