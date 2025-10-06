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
    private ICasinoService casinoService;
    private IPhotonTurnManager photonTurnManager;
    private IEventBus eventBus;

    #region LIFE_CYCLE

    [Inject]
    public void Construct(IBoardService boardService, ICompanyService companyService, IPhotonChanceManager photonChanceManager,
     IPhotonJailManager photonJailManager, ICasinoService casinoService, IPhotonTurnManager photonTurnManager, IEventBus eventBus)
    {
        this.boardService = boardService;
        this.companyService = companyService;
        this.photonChanceManager = photonChanceManager;
        this.photonJailManager = photonJailManager;
        this.casinoService = casinoService;
        this.photonTurnManager = photonTurnManager;
        this.eventBus = eventBus;
    }
    void IInitializable.Initialize()
    {
        eventBus.Subscribe<HandleCellEvent>(OnHandleCell);
    }
    void IDisposable.Dispose()
    {
        eventBus.Unsubscribe<HandleCellEvent>(OnHandleCell);

    }

    #endregion LIFE_CYCLE

    #region PUBLIC_METHODS

    public void OnHandleCell(HandleCellEvent e)
    {
        int cellIndex = e.CellID;
        if (cellIndex >= boardService.CellsCount) return;

        var cellData = boardService.GetCellData(cellIndex);
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
                        if (PhotonNetwork.IsMasterClient)
                        {
                            photonTurnManager.RequestEndTurn();
                        }
                        break;

                    case CornerType.Caisno:
                        casinoService.OfferCasino(e.PlayerID);
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
