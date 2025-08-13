using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SocialPlatforms;

public class GameManager : MonoBehaviourPunCallbacks
{
    public static GameManager Instance { get; private set; }

    [Header("Root transforms")]
    [SerializeField] private Transform playersStatsContainerTrans;
    [SerializeField] private Transform playerRootGO;
    [SerializeField] private Transform cellsRootTransforms;

    [Header("Prefabs")]
    [SerializeField] private UIPlayerStats playersStatsPrefab;
    [SerializeField] private GameObject playerPiecePrefab; 

    [Header("Settings")]
    [SerializeField] private Color[] playerColors;
    [SerializeField] private Vector3 startPlayerPosition = new Vector3(-240f, 390f, 0f);

    private List<PlayerData> players = new List<PlayerData>();
    private Dictionary<int, PlayerMove> playerMoves = new Dictionary<int, PlayerMove>();
    private Dictionary<int, UIPlayerStats> uiPlayerStatsDict = new Dictionary<int, UIPlayerStats>();

    public Transform CellsRootTransforms => cellsRootTransforms;
    public Transform PlayerRootTransform => playerRootGO;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        Debug.Log($"GameManager создан через: {Environment.StackTrace}");

        Debug.Log($"[GameManager] Awake в {Time.time}, Scene: {gameObject.scene.name}, InstanceID: {GetInstanceID()}");

    }

    private void Start()
    {
        Debug.Log($"[GameManager] Start в {Time.time}, Scene: {gameObject.scene.name}, InstanceID: {GetInstanceID()}");

        Debug.Log("Start: already in room, creating UI and local player if needed.");
          CreateAllPlayersUI();
          CreateLocalPlayerIfNeeded();
          CheckStartGame();
        
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Debug.Log($"{newPlayer.NickName} зашЄл в комнату (actor {newPlayer.ActorNumber}).");
        SetupPlayerUI(newPlayer);
        CheckStartGame();
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        Debug.Log($"{otherPlayer.NickName} вышел из комнаты (actor {otherPlayer.ActorNumber}).");
        int id = otherPlayer.ActorNumber;

        var p = GetPlayerById(id);
        if (p != null) players.Remove(p);

        if (uiPlayerStatsDict.TryGetValue(id, out var ui))
        {
            Destroy(ui.gameObject);
            uiPlayerStatsDict.Remove(id);
        }

        if (playerMoves.TryGetValue(id, out var pm))
        {
            // попытка уничтожить сетевой объект
            if (pm != null && pm.photonView != null && pm.photonView.IsMine)
            {
                PhotonNetwork.Destroy(pm.gameObject);
            }
            else if (pm != null && pm.gameObject != null)
            {
                Destroy(pm.gameObject); // если не владеем Ч просто уничтожим локально
            }
            playerMoves.Remove(id);
        }
    }

    private void CreateAllPlayersUI()
    {
        foreach (var p in PhotonNetwork.PlayerList)
            SetupPlayerUI(p);
    }

    private void SetupPlayerUI(Player photonPlayer)
    {
        int playerId = photonPlayer.ActorNumber;

        if (players.Exists(x => x.id == playerId))
            return;

        var uiInst = Instantiate(playersStatsPrefab, playersStatsContainerTrans);
        var uiPlayerStats = uiInst as UIPlayerStats;

        int colorIndex = Mathf.Clamp(playerId - 1, 0, playerColors.Length - 1);
        PlayerData player = new PlayerData(
            photonPlayer.NickName,
            15000,
            playerId,
            playerColors[colorIndex],
            photonPlayer
        );

        players.Add(player);
        uiPlayerStats.SetPlayerStats(player);
        uiPlayerStatsDict[playerId] = uiPlayerStats;

        TurnManager.Instance.RegisterPlayerUI(playerId, uiPlayerStats);

        Bank.Instance.OnBalanceChanged += (changedPlayer, money) =>
        {
            if (changedPlayer.id == player.id)
                uiPlayerStats.SetMoneyPlayerText(money);
        };
    }

    private void CreateLocalPlayerIfNeeded()
    {

        int localId = PhotonNetwork.LocalPlayer.ActorNumber;
        if (playerMoves.ContainsKey(localId)) return;

        int colorIndex = Mathf.Clamp(localId - 1, 0, playerColors.Length - 1);

        // передаЄм индекс цвета через instantiationData, чтобы все копии получили одинаковый цвет
        GameObject playerPiece = PhotonNetwork.Instantiate(
            playerPiecePrefab.name,
            startPlayerPosition,
            Quaternion.identity,
            0,
            new object[] { colorIndex }
        );

        var pm = playerPiece.GetComponent<PlayerMove>();
        pm.id = localId;

        // локально можно настроить дополнительные вещи, но SetRootTransform будет вызван в Start() у PlayerMove на всех клиентах
        playerMoves[localId] = pm;
    }

    private void CheckStartGame()
    {
        if (!PhotonNetwork.InRoom) return;

        if (PhotonNetwork.CurrentRoom.PlayerCount == PhotonNetwork.CurrentRoom.MaxPlayers &&
            PhotonNetwork.IsMasterClient)
        {
            TurnManager.Instance.StartRandomTurn();
        }
    }

    public PlayerData GetPlayerById(int id)
    {
        return players.Find(p => p.id == id);
    }

    public Color GetColorByIndex(int idx)
    {
        if (playerColors == null || playerColors.Length == 0) return Color.white;
        return playerColors[Mathf.Clamp(idx, 0, playerColors.Length - 1)];
    }

    public Color GetColorForActor(int actorNumber)
    {
        return GetColorByIndex(actorNumber - 1);
    }
}
