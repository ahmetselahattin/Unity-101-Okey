using System;
using System.Collections.Generic;

public enum MeldType
{
    Invalid,
    Group,
    Sequence,
    Pair
}

[Serializable]
public class Meld
{
    public MeldType Type { get; set; }
    public List<Tile> Tiles { get; private set; }
    public int TotalPoints { get; set; }

    public Meld()
    {
        Tiles = new List<Tile>();
        Type = MeldType.Invalid;
        TotalPoints = 0;
    }

    public Meld(List<Tile> tiles, MeldType type, int totalPoints = 0)
    {
        Tiles = tiles != null ? new List<Tile>(tiles) : new List<Tile>();
        Type = type;
        TotalPoints = totalPoints;
    }
}
