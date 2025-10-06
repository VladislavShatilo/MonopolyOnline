using Photon.Pun;
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
        this.playerRepository = playerRepository;
        this.photonTurnSynchronizer = photonTurnSynchronizer;
        this.timerManager = timerManager;
        this.gameSettings = gameSettings;
        turn = new Turn();
    }

    #endregion LIFE_CYCLE

    #region PUBLIC_METHODS

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
        timerManager.StartTurnTimer(playerId, gameSettings.turnTime);
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

        var nextPlayer = playerRepository.GetNextPlayerId(currentId);
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
        Debug.Log("RegisterDouble 1");

        var player = playerRepository.GetPlayerById(playerId);
        if (player.SkipNextTurn) return;
        Debug.Log("RegisterDouble 2");
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