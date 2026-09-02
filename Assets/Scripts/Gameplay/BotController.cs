using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BotController : MonoBehaviour
{
    [Header("Bot Düşünme Süreleri")]
    public float DecisionDelay = 0.9f;
    public float ActionDelay = 0.6f;

    public IEnumerator ExecuteBotTurn(
        Player botPlayer,
        DeckManager deck,
        Tile leftDiscardTile,
        List<Meld> tableMelds,
        Action onTableUpdated,
        Action<Tile> onBotDiscarded,
        Action onTurnComplete)
    {
        if (botPlayer == null || deck == null)
        {
            onTurnComplete?.Invoke();
            yield break;
        }

        yield return new WaitForSeconds(DecisionDelay);

        Tile okeyTile = deck.OkeyTile;

        // ── 1. TAŞ ÇEKME KARARI ──
        bool drewFromLeft = false;
        if (leftDiscardTile != null && OkeyRuleEngine.CanDrawFromDiscard(botPlayer, leftDiscardTile, okeyTile, tableMelds, out _))
        {
            botPlayer.AddTile(leftDiscardTile);
            botPlayer.HasDrawnFromDiscard = true;
            drewFromLeft = true;
            Debug.Log($"[BotController] Bot {botPlayer.SeatIndex} solundaki oyuncudan yandan taş aldı: {leftDiscardTile}");
        }
        else
        {
            if (deck.RemainingCount > 0)
            {
                Tile drawn = deck.DrawTile();
                botPlayer.AddTile(drawn);
                Debug.Log($"[BotController] Bot {botPlayer.SeatIndex} desteden taş çekti.");
            }
        }

        yield return new WaitForSeconds(ActionDelay);

        // ── 2. EL AÇMA KARARI (101 BARAJI VEYA 5+ ÇİFT) ──
        if (!botPlayer.HasOpenedHand)
        {
            // 2a. 101 Seri/Grup per açma kontrolü
            OkeyRuleEngine.FindOptimalMelds(botPlayer.Hand, okeyTile, out int totalPoints, out List<List<Tile>> bestMelds);

            int meldsTilesCount = 0;
            if (bestMelds != null)
            {
                foreach (var g in bestMelds) meldsTilesCount += g.Count;
            }

            // Bot da elinde taş atmak için en az 1 taş bırakmak zorundadır
            while (bestMelds != null && bestMelds.Count > 0 && (botPlayer.Hand.Count - meldsTilesCount < 1))
            {
                var removed = bestMelds[bestMelds.Count - 1];
                meldsTilesCount -= removed.Count;
                bestMelds.RemoveAt(bestMelds.Count - 1);
            }

            int currentScore = OkeyRuleEngine.CalculateTotalPoints(bestMelds, okeyTile);

            if (currentScore >= OkeyRuleEngine.OpenHandThreshold && bestMelds.Count > 0 && (botPlayer.Hand.Count - meldsTilesCount >= 1))
            {
                Debug.Log($"[BotController] Bot {botPlayer.SeatIndex} 101 barajını aştı ({currentScore} Puan), elini masaya açıyor!");

                foreach (var tileGroup in bestMelds)
                {
                    MeldType type = OkeyRuleEngine.CheckGroupPer(tileGroup, okeyTile) ? MeldType.Group : MeldType.Sequence;
                    int points = OkeyRuleEngine.CalculateMeldPoints(tileGroup, okeyTile);
                    Meld m = new Meld(tileGroup, type, points);
                    tableMelds.Add(m);
                    botPlayer.OpenedMelds.Add(m);

                    foreach (var t in tileGroup)
                    {
                        botPlayer.RemoveTile(t);
                    }
                }

                botPlayer.HasOpenedHand = true;
                botPlayer.HasOpenedPairs = false;
                botPlayer.HasDrawnFromDiscard = false;
                onTableUpdated?.Invoke();

                yield return new WaitForSeconds(ActionDelay);
            }
            else
            {
                // 2b. 5+ Çift açma kontrolü
                if (OkeyRuleEngine.DetectPairs(botPlayer.Hand, okeyTile, out List<Meld> pairs))
                {
                    while (pairs != null && pairs.Count > 0 && (botPlayer.Hand.Count - (pairs.Count * 2) < 1))
                    {
                        pairs.RemoveAt(pairs.Count - 1);
                    }

                    if (pairs != null && pairs.Count >= OkeyRuleEngine.MinPairsToOpen)
                    {
                        Debug.Log($"[BotController] Bot {botPlayer.SeatIndex} {pairs.Count} çift ile elini masaya açıyor!");

                        foreach (var pairMeld in pairs)
                        {
                            tableMelds.Add(pairMeld);
                            botPlayer.OpenedMelds.Add(pairMeld);

                            foreach (var t in pairMeld.Tiles)
                            {
                                botPlayer.RemoveTile(t);
                            }
                        }

                        botPlayer.HasOpenedHand = true;
                        botPlayer.HasOpenedPairs = true;
                        botPlayer.HasDrawnFromDiscard = false;
                        onTableUpdated?.Invoke();

                        yield return new WaitForSeconds(ActionDelay);
                    }
                }
            }
        }
        else
        {
            // Daha önce açmış bot: Elinde kalan taşlardan yeni perler oluşursa masaya ekler
            OkeyRuleEngine.FindOptimalMelds(botPlayer.Hand, okeyTile, out int addPoints, out List<List<Tile>> addMelds);
            int addTilesCount = 0;
            if (addMelds != null)
            {
                foreach (var g in addMelds) addTilesCount += g.Count;
            }

            while (addMelds != null && addMelds.Count > 0 && (botPlayer.Hand.Count - addTilesCount < 1))
            {
                var removed = addMelds[addMelds.Count - 1];
                addTilesCount -= removed.Count;
                addMelds.RemoveAt(addMelds.Count - 1);
            }

            if (addMelds != null && addMelds.Count > 0 && (botPlayer.Hand.Count - addTilesCount >= 1))
            {
                Debug.Log($"[BotController] Bot {botPlayer.SeatIndex} elindeki yeni perleri masaya ekliyor ({addPoints} Puan)!");

                foreach (var tileGroup in addMelds)
                {
                    MeldType type = OkeyRuleEngine.CheckGroupPer(tileGroup, okeyTile) ? MeldType.Group : MeldType.Sequence;
                    int points = OkeyRuleEngine.CalculateMeldPoints(tileGroup, okeyTile);
                    Meld m = new Meld(tileGroup, type, points);
                    tableMelds.Add(m);
                    botPlayer.OpenedMelds.Add(m);

                    foreach (var t in tileGroup)
                    {
                        botPlayer.RemoveTile(t);
                    }
                }

                botPlayer.HasDrawnFromDiscard = false;
                onTableUpdated?.Invoke();

                yield return new WaitForSeconds(ActionDelay);
            }
        }

        // ── 3. TAŞ İŞLEME KARARI (AÇILMIŞ PERLERE TAŞ EKLEME) ──
        if (botPlayer.HasOpenedHand && tableMelds.Count > 0)
        {
            bool processedAny = false;
            bool keepChecking = true;

            while (keepChecking && botPlayer.Hand.Count > 1) // Elinde en az 1 taş bırakmak üzere
            {
                keepChecking = false;

                for (int i = 0; i < botPlayer.Hand.Count; i++)
                {
                    Tile candidate = botPlayer.Hand[i];

                    // Okey taşını elden çıkarmamak için koru
                    if (OkeyRuleEngine.IsOkeyTile(candidate, okeyTile) && botPlayer.Hand.Count > 2)
                        continue;

                    for (int m = 0; m < tableMelds.Count; m++)
                    {
                        Meld targetMeld = tableMelds[m];

                        if (OkeyRuleEngine.CanProcessTileToMeld(candidate, targetMeld, okeyTile, botPlayer.HasOpenedPairs, out bool addToStart))
                        {
                            if (addToStart) targetMeld.Tiles.Insert(0, candidate);
                            else targetMeld.Tiles.Add(candidate);

                            botPlayer.RemoveTile(candidate);
                            botPlayer.HasDrawnFromDiscard = false;
                            processedAny = true;
                            keepChecking = true;
                            Debug.Log($"[BotController] Bot {botPlayer.SeatIndex} masadaki pere taş işledi: {candidate}");
                            break;
                        }
                    }

                    if (keepChecking) break;
                }
            }

            if (processedAny)
            {
                onTableUpdated?.Invoke();
                yield return new WaitForSeconds(ActionDelay);
            }
        }

        // ── 4. AKILLI TAŞ ATMA KARARI ──
        Tile discardedTile = ChooseSmartDiscardTile(botPlayer.Hand, okeyTile);

        if (discardedTile != null)
        {
            botPlayer.RemoveTile(discardedTile);
            Debug.Log($"[BotController] Bot {botPlayer.SeatIndex} akıllıca bir taş attı: {discardedTile}");
        }

        onBotDiscarded?.Invoke(discardedTile);
        onTurnComplete?.Invoke();
    }

    /// <summary>
    /// Botun elindeki taşları analiz ederek elden çıkarılacak en işe yaramaz taşı belirler.
    /// </summary>
    private Tile ChooseSmartDiscardTile(List<Tile> hand, Tile okeyTile)
    {
        if (hand == null || hand.Count == 0) return null;
        if (hand.Count == 1) return hand[0];

        Tile bestDiscard = null;
        float highestUselessScore = float.MinValue;

        for (int i = 0; i < hand.Count; i++)
        {
            Tile candidate = hand[i];
            float score = CalculateUselessScore(candidate, hand, okeyTile);

            if (score > highestUselessScore)
            {
                highestUselessScore = score;
                bestDiscard = candidate;
            }
        }

        return bestDiscard ?? hand[UnityEngine.Random.Range(0, hand.Count)];
    }

    private float CalculateUselessScore(Tile candidate, List<Tile> hand, Tile okeyTile)
    {
        // 1. Gerçek Okey asla atılmaz (çok yüksek ceza puanı)
        if (OkeyRuleEngine.IsOkeyTile(candidate, okeyTile))
        {
            return -10000f;
        }

        float uselessScore = candidate.GetEffectiveValue(okeyTile);

        // 2. Çift oluşturuyor mu kontrol et
        int sameCount = 0;
        foreach (var t in hand)
        {
            if (t != candidate && t.GetEffectiveColor(okeyTile) == candidate.GetEffectiveColor(okeyTile) &&
                t.GetEffectiveValue(okeyTile) == candidate.GetEffectiveValue(okeyTile))
            {
                sameCount++;
            }
        }
        if (sameCount > 0) uselessScore -= 100f; // Çifti bozma!

        // 3. Seri oluşturma potansiyeli var mı (Komşu sayılar)
        int neighbors = 0;
        foreach (var t in hand)
        {
            if (t != candidate && t.GetEffectiveColor(okeyTile) == candidate.GetEffectiveColor(okeyTile))
            {
                int diff = Mathf.Abs(t.GetEffectiveValue(okeyTile) - candidate.GetEffectiveValue(okeyTile));
                if (diff == 1 || diff == 2)
                {
                    neighbors++;
                }
            }
        }
        if (neighbors > 0) uselessScore -= (neighbors * 40f); // Seriyi bozma!

        // 4. Grup oluşturma potansiyeli var mı (Aynı sayı, farklı renkler)
        int sameValueDiffColor = 0;
        foreach (var t in hand)
        {
            if (t != candidate && t.GetEffectiveValue(okeyTile) == candidate.GetEffectiveValue(okeyTile) &&
                t.GetEffectiveColor(okeyTile) != candidate.GetEffectiveColor(okeyTile))
            {
                sameValueDiffColor++;
            }
        }
        if (sameValueDiffColor > 0) uselessScore -= (sameValueDiffColor * 35f); // Grubu bozma!

        return uselessScore;
    }
}
