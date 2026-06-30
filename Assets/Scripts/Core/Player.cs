using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
public class Player
{
    public List<Tile> Hand = new List<Tile>();
    public void AddTile(Tile newTile) 
    {
        Hand.Add(newTile);
    }



}
