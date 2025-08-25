using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviourPunCallbacks
{
    public static GameManager Instance { get; private set; }

    [Header("Root Transforms")]
    [SerializeField] private Transform playerRoot;
    [SerializeField] private Transform cellsRoot;

    [Header("Prefabs")]
    [SerializeField] private GameObject playerPiecePrefab;

    [Header("Settings")]
    [SerializeField] private Color[] playerColors;
    [SerializeField] private Vector3 startPlayerPosition = new Vector3(-240f, 390f, 0f);

    private readonly List<PlayerData> players = new();
    private readonly Dictionary<int, PlayerMove> playerMoves = new();

    public Transform CellsRoot => cellsRoot;
    public Transform PlayerRoot => playerRoot;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        InitializeAllPlayers();
        SpawnLocalPlayerIfNeeded();
        TryStartGame();
    }

    #region Photon Callbacks

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Debug.Log($"Player {newPlayer.NickName} joined (actor {newPlayer.ActorNumber})");
        SetupPlayer(newPlayer);
        TryStartGame();
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        Debug.Log($"Player {otherPlayer.NickName} left (actor {otherPlayer.ActorNumber})");
        RemovePlayer(otherPlayer.ActorNumber);
    }

    #endregion

    #region Player Management

    private void InitializeAllPlayers()
    {
        foreach (var p in PhotonNetwork.PlayerList)
            SetupPlayer(p);
    }

    private void SetupPlayer(Player photonPlayer)
    {
        int playerId = photonPlayer.ActorNumber;
        if (players.Exists(p => p.id == playerId)) return;

        PlayerData player = new(photonPlayer.NickName, 100_000, playerId, GetColorForActor(playerId), photonPlayer);
        players.Add(player);

        // Событие
        EventBus.Publish(new PlayerJoinedEvent(player));
    }

    private void SpawnLocalPlayerIfNeeded()
    {
        int localId = PhotonNetwork.LocalPlayer.ActorNumber;
        if (playerMoves.ContainsKey(localId)) return;

        GameObject playerPiece = PhotonNetwork.Instantiate(
            playerPiecePrefab.name,
            startPlayerPosition,
            Quaternion.identity,
            0,
            new object[] { localId - 1 }
        );

        var pm = playerPiece.GetComponent<PlayerMove>();
        playerMoves[localId] = pm;

    }

    private void RemovePlayer(int playerId)
    {
        var playerData = GetPlayerById(playerId);
        if (playerData != null) players.Remove(playerData);

        if (playerMoves.TryGetValue(playerId, out var move))
        {
            if (move != null && move.photonView != null && move.photonView.IsMine)
                PhotonNetwork.Destroy(move.gameObject);
            else if (move != null)
                Destroy(move.gameObject);

            playerMoves.Remove(playerId);
        }

        EventBus.Publish(new PlayerLeftEvent(playerId));
    }

    #endregion

    #region Game Flow

    private void TryStartGame()
    {
        if (!PhotonNetwork.InRoom) return;

        if (PhotonNetwork.CurrentRoom.PlayerCount == PhotonNetwork.CurrentRoom.MaxPlayers &&
            PhotonNetwork.IsMasterClient)
        {
            EventBus.Publish(new AllPlayersInitializedEvent(players));
        }
    }

    #endregion

    #region Helpers

    public PlayerData GetPlayerById(int id) => players.Find(p => p.id == id);
    public List<PlayerData> Players() => players;

    public Color GetColorForActor(int actorNumber)
    {
        if (playerColors == null || playerColors.Length == 0) return Color.white;
        return playerColors[actorNumber - 1];
    }

    #endregion
}
public class PlayerJoinedEvent
{
    public PlayerData Player { get; }

    public PlayerJoinedEvent(PlayerData player)
    {
        Player = player;
    }
}

public class PlayerLeftEvent
{
    public int PlayerId { get; }

    public PlayerLeftEvent(int playerId)
    {
        PlayerId = playerId;
    }
}

public class AllPlayersInitializedEvent
{
    public List<PlayerData> Players { get; }

    public AllPlayersInitializedEvent(List<PlayerData> players)
    {
        Players = players;
    }
}


public class TryStartGameEvent { }