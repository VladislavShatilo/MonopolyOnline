using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardRepository : IBoardRepository
{
    private readonly List<CellData> cells;

    public BoardRepository(BoardConfig config)
    {
        // Просто копируем данные из конфигурации
        cells = new List<CellData>(config.cells);
    }

    public CellData GetCell(int id) => id >= 0 && id < cells.Count ? cells[id] : null;
    public IReadOnlyList<CellData> GetAllCells() => cells.AsReadOnly();
}
