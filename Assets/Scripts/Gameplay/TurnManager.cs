using System;
using UnityEngine;

public class TurnManager : MonoBehaviour
{
    public int CurrentPlayerIndex { get; set; } = 0;
    public bool IsFirstTurn { get; set; } = true;

    public event Action<int> OnTurnChanged;

    public void ResetTurns()
    {
        CurrentPlayerIndex = 0;
        IsFirstTurn = true;
    }

    public void SetTurn(int activeIndex, bool isFirst)
    {
        CurrentPlayerIndex = activeIndex;
        IsFirstTurn = isFirst;
    }

    public void StartTurn()
    {
        Debug.Log($"[TurnManager] Sıra şu oyuncuda: {CurrentPlayerIndex}");
        OnTurnChanged?.Invoke(CurrentPlayerIndex);
    }

    public void NextTurn()
    {
        if (CurrentPlayerIndex == 0 && IsFirstTurn)
        {
            IsFirstTurn = false;
        }

        CurrentPlayerIndex = (CurrentPlayerIndex + 1) % 4;
        StartTurn();
    }
}
