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
        this.boardService = boardService;
    }

    #endregion LIFE_CYCLE

    #region PUBLIC_METHODS

    public void ShowHighlight(int cellId)
    {
        fadeImage.SetActive(true);

        var cell = boardService.GetCellGameObject(cellId);
        tempCell = Instantiate(cell, fadeImage.transform);

        var rectTransform = tempCell.GetComponent<RectTransform>();
        rectTransform.position = new Vector2(
            rectTransform.position.x + distanceCorrection,
            rectTransform.position.y - distanceCorrection);
    }

    public void HideHighlight()
    {
        fadeImage.SetActive(false);
        if (tempCell != null) Destroy(tempCell);

        dice1GO.SetActive(false);
        dice2GO.SetActive(false);
    }

    #endregion PUBLIC_METHODS
}
