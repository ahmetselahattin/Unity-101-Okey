using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ScoreboardUI : MonoBehaviour
{
    [Header("UI Referansları")]
    public GameObject ScorePanel;
    public TextMeshProUGUI TitleText;
    public TextMeshProUGUI DetailsText;
    public Button NextRoundButton;

    private void Awake()
    {
        if (NextRoundButton != null)
        {
            NextRoundButton.onClick.AddListener(OnNextRoundClicked);
        }
    }

    public void ShowScores(Player winner, FinishType finishType, List<PlayerScoreInfo> scores)
    {
        if (ScorePanel != null)
        {
            ScorePanel.SetActive(true);
            ScorePanel.transform.SetAsLastSibling(); // Ekranın en ön katmanına al!
        }

        if (TitleText != null)
        {
            string finishBadge = finishType switch
            {
                FinishType.Okey => "🏆 OKEY ATARAK BİTİLDİ! (2 Kat Ceza)",
                FinishType.Pairs => "👥 ÇİFTTEN BİTİLDİ! (2 Kat Ceza)",
                FinishType.DeckOut => "📦 DESTEDEKİ TAŞLAR BİTTİ!",
                _ => "🏆 EL TAMAMLANDI!"
            };

            if (winner != null)
            {
                TitleText.text = $"{finishBadge}\n<color=#4CAF50><b>Kazanan: {winner.NickName}</b></color>";
            }
            else
            {
                TitleText.text = $"{finishBadge}\n<color=#FFC107><b>(Deste Tükendi - Herkes Kalan Taşına Göre Ceza Aldı)</b></color>";
            }
        }

        if (DetailsText != null)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.AppendLine("<b>── 📊 OYUNCU CEZA VE PUAN RAPORU ──</b>\n");

            foreach (var s in scores)
            {
                string statusIcon = s.IsWinner ? "🏆" : (s.HasOpenedHand ? "✅" : "❌");
                string colorHex = s.IsWinner ? "#4CAF50" : (s.RoundPenalty > 100 ? "#F44336" : "#FF9800");
                string sign = s.RoundPenalty > 0 ? "+" : "";

                sb.AppendLine($"{statusIcon} <b>{s.NickName}</b>: <color={colorHex}><b>{sign}{s.RoundPenalty} Ceza Puanı</b></color>  <i>({s.SummaryText})</i>  |  Genel Toplam: <b>{s.CumulativeScore}</b>");
            }

            DetailsText.text = sb.ToString();
        }
    }

    public void Hide()
    {
        if (ScorePanel != null)
        {
            ScorePanel.SetActive(false);
        }
    }

    private void OnNextRoundClicked()
    {
        Hide();
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OyunuBaslat();
        }
    }
}
