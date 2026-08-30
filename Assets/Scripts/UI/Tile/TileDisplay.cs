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

            if (data.IsFakeOkey)
            {
                NumberText.text = "SO";
                NumberText.color = new Color(0.15f, 0.15f, 0.15f, 1f);
                return;
            }

            NumberText.text = data.TileValue.ToString();

            switch (data.Color)
            {
                case TileColor.Red:
                    NumberText.color = new Color(0.9f, 0.1f, 0.1f, 1f);
                    break;
                case TileColor.Yellow:
                    NumberText.color = new Color(0.95f, 0.65f, 0f, 1f);
                    break;
                case TileColor.Blue:
                    NumberText.color = new Color(0.1f, 0.45f, 0.95f, 1f);
                    break;
                case TileColor.Black:
                default:
                    NumberText.color = new Color(0.15f, 0.15f, 0.15f, 1f);
                    break;
            }
        }
    }
}
