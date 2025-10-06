using JetBrains.Annotations;
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
        cells = new List<CellData>(config.cells);
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