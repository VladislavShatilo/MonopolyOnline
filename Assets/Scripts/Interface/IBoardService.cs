using System.Collections.Generic;
using UnityEngine;

public interface IBoardService
{
    void InitializeBoard();
    RectTransform GetCellRectTransform(int index);
    GameObject GetCellGameObject(int index);
    CellData GetCellData(int index);
    IReadOnlyList<CellData> GetAllCellData();
    int CellsCount { get; }

}
