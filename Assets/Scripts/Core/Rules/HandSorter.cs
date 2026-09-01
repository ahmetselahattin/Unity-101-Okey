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

    /// <summary>
    /// Taşları akıllıca analiz eder: Önce geçerli 3+ taşlık perleri (Seri & Grup) gruplayıp aralarında
    /// 1 boşluk bırakarak dizer, ardından kalan per dışı taşları yan yana düzenler.
    /// </summary>
    public static void ArrangeHandSmartly(
        List<Tile> hand,
        Tile okeyTile,
        out List<List<Tile>> bestMelds,
        out List<Tile> remainingTiles,
        out Tile[] istakaLayout)
    {
        istakaLayout = new Tile[42];
        remainingTiles = new List<Tile>();
        bestMelds = new List<List<Tile>>();

        if (hand == null || hand.Count == 0) return;

        OkeyRuleEngine.FindOptimalMelds(hand, okeyTile, out int totalPoints, out bestMelds);

        HashSet<Tile> usedTiles = new HashSet<Tile>();
        foreach (var m in bestMelds)
        {
            if (OkeyRuleEngine.CheckSequencePer(m, okeyTile))
            {
                m.Sort((a, b) => a.GetEffectiveValue(okeyTile).CompareTo(b.GetEffectiveValue(okeyTile)));
            }
            else
            {
                m.Sort((a, b) => a.GetEffectiveColor(okeyTile).CompareTo(b.GetEffectiveColor(okeyTile)));
            }

            foreach (var t in m) usedTiles.Add(t);
        }

        foreach (var t in hand)
        {
            if (!usedTiles.Contains(t))
            {
                remainingTiles.Add(t);
            }
        }

        SortByColorAndValue(remainingTiles, okeyTile);

        // 42 Slotluk Istakaya Perleri Aralıklı Yerleştirme:
        int currentSlot = 0;
        foreach (var meld in bestMelds)
        {
            // Üst satır sınır kontrolü (0..20)
            if (currentSlot <= 20)
            {
                if (currentSlot + meld.Count > 21)
                {
                    currentSlot = 21; // Alt satıra geç
                }
            }
            else if (currentSlot <= 41)
            {
                if (currentSlot + meld.Count > 42)
                {
                    break;
                }
            }

            foreach (var t in meld)
            {
                if (currentSlot < 42)
                {
                    istakaLayout[currentSlot++] = t;
                }
            }

            // Perler arası 1 slot boşluk bırak
            currentSlot++;
        }

        // Kalan per dışı taşları yerleştir
        int remSlot = (currentSlot <= 17 && bestMelds.Count > 0) ? 21 : currentSlot;
        foreach (var t in remainingTiles)
        {
            while (remSlot < 42 && istakaLayout[remSlot] != null)
            {
                remSlot++;
            }
            if (remSlot < 42)
            {
                istakaLayout[remSlot++] = t;
            }
        }
    }
}
