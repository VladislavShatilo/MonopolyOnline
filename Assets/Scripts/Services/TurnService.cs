using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;

public class TurnService : ITurnService
{
    private IPlayerRepository playerRepository;
    private IPhotonTurnSynchronizer photonTurnSynchronizer;
    private ITimerManager timerManager;
    private GameSettings gameSettings;

    private Turn turn;

    #region LIFE_CYCLE

    [Inject]
    public void Construct(IPlayerRepository playerRepository, IPhotonTurnSynchronizer photonTurnSynchronizer, ITimerManager timerManager, GameSettings gameSettings)
    {
        this.playerRepository = playerRepository ?? throw new ArgumentNullException(nameof(playerRepository));
        this.photonTurnSynchronizer = photonTurnSynchronizer ?? throw new ArgumentNullException(nameof(photonTurnSynchronizer));
        this.timerManager = timerManager ?? throw new ArgumentNullException(nameof(timerManager));
        this.gameSettings = gameSettings ?? throw new ArgumentNullException(nameof(gameSettings));
        turn = new Turn();
    }

    #endregion LIFE_CYCLE

    #region PUBLIC_METHODS

    public void StartRandomTurn()
    {
        var players = playerRepository.GetAllPlayers() ?? throw new InvalidOperationException(nameof(StartRandomTurn));
        if (players.Count == 0) return;
        var randomPlayer = players[UnityEngine.Random.Range(0, players.Count)];
        StartTurn(randomPlayer.Id, true);
    }

    public void StartTurn(int playerId, bool isNext)
    {
        turn.StartTurn(playerId);
        timerManager.StartTurnTimer(playerId, gameSettings.turnTime);
        photonTurnSynchronizer.RequestStartTurn(playerId, isNext);
    }

    public void EndTurn()
    {
        if (!turn.IsActive) return;

        var currentId = turn.CurrentPlayerId;

        if (turn.HasExtraTurn(currentId))
        {
            var pd = playerRepository.GetPlayerById(currentId) ?? throw new InvalidOperationException(nameof(EndTurn));

            if (pd.SkipNextTurn)
            {
                pd.SkipNextTurn = false;
                turn.RemoveExtraTurn(currentId);
            }
            else
            {
                turn.RemoveExtraTurn(currentId);
                StartTurn(currentId, false);
                return;
            }
        }

        var nextPlayer = playerRepository.GetNextPlayerId(currentId) ?? throw new InvalidOperationException(nameof(EndTurn));
        if (nextPlayer != null)
        {
            if (nextPlayer.SkipNextTurn)
            {
                nextPlayer.SkipNextTurn = false;
                StartTurn(currentId, true);

                return;
            }

            StartTurn(nextPlayer.Id, true);
        }
    }

    public void RegisterDouble(int playerId)
    {
        var player = playerRepository.GetPlayerById(playerId) ?? throw new InvalidOperationException(nameof(RegisterDouble));
        if (player.SkipNextTurn) return;

        turn.AddExtraTurn(playerId);
    }

    public void SetMode(TurnMode mode) => turn.SetMode(mode);

    #endregion PUBLIC_METHODS
}

public enum TurnMode
{
    Normal,
    Auction,
    Trade  
}