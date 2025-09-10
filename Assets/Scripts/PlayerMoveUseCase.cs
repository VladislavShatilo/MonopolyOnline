using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMoveUseCase : IPlayerMoveUseCase
{
    private readonly IPlayerRepository playerRepository;
    private readonly IBoardService boardService;
    private readonly IPhotonTurnManager photonTurnManager;

    public PlayerMoveUseCase(IPlayerRepository playerRepository, IBoardService boardService,
        IPhotonTurnManager photonTurnManager)
    {
        this.playerRepository = playerRepository;
        this.boardService = boardService;
        this.photonTurnManager = photonTurnManager;
    }

    public void MovePlayer(int playerId, int steps, bool isForward)
    {
        PlayerData player = playerRepository.GetPlayerById(playerId);
        int targetIndex = isForward
            ? (player.CurrentCellId + steps) % boardService.CellsCount
            : (player.CurrentCellId - steps + boardService.CellsCount) % boardService.CellsCount;

       // EventBus.Publish(new DiceFadeEvent(targetIndex, true));
      //  EventBus.Publish(new PlayerMoveUnregister(player.CurrentCellId, playerId));

        EventBus.Publish(new MovePlayerEvent(playerId, player.CurrentCellId, steps,isForward));

        

        // Логика изменения позиции в модели
        player.CurrentCellId = targetIndex;
       // EventBus.Publish(new DiceFadeEvent(targetIndex, false));
       //EventBus.Publish(new PlayerMoveRegister( playerId, player.CurrentCellId));
    }

    public void TeleportPlayer(int playerId, int targetCellIndex)
    {
        PlayerData player = playerRepository.GetPlayerById(playerId);

        EventBus.Publish(new PlayerMoveUnregister(player.CurrentCellId, playerId));
        player.CurrentCellId = targetCellIndex;
        EventBus.Publish(new PlayerMoveRegister(playerId, player.CurrentCellId));
        EventBus.Publish(new HandleCellEvent(playerId, player.CurrentCellId));
    }

    public void MovePlayerToJail(int playerId)
    {
        MovePlayer(playerId, 10, true); // JAIL_CELL_ID = 10
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