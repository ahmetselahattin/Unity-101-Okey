using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TableView : MonoBehaviour
{
    [Header("Tile Prefab")]
    public GameObject TilePrefab;

    [Header("Gösterge Taş UI Referansları")]
    public GameObject centerStone;
    public TileDisplay gostergeTileDisplay;
    public TextMeshProUGUI centerStoneText;

    [Header("Sol Taş Alma (Yandan Taş) Referanslar")]
    public GameObject leftDiscardArea;
    public TileDisplay leftDiscardTileDisplay;
    public Button leftDiscardButton;

    [Header("Masa Ortası Açılan Taşlar Alanları")]
    public Transform tableCenterContainer; // Seri ve Grup perleri için çok satırlı grid alanı
    public Transform tablePairsContainer;  // Sağ tarafta sadece çift açanlar için alan

    private const float MaxRowWidth = 1220f;
    private const float MeldSpacing = 16f;

    private void Start()
    {
        SetLeftDiscardTile(null, false);

        if (leftDiscardButton != null)
        {
            leftDiscardButton.onClick.RemoveAllListeners();
            leftDiscardButton.onClick.AddListener(OnLeftDiscardButtonClicked);
        }
    }

    public void OnLeftDiscardButtonClicked()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnLeftDiscardClicked();
        }
    }

    public void ShowGosterge(Tile gostergeTile)
    {
        if (centerStone != null) centerStone.SetActive(true);
        if (gostergeTile == null) return;

        if (gostergeTileDisplay != null)
        {
            gostergeTileDisplay.CanFlip = false;
            gostergeTileDisplay.SetTile(gostergeTile);
            return;
        }

        if (centerStoneText != null)
        {
            centerStoneText.fontStyle = FontStyles.Bold;

            if (gostergeTile.IsFakeOkey)
            {
                centerStoneText.text = "<b>SO</b>";
                centerStoneText.color = TileColorExtensions.BlackColor;
                centerStoneText.fontSize = 28;
                return;
            }

            centerStoneText.fontSize = 28;
            centerStoneText.text = gostergeTile.TileValue.ToString();
            centerStoneText.color = gostergeTile.Color.ToColor();
        }
    }

    public void SetLeftDiscardTile(Tile tile, bool isInteractable = false)
    {
        if (leftDiscardArea != null)
        {
            leftDiscardArea.SetActive(true);
        }

        if (leftDiscardTileDisplay != null)
        {
            leftDiscardTileDisplay.CanFlip = false;
            if (tile == null)
            {
                leftDiscardTileDisplay.gameObject.SetActive(false);
            }
            else
            {
                leftDiscardTileDisplay.gameObject.SetActive(true);
                leftDiscardTileDisplay.SetTile(tile);
            }
        }

        if (leftDiscardButton != null)
        {
            leftDiscardButton.interactable = isInteractable;
        }
    }

    public void SetLeftDiscardButtonInteractable(bool isInteractable)
    {
        if (leftDiscardButton != null)
        {
            leftDiscardButton.interactable = isInteractable;
        }
    }

    public void DrawOpenedMelds(List<Meld> melds)
    {
        ClearContainer(tableCenterContainer);
        ClearContainer(tablePairsContainer);

        if (melds == null || melds.Count == 0) return;

        Debug.Log($"[TableView] Masaya toplam {melds.Count} adet per sergileniyor.");

        GameObject currentRowObj = null;
        float currentRowWidth = 0f;
        int rowIndex = 0;

        for (int mIndex = 0; mIndex < melds.Count; mIndex++)
        {
            Meld meld = melds[mIndex];

            // ── ÇİFT PERLERİ (SAĞ BÖLÜMDEKİ ÇİFTLER ALANINA) ──
            if (meld.Type == MeldType.Pair)
            {
                if (tablePairsContainer != null)
                {
                    CreatePairUI(meld, mIndex, tablePairsContainer);
                }
                continue;
            }

            // ── SERİ VE GRUP PERLERİ (SOL/ORTA ÇOK SATIRLI ALANA) ──
            if (tableCenterContainer == null) continue;

            float meldWidth = (meld.Tiles.Count * 44f) + 16f;

            if (currentRowObj == null || (currentRowWidth + meldWidth + MeldSpacing) > MaxRowWidth)
            {
                currentRowObj = CreateNewRow(tableCenterContainer, rowIndex++);
                currentRowWidth = 0f;
            }

            CreateMeldUI(meld, mIndex, currentRowObj.transform, meldWidth);
            currentRowWidth += meldWidth + MeldSpacing;
        }
    }

    private void ClearContainer(Transform container)
    {
        if (container == null) return;
        for (int i = container.childCount - 1; i >= 0; i--)
        {
            Destroy(container.GetChild(i).gameObject);
        }
    }

    private GameObject CreateNewRow(Transform parent, int index)
    {
        GameObject row = new GameObject("MeldRow_" + index);
        row.transform.SetParent(parent, false);

        RectTransform rt = row.AddComponent<RectTransform>();
        rt.sizeDelta = new Vector2(MaxRowWidth, 74f);

        HorizontalLayoutGroup layout = row.AddComponent<HorizontalLayoutGroup>();
        layout.spacing = MeldSpacing;
        layout.childAlignment = TextAnchor.MiddleLeft;
        layout.childControlWidth = false;
        layout.childControlHeight = false;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = false;

        return row;
    }

    private void CreateMeldUI(Meld meld, int mIndex, Transform parentRow, float meldWidth)
    {
        GameObject meldObj = new GameObject("Meld_" + mIndex);
        meldObj.transform.SetParent(parentRow, false);

        RectTransform meldRect = meldObj.AddComponent<RectTransform>();
        meldRect.sizeDelta = new Vector2(meldWidth, 70f);

        Image bgImg = meldObj.AddComponent<Image>();
        bgImg.color = new Color(0.08f, 0.10f, 0.13f, 0.90f);

        HorizontalLayoutGroup layout = meldObj.AddComponent<HorizontalLayoutGroup>();
        layout.padding = new RectOffset(6, 6, 4, 4);
        layout.spacing = 3f;
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.childControlWidth = false;
        layout.childControlHeight = false;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = false;

        MeldGroupUI groupUI = meldObj.AddComponent<MeldGroupUI>();
        groupUI.Initialize(mIndex, meld);

        if (TilePrefab != null)
        {
            foreach (Tile tile in meld.Tiles)
            {
                CreateTileInMeld(tile, meldObj.transform, new Vector2(40f, 60f));
            }
        }
    }

    private void CreatePairUI(Meld pairMeld, int mIndex, Transform parentContainer)
    {
        GameObject pairObj = new GameObject("Pair_" + mIndex);
        pairObj.transform.SetParent(parentContainer, false);

        RectTransform pairRect = pairObj.AddComponent<RectTransform>();
        pairRect.sizeDelta = new Vector2(90f, 66f);

        Image bgImg = pairObj.AddComponent<Image>();
        bgImg.color = new Color(0.12f, 0.08f, 0.16f, 0.92f);

        HorizontalLayoutGroup layout = pairObj.AddComponent<HorizontalLayoutGroup>();
        layout.padding = new RectOffset(4, 4, 3, 3);
        layout.spacing = 3f;
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.childControlWidth = false;
        layout.childControlHeight = false;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = false;

        MeldGroupUI groupUI = pairObj.AddComponent<MeldGroupUI>();
        groupUI.Initialize(mIndex, pairMeld);

        if (TilePrefab != null)
        {
            foreach (Tile tile in pairMeld.Tiles)
            {
                CreateTileInMeld(tile, pairObj.transform, new Vector2(38f, 58f));
            }
        }
    }

    private void CreateTileInMeld(Tile tile, Transform parent, Vector2 size)
    {
        GameObject tileObj = Instantiate(TilePrefab, parent);
        tileObj.name = "TableTile_" + tile.TileValue;

        RectTransform rt = tileObj.GetComponent<RectTransform>();
        if (rt != null)
        {
            rt.sizeDelta = size;
        }

        DraggableTile dragComp = tileObj.GetComponent<DraggableTile>();
        if (dragComp != null) Destroy(dragComp);

        CanvasGroup cg = tileObj.GetComponent<CanvasGroup>();
        if (cg != null) cg.blocksRaycasts = false;

        TileDisplay display = tileObj.GetComponent<TileDisplay>();
        if (display != null)
        {
            display.CanFlip = false;
            display.SetTile(tile);
        }
    }
}
