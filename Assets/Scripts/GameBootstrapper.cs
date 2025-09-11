using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class GameBootstrapper : MonoBehaviour
{
    private GameManager gameManager;
    private IBoardService boardService;
    private ICompanyUIService companyUIService;
    private IPlayerViewService playerViewService;
    private ICellOccupancyService cellOccupancyService;

    [Inject]
    public void Construct(GameManager gameManager, IBoardService boardService,ICompanyUIService companyUIService, IPlayerViewService playerViewService,
        ICellOccupancyService cellOccupancyService)
    {
        this.gameManager = gameManager;
        this.boardService = boardService;
        this.companyUIService = companyUIService;
        this.playerViewService = playerViewService;
        this.cellOccupancyService = cellOccupancyService;
        //EventBus.Subscribe<HandleCellEvent>(cellEventHandler.OnHandleCell);IBoardService

        //gameManager.Initialize();
    }
    private void OnDestroy()
    {
        //if (cellEventHandler != null)
        //    EventBus.Unsubscribe<HandleCellEvent>(cellEventHandler.OnHandleCell);
    }
    private void Start()
    {

        boardService.InitializeBoard();
        companyUIService.InitializeUI();
        playerViewService.InitializePlayer();
        cellOccupancyService.InitializePlayer();
        //playerStatsService.Initialize();
        // геймплей запускаем только на старте сцены, когда всё точно готово
        gameManager.Initialize();
    }
}
