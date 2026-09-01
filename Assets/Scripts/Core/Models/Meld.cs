using System;
using System.Collections.Generic;
using System.Text;

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

    public static string SerializeMelds(List<Meld> melds)
    {
        if (melds == null || melds.Count == 0) return string.Empty;

        StringBuilder sb = new StringBuilder();
        for (int m = 0; m < melds.Count; m++)
        {
            if (m > 0) sb.Append(";");
            sb.Append((int)melds[m].Type).Append(":");

            for (int t = 0; t < melds[m].Tiles.Count; t++)
            {
                if (t > 0) sb.Append(",");
                Tile tile = melds[m].Tiles[t];
                sb.Append(tile.TileValue).Append("_").Append((int)tile.Color).Append("_").Append(tile.IsFakeOkey ? 1 : 0);
            }
        }
        return sb.ToString();
    }

    public static List<Meld> DeserializeMelds(string data)
    {
        List<Meld> melds = new List<Meld>();
        if (string.IsNullOrEmpty(data)) return melds;

        string[] meldStrings = data.Split(';');
        foreach (var mStr in meldStrings)
        {
            string[] parts = mStr.Split(':');
            if (parts.Length < 2) continue;

            MeldType type = (MeldType)int.Parse(parts[0]);
            List<Tile> tileList = new List<Tile>();

            string[] tileStrings = parts[1].Split(',');
            foreach (var tStr in tileStrings)
            {
                string[] tData = tStr.Split('_');
                if (tData.Length >= 3)
                {
                    int val = int.Parse(tData[0]);
                    TileColor col = (TileColor)int.Parse(tData[1]);
                    bool isFake = (tData[2] == "1");
                    tileList.Add(new Tile(val, col, isFake));
                }
            }

            melds.Add(new Meld(tileList, type));
        }

        return melds;
    }
}
