using System;
using System.Collections.Generic;

public static class OkeyRuleEngine
{
    public const int OpenHandThreshold = 101;
    public const int MinPairsToOpen = 5;

    /// <summary>
    /// Taşın gerçek Joker (Okey) olup olmadığını doğrular. (Sahte okey joker DEĞİLDİR).
    /// </summary>
    public static bool IsOkeyTile(Tile tile, Tile okeyTile)
    {
        if (tile == null || okeyTile == null) return false;
        return !tile.IsFakeOkey && tile.Color == okeyTile.Color && tile.TileValue == okeyTile.TileValue;
    }

    /// <summary>
    /// Grup peri kontrolü: Aynı rakam, farklı renklerden 3 veya 4 taş (Okey joker olarak kullanılabilir).
    /// </summary>
    public static bool CheckGroupPer(List<Tile> tileList, Tile okeyTile)
    {
        if (tileList == null || tileList.Count < 3 || tileList.Count > 4) return false;

        List<Tile> normalTiles = new List<Tile>();
        int okeyCount = 0;

        foreach (Tile t in tileList)
        {
            if (IsOkeyTile(t, okeyTile)) okeyCount++;
            else normalTiles.Add(t);
        }

        if (normalTiles.Count <= 1) return true;

        int expectedValue = normalTiles[0].GetEffectiveValue(okeyTile);
        HashSet<TileColor> usedColors = new HashSet<TileColor>();

        foreach (Tile t in normalTiles)
        {
            if (t.GetEffectiveValue(okeyTile) != expectedValue) return false;
            if (!usedColors.Add(t.GetEffectiveColor(okeyTile))) return false;
        }

        return true;
    }

    /// <summary>
    /// Seri peri kontrolü: Aynı renkten ardışık sayılar (Örn: Mavi 4-5-6-7).
    /// </summary>
    public static bool CheckSequencePer(List<Tile> tileList, Tile okeyTile)
    {
        if (tileList == null || tileList.Count < 3) return false;

        List<Tile> normalTiles = new List<Tile>();
        int okeyCount = 0;

        foreach (Tile t in tileList)
        {
            if (IsOkeyTile(t, okeyTile)) okeyCount++;
            else normalTiles.Add(t);
        }

        if (normalTiles.Count <= 1) return true;

        normalTiles.Sort((t1, t2) => t1.GetEffectiveValue(okeyTile).CompareTo(t2.GetEffectiveValue(okeyTile)));

        TileColor expectedColor = normalTiles[0].GetEffectiveColor(okeyTile);
        int requiredOkeys = 0;

        for (int i = 0; i < normalTiles.Count; i++)
        {
            if (normalTiles[i].GetEffectiveColor(okeyTile) != expectedColor) return false;

            if (i > 0)
            {
                int diff = normalTiles[i].GetEffectiveValue(okeyTile) - normalTiles[i - 1].GetEffectiveValue(okeyTile);
                if (diff == 0) return false; // Aynı sayıdan iki adet olamaz
                requiredOkeys += (diff - 1);
            }
        }

        return requiredOkeys <= okeyCount;
    }

    /// <summary>
    /// Çift kontrolü: En az 5 adet birebir aynı çift taş var mı?
    /// </summary>
    public static bool CheckForPairs(List<Tile> hand, Tile okeyTile, out int pairCount)
    {
        pairCount = 0;
        if (hand == null || hand.Count < 2) return false;

        List<Tile> sorted = new List<Tile>(hand);
        sorted.Sort((a, b) =>
        {
            int c = a.GetEffectiveColor(okeyTile).CompareTo(b.GetEffectiveColor(okeyTile));
            return c != 0 ? c : a.GetEffectiveValue(okeyTile).CompareTo(b.GetEffectiveValue(okeyTile));
        });

        for (int i = 0; i < sorted.Count - 1; i++)
        {
            int valA = sorted[i].GetEffectiveValue(okeyTile);
            int valB = sorted[i + 1].GetEffectiveValue(okeyTile);
            TileColor colA = sorted[i].GetEffectiveColor(okeyTile);
            TileColor colB = sorted[i + 1].GetEffectiveColor(okeyTile);

            if (colA == colB && valA == valB && valA > 0)
            {
                pairCount++;
                i++; // Bu çifti atla
            }
        }

        return pairCount >= MinPairsToOpen;
    }

