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

    #region LIFE_CYCLE

    [Inject]
    public void Construct(IPlayerRepository playerRepository, IBoardService boardService, IEventBus eventBus, ICellHighlighterService cellHighlighterService)
    {
        this.playerRepository = playerRepository ?? throw new ArgumentNullException(nameof(playerRepository));
        this.boardService = boardService ?? throw new ArgumentNullException(nameof(boardService));
        this.eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
        this.cellHighlighterService = cellHighlighterService ?? throw new ArgumentNullException(nameof(cellHighlighterService));
    }
    public void Initialize()
    {
        eventBus.Subscribe<DiceFadeEvent>(HighlightCell);
    }
    public void Dispose()
    {
        eventBus.Unsubscribe<DiceFadeEvent>(HighlightCell);
    }

    #endregion LIFE_CYCLE

    #region PUBLIC_METHODS

    public void MovePlayer(int playerId, int steps, bool isForward)
    {
        PlayerData player = playerRepository.GetPlayerById(playerId) ?? throw new NullReferenceException(nameof(MovePlayer)); ;
        player.LastDiceSum = steps;
        int targetIndex = isForward
            ? (player.CurrentCellId + steps) % boardService.CellsCount
            : (player.CurrentCellId - steps + boardService.CellsCount) % boardService.CellsCount;

        eventBus.Publish(new MovePlayerEvent(playerId, player.CurrentCellId, steps, isForward, targetIndex));

        player.CurrentCellId = targetIndex;

    }

    public void TeleportPlayer(int playerId, int randomIndex, int currentCellIndex)
    {
        int steps;
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

    #endregion PUBLIC_METHODS

    #region CALLBACKS

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

    #endregion CALLBACKS



}
