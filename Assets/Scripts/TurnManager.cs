using Photon.Pun;
using Photon.Pun.Demo.PunBasics;
using Photon.Realtime;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
public enum TurnMode
{
    Normal,
    Auction,
    Trade  // новый режим для предложения договора
}
public class TurnManager : MonoBehaviourPunCallbacks
{
    public static TurnManager Instance { get; private set; }

    [Header("Settings")]
    [SerializeField] private float turnDuration = 90f;

    private double turnStartTime;
    private int currentTurnPlayerId;
    private bool isTurnActive = false;
    public int CurrentTurnPlayerId => currentTurnPlayerId;
    private HashSet<int> playersWithExtraTurn = new HashSet<int>();
    private float pausedTimeLeft;


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public override void OnEnable()
    {
        EventBus.Subscribe<AllPlayersInitializedEvent>(StartRandomTurn);
        EventBus.Subscribe<PlayerRolledDoubleEvent>(OnPlayerRolledDouble);

    }

    public override void OnDisable()
    {
        EventBus.Unsubscribe<AllPlayersInitializedEvent>(StartRandomTurn);
        EventBus.Unsubscribe<PlayerRolledDoubleEvent>(OnPlayerRolledDouble);

    }

    private void Update()
    {
        if (!isTurnActive) return;

        // считаем только если режим NORMAL
        if (CurrentMode != TurnMode.Normal) return;

        double elapsed = PhotonNetwork.Time - turnStartTime;
        float timeLeft = Mathf.Clamp((float)(turnDuration - elapsed), 0, turnDuration);

        UpdateTurnTimer(timeLeft);

        if (timeLeft <= 0f)
            EndTurnInternal();
    }

    #region Player UI

    private void UpdateTurnTimer(float timeLeft)
    {
        foreach (var player in PhotonNetwork.PlayerList)
        {
            bool isCurrent = player.ActorNumber == currentTurnPlayerId;
            EventBus.Publish(new TurnTimerUpdatedEvent(player.ActorNumber, timeLeft, isCurrent));
        }
    }

    #endregion Player UI

    #region Turn Management
    public void RegisterDoubleForTurn(int playerId, bool isDouble)
    {
        if (!PhotonNetwork.IsMasterClient) return;
        if (!isTurnActive) return;
        if (!isDouble) return;
        if (playerId != currentTurnPlayerId) return; // защита от гонок

        playersWithExtraTurn.Add(playerId);
    }
    public void StartRandomTurn(AllPlayersInitializedEvent e)
    {
        if (!PhotonNetwork.IsMasterClient || PhotonNetwork.PlayerList.Length == 0) return;

        var players = PhotonNetwork.PlayerList;
        var randomPlayer = players[UnityEngine.Random.Range(0, players.Length)];
        StartTurn(randomPlayer.ActorNumber);
    }

    public void StartTurn(int playerId)
    {
        if (!PhotonNetwork.IsMasterClient) return;

        currentTurnPlayerId = playerId;
        turnStartTime = PhotonNetwork.Time;
        isTurnActive = true;
        SetMode(TurnMode.Normal);

        photonView.RPC(nameof(RPC_StartTurn), RpcTarget.All, playerId, turnStartTime);
    }

