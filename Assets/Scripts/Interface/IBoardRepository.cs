using System.Collections.Generic;

public interface IBoardRepository
{
    CellData GetCell(int id);
    IReadOnlyList<CellData> GetAllCells();
}