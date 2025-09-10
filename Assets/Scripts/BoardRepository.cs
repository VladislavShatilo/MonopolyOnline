using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class BoardRepository : IBoardRepository
{
    private  List<CellData> cells;

    [Inject]
    public void Construct (BoardConfig config)
    {
        cells = new List<CellData>(config.cells);
    }

    public CellData GetCell(int id) => id >= 0 && id < cells.Count ? cells[id] : null;
    public IReadOnlyList<CellData> GetAllCells() => cells.AsReadOnly();
}
