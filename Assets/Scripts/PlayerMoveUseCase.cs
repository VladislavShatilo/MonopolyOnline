using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;
using Zenject;

public class PlayerMoveUseCase : IPlayerMoveUseCase
{
    private IPlayerRepository playerRepository;
    private IBoardService boardService;
    private IPhotonTurnManager photonTurnManager;
    private IEventBus eventBus;


    [Inject]
    public void Construct(IPlayerRepository playerRepository, IBoardService boardService,
        IPhotonTurnManager photonTurnManager, IEventBus eventBus)
    {
        this.playerRepository = playerRepository;
        this.boardService = boardService;
        this.photonTurnManager = photonTurnManager;
        this.eventBus = eventBus;
    }

    public void MovePlayer(int playerId, int steps, bool isForward)
    {
        PlayerData player = playerRepository.GetPlayerById(playerId);
        int targetIndex = isForward
            ? (player.CurrentCellId + steps) % boardService.CellsCount
            : (player.CurrentCellId - steps + boardService.CellsCount) % boardService.CellsCount;

        // EventBus.Publish(new DiceFadeEvent(targetIndex, true));
        //  EventBus.Publish(new PlayerMoveUnregister(player.CurrentCellId, playerId));

        eventBus.Publish(new MovePlayerEvent(playerId, player.CurrentCellId, steps,isForward));

        

        // Логика изменения позиции в модели
        player.CurrentCellId = targetIndex;
       // EventBus.Publish(new DiceFadeEvent(targetIndex, false));
       //EventBus.Publish(new PlayerMoveRegister( playerId, player.CurrentCellId));
    }
 
    public void TeleportPlayer(int playerId,int randomIndex, int currentCellIndex)
    {
        int steps = 0;
        if (randomIndex > currentCellIndex)
        {
            steps = randomIndex - currentCellIndex;
        }
        else
        {
            steps = randomIndex + boardService.CellsCount - currentCellIndex;
        }

        MovePlayer(playerId, steps, true);
    }

  
}
public class MovePlayerEvent
{
    public int PlayerId;
    public int CurrentCellIndex;
    public int Steps;
    public bool IsForward;

    public MovePlayerEvent(int playerId,int currentCellIndex,int steps,bool isForward)
    {
        PlayerId = playerId;
        CurrentCellIndex = currentCellIndex;
        Steps = steps;
        IsForward = isForward;
    }
}