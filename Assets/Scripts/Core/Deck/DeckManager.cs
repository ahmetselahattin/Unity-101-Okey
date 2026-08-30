using System;
using System.Collections.Generic;

public class DeckManager
{
    public List<Tile> AllTiles { get; private set; } = new List<Tile>();
    public Tile Gosterge { get; private set; }
    public Tile OkeyTile { get; private set; }

    public int RemainingCount => AllTiles.Count;

    public void CreateDeck()
    {
        AllTiles.Clear();

        // 4 Renk iin 2'er deste (1-13) oluturulur = 104 ta
        foreach (TileColor color in Enum.GetValues(typeof(TileColor)))
        {
            for (int val = 1; val <= 13; val++)
            {
                AllTiles.Add(new Tile(val, color, false));
                AllTiles.Add(new Tile(val, color, false));
            }
        }

        // 2 adet Sahte Okey ta eklenir = Toplam 106 ta
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
        // Gsterge tan belirle (Sahte okey olmamal)
        for (int i = 0; i < AllTiles.Count; i++)
        {
            if (!AllTiles[i].IsFakeOkey)
            {
                Gosterge = AllTiles[i];
                AllTiles.RemoveAt(i);
                break;
            }
        }

        // Okey ta: Gstergenin 1 fazlas ve ayn rengidir (13 ise 1 olur)
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
