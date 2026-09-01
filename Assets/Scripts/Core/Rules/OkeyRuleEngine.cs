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
                if (diff <= 0) return false; // Aynı taştan 2 tane olamaz
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

        // 1. Durum: Daha önce el açmış oyuncu masaya işleyebiliyorsa alabilir
        if (player.HasOpenedHand)
        {
            if (CanProcessTileToAnyMeld(candidateTile, tableMelds, okeyTile, player.HasOpenedPairs))
            {
                return true;
            }

            reason = "Bu taş masada açılmış hiçbir pere işlenemediği için yandan taş alamazsınız!";
            return false;
        }

        // 2. Durum: Henüz el açmamış oyuncu için simülasyon
        List<Tile> tempHand = new List<Tile>(player.Hand) { candidateTile };

        // 2a. Çift kontrolü
        if (DetectPairs(tempHand, okeyTile, out List<Meld> pairs))
        {
            bool tileUsedInPairs = pairs.Exists(p => p.Tiles.Contains(candidateTile));
            if (tileUsedInPairs && pairs.Count >= MinPairsToOpen)
            {
                return true;
            }
        }

        // 2b. 101 Seri/Grup per kontrolü
        FindOptimalMelds(tempHand, okeyTile, out int totalPoints, out List<List<Tile>> bestMelds);
        bool candidateUsed = bestMelds.Exists(m => m.Contains(candidateTile));

        if (candidateUsed && totalPoints >= OpenHandThreshold)
        {
            return true;
        }

        reason = $"Bu taş ile 101 barajını aşamadığınız veya 5 çift oluşturamadığınız için yandan taş ALAMAZSINIZ! (Mevcut Maksimum Puan: {totalPoints} / Gerekli: 101)";
        return false;
    }

    /// <summary>
    /// Verilen taş havuzundan çakışmayan en yüksek puanlı tüm geçerli perleri (Seri & Grup) tam kapsamlı bulur.
    /// </summary>
    public static void FindOptimalMelds(List<Tile> hand, Tile okeyTile, out int totalPoints, out List<List<Tile>> bestMelds)
    {
        totalPoints = 0;
        bestMelds = new List<List<Tile>>();
        if (hand == null || hand.Count < 3) return;

        List<List<Tile>> allCandidateMelds = GenerateAllPossibleMelds(hand, okeyTile);

        // En yüksek puanı veren çakışmayan kombinasyonu bul
        List<List<Tile>> currentCombo = new List<List<Tile>>();
        HashSet<Tile> usedTiles = new HashSet<Tile>();

        int maxScore = 0;
        List<List<Tile>> maxMelds = new List<List<Tile>>();

        FindBestCombinationRecursive(allCandidateMelds, 0, currentCombo, usedTiles, okeyTile, ref maxScore, ref maxMelds);

        totalPoints = maxScore;
        bestMelds = maxMelds;
    }

    private static void FindBestCombinationRecursive(
        List<List<Tile>> candidateMelds,
        int startIndex,
        List<List<Tile>> currentCombo,
        HashSet<Tile> usedTiles,
        Tile okeyTile,
        ref int maxScore,
        ref List<List<Tile>> maxMelds)
    {
        int currentScore = CalculateTotalPoints(currentCombo, okeyTile);
        if (currentScore > maxScore)
        {
            maxScore = currentScore;
            maxMelds = new List<List<Tile>>(currentCombo);
        }

        for (int i = startIndex; i < candidateMelds.Count; i++)
        {
            List<Tile> meld = candidateMelds[i];

            // Çakışma var mı kontrol et
            bool overlaps = false;
            foreach (var t in meld)
            {
                if (usedTiles.Contains(t))
                {
                    overlaps = true;
                    break;
                }
            }

            if (!overlaps)
            {
                foreach (var t in meld) usedTiles.Add(t);
                currentCombo.Add(meld);

                FindBestCombinationRecursive(candidateMelds, i + 1, currentCombo, usedTiles, okeyTile, ref maxScore, ref maxMelds);

                currentCombo.RemoveAt(currentCombo.Count - 1);
                foreach (var t in meld) usedTiles.Remove(t);
            }
        }
    }

    private static List<List<Tile>> GenerateAllPossibleMelds(List<Tile> hand, Tile okeyTile)
    {
        List<List<Tile>> candidateMelds = new List<List<Tile>>();

        // 1. Grupları bul (Aynı sayı, farklı renkler: 3'lü veya 4'lü)
        Dictionary<int, List<Tile>> byVal = new Dictionary<int, List<Tile>>();
        foreach (var t in hand)
        {
            int v = t.GetEffectiveValue(okeyTile);
            if (!byVal.ContainsKey(v)) byVal[v] = new List<Tile>();
            byVal[v].Add(t);
        }

        foreach (var kvp in byVal)
        {
            List<Tile> sameValList = kvp.Value;

            // Renklere göre tekilleştirilmiş kombinasyonları bul
            Dictionary<TileColor, List<Tile>> colMap = new Dictionary<TileColor, List<Tile>>();
            foreach (var t in sameValList)
            {
                TileColor c = t.GetEffectiveColor(okeyTile);
                if (!colMap.ContainsKey(c)) colMap[c] = new List<Tile>();
                colMap[c].Add(t);
            }

            if (colMap.Count >= 3)
            {
                List<TileColor> cols = new List<TileColor>(colMap.Keys);

                // 3'lü kombinasyonlar
                for (int i = 0; i < cols.Count; i++)
                {
                    for (int j = i + 1; j < cols.Count; j++)
                    {
                        for (int k = j + 1; k < cols.Count; k++)
                        {
                            foreach (var t1 in colMap[cols[i]])
                            {
                                foreach (var t2 in colMap[cols[j]])
                                {
                                    foreach (var t3 in colMap[cols[k]])
                                    {
                                        candidateMelds.Add(new List<Tile> { t1, t2, t3 });
                                    }
                                }
                            }
                        }
                    }
                }

                // 4'lü kombinasyon
                if (colMap.Count == 4)
                {
                    foreach (var t1 in colMap[cols[0]])
                    {
                        foreach (var t2 in colMap[cols[1]])
                        {
                            foreach (var t3 in colMap[cols[2]])
                            {
                                foreach (var t4 in colMap[cols[3]])
                                {
                                    candidateMelds.Add(new List<Tile> { t1, t2, t3, t4 });
                                }
                            }
                        }
                    }
                }
            }
        }

        // 2. Serileri bul (Aynı renk, ardışık sayılar)
        Dictionary<TileColor, List<Tile>> byColor = new Dictionary<TileColor, List<Tile>>();
        foreach (var t in hand)
        {
            TileColor c = t.GetEffectiveColor(okeyTile);
            if (!byColor.ContainsKey(c)) byColor[c] = new List<Tile>();
            byColor[c].Add(t);
        }

        foreach (var kvp in byColor)
        {
            List<Tile> colorTiles = kvp.Value;
            Dictionary<int, List<Tile>> valMap = new Dictionary<int, List<Tile>>();
            foreach (var t in colorTiles)
            {
                int v = t.GetEffectiveValue(okeyTile);
                if (!valMap.ContainsKey(v)) valMap[v] = new List<Tile>();
                valMap[v].Add(t);
            }

            List<int> sortedVals = new List<int>(valMap.Keys);
            sortedVals.Sort();

            for (int i = 0; i < sortedVals.Count; i++)
            {
                List<int> currentRun = new List<int> { sortedVals[i] };
                for (int j = i + 1; j < sortedVals.Count; j++)
                {
                    if (sortedVals[j] == currentRun[currentRun.Count - 1] + 1)
                    {
                        currentRun.Add(sortedVals[j]);
                        if (currentRun.Count >= 3)
                        {
                            // Bu run için taş kombinasyonlarını ekle
                            AddSequenceCombinations(valMap, currentRun, candidateMelds);
                        }
                    }
                    else
                    {
                        break;
                    }
                }
            }
        }

        return candidateMelds;
    }

    private static void AddSequenceCombinations(Dictionary<int, List<Tile>> valMap, List<int> runValues, List<List<Tile>> candidateMelds)
    {
        // runValues: örn [5, 6, 7]
        List<List<Tile>> combos = new List<List<Tile>> { new List<Tile>() };

        foreach (int v in runValues)
        {
            List<Tile> options = valMap[v];
            List<List<Tile>> newCombos = new List<List<Tile>>();

            foreach (var c in combos)
            {
                foreach (var opt in options)
                {
                    List<Tile> extended = new List<Tile>(c) { opt };
                    newCombos.Add(extended);
                }
            }
            combos = newCombos;
        }

        foreach (var c in combos)
        {
            if (c.Count >= 3)
            {
                candidateMelds.Add(c);
            }
        }
    }
}
