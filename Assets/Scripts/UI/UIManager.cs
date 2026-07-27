using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("Istaka ve Deste Öðeleri")]
    public GameObject TilePrefab;
    public Transform HandPanel;
    public Button DeckButton;

    [Header("Gösterge Taþý UI Öðeleri")]
    // GameManager'daki centerStone objesinin içindeki yazý bileþeni
    public TextMeshProUGUI centerStoneText;

    // --- BUTON VE ETKÝLEÞÝM YÖNETÝMÝ ---
    public void SetDeckButtonState(bool isInteractable)
    {
        if (DeckButton != null)
        {
            DeckButton.interactable = isInteractable;
        }
    }

    // --- ISTAKA VE TAÞ ÇÝZÝM YÖNETÝMÝ ---
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

        // 2. Çizdirme fonksiyonunu çaðýrarak sýralý listeyi ekrana bas
        DrawPlayerHand(playerHand);
    }

    // --- GÖSTERGE YANSITMA YÖNETÝMÝ ---
    public void GostergeyiEkranaYansit(Tile gostergeTile)
    {
        if (centerStoneText == null) return;

        // Sahte okey gösterge açýldýysa (genelde üzerinde bir sembol olur)
        if (gostergeTile.IsFakeOkey)
        {
            centerStoneText.text = "J"; // Joker anlamýnda
            centerStoneText.color = Color.black;
            return;
        }

        // Göstergenin sayýsýný yazdýrýyoruz
        centerStoneText.text = gostergeTile.TileValue.ToString();

        // Göstergenin rengine göre UI metnini boyuyoruz
        switch (gostergeTile.Color)
        {
            case TileColor.Red:
                centerStoneText.color = Color.red;
                break;
            case TileColor.Black:
                centerStoneText.color = Color.black;
                break;
            case TileColor.Blue:
                centerStoneText.color = Color.blue;
                break;
            case TileColor.Yellow:
                // Tam sarý ekranda zor okunur, hafif turuncumsu veya altýn sarýsý
                centerStoneText.color = new Color(1f, 0.8f, 0f);
                break;
        }
    }
}