using System;
using System.Collections.Generic;

[Serializable]
public class Player
{
    public int SeatIndex;
    public string NickName;
    public bool IsAI;
    public bool HasOpenedHand;
    public bool HasOpenedPairs;
    public bool HasDrawnFromDiscard;
    public int TotalScore;

    public List<Tile> Hand { get; private set; }
    public List<Meld> OpenedMelds { get; private set; }

    public int PairCount => OpenedMelds.FindAll(m => m.Type == MeldType.Pair).Count;

    public Player(int seatIndex = 0, string nickName = "Player", bool isAI = false)
    {
        SeatIndex = seatIndex;
        NickName = nickName;
        IsAI = isAI;
        HasOpenedHand = false;
        HasOpenedPairs = false;
        HasDrawnFromDiscard = false;
        TotalScore = 0;
        Hand = new List<Tile>();
        OpenedMelds = new List<Meld>();
    }

    public void AddTile(Tile newTile)
    {
        if (newTile != null)
        {
            Hand.Add(newTile);
        }
    }

    public bool RemoveTile(Tile tile)
    {
        if (tile == null) return false;
        int index = Hand.FindIndex(t => t == tile || t.IsSame(tile));
        if (index >= 0)
        {
            Hand.RemoveAt(index);
            return true;
        }
        return false;
    }

    public void ClearHand()
    {
        Hand.Clear();
        OpenedMelds.Clear();
        HasOpenedHand = false;
        HasOpenedPairs = false;
        HasDrawnFromDiscard = false;
    }
}
