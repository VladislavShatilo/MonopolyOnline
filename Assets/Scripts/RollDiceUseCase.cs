using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;
using static UnityEngine.ParticleSystem;

public class RollDiceUseCase : IRollDiceUseCase
{
    private IDiceService diceService;
    private ITurnService turnService; 
    private IPlayerRepository playerRepository; 
    private ILocalPlayerService localPlayerService;
    private IEventBus eventBus;

    [Inject]
    public void Construct(IDiceService diceService, ITurnService turnService, IPlayerRepository playerRepository, ILocalPlayerService localPlayerService, IEventBus eventBus)
    {
        this.diceService = diceService;
        this.turnService = turnService;
        this.playerRepository = playerRepository;
        this.localPlayerService = localPlayerService;
        this.eventBus = eventBus;
    }

    public DiceResult GetDiceResult(int playerId, bool isForJail)
    {
        var diceResult = diceService.Roll();
        return diceResult;
    }

    public void HandleDice(int first,int second, int playerId, bool isForJail)
    {
        DiceResult diceResult = new DiceResult(first, second);
        eventBus.Publish(new DiceRolledEvent(diceResult, playerId, isForJail));

        //if (!isForJail)
        //{
        //    turnService.RegisterDouble(playerId, diceResult.IsDouble);
        //}

        if (isForJail)
        {
            eventBus.Publish(new CheckDiceJailEvent(diceResult.First, diceResult.Second, playerId));
        }
        else
        {
            HandlePlayerMove(diceResult, playerId);
        }
    }
    private void HandlePlayerMove(DiceResult diceResult,int playerId)
    {
        PlayerData player = playerRepository.GetPlayerById(playerId);

        if (localPlayerService.GetLocalPlayerId() == playerId)
        {
            if (player.NextMoveBackward)
            {
                player.NextMoveBackward = false; // сбросим, чтобы только один ход был назад
                eventBus.Publish(new OnPlayerMoveEvent(playerId,diceResult.Sum, false));
            }
            else
            {

                eventBus.Publish(new OnPlayerMoveEvent(playerId, diceResult.Sum, true));
            }
            if (diceResult.IsDouble)
            {
                eventBus.Publish(new PlayerRolledDoubleEvent(playerId, diceResult.IsDouble));
            }
        }

    }

}
public class DiceRolledEvent
{
    public DiceResult DiceResult { get; }
    public int PlayerId { get; }
    public bool IsForJail { get; }

    public DiceRolledEvent(DiceResult diceResult, int playerId, bool isForJail)
    {
        DiceResult=diceResult;
        PlayerId = playerId;
        IsForJail = isForJail;
    }
}