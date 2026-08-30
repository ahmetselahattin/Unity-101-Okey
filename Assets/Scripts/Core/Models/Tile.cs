using System;

[Serializable]
public class Tile
{
    public int TileValue;
    public TileColor Color;
    public bool IsFakeOkey;

    public Tile(int tileValue, TileColor color, bool isFakeOkey = false)
    {
        TileValue = tileValue;
        Color = color;
        IsFakeOkey = isFakeOkey;
    }

    public int GetEffectiveValue(Tile okeyTile)
    {
        if (IsFakeOkey && okeyTile != null)
        {
            return okeyTile.TileValue;
        }
        return TileValue;
    }

    public TileColor GetEffectiveColor(Tile okeyTile)
    {
        if (IsFakeOkey && okeyTile != null)
        {
            return okeyTile.Color;
        }
        return Color;
    }

    public bool IsSame(Tile other)
    {
        if (other == null) return false;
        return TileValue == other.TileValue && Color == other.Color && IsFakeOkey == other.IsFakeOkey;
    }

    public override string ToString()
    {
        if (IsFakeOkey) return "[Fake Okey (SO)]";
        return $"[{Color} {TileValue}]";
    }
}
