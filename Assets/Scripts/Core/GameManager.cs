using UnityEngine;

public class GameManager : MonoBehaviour
{
    public Player[] players = new Player[4];
    DeckManager deckManager;

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

    void Update()
    {

    }
}