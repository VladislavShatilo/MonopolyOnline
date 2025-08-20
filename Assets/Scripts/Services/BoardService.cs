using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardService
{
    private readonly BoardConfig boardConfig;
    private readonly Transform parentTransform;
    private readonly List<Transform> boardCells = new();

    public BoardService(BoardConfig config, Transform parent)
    {
        boardConfig = config;
        parentTransform = parent;
    }

    public void InitializeBoard()
    {
        CacheBoardCells();
        RegisterCellsInDatabase();
    }

    private void CacheBoardCells()
    {
        foreach (Transform child in parentTransform)
            boardCells.Add(child);
    }

    private void RegisterCellsInDatabase()
    {
        for (int i = 0; i < boardConfig.cells.Count; i++)
        {
            var cell = boardConfig.cells[i];
            switch (cell.cellType)
            {
                case CellType.Company:
                    CompanyDatabase.Instance.AddCompanyData(i, cell.companyData);
                    break;
                case CellType.FieldCompany:
                    CompanyDatabase.Instance.AddCompanyData(i, cell.fieldCompanyData);
                    break;
                case CellType.DiceCompany:
                    CompanyDatabase.Instance.AddCompanyData(i, cell.diceCompanyData);
                    break;
            }
        }
    }

    public Transform GetCellTransform(int index) => index >= 0 && index < boardCells.Count ? boardCells[index] : null;
    public GameObject GetCellGameObject(int index) => GetCellTransform(index)?.gameObject;
    public CellData GetCellData(int index) => boardConfig.cells[index];
    public List<CellData> GetAllCellData() => boardConfig.cells;
}
