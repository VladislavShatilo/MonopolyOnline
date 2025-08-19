using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviourPunCallbacks
{
    public static GameManager Instance { get; private set; }

    [Header("Root Transforms")]
    [SerializeField] private Transform playersStatsContainer;
    [SerializeField] private Transform playerRoot;
    [SerializeField] private Transform cellsRoot;

    [Header("Prefabs")]
    [SerializeField] private UIPlayerStats playerStatsPrefab;
    [SerializeField] private GameObject playerPiecePrefab;

    [Header("Settings")]
    [SerializeField] private Color[] playerColors;
    [SerializeField] private Vector3 startPlayerPosition = new Vector3(-240f, 390f, 0f);

    private readonly List<PlayerData> players = new();
    private readonly Dictionary<int, PlayerMove> playerMoves = new();
    private readonly Dictionary<int, UIPlayerStats> uiPlayerStatsDict = new();

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
        InitializeAllPlayersUI();
        SpawnLocalPlayerIfNeeded();
        TryStartGame();
    }

    #region Photon Callbacks

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Debug.Log($"Player {newPlayer.NickName} joined (actor {newPlayer.ActorNumber})");
        SetupPlayerUI(newPlayer);
        TryStartGame();
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        Debug.Log($"Player {otherPlayer.NickName} left (actor {otherPlayer.ActorNumber})");
        RemovePlayer(otherPlayer.ActorNumber);
    }

    #endregion

    #region Player Management

    private void InitializeAllPlayersUI()
    {
        foreach (var p in PhotonNetwork.PlayerList)
            SetupPlayerUI(p);
    }

    private void SetupPlayerUI(Player photonPlayer)
    {
        int playerId = photonPlayer.ActorNumber;

        if (players.Exists(p => p.id == playerId)) return;

        // Instantiate UI
        var uiStats = Instantiate(playerStatsPrefab, playersStatsContainer);
        int colorIndex = Mathf.Clamp(playerId - 1, 0, playerColors.Length - 1);
        PlayerData player = new(photonPlayer.NickName, 100_000, playerId, playerColors[colorIndex], photonPlayer);

        players.Add(player);
        uiStats.SetPlayerStats(player);
        uiPlayerStatsDict[playerId] = uiStats;

        // Register UI for turn updates
        TurnManager.Instance.RegisterPlayerUI(playerId, uiStats);

        // Listen to bank updates
        Bank.Instance.OnBalanceChanged += (changedPlayer, money) =>
        {
            if (changedPlayer.id == player.id)
                uiStats.SetMoneyPlayerText(money);
        };
    }

    private void RemovePlayer(int playerId)
    {
        var playerData = GetPlayerById(playerId);
        if (playerData != null) players.Remove(playerData);

        if (uiPlayerStatsDict.TryGetValue(playerId, out var ui))
        {
            Destroy(ui.gameObject);
            uiPlayerStatsDict.Remove(playerId);
        }

        if (playerMoves.TryGetValue(playerId, out var move))
        {
            if (move != null && move.photonView != null && move.photonView.IsMine)
                PhotonNetwork.Destroy(move.gameObject);
            else if (move != null)
                Destroy(move.gameObject);

            playerMoves.Remove(playerId);
        }
    }

    private void SpawnLocalPlayerIfNeeded()
    {
        int localId = PhotonNetwork.LocalPlayer.ActorNumber;
        if (playerMoves.ContainsKey(localId)) return;

        int colorIndex = Mathf.Clamp(localId - 1, 0, playerColors.Length - 1);
        GameObject playerPiece = PhotonNetwork.Instantiate(
            playerPiecePrefab.name,
            startPlayerPosition,
            Quaternion.identity,
            0,
            new object[] { colorIndex }
        );

        var pm = playerPiece.GetComponent<PlayerMove>();
        pm.id = localId;
        playerMoves[localId] = pm;
    }

    #endregion

    #region Game Flow

    private void TryStartGame()
    {
        if (!PhotonNetwork.InRoom) return;

        if (PhotonNetwork.CurrentRoom.PlayerCount == PhotonNetwork.CurrentRoom.MaxPlayers &&
            PhotonNetwork.IsMasterClient)
        {
            TurnManager.Instance.StartRandomTurn();
        }
    }

    #endregion

    #region Helpers

    public PlayerData GetPlayerById(int id)
    {
        return players.Find(p => p.id == id);
    }

    public Color GetColorByIndex(int idx)
    {
        if (playerColors == null || playerColors.Length == 0) return Color.white;
        return playerColors[Mathf.Clamp(idx, 0, playerColors.Length - 1)];
    }

    public Color GetColorForActor(int actorNumber) => GetColorByIndex(actorNumber - 1);

    #endregion
}
