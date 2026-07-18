using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class TableManager : MonoBehaviourPunCallbacks
{
    // Okey masasý için gereken tam oyuncu sayýsý
    private readonly int requiredPlayers = 4;

    void Start()
    {
        // Sahne yüklendiðinde mevcut durumu kontrol et
        CheckPlayerCount();
    }

    // newPlayer kelimesinin solundaki Player kýsmýný aþaðýdaki gibi deðiþtiriyoruz
    public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer)
    {
        Debug.Log(newPlayer.NickName + " masaya oturdu!");
        CheckPlayerCount();
    }

    // Ayný þekilde burayý da güncelliyoruz
    public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer)
    {
        Debug.Log("Bir oyuncu masadan kalktý. Oyun beklemeye alýndý/iptal edildi.");
        CheckPlayerCount();
    }

    // Masadaki kiþi sayýsýný kontrol eden ana fonksiyonumuz
    private void CheckPlayerCount()
    {
        int currentPlayers = PhotonNetwork.CurrentRoom.PlayerCount;
        Debug.Log("Oyuncular bekleniyor... " + currentPlayers + "/" + requiredPlayers);

        // Eðer masada 4 kiþi olduysa
        if (currentPlayers == requiredPlayers)
        {
            Debug.Log("Masa doldu!");

            // ÇOK KRÝTÝK: Oyunu 4 kiþi ayný anda baþlatmaya çalýþmasýn!
            // Sadece odayý kuran kiþi (Masa Sahibi) taþlarý daðýtma komutunu versin.
            if (PhotonNetwork.IsMasterClient)
            {
                // GameManager'daki hazýrladýðýmýz fonksiyonu çaðýrýyoruz
                FindObjectOfType<GameManager>().OyunuBaslat();
            }
        }
    }
}