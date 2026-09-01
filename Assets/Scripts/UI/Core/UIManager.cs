using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("Istaka ve Deste Referansları")]
    public GameObject TilePrefab;
    public GameObject SlotPrefab;
    public Transform HandPanel;
    public Button DeckButton;

    [Header("Gösterge Taş UI Referansları")]
    public GameObject GostergeTile;
    public TileDisplay GostergeTileDisplay;
    public TextMeshProUGUI centerStoneText;

    [Header("Sol Taş Alma Referansları")]
    public GameObject LeftDiscardArea;
    public TileDisplay LeftDiscardTileDisplay;
    public Button LeftDiscardButton;

    [Header("Masa Ortası Açılan Taşlar Alanları")]
    public Transform TableCenterContainer; // Sol/Orta Seri & Grup Perleri Alanı
    public Transform TablePairsContainer;  // Sağ Çift Açanlar Alanı

    [Header("Alt Kontrolcüler")]
    public IstakaController istakaController;
    public TableView tableView;
    public ScoreboardUI scoreboardUI;

    private void Awake()
    {
        if (istakaController == null)
        {
            istakaController = GetComponent<IstakaController>();
            if (istakaController == null) istakaController = gameObject.AddComponent<IstakaController>();
        }

        if (tableView == null)
        {
            tableView = GetComponent<TableView>();
            if (tableView == null) tableView = gameObject.AddComponent<TableView>();
        }

        if (scoreboardUI == null)
        {
            scoreboardUI = GetComponent<ScoreboardUI>();
        }

        istakaController.TilePrefab = TilePrefab;
        istakaController.SlotPrefab = SlotPrefab;
        istakaController.HandPanel = HandPanel;
        
        tableView.TilePrefab = TilePrefab;
        tableView.tableCenterContainer = TableCenterContainer;
        tableView.tablePairsContainer = TablePairsContainer;
        tableView.centerStone = GostergeTile;
        tableView.gostergeTileDisplay = GostergeTileDisplay;
        tableView.centerStoneText = centerStoneText;

        tableView.leftDiscardArea = LeftDiscardArea;
        tableView.leftDiscardTileDisplay = LeftDiscardTileDisplay;
        tableView.leftDiscardButton = LeftDiscardButton;
    }

    private void Start()
    {
        if (istakaController != null)
        {
            istakaController.Initialize();
        }

        if (scoreboardUI != null)
        {
            scoreboardUI.Hide();
        }
    }

    public void DrawPlayerHand(List<Tile> playerHand)
    {
        if (istakaController != null)
        {
            istakaController.DrawHand(playerHand);
        }
    }

    public void RefreshHand(List<Tile> playerHand)
    {
        if (istakaController != null)
        {
            istakaController.DrawHand(playerHand);
        }
    }

    public void AddSingleTileToHand(Tile tileData)
    {
        if (istakaController != null)
        {
            istakaController.AddSingleTile(tileData);
        }
    }

    public void SetDeckButtonState(bool isInteractable)
    {
        if (DeckButton != null)
        {
            DeckButton.interactable = isInteractable;
        }
    }

    public void SetLeftDiscardTile(Tile tile, bool isInteractable)
    {
        if (tableView != null)
        {
            tableView.SetLeftDiscardTile(tile, isInteractable);
        }
    }

    public void SetLeftDiscardButtonState(bool isInteractable)
    {
        if (tableView != null)
        {
            tableView.SetLeftDiscardButtonInteractable(isInteractable);
        }
    }

    public void GostergeyiEkranaYansit(Tile gostergeTile)
    {
        if (tableView != null)
        {
            tableView.ShowGosterge(gostergeTile);
        }
    }

    public void DrawOpenedMeldsOnTable(List<Meld> melds)
    {
        if (tableView != null)
        {
            tableView.DrawOpenedMelds(melds);
        }
    }

    public void ShowScoreboard(Player winner, FinishType finishType, List<PlayerScoreInfo> scores)
    {
        if (scoreboardUI != null)
        {
            scoreboardUI.ShowScores(winner, finishType, scores);
        }
    }

    public List<List<Tile>> GetMeldsFromIstaka()
    {
        return istakaController != null ? istakaController.GetMeldsFromIstaka() : new List<List<Tile>>();
    }
}
