using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class UIManager : MonoBehaviour
{
    public GameObject TilePrefab; 
    public Transform HandPanel;
    public Button DeckButton;
    public void SetDeckButtonState(bool isInteractable)
    {
        if (DeckButton != null)
        {
            DeckButton.interactable = isInteractable;
        }
    }
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
    public void RefreshHand(List<Tile> playerHand)
    {
        // 1. Istakanýn (HandPanel) içindeki tüm eski fiziksel taþlarý yok et
        foreach (Transform child in HandPanel)
        {
            Destroy(child.gameObject);
        }

        // 2. Senin önceden yazdýðýn çizdirme fonksiyonunu çaðýrarak sýralý listeyi ekrana bas
        DrawPlayerHand(playerHand);
    }
}