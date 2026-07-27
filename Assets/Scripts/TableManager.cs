using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using TMPro; // TextMeshPro kullanmak için bu kütüphaneyi ekliyoruz

public class TableManager : MonoBehaviourPunCallbacks
{
    // Unity arayüzünden sürükleyip býrakacaðýmýz yazý objesi
    public TextMeshProUGUI waitingText;

    private readonly int requiredPlayers = 4;

    void Start()
    {
        CheckPlayerCount();
    }

    public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer)
    {
        Debug.Log(newPlayer.NickName + " masaya oturdu!");
        CheckPlayerCount();
    }

    public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer)
    {
        Debug.Log("Bir oyuncu masadan kalktý.");
        CheckPlayerCount();
    }

    private void CheckPlayerCount()
    {
        int currentPlayers = PhotonNetwork.CurrentRoom.PlayerCount;

        // Ekrana kaç kiþi olduðunu yazdýrýyoruz
        if (waitingText != null)
        {
            waitingText.text = "Oyuncular Bekleniyor... " + currentPlayers + " / " + requiredPlayers;
        }

        if (currentPlayers == requiredPlayers)
        {
            Debug.Log("Masa doldu!");

            // 4 kiþi dolduðunda bekleme yazýsýný ekrandan gizle
            if (waitingText != null)
            {
                waitingText.gameObject.SetActive(false);
            }

            if (PhotonNetwork.IsMasterClient)
            {
                FindObjectOfType<GameManager>().OyunuBaslat();
            }
        }
    }
}