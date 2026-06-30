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
    public void AddSingleTileToHand(Tile tileData)
    {
        // Þablondan yeni bir taþ klonla ve HandPanel'e koy
        GameObject cloneTile = Instantiate(TilePrefab, HandPanel);

        // Taþa verisini (renk, sayý) gönder
        cloneTile.GetComponent<TileDisplay>().SetTile(tileData);
    }
}