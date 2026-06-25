using UnityEngine;

public class GameManager : MonoBehaviour
{
    public Player[] players = new Player[4];
    DeckManager deckManager;
    void Start()
    {
        for (int i = 0; i < 3; i++)
        {
            players[i] = new Player();
        }
        deckManager = new DeckManager();
        deckManager.CreateDeck();
        deckManager.Shuffle();
        deckManager.DetermineOkey();
        DistributeTiles();
    }
    public void DistributeTiles()
    {
        for (int i = 0; i < 4; i++)
        {
            if(i == 0) 
            {
                for (int j = 0; j < 22; j++) 
                {
                    players[i].AddTile(deckManager.AllTiles[j]);
                    deckManager.AllTiles.RemoveAt(j);
                }
            }
            else
            {
                for (int j = 0; j < 21; j++)
                {
                    players[i].AddTile(deckManager.AllTiles[j]);
                    deckManager.AllTiles.RemoveAt(j);

                }

            }
        }

    }
    void Update()
    {
        
    }
}
