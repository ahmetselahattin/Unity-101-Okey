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
    /// Verilen taş listesindeki tüm geçerli çiftleri tespit eder ve Meld listesi olarak döner.
    /// </summary>
    public static bool DetectPairs(List<Tile> hand, Tile okeyTile, out List<Meld> detectedPairs)
    {
        detectedPairs = new List<Meld>();
        if (hand == null || hand.Count < 2) return false;

        List<Tile> pool = new List<Tile>(hand);
        List<Tile> okeys = new List<Tile>();
        List<Tile> normals = new List<Tile>();

        foreach (var t in pool)
        {
            if (IsOkeyTile(t, okeyTile)) okeys.Add(t);
            else normals.Add(t);
        }

        normals.Sort((a, b) =>
        {
            int c = a.GetEffectiveColor(okeyTile).CompareTo(b.GetEffectiveColor(okeyTile));
            return c != 0 ? c : a.GetEffectiveValue(okeyTile).CompareTo(b.GetEffectiveValue(okeyTile));
        });

        // 1. Birebir aynı olan normal taşları çift yap
        bool[] used = new bool[normals.Count];
        for (int i = 0; i < normals.Count - 1; i++)
        {
            if (used[i]) continue;

            for (int j = i + 1; j < normals.Count; j++)
            {
                if (used[j]) continue;

                if (normals[i].GetEffectiveColor(okeyTile) == normals[j].GetEffectiveColor(okeyTile) &&
                    normals[i].GetEffectiveValue(okeyTile) == normals[j].GetEffectiveValue(okeyTile) &&
                    normals[i].GetEffectiveValue(okeyTile) > 0)
                {
                    used[i] = true;
                    used[j] = true;
                    int points = normals[i].GetEffectiveValue(okeyTile) * 2;
                    detectedPairs.Add(new Meld(new List<Tile> { normals[i], normals[j] }, MeldType.Pair, points));
                    break;
                }
            }
        }

        // 2. Kalan tek taşlar varsa ve elimizde Joker (Okey) varsa Joker ile çift tamamla
        for (int i = 0; i < normals.Count; i++)
        {
            if (!used[i] && okeys.Count > 0)
            {
                used[i] = true;
                Tile joker = okeys[0];
                okeys.RemoveAt(0);
                int points = normals[i].GetEffectiveValue(okeyTile) * 2;
                detectedPairs.Add(new Meld(new List<Tile> { normals[i], joker }, MeldType.Pair, points));
            }
        }

        // 3. İki adet Joker kaldıysa birbiriyle çift yap
        while (okeys.Count >= 2)
        {
            Tile j1 = okeys[0];
            Tile j2 = okeys[1];
            okeys.RemoveRange(0, 2);
            int points = okeyTile.TileValue * 2;
            detectedPairs.Add(new Meld(new List<Tile> { j1, j2 }, MeldType.Pair, points));
        }

        return detectedPairs.Count >= MinPairsToOpen;
    }

    /// <summary>
    /// Çift açma kurallarını doğrular (En az 5 çift).
    /// </summary>
    public static bool ValidateOpenPairs(List<Tile> hand, Tile okeyTile, out List<Meld> pairsToOpen, out string error)
    {
        error = string.Empty;
        if (DetectPairs(hand, okeyTile, out pairsToOpen))
        {
            return true;
        }

        error = $"Çift açabilmek için en az {MinPairsToOpen} çift gereklidir! Mevcut çift sayısı: {pairsToOpen.Count}";
        return false;
    }

    /// <summary>
    /// Seri/Grup per açma kurallarını doğrular (101 barajı).
    /// </summary>
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

    /// <summary>
    /// Bir taşın masada açılmış bir pere işlenip işlenemeyeceğini kontrol eder.
    /// Kural: Çift açmış oyuncu seri perlere taş işleyemez!
    /// </summary>
    public static bool CanProcessTileToMeld(Tile tile, Meld targetMeld, Tile okeyTile, bool playerOpenedPairs, out bool addToStart)
    {
        addToStart = false;
        if (tile == null || targetMeld == null || targetMeld.Tiles == null || targetMeld.Tiles.Count == 0)
            return false;

        // 101 Kuralı: Çift açan oyuncu seri perlere taş işleyemez!
        if (playerOpenedPairs)
        {
            return false;
        }

        // Çift meldlerine doğrudan tek taş işlenemez (çift 2 taştır)
        if (targetMeld.Type == MeldType.Pair)
        {
            return false;
        }

        int tileVal = tile.GetEffectiveValue(okeyTile);
        TileColor tileCol = tile.GetEffectiveColor(okeyTile);

        // 1. GRUP PERİNE İŞLEME (3'lü grup peri 4'lüye tamamlanabilir)
        if (targetMeld.Type == MeldType.Group || CheckGroupPer(targetMeld.Tiles, okeyTile))
        {
            if (targetMeld.Tiles.Count >= 4) return false;

            Tile sampleTile = targetMeld.Tiles.Find(t => !IsOkeyTile(t, okeyTile));
            int expectedVal = sampleTile != null ? sampleTile.GetEffectiveValue(okeyTile) : okeyTile.TileValue;

            if (tileVal != expectedVal) return false;

            foreach (Tile t in targetMeld.Tiles)
            {
                if (!IsOkeyTile(t, okeyTile) && t.GetEffectiveColor(okeyTile) == tileCol)
                {
                    return false;
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

            if (tileVal == firstVal - 1 && firstVal > 1)
            {
                addToStart = true;
                return true;
            }

            if (tileVal == lastVal + 1 && lastVal < 13)
            {
                addToStart = false;
                return true;
            }
        }

        return false;
    }
}
