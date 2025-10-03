using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class CompanyUIService : ICompanyUIService, IInitializable, IDisposable
{
    private IBoardService boardService;
    private IEventBus eventBus;
    private Dictionary<int, UICompanyCell> companyUIs = new();
    private readonly Dictionary<int, CompanyWindowPopup> popups = new();

    [Inject]
    public void Construct(IBoardService boardService, IEventBus eventBus)
    {
        this.boardService = boardService;
        this.eventBus = eventBus;
    }

    void IInitializable.Initialize()
    {
        eventBus.Subscribe<HideButtonsTradeEvent>(HideAllButtonsOnTrade);
    }

    void IDisposable.Dispose()
    {
        eventBus.Unsubscribe<HideButtonsTradeEvent>(HideAllButtonsOnTrade);
    }

    public void InitializeUI()
    {
        var allCells = boardService.GetAllCellData();

        for (int i = 0; i < allCells.Count; i++)
        {
            var cell = allCells[i];

            var cellTransform = boardService.GetCellRectTransform(i);
            if (cellTransform.TryGetComponent(out CompanyWindowPopup popup))
            {
                popup.Init(i);
                popups[i] = popup;

                popup.OnCompanyClicked += (id) =>
                {
                    switch (cell.cellType)
                    {
                        case CellType.Company:
                            eventBus.Publish(new ShowCompanyWindowEvent(cellTransform, cell.companyData.popupData, cell.companyData));
                            break;

                        case CellType.FieldCompany:
                            eventBus.Publish(new ShowFieldCompanyWindowEvent(cellTransform, cell.fieldCompanyData.popupData, cell.fieldCompanyData));
                            break;

                        case CellType.DiceCompany:
                            eventBus.Publish(new ShowDiceCompanyWindowEvent(cellTransform, cell.diceCompanyData.popupData, cell.diceCompanyData));
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

    public UICompanyCell GetCompanyUI(int index)
    {
        companyUIs.TryGetValue(index, out var ui);
        return ui;
    }

    private void HideAllButtonsOnTrade(HideButtonsTradeEvent e)
    {
        companyUIs[e.CompanyId].HideAllBranchButtons();
        companyUIs[e.CompanyId].HideAllMortgageButtons();
    }
}

public class HideButtonsTradeEvent
{
    public int CompanyId;

    public HideButtonsTradeEvent(int CompanyId)
    {
        this.CompanyId = CompanyId;
    }
}