    /// <summary>
    /// Bir per grubunun toplam puanını hesaplar.
    /// </summary>
    public static int CalculateMeldPoints(List<Tile> meld, Tile okeyTile)
    {
        if (meld == null || meld.Count == 0) return 0;

        if (CheckGroupPer(meld, okeyTile))
        {
            Tile normalTile = meld.Find(t => !IsOkeyTile(t, okeyTile));
            int val = (normalTile != null) ? normalTile.GetEffectiveValue(okeyTile) : (okeyTile != null ? okeyTile.TileValue : 1);
            return val * meld.Count;
        }

        if (CheckSequencePer(meld, okeyTile))
        {
            List<Tile> sorted = new List<Tile>(meld);
            sorted.Sort((a, b) => a.GetEffectiveValue(okeyTile).CompareTo(b.GetEffectiveValue(okeyTile)));

            Tile normalTile = sorted.Find(t => !IsOkeyTile(t, okeyTile));
            int baseVal = (normalTile != null) ? normalTile.GetEffectiveValue(okeyTile) : 1;

            int sum = 0;
            for (int i = 0; i < meld.Count; i++)
            {
                sum += (baseVal + i);
            }
            return sum;
        }

        return 0;
    }

    public static int CalculateTotalPoints(List<List<Tile>> allMelds, Tile okeyTile)
    {
        int total = 0;
        if (allMelds == null) return 0;

        foreach (var meld in allMelds)
        {
            total += CalculateMeldPoints(meld, okeyTile);
        }

        return total;
    }

    public static bool ValidateOpenHand(List<List<Tile>> melds, Tile okeyTile, out int totalPoints, out string error)
    {
        totalPoints = 0;
        error = string.Empty;

        if (melds == null || melds.Count == 0)
        {
            error = "Istakada açılacak geçerli bir per grubu bulunamadı!";
            return false;
        }

        foreach (var meld in melds)
        {
            bool isGroup = CheckGroupPer(meld, okeyTile);
            bool isSequence = CheckSequencePer(meld, okeyTile);

            if (!isGroup && !isSequence)
            {
                error = "Geçersiz per tespit edildi! Kurallara uymayan taş dizilimi var.";
                return false;
            }
        }

        totalPoints = CalculateTotalPoints(melds, okeyTile);

        if (totalPoints < OpenHandThreshold)
        {
            error = $"101 barajı aşılamadı! Toplam Puan: {totalPoints} / Gerekli: {OpenHandThreshold}";
            return false;
        }

        return true;
    }

    /// <summary>
    /// Bir taşın masada açılmış bir pere işlenip işlenemeyeceğini kontrol eder.
    /// </summary>
    public static bool CanProcessTileToMeld(Tile tile, Meld targetMeld, Tile okeyTile, out bool addToStart)
    {
        addToStart = false;
        if (tile == null || targetMeld == null || targetMeld.Tiles == null || targetMeld.Tiles.Count == 0)
            return false;

        int tileVal = tile.GetEffectiveValue(okeyTile);
        TileColor tileCol = tile.GetEffectiveColor(okeyTile);

        // 1. GRUP PERİNE İŞLEME (Örn: 3'lü grup peri 4'lüye tamamlanabilir)
        if (targetMeld.Type == MeldType.Group || CheckGroupPer(targetMeld.Tiles, okeyTile))
        {
            if (targetMeld.Tiles.Count >= 4) return false; // 4 renkten fazla grup olamaz

            Tile sampleTile = targetMeld.Tiles.Find(t => !IsOkeyTile(t, okeyTile));
            int expectedVal = sampleTile != null ? sampleTile.GetEffectiveValue(okeyTile) : okeyTile.TileValue;

            if (tileVal != expectedVal) return false;

            // Rengin grupta henüz kullanılmamış olması gerekir
            foreach (Tile t in targetMeld.Tiles)
            {
                if (!IsOkeyTile(t, okeyTile) && t.GetEffectiveColor(okeyTile) == tileCol)
                {
                    return false; // Aynı renk zaten var
                }
            }

            return true;
        }

        // 2. SERİ PERİNE İŞLEME (Başa veya Sona ekleme)
        if (targetMeld.Type == MeldType.Sequence || CheckSequencePer(targetMeld.Tiles, okeyTile))
        {
            List<Tile> tiles = targetMeld.Tiles;
            Tile sampleTile = tiles.Find(t => !IsOkeyTile(t, okeyTile));
            TileColor expectedCol = sampleTile != null ? sampleTile.GetEffectiveColor(okeyTile) : okeyTile.Color;

            if (tileCol != expectedCol && !IsOkeyTile(tile, okeyTile)) return false;

            int firstVal = tiles[0].GetEffectiveValue(okeyTile);
            int lastVal = tiles[tiles.Count - 1].GetEffectiveValue(okeyTile);

            // Başa eklenebilir mi? (Örn: 5-6-7'ye 4)
            if (tileVal == firstVal - 1 && firstVal > 1)
            {
                addToStart = true;
                return true;
            }

            // Sona eklenebilir mi? (Örn: 5-6-7'ye 8)
            if (tileVal == lastVal + 1 && lastVal < 13)
            {
                addToStart = false;
                return true;
            }
        }

        return false;
    }
}
