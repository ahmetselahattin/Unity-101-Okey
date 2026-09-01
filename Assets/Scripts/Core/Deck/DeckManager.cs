using System;
using System.Collections.Generic;

public class DeckManager
{
    public List<Tile> AllTiles { get; set; } = new List<Tile>();
    public Tile Gosterge { get; set; }
    public Tile OkeyTile { get; set; }

    public int RemainingCount => AllTiles.Count;

    public void SetGostergeAndOkey(Tile gosterge, Tile okey)
    {
        Gosterge = gosterge;
        OkeyTile = okey;
    }

    public void CreateDeck()
    {
        AllTiles.Clear();

        foreach (TileColor color in Enum.GetValues(typeof(TileColor)))
        {
            for (int val = 1; val <= 13; val++)
            {
                AllTiles.Add(new Tile(val, color, false));
                AllTiles.Add(new Tile(val, color, false));
            }
        }

        AllTiles.Add(new Tile(0, TileColor.Yellow, true));
        AllTiles.Add(new Tile(0, TileColor.Yellow, true));
    }

    public void Shuffle()
    {
        Random rnd = new Random();
        for (int i = AllTiles.Count - 1; i > 0; i--)
        {
            int swapIndex = rnd.Next(0, i + 1);
            (AllTiles[i], AllTiles[swapIndex]) = (AllTiles[swapIndex], AllTiles[i]);
        }
    }

    public void DetermineOkey()
    {
        for (int i = 0; i < AllTiles.Count; i++)
        {
            if (!AllTiles[i].IsFakeOkey)
            {
                Gosterge = AllTiles[i];
                AllTiles.RemoveAt(i);
                break;
            }
        }

        int okeyValue = Gosterge.TileValue == 13 ? 1 : Gosterge.TileValue + 1;
        OkeyTile = new Tile(okeyValue, Gosterge.Color, false);
    }

    public Tile DrawTile()
    {
        if (AllTiles.Count == 0) return null;
        Tile drawn = AllTiles[0];
        AllTiles.RemoveAt(0);
        return drawn;
    }
}
