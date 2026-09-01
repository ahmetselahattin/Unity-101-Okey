using System;
using System.Collections.Generic;

public static class OkeyRuleEngine
{
    public const int OpenHandThreshold = 101;
    public const int MinPairsToOpen = 5;

    public static bool IsOkeyTile(Tile tile, Tile okeyTile)
    {
        if (tile == null || okeyTile == null) return false;
        return !tile.IsFakeOkey && tile.Color == okeyTile.Color && tile.TileValue == okeyTile.TileValue;
    }

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
                if (diff == 0) return false;
                requiredOkeys += (diff - 1);
            }
        }

        return requiredOkeys <= okeyCount;
    }

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

    public static bool CanProcessTileToMeld(Tile tile, Meld targetMeld, Tile okeyTile, bool playerOpenedPairs, out bool addToStart)
    {
        addToStart = false;
        if (tile == null || targetMeld == null || targetMeld.Tiles == null || targetMeld.Tiles.Count == 0)
            return false;

        if (playerOpenedPairs || targetMeld.Type == MeldType.Pair)
            return false;

        int tileVal = tile.GetEffectiveValue(okeyTile);
        TileColor tileCol = tile.GetEffectiveColor(okeyTile);

        // 1. GRUP PERİNE İŞLEME
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

        // 2. SERİ PERİNE İŞLEME
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

    public static bool CanProcessTileToAnyMeld(Tile tile, List<Meld> tableMelds, Tile okeyTile, bool playerOpenedPairs)
    {
        if (tile == null || tableMelds == null) return false;

        foreach (var meld in tableMelds)
        {
            if (CanProcessTileToMeld(tile, meld, okeyTile, playerOpenedPairs, out _))
            {
                return true;
            }
        }
        return false;
    }

    public static bool CanDrawFromDiscard(Player player, Tile candidateTile, Tile okeyTile, List<Meld> tableMelds, out string reason)
    {
        reason = string.Empty;
        if (player == null || candidateTile == null)
        {
            reason = "Geçersiz taş veya oyuncu!";
            return false;
        }

        if (player.HasOpenedHand)
        {
            if (CanProcessTileToAnyMeld(candidateTile, tableMelds, okeyTile, player.HasOpenedPairs))
            {
                return true;
            }

            reason = "Bu taş masada açılmış hiçbir pere işlenemediği için yandan taş alamazsınız!";
            return false;
        }

        List<Tile> tempHand = new List<Tile>(player.Hand) { candidateTile };

        if (DetectPairs(tempHand, okeyTile, out List<Meld> pairs))
        {
            bool tileUsedInPairs = pairs.Exists(p => p.Tiles.Contains(candidateTile));
            if (tileUsedInPairs && pairs.Count >= MinPairsToOpen)
            {
                return true;
            }
        }

        FindOptimalMelds(tempHand, okeyTile, out int totalPoints, out List<List<Tile>> bestMelds);
        bool candidateUsed = bestMelds.Exists(m => m.Contains(candidateTile));

        if (candidateUsed && totalPoints >= OpenHandThreshold)
        {
            return true;
        }

        reason = "Bu taş ile 101 barajını aşamadığınız veya 5 çift oluşturamadığınız için yandan taş ALAMAZSINIZ! Desteden çekmelisiniz.";
        return false;
    }

    /// <summary>
    /// Verilen taş listesinden çakışmayan en yüksek puanlı geçerli perleri (Seri & Grup) akıllıca bulur.
    /// </summary>
    public static void FindOptimalMelds(List<Tile> hand, Tile okeyTile, out int totalPoints, out List<List<Tile>> bestMelds)
    {
        totalPoints = 0;
        bestMelds = new List<List<Tile>>();
        if (hand == null || hand.Count < 3) return;

        List<Tile> remaining = new List<Tile>(hand);

        // 1. Önce Serileri Bul (Örn: Kırmızı 7-8-9, Mavi 3-4-5)
        Dictionary<TileColor, List<Tile>> byColor = new Dictionary<TileColor, List<Tile>>();
        foreach (var t in remaining)
        {
            TileColor col = t.GetEffectiveColor(okeyTile);
            if (!byColor.ContainsKey(col)) byColor[col] = new List<Tile>();
            byColor[col].Add(t);
        }

        foreach (var kvp in byColor)
        {
            List<Tile> colorList = kvp.Value;
            colorList.Sort((a, b) => a.GetEffectiveValue(okeyTile).CompareTo(b.GetEffectiveValue(okeyTile)));

            List<Tile> currentSeq = new List<Tile>();
            for (int i = 0; i < colorList.Count; i++)
            {
                if (currentSeq.Count == 0)
                {
                    currentSeq.Add(colorList[i]);
                }
                else
                {
                    int diff = colorList[i].GetEffectiveValue(okeyTile) - currentSeq[currentSeq.Count - 1].GetEffectiveValue(okeyTile);
                    if (diff == 1)
                    {
                        currentSeq.Add(colorList[i]);
                    }
                    else if (diff > 1)
                    {
                        if (currentSeq.Count >= 3)
                        {
                            bestMelds.Add(new List<Tile>(currentSeq));
                            foreach (var t in currentSeq) remaining.Remove(t);
                        }
                        currentSeq.Clear();
                        currentSeq.Add(colorList[i]);
                    }
                }
            }
            if (currentSeq.Count >= 3)
            {
                bestMelds.Add(new List<Tile>(currentSeq));
                foreach (var t in currentSeq) remaining.Remove(t);
            }
        }

        // 2. Kalan Taşlardan Grupları Bul (Örn: 8-8-8 farklı renkler)
        Dictionary<int, List<Tile>> byValue = new Dictionary<int, List<Tile>>();
        foreach (var t in remaining)
        {
            int val = t.GetEffectiveValue(okeyTile);
            if (!byValue.ContainsKey(val)) byValue[val] = new List<Tile>();
            byValue[val].Add(t);
        }

        foreach (var kvp in byValue)
        {
            if (kvp.Value.Count >= 3)
            {
                HashSet<TileColor> seen = new HashSet<TileColor>();
                List<Tile> group = new List<Tile>();
                foreach (var t in kvp.Value)
                {
                    if (seen.Add(t.GetEffectiveColor(okeyTile)))
                    {
                        group.Add(t);
                        if (group.Count == 4) break;
                    }
                }

                if (group.Count >= 3)
                {
                    bestMelds.Add(group);
                    foreach (var t in group) remaining.Remove(t);
                }
            }
        }

        totalPoints = CalculateTotalPoints(bestMelds, okeyTile);
    }
}
