using System;
using System.Collections.Generic;

public static class HandSorter
{
    public static void SortByColorAndValue(List<Tile> hand, Tile okeyTile = null)
    {
        if (hand == null) return;
        hand.Sort((t1, t2) =>
        {
            TileColor col1 = t1.GetEffectiveColor(okeyTile);
            TileColor col2 = t2.GetEffectiveColor(okeyTile);

            int colorComparison = col1.CompareTo(col2);
            if (colorComparison != 0) return colorComparison;

            int val1 = t1.GetEffectiveValue(okeyTile);
            int val2 = t2.GetEffectiveValue(okeyTile);

            return val1.CompareTo(val2);
        });
    }

    public static void SortByValueAndColor(List<Tile> hand, Tile okeyTile = null)
    {
        if (hand == null) return;
        hand.Sort((t1, t2) =>
        {
            int val1 = t1.GetEffectiveValue(okeyTile);
            int val2 = t2.GetEffectiveValue(okeyTile);

            int valueComparison = val1.CompareTo(val2);
            if (valueComparison != 0) return valueComparison;

            TileColor col1 = t1.GetEffectiveColor(okeyTile);
            TileColor col2 = t2.GetEffectiveColor(okeyTile);

            return col1.CompareTo(col2);
        });
    }

    public static void SortPairs(List<Tile> hand, Tile okeyTile = null)
    {
        if (hand == null) return;
        SortByColorAndValue(hand, okeyTile);
    }
}
