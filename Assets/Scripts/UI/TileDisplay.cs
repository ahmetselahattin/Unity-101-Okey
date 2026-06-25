using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class TileDisplay : MonoBehaviour
{
    public TextMeshProUGUI NumberText;
    public Image BackgroundImage; 
    public void SetTile(Tile tileData) 
    {
        NumberText.text = tileData.TileValue.ToString(); 
        if(tileData.Color == TileColor.Red)
        {
            NumberText.color =Color.red;
        }
        else if(tileData.Color == TileColor.Yellow) 
        {
            NumberText.color =Color.yellow;
        }else if(tileData.Color == TileColor.Blue) 
        {
            NumberText.color =Color.blue;
        }
        else
        {
            NumberText.color = Color.black;
        }
        if (tileData.IsFakeOkey)
        {
            NumberText.text = "SO";
            NumberText.color = Color.black;
        }
    }
}
