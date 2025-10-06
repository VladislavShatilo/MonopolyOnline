using NUnit.Framework;
using Photon.Pun;
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
        this.diceService = diceService;
        this.playerRepository = playerRepository;
        this.localPlayerService = localPlayerService;
        this.eventBus = eventBus;
        this.photonJailManager = photonJailManager;
        this.photonTurnManager = photonTurnManager;
    }

    #endregion LIFE_CYCLE

    #region PUBLIC_METHODS

    public DiceResult GetDiceResult(int playerId, bool isForJail)
    {
        var diceResult = diceService.Roll();
        return diceResult;
    }

    public IEnumerator HandleDice(int first, int second, int playerId, bool isForJail)
    {
        DiceResult diceResult = new DiceResult(first, second);
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
        PlayerData player = playerRepository.GetPlayerById(playerId);

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