using System;
using UnityEngine;
using Zenject;

public class CellHighlighterService : MonoBehaviour, ICellHighlighterService
{
    [SerializeField] private GameObject fadeImage;
    [SerializeField] private GameObject dice1GO;
    [SerializeField] private GameObject dice2GO;
    [SerializeField] private float distanceCorrection = 45f;

    private GameObject tempCell;
    private IBoardService boardService;

    #region LIFE_CYCLE

    [Inject]
    public void Construct(IBoardService boardService)
    {
        this.boardService = boardService ?? throw new ArgumentNullException(nameof(boardService));
    }

    #endregion LIFE_CYCLE

    #region PUBLIC_METHODS

    public void ShowHighlight(int cellId)
    {
        if(fadeImage != null)
            fadeImage.SetActive(true);

        var cell = boardService.GetCellGameObject(cellId) ?? throw new InvalidOperationException(nameof(boardService));
        tempCell = Instantiate(cell, fadeImage.transform);

        if (tempCell.TryGetComponent<RectTransform>(out var rectTransform))
        {
            rectTransform.position = new Vector2(
                rectTransform.position.x + distanceCorrection,
                rectTransform.position.y - distanceCorrection);
        }
    }

    public void HideHighlight()
    {
        if (fadeImage != null)
            fadeImage.SetActive(false);
        if (tempCell != null) 
            Destroy(tempCell);
        if (dice1GO != null)
            dice1GO.SetActive(false);
        if (dice2GO != null)
            dice2GO.SetActive(false);
    }

    #endregion PUBLIC_METHODS
}
