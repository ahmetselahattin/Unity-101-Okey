using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using TMPro;

public class TableManager : MonoBehaviourPunCallbacks
{
    [Header("UI Referans")]
    public TextMeshProUGUI waitingText;

    [Header("Test Modu")]
    public bool testModuAktif = false;

    private const int RequiredPlayers = 4;

    private void Start()
    {
        CheckPlayerCount();
    }

    public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer)
    {
        Debug.Log($"[TableManager] {newPlayer.NickName} masaya oturdu.");
        CheckPlayerCount();
    }

    public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer)
    {
        Debug.Log("[TableManager] Bir oyuncu masadan kalkt.");
        CheckPlayerCount();
    }

    private void CheckPlayerCount()
    {
        if (testModuAktif)
        {
            if (waitingText != null) waitingText.gameObject.SetActive(false);

            GameManager gm = FindObjectOfType<GameManager>();
            if (gm != null) gm.OyunuBaslat();
            return;
        }

        int currentPlayers = (PhotonNetwork.CurrentRoom != null) ? PhotonNetwork.CurrentRoom.PlayerCount : 1;

        if (waitingText != null)
        {
            waitingText.text = $"Oyuncular Bekleniyor... {currentPlayers} / {RequiredPlayers}";
        }

        if (currentPlayers >= RequiredPlayers)
        {
            Debug.Log("[TableManager] Masa doldu! Oyun balatlyor.");

            if (waitingText != null)
            {
                waitingText.gameObject.SetActive(false);
            }

            if (PhotonNetwork.IsMasterClient)
            {
                GameManager gm = FindObjectOfType<GameManager>();
                if (gm != null) gm.OyunuBaslat();
            }
        }
    }
}
