using System.Collections;
using System.Collections.Generic;
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
        this.gameManager = gameManager;
        this.boardService = boardService;
        this.companyUIService = companyUIService;
        this.cellOccupancyService = cellOccupancyService;
        this.playerStatsService = playerStatsService;
    }

    private void Awake()
    {
        boardService.InitializeBoard();
        companyUIService.InitializeUI();
        cellOccupancyService.InitializePlayer();
        playerStatsService.Initialize();
        gameManager.Initialize();
    }

    #endregion LIFE_CYCLE


}