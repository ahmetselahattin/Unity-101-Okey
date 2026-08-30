using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Test Ayarları")]
    public bool testModuAktif = false;

    [Header("Oyun İçi Objeler")]
    public GameObject inGameUI;
    public GameObject centerStone;

    [Header("UI ve Alt Yöneticiler")]
    public UIManager uiManager;
    public TurnManager turnManager;
    public BotController botController;

    [Header("Oyuncu ve Deste Durumu")]
    public Player[] players = new Player[4];
    public DeckManager deckManager;

    [Header("Masa Durumu ve Kurallar")]
    public List<Meld> tableMelds = new List<Meld>();
    public Tile lastDiscardedTileByLeftPlayer = null;
    public bool hasDrawnTileThisTurn = false;

    public int currentPlayerIndex => turnManager != null ? turnManager.CurrentPlayerIndex : 0;
    public bool isFirstTurn => turnManager != null ? turnManager.IsFirstTurn : true;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }

        if (turnManager == null)
        {
            turnManager = GetComponent<TurnManager>();
            if (turnManager == null) turnManager = gameObject.AddComponent<TurnManager>();
        }

        if (botController == null)
        {
            botController = GetComponent<BotController>();
            if (botController == null) botController = gameObject.AddComponent<BotController>();
        }
    }

    private void Start()
    {
        if (inGameUI != null) inGameUI.SetActive(false);
        if (centerStone != null) centerStone.SetActive(false);

        if (turnManager != null)
        {
            turnManager.OnTurnChanged += HandleTurnChanged;
        }
    }

    private void OnDestroy()
    {
        if (turnManager != null)
        {
            turnManager.OnTurnChanged -= HandleTurnChanged;
        }
    }

    public void OyunuBaslat()
    {
        Debug.Log("[GameManager] Oyun Başlıyor! Taşlar dağıtılıyor...");

        if (inGameUI != null) inGameUI.SetActive(true);
        if (centerStone != null) centerStone.SetActive(true);

        tableMelds.Clear();
        lastDiscardedTileByLeftPlayer = null;
        hasDrawnTileThisTurn = false;

        for (int i = 0; i < 4; i++)
        {
            players[i] = new Player(i, i == 0 ? "Sen" : $"Bot_{i}", i != 0);
        }

        deckManager = new DeckManager();
        deckManager.CreateDeck();
        deckManager.Shuffle();
        deckManager.DetermineOkey();

        if (uiManager != null && deckManager.Gosterge != null)
        {
            uiManager.GostergeyiEkranaYansit(deckManager.Gosterge);
        }

        DistributeTiles();

        if (uiManager != null)
        {
            uiManager.DrawPlayerHand(players[0].Hand);
        }

        if (turnManager != null)
        {
            turnManager.ResetTurns();
            turnManager.StartTurn();
        }
    }

    public void DistributeTiles()
    {
        if (deckManager == null) return;

        for (int i = 0; i < 4; i++)
        {
            int tileCount = (i == 0) ? 22 : 21;
            for (int j = 0; j < tileCount; j++)
            {
                Tile tile = deckManager.DrawTile();
                if (tile != null)
                {
                    players[i].AddTile(tile);
                }
            }
        }
    }

    public void DrawTileFromDeck()
    {
        if (hasDrawnTileThisTurn)
        {
            Debug.LogWarning("[GameManager] Bu tur zaten taş çektiniz!");
            return;
        }

        if (deckManager != null && deckManager.RemainingCount > 0)
        {
            Tile drawnTile = deckManager.DrawTile();
            players[0].AddTile(drawnTile);
            hasDrawnTileThisTurn = true;

            if (uiManager != null)
            {
                uiManager.AddSingleTileToHand(drawnTile);
                uiManager.SetDeckButtonState(false);
                uiManager.SetLeftDiscardButtonState(false);
            }

            Debug.Log($"[GameManager] Desteden taş çekildi: {drawnTile}");
        }
    }

    public void DrawTileFromLeftDiscard()
    {
        if (hasDrawnTileThisTurn)
        {
            Debug.LogWarning("[GameManager] Bu tur zaten taş çektiniz!");
            return;
        }

        if (lastDiscardedTileByLeftPlayer == null)
        {
            Debug.LogWarning("[GameManager] Yandan alınacak taş yok!");
            return;
        }

        Tile drawnTile = lastDiscardedTileByLeftPlayer;
        lastDiscardedTileByLeftPlayer = null;

        players[0].AddTile(drawnTile);
        players[0].HasDrawnFromDiscard = true;
        hasDrawnTileThisTurn = true;

        if (uiManager != null)
        {
            uiManager.AddSingleTileToHand(drawnTile);
            uiManager.SetLeftDiscardTile(null, false);
            uiManager.SetDeckButtonState(false);
            uiManager.SetLeftDiscardButtonState(false);
        }

        Debug.Log($"[GameManager] Yandan taş alındı: {drawnTile}. (DİKKAT: Bu tur elinizi açmak zorundasınız!)");
    }

    public void OnAutoSortClicked()
    {
        if (players[0] != null && deckManager != null)
        {
            HandSorter.SortByColorAndValue(players[0].Hand, deckManager.OkeyTile);
            if (uiManager != null)
            {
                uiManager.RefreshHand(players[0].Hand);
            }
        }
    }

    /// <summary>
    /// 101 Barajı ve Seri/Grup perleri doğrulayarak el açma işlemi.
    /// </summary>
    public void OnOpenHandClicked()
    {
        if (uiManager == null || deckManager == null) return;

        List<List<Tile>> rawMelds = uiManager.GetMeldsFromIstaka();

        if (OkeyRuleEngine.ValidateOpenHand(rawMelds, deckManager.OkeyTile, out int totalPoints, out string error))
        {
            Debug.Log($"[GameManager] Tebrikler! 101 barajı aşıldı (Toplam Puan: {totalPoints}), el masaya açılıyor.");

            List<Meld> newOpenedMelds = new List<Meld>();
            foreach (var tileGroup in rawMelds)
            {
                MeldType type = OkeyRuleEngine.CheckGroupPer(tileGroup, deckManager.OkeyTile) ? MeldType.Group : MeldType.Sequence;
                int points = OkeyRuleEngine.CalculateMeldPoints(tileGroup, deckManager.OkeyTile);
                Meld m = new Meld(tileGroup, type, points);
                newOpenedMelds.Add(m);
                tableMelds.Add(m);
                players[0].OpenedMelds.Add(m);
            }

            players[0].HasOpenedHand = true;
            players[0].HasOpenedPairs = false;
            players[0].HasDrawnFromDiscard = false;

            foreach (var m in newOpenedMelds)
            {
                foreach (var tile in m.Tiles)
                {
                    players[0].RemoveTile(tile);
                }
            }

            uiManager.RefreshHand(players[0].Hand);
            uiManager.DrawOpenedMeldsOnTable(tableMelds);
        }
        else
        {
            Debug.LogWarning($"[GameManager] El açma başarısız: {error}");
        }
    }

    /// <summary>
    /// En az 5 çift (10 taş) ile çift açma işlemi.
    /// </summary>
    public void OnOpenPairsClicked()
    {
        if (deckManager == null || players[0] == null) return;

        if (OkeyRuleEngine.ValidateOpenPairs(players[0].Hand, deckManager.OkeyTile, out List<Meld> pairsToOpen, out string error))
        {
            Debug.Log($"[GameManager] Tebrikler! {pairsToOpen.Count} çift ile el masaya açılıyor.");

            foreach (var pairMeld in pairsToOpen)
            {
                tableMelds.Add(pairMeld);
                players[0].OpenedMelds.Add(pairMeld);

                foreach (var tile in pairMeld.Tiles)
                {
                    players[0].RemoveTile(tile);
                }
            }

            players[0].HasOpenedHand = true;
            players[0].HasOpenedPairs = true;
            players[0].HasDrawnFromDiscard = false;

            if (uiManager != null)
            {
                uiManager.RefreshHand(players[0].Hand);
                uiManager.DrawOpenedMeldsOnTable(tableMelds);
            }
        }
        else
        {
            Debug.LogWarning($"[GameManager] Çift açma başarısız: {error}");
        }
    }

    /// <summary>
    /// Masada daha önce açılmış olan bir pere taş işleme (ekleme) işlemi.
    /// </summary>
    public bool ProcessTileToTable(Tile tile, int meldIndex)
    {
        if (!players[0].HasOpenedHand)
        {
            Debug.LogWarning("[GameManager] Masaya taş işleyebilmek için önce elinizi açmış olmanız gerekir!");
            return false;
        }

        if (meldIndex < 0 || meldIndex >= tableMelds.Count || tile == null)
            return false;

        Meld targetMeld = tableMelds[meldIndex];

        if (OkeyRuleEngine.CanProcessTileToMeld(tile, targetMeld, deckManager.OkeyTile, players[0].HasOpenedPairs, out bool addToStart))
        {
            if (addToStart)
            {
                targetMeld.Tiles.Insert(0, tile);
            }
            else
            {
                targetMeld.Tiles.Add(tile);
            }

            players[0].RemoveTile(tile);

            if (uiManager != null)
            {
                uiManager.RefreshHand(players[0].Hand);
                uiManager.DrawOpenedMeldsOnTable(tableMelds);
            }

            Debug.Log($"[GameManager] {tile} taşı masadaki pere başarıyla işlendi!");
            return true;
        }

        Debug.LogWarning($"[GameManager] {tile} taşı bu pere işlenemez!");
        return false;
    }

    public void OnPlayerDiscardTile(Tile discardedTile)
    {
        if (players[0].HasDrawnFromDiscard)
        {
            Debug.LogError("[GameManager] KURAL İHLALİ: Yandan taş aldığınız için bu tur elinizi açmak zorundasınız!");
            if (uiManager != null)
            {
                uiManager.RefreshHand(players[0].Hand);
            }
            return;
        }

        if (discardedTile != null && players[0] != null)
        {
            players[0].RemoveTile(discardedTile);
            Debug.Log($"[GameManager] Oyuncu bir taş attı: {discardedTile}");
        }

        hasDrawnTileThisTurn = false;
        EndTurn();
    }

    public void EndTurn()
    {
        if (turnManager != null)
        {
            turnManager.NextTurn();
        }
    }

    private void HandleTurnChanged(int activePlayerIndex)
    {
        if (activePlayerIndex == 0)
        {
            hasDrawnTileThisTurn = false;

            if (isFirstTurn)
            {
                Debug.Log("[GameManager] Senin sıran! İlk elde taş çekmeden doğrudan at.");
                if (uiManager != null)
                {
                    uiManager.SetDeckButtonState(false);
                    uiManager.SetLeftDiscardButtonState(false);
                }
            }
            else
            {
                Debug.Log("[GameManager] Senin sıran! Desteden veya sol oyuncunun attığı taştan çek.");
                if (uiManager != null)
                {
                    uiManager.SetDeckButtonState(true);
                    bool canDrawLeft = (lastDiscardedTileByLeftPlayer != null);
                    uiManager.SetLeftDiscardButtonState(canDrawLeft);
                }
            }
        }
        else
        {
            if (uiManager != null)
            {
                uiManager.SetDeckButtonState(false);
                uiManager.SetLeftDiscardButtonState(false);
            }

            if (botController != null && players[activePlayerIndex] != null)
            {
                StartCoroutine(botController.ExecuteBotTurn(players[activePlayerIndex], deckManager, (botDiscard) =>
                {
                    if (activePlayerIndex == 3)
                    {
                        lastDiscardedTileByLeftPlayer = botDiscard;
                        if (uiManager != null)
                        {
                            uiManager.SetLeftDiscardTile(botDiscard, false);
                        }
                    }
                }, EndTurn));
            }
        }
    }

    // --- UYUMLULUK VE KURAL KÖPRÜLERİ ---
    public bool TasOkeyMi(Tile tas) => OkeyRuleEngine.IsOkeyTile(tas, deckManager?.OkeyTile);
    public bool CheckGroupPer(List<Tile> tileList) => OkeyRuleEngine.CheckGroupPer(tileList, deckManager?.OkeyTile);
    public bool CheckSequencePer(List<Tile> tileList) => OkeyRuleEngine.CheckSequencePer(tileList, deckManager?.OkeyTile);
    public bool CheckForPairs(List<Tile> hand) => OkeyRuleEngine.DetectPairs(hand, deckManager?.OkeyTile, out _);
    public int CalculateTotalPointsWithOkey(List<List<Tile>> allMelds) => OkeyRuleEngine.CalculateTotalPoints(allMelds, deckManager?.OkeyTile);
    public void SortHand(List<Tile> handToSort) => HandSorter.SortByColorAndValue(handToSort, deckManager?.OkeyTile);
}
