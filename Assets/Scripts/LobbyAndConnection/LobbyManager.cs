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

    private readonly Dictionary<string, GameObject> roomUIWindows = new();

    #region LIFE_CYCLE

    private void Start()
    {
        PhotonNetwork.JoinLobby();
        createRoomButton.onClick.AddListener(() => OnCreateRoomClicked());
        PhotonNetwork.ConnectUsingSettings();
    }

    #endregion LIFE_CYCLE

    #region PRIVATE_METHODS

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

    #endregion PRIVATE_METHODS

    #region CALLBACKS

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        Debug.Log("OnRoomListUpdate");
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

    public override void OnJoinedRoom()
    {
        PhotonNetwork.LoadLevel("GameScene"); // та же игровая сцена
    }

    public override void OnCreatedRoom()
    {
        Debug.Log("Комната создана: " + PhotonNetwork.CurrentRoom.Name);
        CreateRoomUI(PhotonNetwork.CurrentRoom);
    }

    public void OnJoinRoomClicked(string roomName)
    {
        PhotonNetwork.JoinRoom(roomName);
    }

    public void OnCreateRoomClicked()
    {
        string roomName = "Room_" + Random.Range(1000, 9999);
        RoomOptions options = new RoomOptions
        {
            MaxPlayers = 2,
            CustomRoomProperties = new ExitGames.Client.Photon.Hashtable
            {
                { "rules", "Правила: играем до 100 очков, без обмена картами" }
            },
            CustomRoomPropertiesForLobby = new string[] { "rules" }
        };
        PhotonNetwork.CreateRoom(roomName, options);
    }

    #endregion CALLBACKS
}