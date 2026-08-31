using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TileDisplay : MonoBehaviour
{
    [Header("UI Referansları")]
    public TextMeshProUGUI NumberText;
    public Image BackgroundImage;

    public Tile tileData { get; private set; }

    public void SetTile(Tile data)
    {
        this.tileData = data;

        if (data == null) return;

        if (NumberText != null)
        {
            NumberText.enableAutoSizing = true;
            NumberText.fontSizeMin = 12;
            NumberText.fontSizeMax = 32;
            NumberText.alignment = TextAlignmentOptions.Center;
            NumberText.fontStyle = FontStyles.Bold;

            if (data.IsFakeOkey)
            {
                NumberText.text = "SO";
                NumberText.color = TileColorExtensions.BlackColor;
                return;
            }

            NumberText.text = data.TileValue.ToString();
            NumberText.color = data.Color.ToColor();
        }
    }
}
