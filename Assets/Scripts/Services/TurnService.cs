using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;

public class TurnService : ITurnService
{
    private  IPlayerRepository playerRepository;
    private IPhotonTurnSynchronizer photonTurnSynchronizer;
    private  Turn turn;

    [Inject]
    public void Construct(IPlayerRepository playerRepository, IPhotonTurnSynchronizer photonTurnSynchronizer)
    {
        this.playerRepository = playerRepository;
        this.photonTurnSynchronizer = photonTurnSynchronizer;
        turn = new Turn();
    }

    public void StartRandomTurn()
    {
        var players = playerRepository.GetAllPlayers();
        if (players.Count == 0) return;
        var randomPlayer = players[UnityEngine.Random.Range(0, players.Count)];
        StartTurn(randomPlayer.Id, true);
    }

    public void StartTurn(int playerId, bool isNext)
    {
        turn.StartTurn(playerId);
        photonTurnSynchronizer.RequestStartTurn(playerId, isNext);

    }

    public void EndTurn()
    {
        if (!turn.IsActive) return;

        var currentId = turn.CurrentPlayerId;

        if (turn.HasExtraTurn(currentId))
        {
            var pd = playerRepository.GetPlayerById(currentId);
            if (pd.SkipNextTurn)
            {
                turn.RemoveExtraTurn(currentId);
            }
            else
            {
                turn.RemoveExtraTurn(currentId);
                StartTurn(currentId, false);
                return;
            }
        }
        int nextId = playerRepository.GetNextPlayerId(currentId).Id;

        StartTurn(nextId, true);
    }

    public void RegisterDouble(int playerId, bool isDouble)
    {
        if (!isDouble) return;

        var player = playerRepository.GetPlayerById(playerId);
        if (player.SkipNextTurn) return;

        turn.AddExtraTurn(playerId);
    }

    public void SetMode(TurnMode mode) => turn.SetMode(mode);
}
public enum TurnMode
{
    Normal,
    Auction,
    Trade  // новый режим для предложения договора
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