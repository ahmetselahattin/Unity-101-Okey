using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public enum FinishType
{
    Normal,
    Okey,
    Pairs,
    DeckOut
}

public class GameManager : MonoBehaviourPunCallbacks
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
    public Tile[] lastDiscardedTiles = new Tile[4];
    public bool hasDrawnTileThisTurn = false;
    public bool isGameOver = false;

    public event Action<Player, FinishType, List<PlayerScoreInfo>> OnGameFinished;

    public bool isOnlineGame => PhotonNetwork.IsConnected && PhotonNetwork.InRoom;

    public int localSeatIndex
    {
        get
        {
            if (isOnlineGame && PhotonNetwork.LocalPlayer != null)
            {
                int actor = PhotonNetwork.LocalPlayer.ActorNumber - 1;
                return Mathf.Clamp(actor, 0, 3);
            }
            return 0;
        }
    }

    public int currentPlayerIndex => turnManager != null ? turnManager.CurrentPlayerIndex : 0;
    public bool isFirstTurn => turnManager != null ? turnManager.IsFirstTurn : true;

    public Tile lastDiscardedTileByLeftPlayer
    {
        get
        {
            int leftSeat = (localSeatIndex + 3) % 4;
            return lastDiscardedTiles[leftSeat];
        }
    }

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
        Debug.Log("[GameManager] Oyun Başlatılıyor...");

        if (isOnlineGame)
        {
            if (PhotonNetwork.IsMasterClient)
            {
                StartOnlineGame();
            }
        }
        else
        {
            StartOfflineGame();
        }
    }

    private void StartOfflineGame()
    {
        isGameOver = false;
        if (inGameUI != null) inGameUI.SetActive(true);
        if (centerStone != null) centerStone.SetActive(true);

        tableMelds.Clear();
        for (int i = 0; i < 4; i++) lastDiscardedTiles[i] = null;
        hasDrawnTileThisTurn = false;

        for (int i = 0; i < 4; i++)
        {
            int prevScore = (players[i] != null) ? players[i].TotalScore : 0;
            players[i] = new Player(i, i == 0 ? "Sen" : $"Bot_{i}", i != 0);
            players[i].TotalScore = prevScore;
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
            uiManager.SetLeftDiscardTile(null, false);
            uiManager.DrawOpenedMeldsOnTable(tableMelds);
            if (uiManager.scoreboardUI != null) uiManager.scoreboardUI.Hide();
        }

        if (turnManager != null)
        {
            turnManager.ResetTurns();
            turnManager.StartTurn();
        }
    }

    private void StartOnlineGame()
    {
        deckManager = new DeckManager();
        deckManager.CreateDeck();
        deckManager.Shuffle();
        deckManager.DetermineOkey();

        Tile g = deckManager.Gosterge;
        Tile o = deckManager.OkeyTile;

        photonView.RPC(nameof(RPC_SyncOkeyAndGameStart), RpcTarget.All,
            g.TileValue, (int)g.Color, g.IsFakeOkey,
            o.TileValue, (int)o.Color);

        for (int i = 0; i < 4; i++)
        {
            int count = (i == 0) ? 22 : 21;
            StringBuilder sb = new StringBuilder();

            for (int j = 0; j < count; j++)
            {
                Tile t = deckManager.DrawTile();
                if (t != null)
                {
                    if (j > 0) sb.Append(",");
                    sb.Append(t.TileValue).Append("_").Append((int)t.Color).Append("_").Append(t.IsFakeOkey ? 1 : 0);
                }
            }

            photonView.RPC(nameof(RPC_ReceiveHand), RpcTarget.All, i, sb.ToString());
        }

        photonView.RPC(nameof(RPC_SyncTurn), RpcTarget.All, 0, true);
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
        if (isGameOver || currentPlayerIndex != localSeatIndex) return;

        if (hasDrawnTileThisTurn)
        {
            Debug.LogWarning("[GameManager] Bu tur zaten taş çektiniz!");
            return;
        }

        if (deckManager == null || deckManager.RemainingCount <= 0)
        {
            Debug.LogWarning("[GameManager] Destedeki tüm taşlar bitti! Oyun sona eriyor...");
            if (isOnlineGame)
            {
                photonView.RPC(nameof(RPC_OnGameFinished), RpcTarget.All, -1, (int)FinishType.DeckOut);
            }
            else
            {
                HandleGameFinished(null, FinishType.DeckOut);
            }
            return;
        }

        Tile drawnTile = deckManager.DrawTile();
        players[localSeatIndex].AddTile(drawnTile);
        hasDrawnTileThisTurn = true;

        if (uiManager != null)
        {
            uiManager.AddSingleTileToHand(drawnTile);
            uiManager.SetDeckButtonState(false);
            uiManager.SetLeftDiscardButtonState(false);
        }

        Debug.Log($"[GameManager] Desteden taş çekildi: {drawnTile}. (Kalan deste: {deckManager.RemainingCount})");
    }

    public void OnLeftDiscardClicked()
    {
        if (isGameOver || currentPlayerIndex != localSeatIndex) return;

        if (players[localSeatIndex].HasDrawnFromDiscard)
        {
            ReturnLeftDiscardTile();
        }
        else
        {
            DrawTileFromLeftDiscard();
        }
    }

    public void DrawTileFromLeftDiscard()
    {
        if (isGameOver || currentPlayerIndex != localSeatIndex) return;

        if (hasDrawnTileThisTurn)
        {
            Debug.LogWarning("[GameManager] Bu tur zaten taş çektiniz!");
            return;
        }

        Tile leftTile = lastDiscardedTileByLeftPlayer;
        if (leftTile == null)
        {
            Debug.LogWarning("[GameManager] Yandan alınacak taş yok!");
            return;
        }

        int leftSeat = (localSeatIndex + 3) % 4;
        lastDiscardedTiles[leftSeat] = null;

        players[localSeatIndex].AddTile(leftTile);
        players[localSeatIndex].HasDrawnFromDiscard = true;
        players[localSeatIndex].DrawnDiscardTile = leftTile;
        hasDrawnTileThisTurn = true;

        if (uiManager != null)
        {
            uiManager.AddSingleTileToHand(leftTile);
            uiManager.SetLeftDiscardTile(null, true);
            uiManager.SetDeckButtonState(false);
            uiManager.SetLeftDiscardButtonState(true);
        }

        Debug.Log($"[GameManager] Yandan {leftTile} taşını denemek için aldınız. (Eğer elinizi açamazsanız bu taşa tekrar tıklayarak geri bırakabilir ve desteden çekebilirsiniz!)");
    }

    public void ReturnLeftDiscardTile()
    {
        if (isGameOver || currentPlayerIndex != localSeatIndex) return;

        if (!players[localSeatIndex].HasDrawnFromDiscard || players[localSeatIndex].DrawnDiscardTile == null)
        {
            Debug.LogWarning("[GameManager] Geri bırakılacak yandan çekilmiş bir taş bulunamadı!");
            return;
        }

        Tile returnedTile = players[localSeatIndex].DrawnDiscardTile;
        int leftSeat = (localSeatIndex + 3) % 4;

        players[localSeatIndex].RemoveTile(returnedTile);
        lastDiscardedTiles[leftSeat] = returnedTile;

        players[localSeatIndex].HasDrawnFromDiscard = false;
        players[localSeatIndex].DrawnDiscardTile = null;
        hasDrawnTileThisTurn = false;

        if (uiManager != null)
        {
            uiManager.RefreshHand(players[localSeatIndex].Hand);
            uiManager.SetLeftDiscardTile(returnedTile, true);
            uiManager.SetDeckButtonState(deckManager != null && deckManager.RemainingCount > 0);
            uiManager.SetLeftDiscardButtonState(true);
        }

        Debug.Log($"[GameManager] Soldan alınan {returnedTile} taşı geri bırakıldı. Şimdi desteden taş çekebilirsiniz.");
    }

    public void OnAutoSortClicked()
    {
        if (players[localSeatIndex] != null && deckManager != null)
        {
            HandSorter.ArrangeHandSmartly(
                players[localSeatIndex].Hand,
                deckManager.OkeyTile,
                out var bestMelds,
                out var remTiles,
                out var istakaLayout);

            if (uiManager != null)
            {
                uiManager.DrawArrangedHand(istakaLayout);
            }

            Debug.Log($"[GameManager] Taşlar akıllıca dizildi! ({bestMelds.Count} per tespit edildi, perler aralıklı yerleştirildi, kalan {remTiles.Count} taş sona dizildi).");
        }
    }

    public void OnOpenHandClicked()
    {
        if (isGameOver || uiManager == null || deckManager == null) return;

        List<List<Tile>> rawMelds = uiManager.GetMeldsFromIstaka(deckManager?.OkeyTile);
        bool manualValid = OkeyRuleEngine.ValidateOpenHand(rawMelds, deckManager.OkeyTile, out int totalPoints, out string manualError);

        if (!manualValid || totalPoints < OkeyRuleEngine.OpenHandThreshold)
        {
            Debug.LogWarning($"[GameManager] El açma başarısız: {manualError} (Istakanızdaki perlerin puan toplamı: {totalPoints} / Gerekli: 101). Lütfen ıstakanızda perlerin arasına boşluk bırakarak doğru perler diziniz.");
            return;
        }

        List<List<Tile>> meldsToOpen = rawMelds;

        // YANDAN TAŞ ALINDIĞINDA O TAŞI KULLANMA ŞARTI KONTROLÜ
        if (players[localSeatIndex].HasDrawnFromDiscard && players[localSeatIndex].DrawnDiscardTile != null)
        {
            bool usedDrawnTile = false;
            foreach (var m in meldsToOpen)
            {
                if (m.Exists(t => t == players[localSeatIndex].DrawnDiscardTile || t.IsSame(players[localSeatIndex].DrawnDiscardTile)))
                {
                    usedDrawnTile = true;
                    break;
                }
            }

            if (!usedDrawnTile)
            {
                Debug.LogError($"[GameManager] KURAL İHLALİ: Soldan aldığınız {players[localSeatIndex].DrawnDiscardTile} taşını açtığınız perlerin içinde kullanmak zorundasınız! Bu taş olmadan el açamazsınız. (Açmayacaksanız sol taşa tıklayarak geri bırakınız).");
                return;
            }
        }

        Debug.Log($"[GameManager] Tebrikler! Istakanızda dizdiğiniz perler ile 101 barajı aşıldı ({totalPoints} Puan), el masaya açılıyor.");

        List<Meld> newOpenedMelds = new List<Meld>();
        foreach (var tileGroup in meldsToOpen)
        {
            MeldType type = OkeyRuleEngine.CheckGroupPer(tileGroup, deckManager.OkeyTile) ? MeldType.Group : MeldType.Sequence;
            int points = OkeyRuleEngine.CalculateMeldPoints(tileGroup, deckManager.OkeyTile);
            Meld m = new Meld(tileGroup, type, points);
            newOpenedMelds.Add(m);
            tableMelds.Add(m);
            players[localSeatIndex].OpenedMelds.Add(m);

            foreach (var tile in tileGroup)
            {
                players[localSeatIndex].RemoveTile(tile);
            }
        }

        players[localSeatIndex].HasOpenedHand = true;
        players[localSeatIndex].HasOpenedPairs = false;
        players[localSeatIndex].HasDrawnFromDiscard = false;
        players[localSeatIndex].DrawnDiscardTile = null;

        uiManager.RefreshHand(players[localSeatIndex].Hand);
        uiManager.DrawOpenedMeldsOnTable(tableMelds);

        if (isOnlineGame)
        {
            photonView.RPC(nameof(RPC_OnPlayerOpenedMelds), RpcTarget.Others, localSeatIndex, Meld.SerializeMelds(newOpenedMelds), false);
        }
    }

    public void OnOpenPairsClicked()
    {
        if (isGameOver || deckManager == null || players[localSeatIndex] == null) return;

        Player player = players[localSeatIndex];

        // 1. Zaten el açmış oyuncunun elindeki çiftleri açması
        if (player.HasOpenedHand)
        {
            if (OkeyRuleEngine.DetectPairs(player.Hand, deckManager.OkeyTile, out List<Meld> remainingPairs) && remainingPairs.Count > 0)
            {
                if (player.HasDrawnFromDiscard && player.DrawnDiscardTile != null)
                {
                    bool usedDrawnTile = remainingPairs.Exists(p => p.Tiles.Exists(t => t == player.DrawnDiscardTile || t.IsSame(player.DrawnDiscardTile)));
                    if (!usedDrawnTile)
                    {
                        Debug.LogError($"[GameManager] KURAL İHLALİ: Soldan aldığınız {player.DrawnDiscardTile} taşını açtığınız çiftlerin içinde kullanmak zorundasınız!");
                        return;
                    }
                }

                Debug.Log($"[GameManager] Tebrikler! Elinizdeki {remainingPairs.Count} adet çifti masaya eklediniz.");

                foreach (var pairMeld in remainingPairs)
                {
                    tableMelds.Add(pairMeld);
                    player.OpenedMelds.Add(pairMeld);

                    foreach (var tile in pairMeld.Tiles)
                    {
                        player.RemoveTile(tile);
                    }
                }

                player.HasDrawnFromDiscard = false;
                player.DrawnDiscardTile = null;

                if (uiManager != null)
                {
                    uiManager.RefreshHand(player.Hand);
                    uiManager.DrawOpenedMeldsOnTable(tableMelds);
                }

                if (isOnlineGame)
                {
                    photonView.RPC(nameof(RPC_OnPlayerOpenedMelds), RpcTarget.Others, localSeatIndex, Meld.SerializeMelds(remainingPairs), true);
                }
                return;
            }
            else
            {
                Debug.LogWarning("[GameManager] Elinizde masaya açılabilecek geçerli bir çift bulunamadı!");
                return;
            }
        }

        // 2. İlk defa çift açan oyuncu (en az 5 çift gerekli)
        if (OkeyRuleEngine.ValidateOpenPairs(player.Hand, deckManager.OkeyTile, out List<Meld> pairsToOpen, out string error))
        {
            if (player.HasDrawnFromDiscard && player.DrawnDiscardTile != null)
            {
                bool usedDrawnTile = false;
                foreach (var p in pairsToOpen)
                {
                    if (p.Tiles.Exists(t => t == player.DrawnDiscardTile || t.IsSame(player.DrawnDiscardTile)))
                    {
                        usedDrawnTile = true;
                        break;
                    }
                }

                if (!usedDrawnTile)
                {
                    Debug.LogError($"[GameManager] KURAL İHLALİ: Soldan aldığınız {player.DrawnDiscardTile} taşını açtığınız çiftlerin içinde kullanmak zorundasınız!");
                    return;
                }
            }

            Debug.Log($"[GameManager] Tebrikler! {pairsToOpen.Count} çift ile el masaya açılıyor.");

            foreach (var pairMeld in pairsToOpen)
            {
                tableMelds.Add(pairMeld);
                player.OpenedMelds.Add(pairMeld);

                foreach (var tile in pairMeld.Tiles)
                {
                    player.RemoveTile(tile);
                }
            }

            player.HasOpenedHand = true;
            player.HasOpenedPairs = true;
            player.HasDrawnFromDiscard = false;
            player.DrawnDiscardTile = null;

            if (uiManager != null)
            {
                uiManager.RefreshHand(player.Hand);
                uiManager.DrawOpenedMeldsOnTable(tableMelds);
            }

            if (isOnlineGame)
            {
                photonView.RPC(nameof(RPC_OnPlayerOpenedMelds), RpcTarget.Others, localSeatIndex, Meld.SerializeMelds(pairsToOpen), true);
            }
        }
        else
        {
            Debug.LogWarning($"[GameManager] Çift açma başarısız: {error}");
        }
    }

    public bool ProcessTileToTable(Tile tile, int meldIndex)
    {
        if (isGameOver || !players[localSeatIndex].HasOpenedHand)
        {
            Debug.LogWarning("[GameManager] Masaya taş işleyebilmek için önce elinizi açmış olmanız gerekir!");
            return false;
        }

        if (meldIndex < 0 || meldIndex >= tableMelds.Count || tile == null)
            return false;

        Meld targetMeld = tableMelds[meldIndex];

        if (OkeyRuleEngine.CanProcessTileToMeld(tile, targetMeld, deckManager.OkeyTile, players[localSeatIndex].HasOpenedPairs, out bool addToStart))
        {
            if (addToStart) targetMeld.Tiles.Insert(0, tile);
            else targetMeld.Tiles.Add(tile);

            players[localSeatIndex].RemoveTile(tile);

            if (players[localSeatIndex].HasDrawnFromDiscard && 
                (tile == players[localSeatIndex].DrawnDiscardTile || tile.IsSame(players[localSeatIndex].DrawnDiscardTile)))
            {
                players[localSeatIndex].HasDrawnFromDiscard = false;
                players[localSeatIndex].DrawnDiscardTile = null;
            }

            if (uiManager != null)
            {
                uiManager.RefreshHand(players[localSeatIndex].Hand);
                uiManager.DrawOpenedMeldsOnTable(tableMelds);
            }

            if (isOnlineGame)
            {
                photonView.RPC(nameof(RPC_OnTileProcessed), RpcTarget.Others, localSeatIndex, meldIndex, tile.TileValue, (int)tile.Color, tile.IsFakeOkey, addToStart);
            }

            Debug.Log($"[GameManager] {tile} taşı masadaki pere başarıyla işlendi!");
            return true;
        }

        Debug.LogWarning($"[GameManager] {tile} taşı bu pere işlenemez!");
        return false;
    }

    public bool CanPlayerDiscard()
    {
        if (isGameOver || currentPlayerIndex != localSeatIndex) return false;
        if (!hasDrawnTileThisTurn && !isFirstTurn) return false;

        if (players[localSeatIndex].HasDrawnFromDiscard) return false;

        return true;
    }

    public void OnPlayerDiscardTile(Tile discardedTile)
    {
        if (!CanPlayerDiscard())
        {
            Debug.LogError("[GameManager] KURAL İHLALİ: Soldan taş aldığınız için bu tur o taşı kullanarak elinizi açmak veya masaya işlemek zorundasınız! Açmayacaksanız soldaki butona tıklayıp taşı geri koyunuz.");
            if (uiManager != null)
            {
                uiManager.RefreshHand(players[localSeatIndex].Hand);
            }
            return;
        }

        if (discardedTile != null && players[localSeatIndex] != null)
        {
            players[localSeatIndex].RemoveTile(discardedTile);
            lastDiscardedTiles[localSeatIndex] = discardedTile;
            Debug.Log($"[GameManager] Oyuncu bir taş attı: {discardedTile}");
        }

        hasDrawnTileThisTurn = false;

        if (isOnlineGame)
        {
            photonView.RPC(nameof(RPC_OnTileDiscarded), RpcTarget.All, localSeatIndex, discardedTile.TileValue, (int)discardedTile.Color, discardedTile.IsFakeOkey);
        }

        // EL BİTİRME KONTROLÜ
        if (players[localSeatIndex].Hand.Count == 0 && players[localSeatIndex].HasOpenedHand)
        {
            bool isOkey = OkeyRuleEngine.IsOkeyTile(discardedTile, deckManager.OkeyTile);
            FinishType finish = isOkey ? FinishType.Okey : (players[localSeatIndex].HasOpenedPairs ? FinishType.Pairs : FinishType.Normal);

            if (isOnlineGame)
            {
                photonView.RPC(nameof(RPC_OnGameFinished), RpcTarget.All, localSeatIndex, (int)finish);
            }
            else
            {
                HandleGameFinished(players[0], finish);
            }
            return;
        }

        if (isOnlineGame)
        {
            int nextSeat = (localSeatIndex + 1) % 4;
            photonView.RPC(nameof(RPC_SyncTurn), RpcTarget.All, nextSeat, false);
        }
        else
        {
            EndTurn();
        }
    }

    public void HandleGameFinished(Player winner, FinishType finishType)
    {
        isGameOver = true;

        List<PlayerScoreInfo> scores = ScoreEngine.CalculateScores(players, winner, finishType, deckManager?.OkeyTile);

        string finishDesc = finishType switch
        {
            FinishType.Okey => "OKEY ATARAK BİTTİ! (-202 Puan / 2 Kat Ceza)",
            FinishType.Pairs => "ÇİFTTEN BİTTİ! (-202 Puan / 2 Kat Ceza)",
            FinishType.DeckOut => "DESTEDEKİ TAŞLAR BİTTİ! (Ortada Taş Kalmadı)",
            _ => "NORMAL BİTTİ! (-101 Puan)"
        };

        StringBuilder sb = new StringBuilder();
        sb.AppendLine("\n=======================================================");
        sb.AppendLine($"🏆 [101 OKEY EL SONU VE CEZA RAPORU]");
        sb.AppendLine($"Sonuç Türü: {finishDesc}");
        if (winner != null) sb.AppendLine($"Kazanan: {winner.NickName}");
        sb.AppendLine("-------------------------------------------------------");

        foreach (var sc in scores)
        {
            string status = sc.IsWinner ? "🏆 KAZANDI" : (sc.HasOpenedHand ? "Açtı (Kalan Taş Sayıldı)" : "AÇAMADI (+101/202)");
            string sign = sc.RoundPenalty > 0 ? $"+{sc.RoundPenalty}" : $"{sc.RoundPenalty}";
            sb.AppendLine($"• Koltuk {sc.SeatIndex + 1} ({sc.NickName}) => Durum: {status} | Bu Tur Ceza: {sign} Puan | Genel Toplam: {sc.CumulativeScore}");
        }
        sb.AppendLine("=======================================================\n");

        Debug.LogWarning(sb.ToString());

        if (uiManager != null)
        {
            uiManager.SetDeckButtonState(false);
            uiManager.SetLeftDiscardButtonState(false);
            uiManager.ShowScoreboard(winner, finishType, scores);
        }

        OnGameFinished?.Invoke(winner, finishType, scores);
    }

    public void EndTurn()
    {
        if (isGameOver) return;

        if (deckManager != null && deckManager.RemainingCount <= 0)
        {
            Debug.LogWarning("[GameManager] Deste tükendi, oyun bitiyor.");
            HandleGameFinished(null, FinishType.DeckOut);
            return;
        }

        if (turnManager != null)
        {
            turnManager.NextTurn();
        }
    }

    private void HandleTurnChanged(int activePlayerIndex)
    {
        if (isGameOver) return;

        if (activePlayerIndex == localSeatIndex)
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
                    uiManager.SetDeckButtonState(deckManager != null && deckManager.RemainingCount > 0);
                    Tile leftTile = lastDiscardedTileByLeftPlayer;
                    bool canDrawLeft = (leftTile != null);
                    uiManager.SetLeftDiscardTile(leftTile, canDrawLeft);
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

            if (!isOnlineGame && botController != null && players[activePlayerIndex] != null)
            {
                int leftSeat = (activePlayerIndex + 3) % 4;
                Tile leftDiscard = lastDiscardedTiles[leftSeat];

                StartCoroutine(botController.ExecuteBotTurn(
                    players[activePlayerIndex],
                    deckManager,
                    leftDiscard,
                    tableMelds,
                    () => {
                        if (uiManager != null) uiManager.DrawOpenedMeldsOnTable(tableMelds);
                    },
                    (botDiscard) =>
                    {
                        lastDiscardedTiles[activePlayerIndex] = botDiscard;

                        if (activePlayerIndex == 3 && uiManager != null)
                        {
                            uiManager.SetLeftDiscardTile(botDiscard, true);
                        }

                        if (players[activePlayerIndex].Hand.Count == 0 && players[activePlayerIndex].HasOpenedHand)
                        {
                            bool isOkey = OkeyRuleEngine.IsOkeyTile(botDiscard, deckManager.OkeyTile);
                            FinishType finish = isOkey ? FinishType.Okey : (players[activePlayerIndex].HasOpenedPairs ? FinishType.Pairs : FinishType.Normal);
                            HandleGameFinished(players[activePlayerIndex], finish);
                        }
                    },
                    EndTurn));
            }
        }
    }

    // ── PHOTON RPC ÇOK OYUNCULU SENKRONİZASYONLARI ──

    [PunRPC]
    public void RPC_SyncOkeyAndGameStart(int gVal, int gCol, bool gFake, int oVal, int oCol)
    {
        isGameOver = false;
        if (inGameUI != null) inGameUI.SetActive(true);
        if (centerStone != null) centerStone.SetActive(true);

        tableMelds.Clear();
        for (int i = 0; i < 4; i++) lastDiscardedTiles[i] = null;
        hasDrawnTileThisTurn = false;

        deckManager = new DeckManager();
        deckManager.SetGostergeAndOkey(new Tile(gVal, (TileColor)gCol, gFake), new Tile(oVal, (TileColor)oCol, false));

        for (int i = 0; i < 4; i++)
        {
            string nick = (i == localSeatIndex) ? "Sen" : $"Oyuncu_{i + 1}";
            players[i] = new Player(i, nick, false);
        }

        if (uiManager != null)
        {
            uiManager.GostergeyiEkranaYansit(deckManager.Gosterge);
            uiManager.SetLeftDiscardTile(null, false);
            uiManager.DrawOpenedMeldsOnTable(tableMelds);
            if (uiManager.scoreboardUI != null) uiManager.scoreboardUI.Hide();
        }
    }

    [PunRPC]
    public void RPC_ReceiveHand(int seatIndex, string handData)
    {
        if (seatIndex != localSeatIndex) return;

        List<Tile> hand = new List<Tile>();
        if (!string.IsNullOrEmpty(handData))
        {
            string[] items = handData.Split(',');
            foreach (var item in items)
            {
                string[] parts = item.Split('_');
                if (parts.Length >= 3)
                {
                    int v = int.Parse(parts[0]);
                    TileColor c = (TileColor)int.Parse(parts[1]);
                    bool f = (parts[2] == "1");
                    hand.Add(new Tile(v, c, f));
                }
            }
        }

        players[localSeatIndex].Hand.Clear();
        foreach (var t in hand) players[localSeatIndex].AddTile(t);

        if (uiManager != null)
        {
            uiManager.DrawPlayerHand(players[localSeatIndex].Hand);
        }
    }

    [PunRPC]
    public void RPC_SyncTurn(int activeSeatIndex, bool isFirst)
    {
        if (turnManager != null)
        {
            turnManager.SetTurn(activeSeatIndex, isFirst);
        }

        HandleTurnChanged(activeSeatIndex);
    }

    [PunRPC]
    public void RPC_OnTileDiscarded(int fromSeatIndex, int tileVal, int tileCol, bool isFake)
    {
        Tile discarded = new Tile(tileVal, (TileColor)tileCol, isFake);
        lastDiscardedTiles[fromSeatIndex] = discarded;

        int myLeftSeat = (localSeatIndex + 3) % 4;
        if (fromSeatIndex == myLeftSeat && uiManager != null)
        {
            uiManager.SetLeftDiscardTile(discarded, currentPlayerIndex == localSeatIndex);
        }
    }

    [PunRPC]
    public void RPC_OnPlayerOpenedMelds(int fromSeatIndex, string meldsData, bool isPairs)
    {
        List<Meld> newMelds = Meld.DeserializeMelds(meldsData);
        foreach (var m in newMelds)
        {
            tableMelds.Add(m);
            players[fromSeatIndex].OpenedMelds.Add(m);
        }

        players[fromSeatIndex].HasOpenedHand = true;
        players[fromSeatIndex].HasOpenedPairs = isPairs;

        if (uiManager != null)
        {
            uiManager.DrawOpenedMeldsOnTable(tableMelds);
        }
    }

    [PunRPC]
    public void RPC_OnTileProcessed(int fromSeatIndex, int meldIndex, int tileVal, int tileCol, bool isFake, bool addToStart)
    {
        if (meldIndex >= 0 && meldIndex < tableMelds.Count)
        {
            Tile tile = new Tile(tileVal, (TileColor)tileCol, isFake);
            if (addToStart) tableMelds[meldIndex].Tiles.Insert(0, tile);
            else tableMelds[meldIndex].Tiles.Add(tile);

            if (uiManager != null)
            {
                uiManager.DrawOpenedMeldsOnTable(tableMelds);
            }
        }
    }

    [PunRPC]
    public void RPC_OnGameFinished(int winnerSeatIndex, int finishTypeInt)
    {
        FinishType finish = (FinishType)finishTypeInt;
        Player winner = (winnerSeatIndex >= 0 && winnerSeatIndex < 4) ? players[winnerSeatIndex] : null;
        HandleGameFinished(winner, finish);
    }
}
