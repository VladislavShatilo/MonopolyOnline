using JetBrains.Annotations;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class BoardRepository : IBoardRepository
{
    private List<CellData> cells;

    #region LIFE_CYCLE

    [Inject]
    public void Construct(BoardConfig config)
    { 
        if(config == null || config.cells == null)
        {
            throw new ArgumentNullException(nameof(config));
        }
        cells = new List<CellData>(config.cells)?? throw new ArgumentNullException(nameof(config));
    }

    #endregion LIFE_CYCLE

    #region PUBLIC_METHODS

    public IReadOnlyList<CellData> GetAllCells() => cells.AsReadOnly();

    public CellData GetCell(int id)
    {
        if (id >= 0 && id < cells.Count)
        {
            return cells[id];
        }
        return null;
    }

    #endregion PUBLIC_METHODS
}