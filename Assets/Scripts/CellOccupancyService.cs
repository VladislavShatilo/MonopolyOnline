using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;

public class CellOccupancyService : ICellOccupancyService, IDisposable
{
    private readonly Dictionary<int, List<PlayerMove>> cellPlayers = new();
    private readonly HashSet<int> pendingCells = new();
    private IBoardService boardService;
    private IEventBus eventBus;

    #region LIFE_CYCLE

    [Inject]
    public void Construct(IBoardService boardService, IEventBus eventBus)
    {
        this.boardService = boardService ?? throw new ArgumentNullException(nameof(boardService));
        this.eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
    }

    public void InitializePlayer()
    {
        eventBus.Subscribe<PlayerOccupancyRegisterEvent>(RegisterPlayerOnCell);
        eventBus.Subscribe<PlayerOccupancyUnregisterEvent>(UnregisterPlayerFromCell);
    }

    void IDisposable.Dispose()
    {
        eventBus.Unsubscribe<PlayerOccupancyRegisterEvent>(RegisterPlayerOnCell);
        eventBus.Unsubscribe<PlayerOccupancyUnregisterEvent>(UnregisterPlayerFromCell);
    }

    #endregion LIFE_CYCLE

    #region PUBLIC_METHODS

    public void UpdatePositions(int cellIndex)
    {
        if (!cellPlayers.ContainsKey(cellIndex)) return;

        var rect = boardService.GetCellRectTransform(cellIndex);
        if (rect == null)
            throw new InvalidOperationException($"CellTransform not found for cellIndex {cellIndex}");

        Vector3 cellCenter = rect.position;

        var players = cellPlayers[cellIndex];
        if (players == null || players.Count == 0) return;

        int count = players.Count;
        Vector3[] positions;

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
            if (pm == null)
                throw new InvalidOperationException($"PlayerMove is null in cell {cellIndex}");

            Vector3 targetPos = positions[i];
            pm.SetTargetPosition(targetPos);
        }
    }

    #endregion PUBLIC_METHODS

    #region PRIVATE_METHODS

    private IEnumerator DelayedUpdate(int cellIndex)
    {
        yield return new WaitForSeconds(0.06f);

        if (cellPlayers.ContainsKey(cellIndex))
        {
            UpdatePositions(cellIndex);
        }

        pendingCells.Remove(cellIndex);
    }

    #endregion PRIVATE_METHODS

    #region CALLBACKS

    private void RegisterPlayerOnCell(PlayerOccupancyRegisterEvent e)
    {
        if (e == null) throw new ArgumentNullException(nameof(e));
        if (e.PlayerMove == null) throw new ArgumentNullException(nameof(e.PlayerMove));

        int cellIndex = e.CellIndex;
        PlayerMove player = e.PlayerMove;

        if (!cellPlayers.ContainsKey(cellIndex))
            cellPlayers[cellIndex] = new List<PlayerMove>();

        if (!cellPlayers[cellIndex].Contains(player))
            cellPlayers[cellIndex].Add(player);

        cellPlayers[cellIndex] = cellPlayers[cellIndex]
            .OrderBy(p => (p?.photonView?.Owner != null) ? p.photonView.Owner.ActorNumber : int.MaxValue)
            .ToList();

        if (pendingCells.Contains(cellIndex)) return;
        pendingCells.Add(cellIndex);

        player.StartCoroutine(DelayedUpdate(cellIndex));
    }

    private void UnregisterPlayerFromCell(PlayerOccupancyUnregisterEvent e)
    {
        if (e == null) throw new ArgumentNullException(nameof(e));
        if (e.PlayerMove == null) throw new ArgumentNullException(nameof(e.PlayerMove));

        int cellIndex = e.CellIndex;
        PlayerMove player = e.PlayerMove;

        if (cellPlayers.ContainsKey(cellIndex))
        {
            cellPlayers[cellIndex].Remove(player);
            if (cellPlayers[cellIndex].Count == 0)
                cellPlayers.Remove(cellIndex);
            else
                UpdatePositions(cellIndex);
        }
    }

    #endregion CALLBACKS
}