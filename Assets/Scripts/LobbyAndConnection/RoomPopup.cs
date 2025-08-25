using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RoomPopup : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI roomNameText;
    [SerializeField] private TextMeshProUGUI rulesText;
    [SerializeField] private TextMeshProUGUI playersCountText;
    [SerializeField] private Button joinButton;

    private string roomName;
    private LobbyManager lobbyManager;

    public void Setup(RoomInfo room, LobbyManager lobby)
    {
        lobbyManager = lobby;
        UpdateInfo(room);

        joinButton.onClick.AddListener(() => lobbyManager.JoinRoom(roomName));
    }

    public void UpdateInfo(RoomInfo room)
    {
        roomName = room.Name;
        roomNameText.text = room.Name;

        if (room.CustomProperties.TryGetValue("rules", out object rules))
            rulesText.text = rules.ToString();
        else
            rulesText.text = "Правила не заданы";

        playersCountText.text = $"{room.PlayerCount} / {room.MaxPlayers}";

        joinButton.interactable = room.PlayerCount < room.MaxPlayers;
    }
}
