using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CellEventHandler
{
    private readonly BoardService boardService;

    public CellEventHandler(BoardService board)
    {
        boardService = board;
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
                CompanyManager.Instance.HandleCell(cellIndex, e.PlayerID);
                break;
            case CellType.Question:
            case CellType.Spend:
                QuestionManager.Instance.HandleQuestionCell(e.PlayerID);            
               
                break;
            case CellType.Corner:

                switch (cellData.cornerData.type)
                {
                    case CornerType.Start:
                        if (!PhotonNetwork.IsMasterClient) return;

                        TurnManager.Instance.RequestEndTurn();
                        break;
                    case CornerType.ChillJail:
                        if (!PhotonNetwork.IsMasterClient) return;

                        TurnManager.Instance.RequestEndTurn();
                        break;
                    case CornerType.Caisno:
                        CasinoManager.Instance.CasinoOffer(e.PlayerID);
                        break;
                    case CornerType.Police:
                       JailManager.Instance.SendToJail(e.PlayerID);
                        break;
                   
                    
                }
               
                break;
        }
    }

 
   
}
