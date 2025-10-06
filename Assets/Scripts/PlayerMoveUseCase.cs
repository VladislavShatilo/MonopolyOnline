using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;
using Zenject;

public class PlayerMoveUseCase : IPlayerMoveUseCase, IInitializable, IDisposable
{
    private IPlayerRepository playerRepository;
    private IBoardService boardService;
    private IEventBus eventBus;
    private ICellHighlighterService cellHighlighterService;

    [Inject]
    public void Construct(IPlayerRepository playerRepository, IBoardService boardService, IEventBus eventBus, ICellHighlighterService cellHighlighterService)
    {
        this.playerRepository = playerRepository;
        this.boardService = boardService;
        this.eventBus = eventBus;
        this.cellHighlighterService = cellHighlighterService;
    }
    void IInitializable.Initialize()
    {
        eventBus.Subscribe<DiceFadeEvent>(HighlightCell);
    }
    void IDisposable.Dispose()
    {
        eventBus.Unsubscribe<DiceFadeEvent>(HighlightCell);
    }
    public void MovePlayer(int playerId, int steps, bool isForward)
    {
        PlayerData player = playerRepository.GetPlayerById(playerId);
        player.LastDiceSum = steps;
        int targetIndex = isForward
            ? (player.CurrentCellId + steps) % boardService.CellsCount
            : (player.CurrentCellId - steps + boardService.CellsCount) % boardService.CellsCount;

        // EventBus.Publish(new DiceFadeEvent(targetIndex, true));
        //  EventBus.Publish(new PlayerMoveUnregister(player.CurrentCellId, playerId));
       
        eventBus.Publish(new MovePlayerEvent(playerId, player.CurrentCellId, steps,isForward,targetIndex));

        

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
    private void HighlightCell(DiceFadeEvent e)
    {
        if (e.IsMovementStart)
        {
            cellHighlighterService.ShowHighlight(e.CellId);
        }
        else 
        {
            cellHighlighterService.HideHighlight();

        }

    }

  
}
public class MovePlayerEvent
{
    public int PlayerId;
    public int CurrentCellIndex;
    public int Steps;
    public bool IsForward;
    public int TargetIndex;
    public MovePlayerEvent(int playerId,int currentCellIndex,int steps,bool isForward,int targetIndex)
    {
        PlayerId = playerId;
        CurrentCellIndex = currentCellIndex;
        Steps = steps;
        IsForward = isForward;
        TargetIndex = targetIndex;
    }
}