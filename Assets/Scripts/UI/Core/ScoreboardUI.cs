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
        }

        if (TitleText != null)
        {
            string finishBadge = finishType switch
            {
                FinishType.Okey => "🔥 OKEY ATARAK BİTİLDİ! (2 Kat Ceza)",
                FinishType.Pairs => "✨ ÇİFTTEN BİTİLDİ! (2 Kat Ceza)",
                _ => "🏆 EL TAMAMLANDI!"
            };
            TitleText.text = $"{finishBadge}\n<b>Kazanan: {winner?.NickName}</b>";
        }

        if (DetailsText != null)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.AppendLine("<b>─── OYUNCU PUANLARI ───</b>\n");

            foreach (var s in scores)
            {
                string colorHex = s.IsWinner ? "#4CAF50" : (s.RoundPenalty > 100 ? "#F44336" : "#FFC107");
                string sign = s.RoundPenalty > 0 ? "+" : "";
                sb.AppendLine($"<b>{s.NickName}</b>: <color={colorHex}>{sign}{s.RoundPenalty} Puan</color>  <i>({s.SummaryText})</i>  | Toplam: <b>{s.CumulativeScore}</b>");
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
