using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;

public class CellOccupancyService : ICellOccupancyService, IDisposable
{
    private  Dictionary<int, List<PlayerMove>> cellPlayers = new();
    private  HashSet<int> pendingCells = new HashSet<int>();
    private  IBoardService boardService;
    private IPlayerRepository playerRepository;
    private IEventBus eventBus;

    [Inject] 
    public void Construct(IBoardService boardService, IPlayerRepository playerRepository, IEventBus eventBus)
    {
        this.boardService = boardService;
        this.playerRepository = playerRepository;
        this.eventBus = eventBus;
    }
    public void InitializePlayer()
    {
        eventBus.Subscribe<PlayerOccupancyRegisterEvent>(RegisterPlayerOnCell);
        eventBus.Subscribe<PlayerOccupancyUnregisterEvent>(UnregisterPlayerFromCell);
    }

    public void Dispose()
    {
        eventBus.Unsubscribe<PlayerOccupancyRegisterEvent>(RegisterPlayerOnCell);
        eventBus.Unsubscribe<PlayerOccupancyUnregisterEvent>(UnregisterPlayerFromCell);
    }
    // Вызов при том как игрок встал на клетку (вызывается на всех клиентах через buffered RPC)
    public void RegisterPlayerOnCell(PlayerOccupancyRegisterEvent e)
    {
        int cellIndex = e.CellIndex;
        PlayerMove player = e.PlayerMove;

        if (!cellPlayers.ContainsKey(cellIndex))
            cellPlayers[cellIndex] = new List<PlayerMove>();

        if (!cellPlayers[cellIndex].Contains(player))
            cellPlayers[cellIndex].Add(player);

        // Сортируем по ActorNumber для детерминированности слотов
        cellPlayers[cellIndex] = cellPlayers[cellIndex]
            .OrderBy(p => (p.photonView != null && p.photonView.Owner != null) ? p.photonView.Owner.ActorNumber : int.MaxValue)
            .ToList();

        // batch-обновление: ждём маленькую паузу чтобы собрать несколько регистраций
        if (pendingCells.Contains(cellIndex)) return;
        pendingCells.Add(cellIndex);
        player.StartCoroutine(DelayedUpdate(cellIndex));
    }

    private  IEnumerator DelayedUpdate(int cellIndex)
    {
        // Небольшая пауза чтобы собрать все buffered RPC, приходящие почти одновременно
        yield return new WaitForSeconds(0.06f);

        if (cellPlayers.ContainsKey(cellIndex))
            UpdatePositions(cellIndex);

        pendingCells.Remove(cellIndex);
    }

    // Вызов когда игрок ушёл с клетки
    public  void UnregisterPlayerFromCell(PlayerOccupancyUnregisterEvent e)
    {
        int cellIndex = e.CellIndex;
        PlayerMove player = e.PlayerMove;
        if (cellPlayers.ContainsKey(cellIndex))
        {
            cellPlayers[cellIndex].Remove(player);
            if (cellPlayers[cellIndex].Count == 0)
            {
                cellPlayers.Remove(cellIndex);
            }
            else
            {
                UpdatePositions(cellIndex);
            }
        }
    }

    // Расстановка игроков внутри клетки и вызов анимации у каждого (локально)
    public void UpdatePositions(int cellIndex)
    {
        if (!cellPlayers.ContainsKey(cellIndex)) return;
        Vector3 cellCenter = boardService.GetCellRectTransform(cellIndex).position;
        var players = cellPlayers[cellIndex];
        int count = players.Count;

        Vector3[] positions;
        // --- здесь оставляем твою логику позиций (скопируй свои варианты) ---
        if (cellIndex % 10 == 0 && cellIndex != 30)
        {
            switch (count)
            {
                case 1: positions = new[] { cellCenter }; break;
                case 2: positions = new[] { cellCenter + new Vector3(0, 30, 0), cellCenter + new Vector3(0, -30, 0) }; break;
                case 3: positions = new[] { cellCenter + new Vector3(0, 30, 0), cellCenter + new Vector3(-35, -35, 0), cellCenter + new Vector3(35, -35, 0) }; break;
                default: positions = new[] { cellCenter + new Vector3(-40, 40, 0), cellCenter + new Vector3(40, 40, 0), cellCenter + new Vector3(40, -40, 0), cellCenter + new Vector3(-40, -40, 0) }; break;
            }
        }
        else if ((cellIndex > 0 && cellIndex < 10) || (cellIndex > 20 && cellIndex < 30))
        {
            switch (count)
            {
                case 1: positions = new[] { cellCenter }; break;
                case 2: positions = new[] { cellCenter + new Vector3(0, 30, 0), cellCenter + new Vector3(0, -30, 0) }; break;
                case 3: positions = new[] { cellCenter + new Vector3(10, 40, 0), cellCenter + new Vector3(-10, 0, 0), cellCenter + new Vector3(10, -40, 0) }; break;
                default: positions = new[] { cellCenter + new Vector3(-15, 49, 0), cellCenter + new Vector3(12.5f, 17.5f, 0), cellCenter + new Vector3(-15, -17.5f, 0), cellCenter + new Vector3(12.5f, -49, 0) }; break;
            }
        }
        else
        {
            switch (count)
            {
                case 1: positions = new[] { cellCenter }; break;
                case 2: positions = new[] { cellCenter + new Vector3(30, 0, 0), cellCenter + new Vector3(-30, 0, 0) }; break;
                case 3: positions = new[] { cellCenter + new Vector3(0, 10, 0), cellCenter + new Vector3(40, -10, 0), cellCenter + new Vector3(-40, -10, 0) }; break;
                default: positions = new[] { cellCenter + new Vector3(49, 15, 0), cellCenter + new Vector3(17.5f, -12.5f, 0), cellCenter + new Vector3(-17.5f, 15, 0), cellCenter + new Vector3(-49, -12.5f, 0) }; break;
            }
        }

        for (int i = 0; i < players.Count; i++)
        {
            var pm = players[i];
            Vector3 targetPos = positions[i];
            pm.SetTargetPosition(targetPos);
            
        }
    }
}
public class PlayerOccupancyRegisterEvent
{
    public int CellIndex;
    public PlayerMove PlayerMove;

    public PlayerOccupancyRegisterEvent(int cellIndex, PlayerMove playerMove)
    {
        CellIndex = cellIndex;
        PlayerMove = playerMove;
    }

}
public class PlayerOccupancyUnregisterEvent
{
    public int CellIndex;
    public PlayerMove PlayerMove;

    public PlayerOccupancyUnregisterEvent(int cellIndex, PlayerMove playerMove)
    {
        CellIndex = cellIndex;
        PlayerMove = playerMove;
    }

}