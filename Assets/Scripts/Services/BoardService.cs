using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class BoardService : IBoardService
{
    private IBoardRepository repository;
    private Transform parentTransform;
    private readonly List<Transform> boardCells = new();

    #region LIFE_CYCLE

    [Inject]
    public void Construct(IBoardRepository repository, [Inject(Id = "BoardParent")] Transform parent)
    {
        this.repository = repository ?? throw new ArgumentNullException(nameof(repository));
        this.parentTransform = parent ?? throw new ArgumentNullException(nameof(parentTransform));
    }


    #endregion LIFE_CYCLE

    #region PUBLIC_METHODS

    public void InitializeBoard()
    {
        CacheBoardCells();
    }

    public RectTransform GetCellRectTransform(int index) => index >= 0 && index < boardCells.Count ? boardCells[index].GetComponent<RectTransform>() : null;
    public GameObject GetCellGameObject(int index) => GetCellRectTransform(index)?.gameObject;
    public CellData GetCellData(int index) => repository.GetCell(index);
    public IReadOnlyList<CellData> GetAllCellData() => repository.GetAllCells();
    public int CellsCount => boardCells.Count;

    #endregion PUBLIC_METHODS

    #region PRIVATE_METHODS

    private void CacheBoardCells()
    {
        boardCells.Clear();
        foreach (Transform child in parentTransform)
        {
            if (child != null)
            {
                boardCells.Add(child);
            }

        }
    }

    #endregion PRIVATE_METHODS

}
