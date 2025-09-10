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
    private IChanceService chanceService;
    private IJailService jailService;
    private ICasinoService casinoService;
    private ITurnService turnService;

    [Inject]
    public void Construct( IBoardService boardService, ICompanyService companyService,  IChanceService chanceService,
        IJailService jailService, ICasinoService casinoService, ITurnService turnService)
    {
        this.boardService = boardService;
        this.companyService = companyService;
        this.chanceService = chanceService;
        this.jailService = jailService;
        this.casinoService = casinoService;
        this.turnService = turnService;
    }
    void IInitializable.Initialize()
    {
        EventBus.Subscribe<HandleCellEvent>(OnHandleCell);
    }
    void IDisposable.Dispose()
    {
        EventBus.Unsubscribe<HandleCellEvent>(OnHandleCell);
       
    }
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
                chanceService.GiveRandomBuff(e.PlayerID);
                break;

            case CellType.Corner:
                switch (cellData.cornerData.type)
                {
                    case CornerType.Start:
                    case CornerType.ChillJail:
                        turnService.EndTurn();
                        break;

                    case CornerType.Caisno:
                        casinoService.OfferCasino(e.PlayerID);
                        break;

                    case CornerType.Police:
                        jailService.SendToJail(e.PlayerID);
                        break;
                }
                break;
        }
    }
}
