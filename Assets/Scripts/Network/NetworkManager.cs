using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class NetworkManager : MonoBehaviourPunCallbacks
{
    public static NetworkManager Instance { get; private set; }

    private const int MaxPlayersPerRoom = 4;
    private const int GameSceneBuildIndex = 1;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        if (!PhotonNetwork.IsConnected)
        {
            Debug.Log("[NetworkManager] Sunucuya bağlanılıyor...");
            PhotonNetwork.ConnectUsingSettings();
            PhotonNetwork.AutomaticallySyncScene = true;
        }
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("[NetworkManager] Master Server'a bağlanıldı! Lobiye geçiliyor...");
        PhotonNetwork.JoinLobby();
    }

    public override void OnJoinedLobby()
    {
        Debug.Log("[NetworkManager] Lobiye girildi.");
    }

    public void CreateRoom()
    {
        string roomName = "Masa_" + Random.Range(1000, 9999);
        Debug.Log($"[NetworkManager] Oda kuruluyor: {roomName}");
        RoomOptions roomOptions = new RoomOptions { MaxPlayers = MaxPlayersPerRoom };
        PhotonNetwork.CreateRoom(roomName, roomOptions);
    }

    public void JoinRandomRoom()
    {
        Debug.Log("[NetworkManager] Rastgele bir odaya katılınıyor...");
        PhotonNetwork.JoinRandomRoom();
    }

    public override void OnJoinedRoom()
    {
        Debug.Log($"[NetworkManager] Odaya girildi! Masadaki kişi sayısı: {PhotonNetwork.CurrentRoom.PlayerCount}");

        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.LoadLevel(GameSceneBuildIndex);
        }
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.Log("[NetworkManager] Boş oda bulunamadı! Yeni masa kuruluyor...");
        CreateRoom();
    }
}
