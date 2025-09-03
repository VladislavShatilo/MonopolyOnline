using Photon.Pun;
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

    public TurnMode CurrentMode { get; private set; } = TurnMode.Normal;

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
    #endregion

    #region Turn Management

    private void OnPlayerRolledDouble(PlayerRolledDoubleEvent e)
    {
        RegisterDoubleForTurn(e.PlayerId, e.IsDouble);
    }

    public void RegisterDoubleForTurn(int playerId, bool isDouble)
    {
        if (!PhotonNetwork.IsMasterClient) return;
        if (!isTurnActive || !isDouble) return;
        if (playerId != currentTurnPlayerId) return;

        var pd = GameManager.Instance.GetPlayerById(playerId);
        if (pd == null) return;

        if (pd.SkipNextTurn)
        {
            Debug.Log($"TurnManager: player {playerId} rolled double but SkipNextTurn=true -> ignoring extra-turn");
            return;
        }

        playersWithExtraTurn.Add(playerId);
    }

    public void StartRandomTurn(AllPlayersInitializedEvent e)
    {
        if (!PhotonNetwork.IsMasterClient || PhotonNetwork.PlayerList.Length == 0) return;

        var players = PhotonNetwork.PlayerList;
        var randomPlayer = players[UnityEngine.Random.Range(0, players.Length)];
        StartTurn(randomPlayer.ActorNumber, true);
    }

    public void StartTurn(int playerId,bool isNext)
    {
        if (!PhotonNetwork.IsMasterClient) return;

        currentTurnPlayerId = playerId;
        turnStartTime = PhotonNetwork.Time;
        isTurnActive = true;
        SetMode(TurnMode.Normal);

        photonView.RPC(nameof(RPC_StartTurn), RpcTarget.All, playerId, turnStartTime, isNext);
    }

    [PunRPC]
    private void RPC_StartTurn(int playerId, double startTime,bool isNext)
    {
        currentTurnPlayerId = playerId;
        turnStartTime = startTime;
        isTurnActive = true;
        CurrentMode = TurnMode.Normal;
        PlayerData player = GameManager.Instance.GetPlayerById(playerId);

        if (isNext)
        {
            if (player.HasLoan)
            {
                EventBus.Publish(new OnStartTurnLoanEvent(playerId));

            }
        }
      
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

        if (playersWithExtraTurn.Contains(starterId))
        {
            var pd = GameManager.Instance.GetPlayerById(starterId);
            if (pd != null && pd.SkipNextTurn)
            {
                playersWithExtraTurn.Remove(starterId);
                Debug.Log($"TurnManager: auction ended, {starterId} had extra turn but SkipNextTurn=true -> skipping");
                int nextId = GetNextNonSkippingPlayer(starterId);
                StartTurn(nextId, true);
            }
            else
            {
                playersWithExtraTurn.Remove(starterId);
                StartTurn(starterId, false);
            }
        }
        else
        {
            int nextPlayerId = GetNextNonSkippingPlayer(starterId);
            StartTurn(nextPlayerId, true);
        }
    }

    public void RequestEndTurn()
    {
        if (PhotonNetwork.IsMasterClient)
            EndTurnInternal();
        else
            photonView.RPC(nameof(RPC_RequestEndTurn), RpcTarget.MasterClient);
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
            var currentPd = GameManager.Instance.GetPlayerById(currentTurnPlayerId);
            if (currentPd != null && currentPd.SkipNextTurn)
            {
                playersWithExtraTurn.Remove(currentTurnPlayerId);
                Debug.Log($"TurnManager: {currentTurnPlayerId} had extra turn but SkipNextTurn=true -> extra-turn removed");
            }
            else
            {
                playersWithExtraTurn.Remove(currentTurnPlayerId);
                StartTurn(currentTurnPlayerId, false);
                return;
            }
        }

        int nextPlayerId = GetNextNonSkippingPlayer(currentTurnPlayerId);
        StartTurn(nextPlayerId, true);
    }

    private int GetNextNonSkippingPlayer(int fromId)
    {
        int nextId = GetNextPlayerId(fromId);
        int attempts = 0;
        int maxAttempts = PhotonNetwork.PlayerList.Length;

        while (attempts < maxAttempts)
        {
            var pd = GameManager.Instance.GetPlayerById(nextId);
            if (pd != null && pd.SkipNextTurn)
            {
                pd.SkipNextTurn = false;
                nextId = GetNextPlayerId(nextId);
                attempts++;
                continue;
            }
            break;
        }
        return nextId;
    }

    public int GetNextPlayerId(int currentId)
    {
        var players = PhotonNetwork.PlayerList.OrderBy(p => p.ActorNumber).ToList();
        int idx = players.FindIndex(p => p.ActorNumber == currentId);
        idx = (idx + 1) % players.Count;
        return players[idx].ActorNumber;
    }

    public void SetMode(TurnMode mode)
    {
        if (CurrentMode == TurnMode.Normal && mode != TurnMode.Normal)
        {
            double elapsed = PhotonNetwork.Time - turnStartTime;
            pausedTimeLeft = Mathf.Clamp((float)(turnDuration - elapsed), 0, turnDuration);
        }
        else if (CurrentMode != TurnMode.Normal && mode == TurnMode.Normal)
        {
            turnStartTime = PhotonNetwork.Time - (turnDuration - pausedTimeLeft);
        }

        CurrentMode = mode;
    }
    #endregion

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