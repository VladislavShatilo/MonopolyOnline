using System;
using UnityEngine;
using Zenject;

public class GameBootstrapper : MonoBehaviour
{
    private GameManager gameManager;
    private IBoardService boardService;
    private ICompanyUIService companyUIService;
    private ICellOccupancyService cellOccupancyService;
    private PlayerStatsService playerStatsService;

    #region LIFE_CYCLE

    [Inject]
    public void Construct(GameManager gameManager, IBoardService boardService, ICompanyUIService companyUIService,
        ICellOccupancyService cellOccupancyService, PlayerStatsService playerStatsService)
    {
        this.gameManager = gameManager ?? throw new ArgumentNullException(nameof(gameManager));
        this.boardService = boardService ?? throw new ArgumentNullException(nameof(boardService));
        this.companyUIService = companyUIService ?? throw new ArgumentNullException(nameof(companyUIService));
        this.cellOccupancyService = cellOccupancyService ?? throw new ArgumentNullException(nameof(cellOccupancyService));
        this.playerStatsService = playerStatsService ?? throw new ArgumentNullException(nameof(playerStatsService));
    }

    private void Awake()
    {
        InitializeServices();
    }

    public void InitializeServices()
    {
        boardService.InitializeBoard();
        companyUIService.InitializeUI();
        cellOccupancyService.InitializePlayer();
        playerStatsService.Initialize();
        gameManager.Initialize();
    }

    #endregion LIFE_CYCLE
}
