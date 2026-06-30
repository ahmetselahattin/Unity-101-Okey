
using UnityEngine;
using System.Collections;
using System.Collections.Generic; // Listeler için gerekli

public class GameManager : MonoBehaviour
{
    // --- SINGLETON YAPISI ---
    public static GameManager Instance;
    // Oyunun ilk turu olup olmadýðýný takip eden bayrak
    public bool isFirstTurn = true;
    public Player[] players = new Player[4];
    DeckManager deckManager;
    public UIManager uiManager;

    // O anki oyuncunun sýrasýný tutan deðiþken
    public int currentPlayerIndex = 0;

    void Awake()
    {
        // GameManager'a her yerden kolayca ulaþabilmemizi saðlar
        if (Instance == null) { Instance = this; }
    }

    void Start()
    {
        for (int i = 0; i < 4; i++)
        {
            players[i] = new Player();
        }

        deckManager = new DeckManager();
        deckManager.CreateDeck();
        deckManager.Shuffle();
        deckManager.DetermineOkey();

        // Taþlarý daðýt
        DistributeTiles();

        // Ýlk oyuncunun elini ekrana çizdir
        uiManager.DrawPlayerHand(players[0].Hand);

        // Oyunu baþlattýðýmýzda ilk turu baþlatalým (0 numara, yani biz baþlýyoruz)
        StartTurn();
    }

    // --- SENÝN ÖNCEDEN YAZDIÐIN TAÞ DAÐITMA FONKSÝYONU ---
    public void DistributeTiles()
    {
        for (int i = 0; i < 4; i++)
        {
            //first player get 22 tile
            if (i == 0)
            {
                for (int j = 0; j < 22; j++)
                {
                    //we add tile and remove tile from list for first player
                    players[i].AddTile(deckManager.AllTiles[0]);
                    deckManager.AllTiles.RemoveAt(0);
                }
            }
            else
            {
                for (int j = 0; j < 21; j++)
                {
                    //we add tile and remove tile from list for other player
                    players[i].AddTile(deckManager.AllTiles[0]);
                    deckManager.AllTiles.RemoveAt(0);
                }
            }
        }
    }

    // --- DESTE VE SIRALAMA FONKSÝYONLARI ---
    public void DrawTileFromDeck()
    {
        if (deckManager.AllTiles.Count > 0)
        {
            Tile drawnTile = deckManager.AllTiles[0];
            players[0].AddTile(drawnTile);
            deckManager.AllTiles.RemoveAt(0);
            uiManager.AddSingleTileToHand(drawnTile);

            // Taþý çektik, artýk butona basamayýz!
            uiManager.SetDeckButtonState(false);
        }
    }

    public void SortHand(List<Tile> handToSort)
    {
        handToSort.Sort((t1, t2) =>
        {
            int colorComparison = t1.Color.CompareTo(t2.Color);
            if (colorComparison != 0)
            {
                return colorComparison;
            }
            return t1.TileValue.CompareTo(t2.TileValue);
        });
    }

    public void OnAutoSortClicked()
    {
        SortHand(players[0].Hand);
        uiManager.RefreshHand(players[0].Hand);
    }

    // --- YENÝ EKLENEN TUR SÝSTEMÝ KODLARI ---
    public void StartTurn()
    {
        Debug.Log("Sýra þu oyuncuda: " + currentPlayerIndex);

        if (currentPlayerIndex == 0)
        {
            // BÝZÝM SIRAMIZ
            if (isFirstTurn)
            {
                Debug.Log("Senin sýran! Ýlk elde taþ çekmeden doðrudan at.");
                uiManager.SetDeckButtonState(false); // Butonu kilitledik!
                isFirstTurn = false;
            }
            else
            {
                Debug.Log("Senin sýran! Önce desteden taþ çek.");
                uiManager.SetDeckButtonState(true); // Butonu açtýk!
            }
        }
        else
        {
            // BOTLARIN SIRASI: Bizim taþ çekmememiz lazým, butonu kilitle!
            uiManager.SetDeckButtonState(false);
            StartCoroutine(PlayAITurn());
        }
    }
    public void EndTurn()
    {
        // Sýrayý bir sonrakine geçir (0 -> 1 -> 2 -> 3 -> 0)
        currentPlayerIndex = (currentPlayerIndex + 1) % 4;
        StartTurn();
    }

