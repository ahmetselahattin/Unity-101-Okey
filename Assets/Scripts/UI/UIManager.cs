using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("Istaka ve Deste Öðeleri")]
    public GameObject TilePrefab;
    public GameObject SlotPrefab; // YENÝ: Boþ yuva þablonu
    public Transform HandPanel;
    public Button DeckButton;

    [Header("Gösterge Taþý UI Öðeleri")]
    public TextMeshProUGUI centerStoneText;

    // YENÝ: Istakadaki tüm boþ yuvalarýn listesi
    private List<Transform> lstakaSlotlari = new List<Transform>();

    private void Start()
    {
        IstakayiOlustur();
    }

    // --- ISTAKA YUVALARINI (SLOTLAR) OLUÞTURMA ---
    private void IstakayiOlustur()
    {
        // 101 Okey ýstakasý genelde 2 satýr x 21 sütun = 42 yuvadan oluþur
        for (int i = 0; i < 42; i++)
        {
            GameObject yeniSlot = Instantiate(SlotPrefab, HandPanel);
            yeniSlot.name = "Slot_" + i;
            lstakaSlotlari.Add(yeniSlot.transform);
        }
    }

    // --- ISTAKA VE TAÞ ÇÝZÝM YÖNETÝMÝ (GÜNCELLENDÝ) ---
    public void DrawPlayerHand(List<Tile> playerHand)
    {
        // Taþlarý en baþtaki slotlardan baþlayarak yerleþtir
        for (int i = 0; i < playerHand.Count; i++)
        {
            // Eðer elimizdeki taþ sayýsý slot sayýsýný geçerse hata almamak için güvenlik önlemi
            if (i >= lstakaSlotlari.Count) break;

            // Taþý doðrudan HandPanel'e deðil, sýradaki Slot'un içine klonla
            GameObject cloneTile = Instantiate(TilePrefab, lstakaSlotlari[i]);
            cloneTile.GetComponent<TileDisplay>().SetTile(playerHand[i]);
        }
    }

    public void RefreshHand(List<Tile> playerHand)
    {
        // Eski sistemde HandPanel'in içini temizliyorduk.
        // Artýk HandPanel'in içinde Slot'lar var. Sadece Slot'larýn içindeki taþlarý silmeliyiz.
        foreach (Transform slot in lstakaSlotlari)
        {
            if (slot.childCount > 0)
            {
                // Slot'un içindeki taþý (ilk çocuðu) yok et
                Destroy(slot.GetChild(0).gameObject);
            }
        }

        // Listeyi tekrar çiz
        DrawPlayerHand(playerHand);
    }

    // --- BUTON VE ETKÝLEÞÝM YÖNETÝMÝ ---
    public void SetDeckButtonState(bool isInteractable)
    {
        if (DeckButton != null)
        {
            DeckButton.interactable = isInteractable;
        }
    }

    // --- ISTAKA VE TAÞ ÇÝZÝM YÖNETÝMÝ ---

    public void AddSingleTileToHand(Tile tileData)
    {
        // Þablondan yeni bir taþ klonla ve HandPanel'e koy
        GameObject cloneTile = Instantiate(TilePrefab, HandPanel);

        // Taþa verisini (renk, sayý) gönder
        cloneTile.GetComponent<TileDisplay>().SetTile(tileData);
    }

    // --- GÖSTERGE YANSITMA YÖNETÝMÝ ---
    public void GostergeyiEkranaYansit(Tile gostergeTile)
    {
        if (centerStoneText == null) return;

        // Sahte okey gösterge açýldýysa (genelde üzerinde bir sembol olur)
        if (gostergeTile.IsFakeOkey)
        {
            centerStoneText.text = "J"; // Joker anlamýnda
            centerStoneText.color = Color.black;
            return;
        }

        // Göstergenin sayýsýný yazdýrýyoruz
        centerStoneText.text = gostergeTile.TileValue.ToString();

        // Göstergenin rengine göre UI metnini boyuyoruz
        switch (gostergeTile.Color)
        {
            case TileColor.Red:
                centerStoneText.color = Color.red;
                break;
            case TileColor.Black:
                centerStoneText.color = Color.black;
                break;
            case TileColor.Blue:
                centerStoneText.color = Color.blue;
                break;
            case TileColor.Yellow:
                // Tam sarý ekranda zor okunur, hafif turuncumsu veya altýn sarýsý
                centerStoneText.color = new Color(1f, 0.8f, 0f);
                break;
        }
    }
}