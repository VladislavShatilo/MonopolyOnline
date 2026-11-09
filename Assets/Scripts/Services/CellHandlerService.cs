using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class CellHandlerService : ICellHandler, IInitializable,IDisposable
{
    private IBoardService boardService;
    private ICompanyService companyService;
    private IPhotonChanceManager photonChanceManager;
    private IPhotonJailManager photonJailManager;
    private IPhotonTurnManager photonTurnManager;
    private IEventBus eventBus;
    private IPhotonNetworkWrapper photonNetworkWrapper;
    #region LIFE_CYCLE

    [Inject]
    public void Construct(IBoardService boardService, ICompanyService companyService, IPhotonChanceManager photonChanceManager,
     IPhotonJailManager photonJailManager, IPhotonTurnManager photonTurnManager, IEventBus eventBus, IPhotonNetworkWrapper photonNetworkWrapper)
    {
        this.boardService = boardService ?? throw new ArgumentNullException(nameof(boardService));
        this.companyService = companyService ?? throw new ArgumentNullException(nameof(companyService));
        this.photonChanceManager = photonChanceManager ?? throw new ArgumentNullException(nameof(photonChanceManager));
        this.photonJailManager = photonJailManager ?? throw new ArgumentNullException(nameof(photonJailManager));
        this.photonTurnManager = photonTurnManager ?? throw new ArgumentNullException(nameof(photonTurnManager));
        this.eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
        this.photonNetworkWrapper = photonNetworkWrapper ?? throw new ArgumentNullException(nameof(photonNetworkWrapper));
    }
    public void Initialize()
    {
        eventBus.Subscribe<HandleCellEvent>(OnHandleCell);
    }
    public void Dispose()
    {
        eventBus.Unsubscribe<HandleCellEvent>(OnHandleCell);

    }

    #endregion LIFE_CYCLE

    #region PUBLIC_METHODS

    public void OnHandleCell(HandleCellEvent e)
    {
        int cellIndex = e.CellID;
        if (cellIndex >= boardService.CellsCount) return;

        var cellData = boardService.GetCellData(cellIndex) ?? throw new NullReferenceException(nameof(OnHandleCell)); ;
        switch (cellData.cellType)
        {
            case CellType.Company:
            case CellType.FieldCompany:
            case CellType.DiceCompany:
                companyService.HandleCell(cellIndex, e.PlayerID);
                break;

            case CellType.Question:
            case CellType.Spend:
                photonChanceManager.GiveRandomBuff(e.PlayerID);
                break;

            case CellType.Corner:
                switch (cellData.cornerData.type)
                {
                    case CornerType.Start:
                    case CornerType.ChillJail:
                        if (photonNetworkWrapper.IsMasterClient)
                        {
                            photonTurnManager.RequestEndTurn();
                        }
                        break;

                    case CornerType.Caisno:
                        if (photonNetworkWrapper.IsMasterClient)
                        {
                            photonTurnManager.RequestEndTurn();
                        }
                        //casinoService.OfferCasino(e.PlayerID);
                        break;

                    case CornerType.Police:
                        photonJailManager.SendToJail(e.PlayerID);
                        break;
                }
                break;
        }
    }

    #endregion PUBLIC_METHODS

}
