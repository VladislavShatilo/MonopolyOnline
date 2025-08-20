using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CompanyUIService
{
    private readonly BoardService boardService;
    private readonly Dictionary<int, CompanyWindowPopup> popups = new();
    private readonly Dictionary<int, UICompanyCell> companyUIs = new();
    public CompanyUIService(BoardService board)
    {
        boardService = board;
    }

    public void InitializeUI()
    {
        var allCells = boardService.GetAllCellData();
        for (int i = 0; i < allCells.Count; i++)
        {
            var cell = allCells[i];
            var cellTransform = boardService.GetCellTransform(i);
            if (cellTransform.TryGetComponent(out CompanyWindowPopup popup))
            {
                popup.Init(i);
                popups[i] = popup;

                popup.OnCompanyClicked += (id) =>
                {
                    RectTransform rect = cellTransform as RectTransform;
                    switch (cell.cellType)
                    {
                        case CellType.Company:
                            EventBus.Publish(new ShowCompanyWindowEvent(rect, cell.companyData.popupData, cell.companyData));
                            break;
                        case CellType.FieldCompany:
                            EventBus.Publish(new ShowFieldCompanyWindowEvent(rect, cell.fieldCompanyData.popupData, cell.fieldCompanyData));
                            break;
                        case CellType.DiceCompany:
                            EventBus.Publish(new ShowDiceCompanyWindowEvent(rect, cell.diceCompanyData.popupData, cell.diceCompanyData));
                            break;
                    }
                };
            }
            if (cellTransform.TryGetComponent(out UICompanyCell companyUI))
            {
                companyUI.Init(i);
                companyUIs[i] = companyUI;

            }
        }
    }
    public UICompanyCell GetCompanyUI(int id)
    {
        return companyUIs[id];
    }
}