    private void OnPlayerRolledDouble(PlayerRolledDoubleEvent e)
    {
        playersWithExtraTurn.Add(e.PlayerId);
    }
    [PunRPC]
    private void RPC_StartTurn(int playerId, double startTime)
    {
        currentTurnPlayerId = playerId;
        turnStartTime = startTime;
        isTurnActive = true;
        CurrentMode = TurnMode.Normal;

        PlayerData player = GameManager.Instance.GetPlayerById(playerId);
        if (player.IsInJail)
        {
            EventBus.Publish(new StartTurnJailEvent(playerId));

        }
        else
        {
            EventBus.Publish(new TurnStartEvent(playerId));

        }
        if (PhotonNetwork.IsMasterClient)
        {
            MortgageManager.Instance.TickMortgageTurnsRequest(playerId);
        }
    }
    public void OnAuctionEnded(int starterId)
    {
        if (!PhotonNetwork.IsMasterClient) return;

        SetMode(TurnMode.Normal);

        // Если игрок, который был инициатором аукциона, имеет дополнительный ход
        if (playersWithExtraTurn.Contains(starterId))
        {
            playersWithExtraTurn.Remove(starterId);
            StartTurn(starterId);
        }
        else
        {
            int nextPlayerId = GetNextPlayerId(starterId);
            StartTurn(nextPlayerId);
        }
    }

    
    public void RequestEndTurn()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            EndTurnInternal();
        }
        else
        {
            photonView.RPC(nameof(RPC_RequestEndTurn), RpcTarget.MasterClient);
        }
    }

    [PunRPC]
    private void RPC_RequestEndTurn() => EndTurnInternal();

    private void EndTurnInternal()
    {
        if (!isTurnActive) return;
        isTurnActive = false;

        if (!PhotonNetwork.IsMasterClient) return;

        if (playersWithExtraTurn.Contains(currentTurnPlayerId))
        {
            playersWithExtraTurn.Remove(currentTurnPlayerId);
            StartTurn(currentTurnPlayerId);
            return;
        }

        int nextPlayerId = GetNextPlayerId(currentTurnPlayerId);
        StartTurn(nextPlayerId);

    }

    public int GetNextPlayerId(int currentId)
    {
        var players = PhotonNetwork.PlayerList.OrderBy(p => p.ActorNumber).ToList();
        int idx = players.FindIndex(p => p.ActorNumber == currentId);
        idx = (idx + 1) % players.Count;
        return players[idx].ActorNumber;
    }
    public TurnMode CurrentMode { get; private set; } = TurnMode.Normal;

    public void SetMode(TurnMode mode)
    {
        if (CurrentMode == TurnMode.Normal && mode != TurnMode.Normal)
        {
            // уходим из Normal → сохраняем остаток
            double elapsed = PhotonNetwork.Time - turnStartTime;
            pausedTimeLeft = Mathf.Clamp((float)(turnDuration - elapsed), 0, turnDuration);
        }
        else if (CurrentMode != TurnMode.Normal && mode == TurnMode.Normal)
        {
            // возвращаемся в Normal → пересчитываем startTime
            turnStartTime = PhotonNetwork.Time - (turnDuration - pausedTimeLeft);
        }

        CurrentMode = mode;
    }

    #endregion Turn Management
}

public class BranchBuyRequestedEvent
{
    public int PlayerId { get; }
    public int CompanyId { get; }

    public BranchBuyRequestedEvent(int playerId, int companyId)
    {
        PlayerId = playerId;
        CompanyId = companyId;
    }
}

public class BranchSellRequestedEvent
{
    public int PlayerId { get; }
    public int CompanyId { get; }

    public BranchSellRequestedEvent(int playerId, int companyId)
    {
        PlayerId = playerId;
        CompanyId = companyId;
    }
}

public class BranchLevelChangedEvent
{
    public int CompanyId { get; }
    public int PlayerId { get; }
    public int NewLevel { get; }

    public BranchLevelChangedEvent(int companyId, int playerId, int newLevel)
    {
        CompanyId = companyId;
        PlayerId = playerId;
        NewLevel = newLevel;
    }
}

public class TurnTimerUpdatedEvent
{
    public int PlayerId;
    public float TimeLeft;
    public bool IsCurrent;

    public TurnTimerUpdatedEvent(int playerId, float timeLeft, bool isCurrent)
    {
        PlayerId = playerId;
        TimeLeft = timeLeft;
        IsCurrent = isCurrent;
    }
}

public class TurnStartEvent
{
    public int PlayerId;

    public TurnStartEvent(int playerId)
    {
        PlayerId = playerId;
    }
}

public class StartTurnJailEvent
{
    public int PlayerId;

    public StartTurnJailEvent(int playerId)
    {
        PlayerId = playerId;
    }
}