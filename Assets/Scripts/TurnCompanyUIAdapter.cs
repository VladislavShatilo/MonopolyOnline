using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class TurnCompanyUIAdapter: IInitializable,IDisposable
{
    private ITurnCompanyUIUseCase turnUIUseCase;
    private ICompanyUIService companyUIService;
    private ICompanyRepository companyRepository;
    private IEventBus eventBus;

    #region LIFE_CYCLE

    [Inject]
    public void Construct(ITurnCompanyUIUseCase turnUIUseCase, IEventBus eventBus, ICompanyUIService companyUIService,
        ICompanyRepository companyRepository)
    {
        this.eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
        this.turnUIUseCase = turnUIUseCase ?? throw new ArgumentNullException(nameof(turnUIUseCase));
        this.companyUIService = companyUIService ?? throw new ArgumentNullException(nameof(companyUIService));
        this.companyRepository = companyRepository ?? throw new ArgumentNullException(nameof(companyRepository));
    }
    public void Initialize()
    {
        eventBus.Subscribe<TurnStartEvent>(OnTurnStart);
    }
    public void Dispose()
    {
        eventBus.Unsubscribe<TurnStartEvent>(OnTurnStart);

    }

    #endregion LIFE_CYCLE

    #region CALLBACKS

    private void OnTurnStart(TurnStartEvent e)
    {
        var actions = turnUIUseCase.GetAvailableActions(e.PlayerId);

        foreach (var action in actions)
        {
            var ui = companyUIService.GetCompanyUI(action.CompanyId) ?? throw new NullReferenceException(nameof(OnTurnStart));

            switch (action.ActionType)
            {
                case CompanyActionType.Mortgage:
                    ui.ShowMortgageButton();
                    break;
                case CompanyActionType.Buyout:
                    ui.ShowBuyoutButton();
                    break;
                case CompanyActionType.ManageBranches:
                    var company = companyRepository.GetCompanyById(action.CompanyId) ?? throw new NullReferenceException(nameof(OnTurnStart));
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

    #endregion CALLBACKS

}
