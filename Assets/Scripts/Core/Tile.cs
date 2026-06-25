using UnityEngine;

// 4 different color for tile
public enum TileColor
{
    Yellow, Blue, Black, Red
}

public class Tile
{
    public int TileValue;
    public TileColor Color;
    public bool IsFakeOkey;
    //constructor for tiles
    public Tile(int tileValue, TileColor color, bool isFakeOkey = false)
    {
        Color = color;
        TileValue = tileValue;
        IsFakeOkey = isFakeOkey;
    }
}