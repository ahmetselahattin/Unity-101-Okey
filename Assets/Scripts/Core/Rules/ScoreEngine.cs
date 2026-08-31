using System;
using System.Collections.Generic;

[Serializable]
public class PlayerScoreInfo
{
    public int SeatIndex;
    public string NickName;
    public int RoundPenalty;
    public int CumulativeScore;
    public bool HasOpenedHand;
    public bool IsWinner;
    public string SummaryText;
}

public static class ScoreEngine
{
    public static List<PlayerScoreInfo> CalculateScores(Player[] players, Player winner, FinishType finishType, Tile okeyTile)
    {
        List<PlayerScoreInfo> results = new List<PlayerScoreInfo>();
        if (players == null) return results;

        bool isDoublePenalty = (finishType == FinishType.Okey || finishType == FinishType.Pairs);

        for (int i = 0; i < players.Length; i++)
        {
            Player p = players[i];
            if (p == null) continue;

            PlayerScoreInfo info = new PlayerScoreInfo
            {
                SeatIndex = p.SeatIndex,
                NickName = p.NickName,
                HasOpenedHand = p.HasOpenedHand,
                IsWinner = (p == winner)
            };

            if (info.IsWinner)
            {
                // Biten oyuncu eksi puan alır
                info.RoundPenalty = isDoublePenalty ? -202 : -101;
                info.SummaryText = isDoublePenalty ? "BİTTİ (2 Kat Bonus: -202)" : "BİTTİ (-101)";
            }
            else
            {
                if (!p.HasOpenedHand)
                {
                    // Elini hiç açamamış oyuncu sabit baraj cezası alır
                    info.RoundPenalty = isDoublePenalty ? 202 : 101;
                    info.SummaryText = isDoublePenalty ? "El Açamadı (2 Kat Ceza: +202)" : "El Açamadı (+101)";
                }
                else
                {
                    // Elini açmış oyuncu: Istakasında kalan taşların sayı toplamı
                    int handSum = 0;
                    bool hasUnusedOkey = false;

                    foreach (var tile in p.Hand)
                    {
                        if (OkeyRuleEngine.IsOkeyTile(tile, okeyTile))
                        {
                            hasUnusedOkey = true;
                            handSum += 101; // Elde kalan Okey taşı 101 ceza puanı ekler
                        }
                        else
                        {
                            handSum += tile.GetEffectiveValue(okeyTile);
                        }
                    }

                    // Eğer biten okeyle veya çiftten bitmişse kalan taş cezası 2 katına çıkar
                    if (isDoublePenalty)
                    {
                        handSum *= 2;
                    }

                    info.RoundPenalty = handSum;
                    info.SummaryText = hasUnusedOkey 
                        ? $"Elde Kalan Taşlar + Okey Cezası (+{handSum})" 
                        : $"Elde Kalan Taşlar (+{handSum})";
                }
            }

            p.TotalScore += info.RoundPenalty;
            info.CumulativeScore = p.TotalScore;
            results.Add(info);
        }

        return results;
    }
}
