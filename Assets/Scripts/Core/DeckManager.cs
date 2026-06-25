using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using System;


public class DeckManager
{
    public List<Tile> AllTiles = new List<Tile>();

    public void CreateDeck()
    {   
        //this structure for adding new tile for each color
        foreach (TileColor color in System.Enum.GetValues(typeof(TileColor)))
        {
            //this structure for adding a 26 tile for each color
            for (int i = 1; i <= 13; i++)
            {
                
                AllTiles.Add(new Tile(i, color, false));
                AllTiles.Add(new Tile(i, color, false));
            }
        }

        //this 2 tile for false okey
        AllTiles.Add(new Tile(0, TileColor.Yellow, true));
        AllTiles.Add(new Tile(0, TileColor.Yellow, true));
    }
    public void Shuffle()
    {
        // Random nesnesi döngünün dýþýnda bir kez oluþturulur
        System.Random rnd = new System.Random();

        // from list last element to 0 
        for (int i = AllTiles.Count - 1; i > 0; i--)
        {
            // choose random number between 0 and i
            // rnd.Next() method doesn't include upper bound thats why we write i+1
            int x = rnd.Next(0, i + 1);

            // swap two element
            (AllTiles[i], AllTiles[x]) = (AllTiles[x], AllTiles[i]);
        }
    }
}