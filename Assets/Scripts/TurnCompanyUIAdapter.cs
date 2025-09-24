using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class TurnCompanyUIAdapter: IInitializable,IDisposable
{
    private  TurnCompanyUIUseCase turnUIUseCase;
    private ICompanyUIService companyUIService;
    private ICompanyRepository companyRepository;
    private IEventBus eventBus;
    [Inject]
    public void Construct(TurnCompanyUIUseCase turnUIUseCase, IEventBus eventBus, ICompanyUIService companyUIService, 
        ICompanyRepository companyRepository)
    {
        this.eventBus = eventBus;
        this.turnUIUseCase = turnUIUseCase;
        this.companyUIService = companyUIService;
        this.companyRepository = companyRepository;
    }
    void IInitializable.Initialize()
    {
        eventBus.Subscribe<TurnStartEvent>(OnTurnStart);
    }
    void IDisposable.Dispose()
    {
        eventBus.Unsubscribe<TurnStartEvent>(OnTurnStart);

    }
    private void OnTurnStart(TurnStartEvent e)
    {
        var actions = turnUIUseCase.GetAvailableActions(e.PlayerId);

        foreach (var action in actions)
        {
            var ui = companyUIService.GetCompanyUI(action.CompanyId);
            if (ui == null) continue;
           
            switch (action.ActionType)
            {
                case CompanyActionType.Mortgage:
                    ui.ShowMortgageButton();
                    break;
                case CompanyActionType.Buyout:
                    ui.ShowBuyoutButton();
                    break;
                case CompanyActionType.ManageBranches:
                    var company= companyRepository.GetCompanyById(action.CompanyId);
                    switch (company.RentLevel)
                    {
                        case 0: ui.ShowBuyFirstBranchButton(); break;
                        case 5: ui.ShowSellFirstButton(); break;
                        default: ui.ShowBuySellButtons(); break;
                    }
                    break;
                case CompanyActionType.None:
                default:
                    ui.HideAllBranchButtons();
                    ui.HideAllMortgageButtons();
                    break;
            }
        }
    }
}
