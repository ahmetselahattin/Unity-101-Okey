using System;
using UnityEngine;

public class TurnManager : MonoBehaviour
{
    public int CurrentPlayerIndex { get; private set; } = 0;
    public bool IsFirstTurn { get; private set; } = true;

    public event Action<int> OnTurnChanged;

    public void ResetTurns()
    {
        CurrentPlayerIndex = 0;
        IsFirstTurn = true;
    }

    public void StartTurn()
    {
        Debug.Log($"[TurnManager] Sra u oyuncuda: {CurrentPlayerIndex}");
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
