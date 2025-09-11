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


    [Inject]
    public void Construct(
        IPlayerRepository playerRepository,
        ICompanyRepository companyRepository, IUICompanyCellRepository uiRepository)
    {
       
        this.playerRepository = playerRepository;
        this.companyRepository = companyRepository;
        this.uiRepository = uiRepository;
    }

    void IInitializable.Initialize()
    {
        Debug.Log("Initialize");
        EventBus.Subscribe<CompanyBoughtEvent>(CompanyBoughtUpdate);

       // view.OnBuyBranchClicked += HandleBuyBranch;
       // view.OnSellBranchClicked += HandleSellBranch;
       // view.OnMortgageClicked += HandleMortgage;
       // view.OnBuyoutClicked += HandleBuyout;
    }

    void IDisposable.Dispose()
    {
        Debug.Log("Dispose");

        EventBus.Unsubscribe<CompanyBoughtEvent>(CompanyBoughtUpdate);

      //  view.OnBuyBranchClicked -= HandleBuyBranch;
      //  view.OnSellBranchClicked -= HandleSellBranch;
       // view.OnMortgageClicked -= HandleMortgage;
        //view.OnBuyoutClicked -= HandleBuyout;
    }
    public void InitCompany(int companyId)
    {
        var view = uiRepository.GetByCompanyId(companyId);
        if (view == null) return;

        var company = companyRepository.GetCompanyById(companyId);

        if (company == null)
            return;
       
        var groupColor = GroupColors.Colors[(int)company.Group];

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

        view.SetRentText(company.GetRent());
    }
    private void HandleBuyBranch(int companyId) { }
    // companyService.BuyBranch(companyId);

    private void HandleSellBranch(int companyId) {}
      //  companyService.SellBranch(companyId);

    private void HandleMortgage(int companyId) { }
       // companyService.Mortgage(companyId);

    private void HandleBuyout(int companyId) { }
        //companyService.Buyout(companyId);
}