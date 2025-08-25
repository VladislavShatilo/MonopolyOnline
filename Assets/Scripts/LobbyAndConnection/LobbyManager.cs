using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using ExitGames.Client.Photon;
using UnityEngine.UI;
public class LobbyManager : MonoBehaviourPunCallbacks
{
    [Header("UI")]
    [SerializeField] private Transform roomListContainer;
    [SerializeField] private GameObject roomPopupPrefab;
    [SerializeField] private Button createRoomButton;

    private Dictionary<string, GameObject> roomUIWindows = new Dictionary<string, GameObject>();

    private void Start()
    {
        PhotonNetwork.JoinLobby();
        createRoomButton.onClick.AddListener(() => CreateRoom());
        PhotonNetwork.ConnectUsingSettings();
    }
    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        foreach (RoomInfo room in roomList)
        {
            if (room.RemovedFromList)
            {
                if (roomUIWindows.TryGetValue(room.Name, out GameObject go))
                {
                    Destroy(go);
                    roomUIWindows.Remove(room.Name);
                }
            }
            else
            {
                if (roomUIWindows.ContainsKey(room.Name))
                    UpdateRoomUI(room);
                else
                    CreateRoomUI(room);
            }
        }
    }

    private void CreateRoomUI(RoomInfo room)
    {
        if (roomUIWindows.ContainsKey(room.Name))
            return; // Уже есть

        GameObject go = Instantiate(roomPopupPrefab, roomListContainer);
        roomUIWindows.Add(room.Name, go);

        RoomPopup popup = go.GetComponent<RoomPopup>();
        popup.Setup(room, this);
    }

    private void UpdateRoomUI(RoomInfo room)
    {
        if (roomUIWindows.TryGetValue(room.Name, out GameObject go))
        {
            RoomPopup popup = go.GetComponent<RoomPopup>();
            popup.UpdateInfo(room);
        }
    }

    public void JoinRoom(string roomName)
    {
        PhotonNetwork.JoinRoom(roomName);
    }
  
    // Коллбэк, когда присоединились к комнате
    public override void OnJoinedRoom()
    {
        Debug.Log("Присоединились к комнате, загружаем игровую сцену...");
        PhotonNetwork.LoadLevel("GameScene"); // та же игровая сцена
    }
    public void CreateRoom()
    {
        string roomName = "Room_" + Random.Range(1000, 9999);
        RoomOptions options = new RoomOptions
        {
            MaxPlayers = 3,
            CustomRoomProperties = new ExitGames.Client.Photon.Hashtable
            {
                { "rules", "Правила: играем до 100 очков, без обмена картами" }
            },
            CustomRoomPropertiesForLobby = new string[] { "rules" }
        };
        PhotonNetwork.CreateRoom(roomName, options);
    }

    // Вот тут создаём UI сразу после создания комнаты
    public override void OnCreatedRoom()
    {
        Debug.Log("Комната создана: " + PhotonNetwork.CurrentRoom.Name);
        CreateRoomUI(PhotonNetwork.CurrentRoom);
    }

}
