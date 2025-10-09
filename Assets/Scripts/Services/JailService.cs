using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class JailService : IJailService
{
    private IPlayerRepository playerRepository;
    private IPhotonTurnManager photonTurnManager;
    private IPhotonNetworkWrapper photonNetworkWrapper;
    private IEventBus eventBus;
    private GameSettings gameSettings;

    #region LIFE_CYCLE

    [Inject]
    public void Construct(IPlayerRepository playerRepository, IEventBus eventBus, IPhotonTurnManager photonTurnManager, GameSettings gameSettings, IPhotonNetworkWrapper photonNetworkWrapper)
    {
        this.playerRepository = playerRepository;
        this.eventBus = eventBus;
        this.photonTurnManager = photonTurnManager;
        this.gameSettings = gameSettings;
        this.photonNetworkWrapper = photonNetworkWrapper;
    }

    #endregion LIFE_CYCLE

    #region PUBLIC_METHODS

    public void SendPlayerToJail(int playerId)
    {
        var player = playerRepository.GetPlayerById(playerId);
        player.CurrentCellId = 10;
        player.SendToJail(gameSettings);
        eventBus.Publish(new SetTurnsJailEvent(playerId, player.JailTurnsLeft));

    }

    public void ReleasePlayer(int playerId, bool payFine)
    {
        var player = playerRepository.GetPlayerById(playerId);
        player.Release();


        eventBus.Publish(new SetTurnsJailEvent(playerId, 0));


    }

    public void TryReleaseByDice(int playerId, int firstDice, int secondDice)
    {
        var player = playerRepository.GetPlayerById(playerId);
        if (!player.IsInJail) return;

        if (firstDice == secondDice)
        {
            ReleasePlayer(playerId, false);

            return;
        }

        if (player.JailTurnsLeft > 0)
        {
            player.JailTurnsLeft--;
        }
        eventBus.Publish(new SetTurnsJailEvent(playerId, player.JailTurnsLeft));

        if (photonNetworkWrapper.IsMasterClient)
            photonTurnManager.RequestEndTurn();
    }
    public int GetTurnsLeft(int playerId)
    {
        return playerRepository.GetPlayerById(playerId).JailTurnsLeft;
    }

    #endregion PUBLIC_METHODS


}
