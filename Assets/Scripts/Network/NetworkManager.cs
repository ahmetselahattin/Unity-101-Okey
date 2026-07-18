using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class NetworkManager : MonoBehaviourPunCallbacks
{
    void Start()
    {
        Debug.Log("Sunucuya baðlanýlýyor...");
        PhotonNetwork.ConnectUsingSettings();

        // ÇOK KRÝTÝK: Kurucu (Master Client) sahne deðiþtirdiðinde, odadaki herkes otomatik onunla o sahneye gitsin.
        PhotonNetwork.AutomaticallySyncScene = true;
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("Master Server'a baðlanýldý! Lobiye geçiliyor...");
        PhotonNetwork.JoinLobby();
    }

    public override void OnJoinedLobby()
    {
        Debug.Log("Lobiye girildi! Artýk butonlara basabilirsin.");
    }

    // --- BUTONLARA BAÐLAYACAÐIMIZ FONKSÝYONLAR ---

    public void CreateRoom()
    {
        Debug.Log("Oda kuruluyor...");
        // Odaya rastgele bir isim veriyoruz, okey olduðu için maksimum 4 kiþi girebilir
        RoomOptions roomOptions = new RoomOptions { MaxPlayers = 4 };
        PhotonNetwork.CreateRoom("Masa_" + Random.Range(1000, 9999), roomOptions);
    }

    public void JoinRandomRoom()
    {
        Debug.Log("Rastgele bir odaya katýlýnýyor...");
        PhotonNetwork.JoinRandomRoom();
    }

    // --- ODA (MASA) ÝÞLEMLERÝ ---

    public override void OnJoinedRoom()
    {
        Debug.Log("Odaya baþarýyla girildi! Masadaki kiþi sayýsý: " + PhotonNetwork.CurrentRoom.PlayerCount);

        // Eðer odayý biz kurduysak (Master Client isek), oyun sahnesini biz yükleriz.
        // Diðer oyuncular "AutomaticallySyncScene = true" komutu sayesinde otomatik olarak peþimizden gelir!
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.LoadLevel(1); // Build Settings'deki 1 numaralý sahneyi (GameScene) yükle
        }
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.Log("Boþ oda bulunamadý! Kendi masamýzý kuruyoruz...");
        CreateRoom(); // Eðer girecek boþ masa yoksa, direkt kendi masamýzý kuralým
    }
}