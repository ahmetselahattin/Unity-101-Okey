using System;
using System.Collections.Generic;
using UnityEngine;

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

        int targetValue = -1;
        HashSet<TileColor> seenColors = new HashSet<TileColor>();
        int okeyCount = 0;

        foreach (Tile t in tileList)
        {
            if (IsOkeyTile(t, okeyTile))
            {
                okeyCount++;
            }
            else
            {
                int val = t.GetEffectiveValue(okeyTile);
                if (targetValue == -1)
                {
                    targetValue = val;
                }
                else if (targetValue != val)
                {
                    return false;
                }

                TileColor col = t.GetEffectiveColor(okeyTile);
                if (seenColors.Contains(col))
                {
                    return false;
                }
                seenColors.Add(col);
            }
        }

        return (seenColors.Count + okeyCount == tileList.Count) && (seenColors.Count + okeyCount <= 4);
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
                if (diff <= 0) return false;
                requiredOkeys += (diff - 1);
            }
        }

        int minV = normalTiles[0].GetEffectiveValue(okeyTile);
        int maxV = normalTiles[normalTiles.Count - 1].GetEffectiveValue(okeyTile);
        int totalSpan = (maxV - minV + 1) + (okeyCount - requiredOkeys);

        return (requiredOkeys <= okeyCount) && (totalSpan <= 13);
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
            List<Tile> normalTiles = new List<Tile>();
            int okeyCount = 0;

            foreach (var t in meld)
            {
                if (IsOkeyTile(t, okeyTile)) okeyCount++;
                else normalTiles.Add(t);
            }

            if (normalTiles.Count == 0)
            {
                int oVal = (okeyTile != null) ? okeyTile.TileValue : 1;
                return oVal * meld.Count;
            }

            normalTiles.Sort((a, b) => a.GetEffectiveValue(okeyTile).CompareTo(b.GetEffectiveValue(okeyTile)));

            int minV = normalTiles[0].GetEffectiveValue(okeyTile);
            int maxV = normalTiles[normalTiles.Count - 1].GetEffectiveValue(okeyTile);

            int insideGaps = (maxV - minV + 1) - normalTiles.Count;
            int remainingOkeys = Mathf.Max(0, okeyCount - insideGaps);

            int highCapacity = Mathf.Max(0, 13 - maxV);
            int okeysAtHigh = Mathf.Min(remainingOkeys, highCapacity);
            int okeysAtLow = remainingOkeys - okeysAtHigh;

            int startVal = Mathf.Max(1, minV - okeysAtLow);
            int sum = 0;
            for (int i = 0; i < meld.Count; i++)
            {
                sum += (startVal + i);
            }
            return sum;
        }

        return 0;
    }

    public static int CalculateTotalPoints(List<List<Tile>> melds, Tile okeyTile)
    {
        if (melds == null) return 0;
        int total = 0;
        foreach (var meld in melds)
        {
            total += CalculateMeldPoints(meld, okeyTile);
        }
        return total;
    }

    public static bool ValidateOpenHand(List<List<Tile>> melds, Tile okeyTile, out int totalPoints, out string error)
    {
        totalPoints = 0;
        error = "";

        if (melds == null || melds.Count == 0)
        {
            error = "Açılacak geçerli per bulunamadı!";
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
            error = $"Toplam puan {totalPoints}, baraj olan {OpenHandThreshold} puanı geçmedi!";
            return false;
        }

        return true;
    }

    public static bool ValidateOpenPairs(List<Tile> hand, Tile okeyTile, out List<Meld> pairsToOpen, out string error)
    {
        pairsToOpen = new List<Meld>();
        error = "";

        if (DetectPairs(hand, okeyTile, out List<Meld> detectedPairs))
        {
            if (detectedPairs.Count >= MinPairsToOpen)
            {
                pairsToOpen = detectedPairs;
                return true;
            }
            error = $"Yeterli çift yok! En az {MinPairsToOpen} çift gerekli (Elinizdeki çift sayısı: {detectedPairs.Count}).";
            return false;
        }

        error = "Hiç çift bulunamadı!";
        return false;
    }

    public static bool DetectPairs(List<Tile> hand, Tile okeyTile, out List<Meld> pairs)
    {
        pairs = new List<Meld>();
        if (hand == null || hand.Count < 2) return false;

        List<Tile> pool = new List<Tile>(hand);
        List<Tile> okeyPool = new List<Tile>();

        for (int i = pool.Count - 1; i >= 0; i--)
        {
            if (IsOkeyTile(pool[i], okeyTile))
            {
                okeyPool.Add(pool[i]);
                pool.RemoveAt(i);
            }
        }

        for (int i = 0; i < pool.Count; i++)
        {
            if (pool[i] == null) continue;

            for (int j = i + 1; j < pool.Count; j++)
            {
                if (pool[j] == null) continue;

                if (pool[i].GetEffectiveColor(okeyTile) == pool[j].GetEffectiveColor(okeyTile) &&
                    pool[i].GetEffectiveValue(okeyTile) == pool[j].GetEffectiveValue(okeyTile))
                {
                    pairs.Add(new Meld(new List<Tile> { pool[i], pool[j] }, MeldType.Pair, pool[i].GetEffectiveValue(okeyTile) * 2));
                    pool[i] = null;
                    pool[j] = null;
                    break;
                }
            }
        }

        // Kalan tekil taşlar için Okey jokerlerini çift olarak bağla
        for (int i = 0; i < pool.Count && okeyPool.Count > 0; i++)
        {
            if (pool[i] != null)
            {
                Tile okeyT = okeyPool[0];
                okeyPool.RemoveAt(0);
                pairs.Add(new Meld(new List<Tile> { pool[i], okeyT }, MeldType.Pair, pool[i].GetEffectiveValue(okeyTile) * 2));
                pool[i] = null;
            }
        }

        return pairs.Count > 0;
    }

    public static bool CanProcessTileToMeld(Tile candidateTile, Meld targetMeld, Tile okeyTile, bool playerHasOpenedPairs, out bool addToStart)
    {
        addToStart = false;
        if (candidateTile == null || targetMeld == null || targetMeld.Tiles == null || targetMeld.Tiles.Count == 0)
            return false;

        if (targetMeld.Type == MeldType.Pair)
        {
            return false;
        }

        if (targetMeld.Type == MeldType.Group)
        {
            if (targetMeld.Tiles.Count >= 4) return false;

            Tile sampleTile = targetMeld.Tiles.Find(t => !IsOkeyTile(t, okeyTile));
            int groupValue = (sampleTile != null) ? sampleTile.GetEffectiveValue(okeyTile) : (okeyTile != null ? okeyTile.TileValue : 1);

            if (IsOkeyTile(candidateTile, okeyTile) || candidateTile.GetEffectiveValue(okeyTile) == groupValue)
            {
                TileColor candidateColor = candidateTile.GetEffectiveColor(okeyTile);
                foreach (var t in targetMeld.Tiles)
                {
                    if (!IsOkeyTile(t, okeyTile) && t.GetEffectiveColor(okeyTile) == candidateColor)
                        return false;
                }
                addToStart = false;
                return true;
            }
            return false;
        }

        if (targetMeld.Type == MeldType.Sequence)
        {
            List<Tile> currentTiles = targetMeld.Tiles;
            Tile firstTile = currentTiles[0];
            Tile lastTile = currentTiles[currentTiles.Count - 1];

            Tile sampleTile = currentTiles.Find(t => !IsOkeyTile(t, okeyTile));
            if (sampleTile == null) return false;

            TileColor seqColor = sampleTile.GetEffectiveColor(okeyTile);
            if (!IsOkeyTile(candidateTile, okeyTile) && candidateTile.GetEffectiveColor(okeyTile) != seqColor)
                return false;

            int firstVal = firstTile.GetEffectiveValue(okeyTile);
            int lastVal = lastTile.GetEffectiveValue(okeyTile);
            int candVal = candidateTile.GetEffectiveValue(okeyTile);

            if (candVal == firstVal - 1 && firstVal > 1)
            {
                addToStart = true;
                return true;
            }

            if (candVal == lastVal + 1 && lastVal < 13)
            {
                addToStart = false;
                return true;
            }
        }

        return false;
    }

    public static bool CanDrawFromDiscard(
        Player player,
        Tile candidateTile,
        Tile okeyTile,
        List<Meld> tableMelds,
        out string reason)
    {
        reason = "";
        if (player == null || candidateTile == null)
        {
            reason = "Geçersiz işlem.";
            return false;
        }

        if (player.HasOpenedHand)
        {
            if (tableMelds != null)
            {
                foreach (var meld in tableMelds)
                {
                    if (CanProcessTileToMeld(candidateTile, meld, okeyTile, player.HasOpenedPairs, out _))
                    {
                        return true;
                    }
                }
            }

            reason = "Bu taş masadaki hiçbir pere İŞLENEMEDİĞİ için el açmış olsanız dahi yandan taş ALAMAZSINIZ!";
            return false;
        }

        List<Tile> tempHand = new List<Tile>(player.Hand) { candidateTile };

        if (DetectPairs(tempHand, okeyTile, out List<Meld> pairs) && pairs.Count >= MinPairsToOpen)
        {
            bool candidateInPairs = pairs.Exists(p => p.Tiles.Contains(candidateTile));
            if (candidateInPairs)
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

        reason = $"Bu taş ile 101 barajını aşamadığınız veya 5 çift oluşturamadığınız için yandan taş ALAMAZSINIZ! (Mevcut Maksimum Puan: {totalPoints} / Gerekli: 101)";
        return false;
    }

    public static void FindOptimalMelds(List<Tile> hand, Tile okeyTile, out int totalPoints, out List<List<Tile>> bestMelds)
    {
        totalPoints = 0;
        bestMelds = new List<List<Tile>>();
        if (hand == null || hand.Count < 3) return;

        List<List<Tile>> allCandidateMelds = GenerateAllPossibleMelds(hand, okeyTile);

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

        List<Tile> normalTiles = new List<Tile>();
        List<Tile> okeyTiles = new List<Tile>();

        foreach (var t in hand)
        {
            if (IsOkeyTile(t, okeyTile)) okeyTiles.Add(t);
            else normalTiles.Add(t);
        }

        // 1. Grupları bul (Aynı sayı, farklı renkler: 3'lü veya 4'lü + Joker desteği)
        Dictionary<int, Dictionary<TileColor, List<Tile>>> byValAndColor = new Dictionary<int, Dictionary<TileColor, List<Tile>>>();
        foreach (var t in normalTiles)
        {
            int v = t.GetEffectiveValue(okeyTile);
            TileColor c = t.GetEffectiveColor(okeyTile);
            if (!byValAndColor.ContainsKey(v)) byValAndColor[v] = new Dictionary<TileColor, List<Tile>>();
            if (!byValAndColor[v].ContainsKey(c)) byValAndColor[v][c] = new List<Tile>();
            byValAndColor[v][c].Add(t);
        }

        foreach (var kvp in byValAndColor)
        {
            var colMap = kvp.Value;
            List<TileColor> cols = new List<TileColor>(colMap.Keys);

            // Normal 3'lü gruplar (Jokersiz)
            if (cols.Count >= 3)
            {
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
            }

            // Normal 4'lü grup (Jokersiz)
            if (cols.Count == 4)
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

            // Jokerli 3'lü gruplar (2 Renk + 1 Okey)
            if (cols.Count >= 2 && okeyTiles.Count >= 1)
            {
                Tile okey1 = okeyTiles[0];
                for (int i = 0; i < cols.Count; i++)
                {
                    for (int j = i + 1; j < cols.Count; j++)
                    {
                        foreach (var t1 in colMap[cols[i]])
                        {
                            foreach (var t2 in colMap[cols[j]])
                            {
                                candidateMelds.Add(new List<Tile> { t1, t2, okey1 });
                            }
                        }
                    }
                }
            }

            // Jokerli 4'lü gruplar (3 Renk + 1 Okey)
            if (cols.Count >= 3 && okeyTiles.Count >= 1)
            {
                Tile okey1 = okeyTiles[0];
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
                                        candidateMelds.Add(new List<Tile> { t1, t2, t3, okey1 });
                                    }
                                }
                            }
                        }
                    }
                }
            }

            // Çift Jokerli 3'lü gruplar (1 Renk + 2 Okey)
            if (cols.Count >= 1 && okeyTiles.Count >= 2)
            {
                Tile o1 = okeyTiles[0];
                Tile o2 = okeyTiles[1];
                foreach (var col in cols)
                {
                    foreach (var t1 in colMap[col])
                    {
                        candidateMelds.Add(new List<Tile> { t1, o1, o2 });
                    }
                }
            }
        }

        // 2. Serileri bul (Aynı renk, ardışık sayılar + Joker desteği)
        for (int c = 0; c < 4; c++)
        {
            TileColor color = (TileColor)c;
            Dictionary<int, List<Tile>> valMap = new Dictionary<int, List<Tile>>();

            foreach (var t in normalTiles)
            {
                if (t.GetEffectiveColor(okeyTile) == color)
                {
                    int v = t.GetEffectiveValue(okeyTile);
                    if (!valMap.ContainsKey(v)) valMap[v] = new List<Tile>();
                    valMap[v].Add(t);
                }
            }

            if (valMap.Count == 0 && okeyTiles.Count < 3) continue;

            // Olası seri uzunlukları 3..7
            for (int len = 3; len <= 7; len++)
            {
                for (int startV = 1; startV <= (14 - len); startV++)
                {
                    List<int> existingVals = new List<int>();
                    int missingCount = 0;

                    for (int step = 0; step < len; step++)
                    {
                        int targetVal = startV + step;
                        if (valMap.ContainsKey(targetVal) && valMap[targetVal].Count > 0)
                        {
                            existingVals.Add(targetVal);
                        }
                        else
                        {
                            missingCount++;
                        }
                    }

                    if (missingCount <= okeyTiles.Count && existingVals.Count >= 1)
                    {
                        // Kombinasyonları üret
                        List<List<Tile>> combos = new List<List<Tile>> { new List<Tile>() };

                        for (int step = 0; step < len; step++)
                        {
                            int targetVal = startV + step;
                            List<List<Tile>> newCombos = new List<List<Tile>>();

                            if (valMap.ContainsKey(targetVal) && valMap[targetVal].Count > 0)
                            {
                                foreach (var combo in combos)
                                {
                                    foreach (var opt in valMap[targetVal])
                                    {
                                        var extended = new List<Tile>(combo) { opt };
                                        newCombos.Add(extended);
                                    }
                                }
                            }
                            else
                            {
                                // Joker ekle
                                foreach (var combo in combos)
                                {
                                    int usedOkeys = combo.FindAll(t => IsOkeyTile(t, okeyTile)).Count;
                                    if (usedOkeys < okeyTiles.Count)
                                    {
                                        var extended = new List<Tile>(combo) { okeyTiles[usedOkeys] };
                                        newCombos.Add(extended);
                                    }
                                }
                            }
                            combos = newCombos;
                        }

                        foreach (var completedSeq in combos)
                        {
                            if (completedSeq.Count == len)
                            {
                                candidateMelds.Add(completedSeq);
                            }
                        }
                    }
                }
            }
        }

        return candidateMelds;
    }
}
