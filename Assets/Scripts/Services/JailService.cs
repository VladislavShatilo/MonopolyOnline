using Photon.Pun;
using System;
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
        this.playerRepository = playerRepository ?? throw new ArgumentNullException(nameof(playerRepository));
        this.eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
        this.photonTurnManager = photonTurnManager ?? throw new ArgumentNullException(nameof(photonTurnManager));
        this.gameSettings = gameSettings ?? throw new ArgumentNullException(nameof(gameSettings));
        this.photonNetworkWrapper = photonNetworkWrapper ?? throw new ArgumentNullException(nameof(photonNetworkWrapper));
    }

    #endregion LIFE_CYCLE

    #region PUBLIC_METHODS

    public void SendPlayerToJail(int playerId)
    {
        var player = playerRepository.GetPlayerById(playerId) ?? throw new NullReferenceException(nameof(SendPlayerToJail));
        player.CurrentCellId = 10;
        player.SendToJail(gameSettings);
        eventBus.Publish(new SetTurnsJailEvent(playerId, player.JailTurnsLeft));
    }

    public void ReleasePlayer(int playerId, bool payFine)
    {
        var player = playerRepository.GetPlayerById(playerId) ?? throw new NullReferenceException(nameof(ReleasePlayer));
        player.Release();

        eventBus.Publish(new SetTurnsJailEvent(playerId, 0));
    }

    public void TryReleaseByDice(int playerId, int firstDice, int secondDice)
    {
        var player = playerRepository.GetPlayerById(playerId) ?? throw new NullReferenceException(nameof(ReleasePlayer));
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
        PlayerData player = playerRepository.GetPlayerById(playerId) ?? throw new NullReferenceException(nameof(GetTurnsLeft));
        return player.JailTurnsLeft;
    }

    #endregion PUBLIC_METHODS
}