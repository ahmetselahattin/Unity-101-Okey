using UnityEngine;
using System.Collections.Generic;

public class UIManager : MonoBehaviour
{
    public GameObject TilePrefab; 
    public Transform HandPanel;   

    public void DrawPlayerHand(List<Tile> playerHand)
    {
        foreach (Tile tileData in playerHand)
        {
            GameObject cloneTile = Instantiate(TilePrefab, HandPanel);

            cloneTile.GetComponent<TileDisplay>().SetTile(tileData);
        }
    }
}