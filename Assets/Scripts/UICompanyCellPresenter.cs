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
    private IGroupColors groupColors;
    [Inject]
    public void Construct( IPlayerRepository playerRepository,ICompanyRepository companyRepository, IUICompanyCellRepository uiRepository,
        ICompanyService companyService, IEventBus eventBus, IGroupColors groupColors)
    {

        this.playerRepository = playerRepository;
        this.companyRepository = companyRepository;
        this.uiRepository = uiRepository;
        this.companyService = companyService;
        this.eventBus = eventBus;   
        this.groupColors = groupColors;
    }

    void IInitializable.Initialize()
    {
        eventBus.Subscribe<CompanyBoughtEvent>(CompanyBoughtUpdate);
    }

    void IDisposable.Dispose()
    {

        eventBus.Unsubscribe<CompanyBoughtEvent>(CompanyBoughtUpdate);
    }
    public void InitCompany(int companyId)
    {
        var view = uiRepository.GetByCompanyId(companyId);
        if (view == null) return;

        var company = companyRepository.GetCompanyById(companyId);

        if (company == null)
            return;
       
        var groupColor = groupColors.Colors[(int)company.Group];

        view.UpdateUI(company.Name, company.Price, groupColor);
    }
    private void CompanyBoughtUpdate(CompanyBoughtEvent e)
    {
        var view = uiRepository.GetByCompanyId(e.CellIndex);

        if (view == null) return;
     
        var owner = playerRepository.GetPlayerById(e.PlayerId);
        

        var ownerColor = owner != null ? owner.PlayerColor.ToUnityColor() : Color.white;
        view.UpdateOwner(ownerColor);

        var company = companyRepository.GetCompanyById(e.CellIndex);
        if (company == null) return;

        view.SetRentText(companyService.CalculateRent(company));
    }
 
}