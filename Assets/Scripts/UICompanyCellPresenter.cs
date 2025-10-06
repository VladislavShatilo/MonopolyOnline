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
        this.playerRepository = playerRepository;
        this.companyRepository = companyRepository;
        this.uiRepository = uiRepository;
        this.companyService = companyService;
        this.eventBus = eventBus;
    }

    void IInitializable.Initialize()
    {
        eventBus.Subscribe<CompanyBoughtEvent>(CompanyBoughtUpdate);
    }

    void IDisposable.Dispose()
    {
        eventBus.Unsubscribe<CompanyBoughtEvent>(CompanyBoughtUpdate);
    }

    #endregion LIFE_CYCLE

    #region CALLBACKS

    private void CompanyBoughtUpdate(CompanyBoughtEvent e)
    {
        CompanyGroup companyGroup = companyRepository.GetCompanyById(e.CellIndex).Group;

        IEnumerable<Company> companies = companyRepository.GetByGroup(companyGroup);

        foreach (var company in companies)
        {
            Debug.Log("company.Name" + company.Name + "  " + "company.OwnerId" + company.OwnerId);
            if (company.OwnerId != e.PlayerId)
                continue;

            var view = uiRepository.GetByCompanyId(company.Id);
            if (view == null)
                continue;

            var owner = playerRepository.GetPlayerById(e.PlayerId);

            var ownerColor = owner != null ? owner.PlayerColor.ToUnityColor() : Color.white;
            view.UpdateOwner(ownerColor);

            if (company == null) return;

            view.SetRentText(companyService.CalculateRent(company, 1));
        }
    }

    #endregion CALLBACKS
}