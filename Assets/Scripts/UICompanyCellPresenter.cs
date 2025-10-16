using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Globalization;
using UnityEngine;
using Zenject;

public class UICompanyCellPresenter : IInitializable, IDisposable
{
    private IPlayerRepository playerRepository;
    private ICompanyRepository companyRepository;
    private IUICompanyCellRepository uiRepository;
    private ICompanyService companyService;
    private IEventBus eventBus;

    #region LIFE_CYCLE

    [Inject]
    public void Construct(IPlayerRepository playerRepository, ICompanyRepository companyRepository, IUICompanyCellRepository uiRepository,
       ICompanyService companyService, IEventBus eventBus)
    {
        this.playerRepository = playerRepository ?? throw new ArgumentNullException(nameof(playerRepository));
        this.companyRepository = companyRepository ?? throw new ArgumentNullException(nameof(companyRepository));
        this.uiRepository = uiRepository ?? throw new ArgumentNullException(nameof(uiRepository));
        this.companyService = companyService ?? throw new ArgumentNullException(nameof(companyService));
        this.eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
    }

    public void Initialize()
    {
        eventBus.Subscribe<CompanyBoughtEvent>(CompanyBoughtUpdate);
    }

    public void Dispose()
    {
        eventBus.Unsubscribe<CompanyBoughtEvent>(CompanyBoughtUpdate);
    }

    #endregion LIFE_CYCLE

    #region CALLBACKS

    private void CompanyBoughtUpdate(CompanyBoughtEvent e)
    {
        CompanyGroup companyGroup = companyRepository.GetCompanyById(e.CellIndex).Group;
       
        IEnumerable<Company> companies = companyRepository.GetByGroup(companyGroup) ?? throw new NullReferenceException(nameof(CompanyBoughtUpdate));

        foreach (var company in companies)
        {
            if (company == null) return;

            if (company.OwnerId != e.PlayerId)
                continue;

            var view = uiRepository.GetByCompanyId(company.Id) ?? throw new NullReferenceException(nameof(CompanyBoughtUpdate)); ;         

            var owner = playerRepository.GetPlayerById(e.PlayerId) ?? throw new NullReferenceException(nameof(CompanyBoughtUpdate)); ;

            var ownerColor = owner != null ? owner.PlayerColor.ToUnityColor() : Color.white;
            view.UpdateOwner(ownerColor);

            view.SetRentText(companyService.CalculateRent(company, 1));
        }
    }

    #endregion CALLBACKS
}