    IEnumerator PlayAITurn()
    {
        // Bot 1.5 saniye düþünsün
        yield return new WaitForSeconds(1.5f);

        // 1. Bot desteden taþ çeksin
        if (deckManager.AllTiles.Count > 0)
        {
            Tile drawnTile = deckManager.AllTiles[0];
            players[currentPlayerIndex].AddTile(drawnTile);
            deckManager.AllTiles.RemoveAt(0);
            Debug.Log("Bot " + currentPlayerIndex + " desteden taþ çekti.");
        }

        // 1 saniye daha düþünsün
        yield return new WaitForSeconds(1.0f);

        // 2. Bot elinden rastgele bir taþ atsýn (Elinde taþ varsa)
        if (players[currentPlayerIndex].Hand.Count > 0)
        {
            int randomDiscardIndex = Random.Range(0, players[currentPlayerIndex].Hand.Count);
            Tile discardedTile = players[currentPlayerIndex].Hand[randomDiscardIndex];
            players[currentPlayerIndex].Hand.RemoveAt(randomDiscardIndex);

            Debug.Log("Bot " + currentPlayerIndex + " bir taþ attý: " + discardedTile.TileValue);
        }

        // 3. Ýþini bitirdi, sýrayý devretsin
        EndTurn();
    }
    // Verilen listenin geçerli bir GRUP peri (Ayný sayý, farklý renk) olup olmadýðýný kontrol eder.
    public bool CheckGroupPer(List<Tile> tileList)
    {
        SortHand(tileList);

        if (tileList.Count == 3 || tileList.Count == 4)
        {
            for (int i = 1; i < tileList.Count; i++) 
            {
                // Sýralý listede ayný renkler varsa mutlaka yan yana düþecektir.
                if (tileList[i].Color == tileList[i - 1].Color)
                {
                    return false;
                }

                // Sayýlarýn hepsi ilk sayýyla ayný olmak zorunda, bu mantýðýn doðru.
                if (tileList[0].TileValue != tileList[i].TileValue)
                {
                    return false;
                }
            }
            return true;
        }

        return false;
    }

    public bool CheckSequencePer(List<Tile> tileList)
    {
        SortHand(tileList);

        if (tileList.Count >= 3)
        {
            for (int i = 1; i < tileList.Count; i++)
            {
                if (tileList[0].Color != tileList[i].Color)
                {
                    return false;
                }

                if (tileList[0].TileValue + i != tileList[i].TileValue)
                {
                    return false;
                }
            }
            return true;
        }

        return false;
    }
    // Parametre olarak "Geçerli perlerin bir listesini" alýyor. 
    // Örneðin: { [Kýrmýzý 7, Mavi 7, Siyah 7], [Mavi 4, Mavi 5, Mavi 6] }
        public int CalculateTotalPoints(List<List<Tile>> allMeldsToOpen)
        {
            int totalPoints = 0;

            for (int i = 0; i < allMeldsToOpen.Count;i++) 
            {
                for (int j = 0; j < allMeldsToOpen[i].Count; j++)
                {
                    totalPoints += allMeldsToOpen[i][j].TileValue;
                }
            }
            return totalPoints;
       }
    // Verilen elde en az 5 adet geçerli çift (tamamen ayný iki taþ) olup olmadýðýný kontrol eder.
    public bool CheckForPairs(List<Tile> hand)
    {
        SortHand(hand);
        int pairCount = 0; // Bulduðumuz çift sayýsýný tutacaðýmýz deðiþken
        for (int i = 0; i < hand.Count-1; i++) 
        {
            if (hand[i].Color == hand[i+1].Color && hand[i].TileValue == hand[i + 1].TileValue) 
            {
                pairCount++;
                i++;
            }
        }
        if (pairCount < 5) 
        {
            return false;
        }
        else
        {
            return true;
        }
    }
}