using NUnit.Framework;
using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class RollDiceUseCase : IRollDiceUseCase
{
    private IDiceService diceService;
    private IPhotonJailManager photonJailManager;
    private IPlayerRepository playerRepository;
    private ILocalPlayerService localPlayerService;
    private IPhotonTurnManager photonTurnManager;
    private IEventBus eventBus;

    #region LIFE_CYCLE

    [Inject]
    public void Construct(IDiceService diceService, IPhotonJailManager photonJailManager, IPlayerRepository playerRepository, ILocalPlayerService localPlayerService,
      IEventBus eventBus, IPhotonTurnManager photonTurnManager)
    {
        this.diceService = diceService ?? throw new ArgumentNullException(nameof(diceService));
        this.playerRepository = playerRepository ?? throw new ArgumentNullException(nameof(playerRepository));
        this.localPlayerService = localPlayerService ?? throw new ArgumentNullException(nameof(localPlayerService));
        this.eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
        this.photonJailManager = photonJailManager ?? throw new ArgumentNullException(nameof(photonJailManager));
        this.photonTurnManager = photonTurnManager ?? throw new ArgumentNullException(nameof(photonTurnManager));
    }

    #endregion LIFE_CYCLE

    #region PUBLIC_METHODS

    public DiceResult GetDiceResult()
    {
        var diceResult = diceService.Roll();
        return diceResult;
    }

    public IEnumerator HandleDice(int first, int second, int playerId, bool isForJail)
    {
        DiceResult diceResult = new(first, second);
        eventBus.Publish(new DiceRolledEvent(diceResult, playerId, isForJail));

        if (isForJail)
        {
            photonJailManager.CheckDice(playerId, diceResult.First, diceResult.Second);
            if (diceResult.First == diceResult.Second)
            {
                HandlePlayerMove(diceResult, playerId, true);
                yield return null;
            }
            yield return new WaitForSeconds(0.7f);
            eventBus.Publish(new DiceFadeEvent(10, false));
        }
        else
        {
            HandlePlayerMove(diceResult, playerId, false);
        }
    }

    #endregion PUBLIC_METHODS

    #region PRIVATE_METHODS

    private void HandlePlayerMove(DiceResult diceResult, int playerId, bool fromJail)
    {
        PlayerData player = playerRepository.GetPlayerById(playerId) ?? throw new NullReferenceException(nameof(HandlePlayerMove)); ;

        if (localPlayerService.GetLocalPlayerId() == playerId)
        {
            if (player.NextMoveBackward)
            {
                player.NextMoveBackward = false;
                eventBus.Publish(new OnPlayerMoveEvent(playerId, diceResult.Sum, false));
            }
            else
            {
                eventBus.Publish(new OnPlayerMoveEvent(playerId, diceResult.Sum, true));
            }
            if (diceResult.IsDouble && !fromJail)
            {
                photonTurnManager.RegisterDouble(playerId);
            }
        }
    }

    #endregion PRIVATE_METHODS
}