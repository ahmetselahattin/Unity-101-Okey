using UnityEngine;

public class GameManager : MonoBehaviour
{
    public Player[] players = new Player[4];
    public DeckManager deckManager;
    public UIManager uiManager;
    void Start()
    {
        //creating 4 player
        for (int i = 0; i < 4; i++)
        {
            players[i] = new Player();
        }
        //creating desk
        deckManager = new DeckManager();
        deckManager.CreateDeck();
        deckManager.Shuffle();
        deckManager.DetermineOkey();
        DistributeTiles();
        uiManager.DrawPlayerHand(players[0].Hand);
    }
    //distribution for each player
    public void DistributeTiles()
    {
        for (int i = 0; i < 4; i++)
        {
            //first player get 22 tile
            if (i == 0)
            {
                for (int j = 0; j < 22; j++)
                {
                    //we add tile and remove tile from list for first player
                    players[i].AddTile(deckManager.AllTiles[0]);
                    deckManager.AllTiles.RemoveAt(0);
                }
            }
            else
            {
                for (int j = 0; j < 21; j++)
                {
                    //we add tile and remove tile from list for other player
                    players[i].AddTile(deckManager.AllTiles[0]);
                    deckManager.AllTiles.RemoveAt(0);
                }
            }
        }
    }
    // Bu metodu GameManager sýnýfýnýn içine ekle
    public void DrawTileFromDeck()
    {
        // Destede taþ kalmadýysa hata vermemesi için kontrol ediyoruz
        if (deckManager.AllTiles.Count > 0)
        {
            // 1. Destedeki en üst taþý (0. indeks) al
            Tile drawnTile = deckManager.AllTiles[0];

            // 2. Taþý arka planda oyuncunun listesine ekle
            players[0].AddTile(drawnTile);

            // 3. Taþý desteden sil
            deckManager.AllTiles.RemoveAt(0);

            // 4. UIManager'a haber ver, ekranda ýstakaya o taþý çizsin!
            uiManager.AddSingleTileToHand(drawnTile);
        }
        else
        {
            Debug.Log("Destede taþ kalmadý!");
        }
    }
}