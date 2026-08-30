using System;
using System.Collections;
using UnityEngine;

public class BotController : MonoBehaviour
{
    public float DecisionDelay = 1.2f;
    public float DiscardDelay = 0.8f;

    public IEnumerator ExecuteBotTurn(Player botPlayer, DeckManager deck, Action<Tile> onBotDiscarded, Action onTurnComplete)
    {
        if (botPlayer == null || deck == null)
        {
            onTurnComplete?.Invoke();
            yield break;
        }

        yield return new WaitForSeconds(DecisionDelay);

        // 1. Desteden taş çek
        if (deck.RemainingCount > 0)
        {
            Tile drawnTile = deck.DrawTile();
            botPlayer.AddTile(drawnTile);
            Debug.Log($"[BotController] Bot {botPlayer.SeatIndex} desteden taş çekti.");
        }

        yield return new WaitForSeconds(DiscardDelay);

        // 2. Elden taş at (Stratejik veya rastgele)
        Tile discardedTile = null;
        if (botPlayer.Hand.Count > 0)
        {
            int randomIndex = UnityEngine.Random.Range(0, botPlayer.Hand.Count);
            discardedTile = botPlayer.Hand[randomIndex];
            botPlayer.RemoveTile(discardedTile);
            Debug.Log($"[BotController] Bot {botPlayer.SeatIndex} bir taş attı: {discardedTile}");
        }

        onBotDiscarded?.Invoke(discardedTile);
        onTurnComplete?.Invoke();
    }
}
