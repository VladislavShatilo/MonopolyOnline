using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class BoardService : IBoardService
{
    private IBoardRepository repository;
    private Transform parentTransform;
    private List<Transform> boardCells = new();


    [Inject]
    public void Construct(IBoardRepository repository, [Inject(Id = "BoardParent")] Transform parent)
    {
        this.repository = repository;
        this.parentTransform = parent;
    }

    public void InitializeBoard()
    {
        CacheBoardCells();
    }

    private void CacheBoardCells()
    {
        foreach (Transform child in parentTransform)
            boardCells.Add(child);
    }

    public RectTransform GetCellRectTransform(int index) => index >= 0 && index < boardCells.Count ? boardCells[index].GetComponent<RectTransform>() : null;
    public GameObject GetCellGameObject(int index) => GetCellRectTransform(index)?.gameObject;
    public CellData GetCellData(int index) => repository.GetCell(index);
    public IReadOnlyList<CellData> GetAllCellData() => repository.GetAllCells();
    public int CellsCount => boardCells.Count;
}
