using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CellHandlerService : ICellHandler
{
    private readonly IBoardService boardService;
    private readonly ICompanyService companyService;
    private readonly IChanceService chanceService;
    private readonly IJailService jailService;
    private readonly ICasinoService casinoService;
    private readonly ITurnService turnService;

    public CellHandlerService(
        IBoardService boardService,
        ICompanyService companyService,
        IChanceService chanceService,
        IJailService jailService,
        ICasinoService casinoService,
        ITurnService turnService)
    {
        this.boardService = boardService;
        this.companyService = companyService;
        this.chanceService = chanceService;
        this.jailService = jailService;
        this.casinoService = casinoService;
        this.turnService = turnService;
    }

    public void OnHandleCell(HandleCellEvent e)
    {
        int cellIndex = e.CellID;
        if (cellIndex >= boardService.GetAllCellData().Count) return;

        var cellData = boardService.GetCellData(cellIndex);

        switch (cellData.cellType)
        {
            case CellType.Company:
            case CellType.FieldCompany:
            case CellType.DiceCompany:
                companyService.HandleCompanyCell(cellIndex, e.PlayerID);
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
