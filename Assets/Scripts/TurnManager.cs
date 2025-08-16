using Photon.Pun;
using System;
using System.Collections.Generic;
using UnityEngine;

public class TurnManager : MonoBehaviourPunCallbacks
{
    public static TurnManager Instance { get; private set; }

    public float turnDuration = 90f;
    [SerializeField] private DiceManagerPhoton diceManager;

    private double turnStartTime; // врем€ старта хода (PhotonNetwork.Time)
    private int currentTurnPlayerId;

    private bool isTurnActive = false;

    // UI игроков, ключ Ч ActorNumber.ToString()
    private Dictionary<int, UIPlayerStats> playerStatsDict = new Dictionary<int, UIPlayerStats>();

    public event Action<int> TurnChanged;
    public int CurrentTurnPlayerId => currentTurnPlayerId;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Update()
    {
        if (!isTurnActive) return;

        double elapsed = PhotonNetwork.Time - turnStartTime;
        float timeLeft = Mathf.Clamp((float)(turnDuration - elapsed), 0f, turnDuration);

        UpdateTurnTimerOnAllClients(timeLeft);

        if (timeLeft <= 0f)
        {
            EndTurn();
        }
    }

    public void RegisterPlayerUI(int playerId, UIPlayerStats uiPlayerStats)
    {
        if (!playerStatsDict.ContainsKey(playerId))
        {
            playerStatsDict.Add(playerId, uiPlayerStats);
        }
    }

    public void StartRandomTurn()
    {
        if (!PhotonNetwork.IsMasterClient || playerStatsDict.Count == 0)
            return;

        var keys = new List<int>(playerStatsDict.Keys);
        int randomIndex = UnityEngine.Random.Range(0, keys.Count);
        int randomPlayerId = keys[randomIndex];

        StartTurn(randomPlayerId);
    }
    public void RequestRollDice(int requestingPlayerId)
    {
        // Ћюбой игрок вызывает бросок Ч отправл€ем запрос мастеру
        photonView.RPC(nameof(RPC_RequestSetNumbersDice), RpcTarget.MasterClient, requestingPlayerId);
    }

    [PunRPC]
    private void RPC_RequestSetNumbersDice(int requestingPlayerId)
    {
        if (!PhotonNetwork.IsMasterClient) return;

        // √енерируем числа кубиков
        int first = UnityEngine.Random.Range(1, 7);
        int second = UnityEngine.Random.Range(1, 7);

        // «апускаем кубики у всех клиентов с этими числами
        photonView.RPC(nameof(RPC_RequestSetNumbersDice), RpcTarget.AllBuffered, first,second,requestingPlayerId);

       
    }
    [PunRPC]
    private void RPC_RequestSetNumbersDice(int first, int second,int requestingPlayerId)
    {
        
        diceManager.StartDiceRollWithResult(first, second, requestingPlayerId);
    }
    public void StartTurn(int playerId)
    {
        if (!PhotonNetwork.IsMasterClient)
            return;

        currentTurnPlayerId = playerId;
        turnStartTime = PhotonNetwork.Time;
        isTurnActive = true;

        photonView.RPC("RPC_StartTurn", RpcTarget.All, playerId, turnStartTime);
        Debug.Log($"Master started turn for player {playerId}");
    }

    [PunRPC]
    private void RPC_StartTurn(int playerId, double startTime)
    {
        currentTurnPlayerId = playerId;
        turnStartTime = startTime;
        isTurnActive = true;

        TurnChanged?.Invoke(currentTurnPlayerId);
        Debug.Log($"RPC_StartTurn: current turn is {playerId}");
    }

    private void UpdateTurnTimerOnAllClients(float timeLeft)
    {
        foreach (var kvp in playerStatsDict)
        {
            if (kvp.Key == currentTurnPlayerId)
            {
                kvp.Value.SetTurnActive(true);
                kvp.Value.UpdateTurnTimer(timeLeft);
            }
            else
            {
                kvp.Value.SetTurnActive(false);
            }
        }
    }

    private void EndTurn()
    {
        if (!isTurnActive) return;

        isTurnActive = false;

        if (PhotonNetwork.IsMasterClient)
        {
            int nextPlayerId = GetNextPlayerId(currentTurnPlayerId);
            StartTurn(nextPlayerId);
            Debug.Log(nextPlayerId);
        }
    }

    private int GetNextPlayerId(int currentId)
    {
        var keys = new List<int>(playerStatsDict.Keys);
        int idx = keys.IndexOf(currentId);
        idx = (idx + 1) % keys.Count;
        return keys[idx];
    }

    public void RequestEndTurn()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            EndTurn();
        }
        else
        {
            photonView.RPC("RPC_RequestEndTurn", RpcTarget.MasterClient);
        }
    }

    [PunRPC]
    private void RPC_RequestEndTurn()
    {
        EndTurn();
    }
}
