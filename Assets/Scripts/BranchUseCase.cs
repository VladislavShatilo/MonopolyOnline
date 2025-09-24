using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using Zenject;

public class BranchUseCase : IBranchUseCase
{
    private IBranchService branchService;
    private ICompanyUIService companyUIService;
    private IBankService bankService;
    private ICompanyRepository companyRepository;

    [Inject]
    public void Construct(IBranchService branchService, ICompanyUIService companyUIService, IBankService bankService, ICompanyRepository companyRepository)
    {
        this.branchService = branchService;
        this.companyUIService = companyUIService;
        this.bankService = bankService;
        this.companyRepository = companyRepository;
    }

    public int BuyBranch(int companyId, int playerId)
    {
        int companyLevel =0;
        if (branchService.TryBuyBranch(companyId, playerId, out var company))
        {
            // Мастер списывает деньги
            bankService.RemoveMoney(playerId, company.BranchPrice);
            companyLevel = company.RentLevel;
            // скрываем кнопки для всех компаний группы
        }
        return companyLevel;
    }
    public int SellBranch(int companyId, int playerId)
    {
        int companyLevel = 0;

        if (branchService.TrySellBranch(companyId, playerId, out var company))
        {
            var ui = companyUIService.GetCompanyUI(company.Id);
            ui?.UpdateBranchStars(company.RentLevel);
            var ownedCount = companyRepository.CountOwnedByPlayer(company.OwnerId, company.Type);
            ui.SetRentText(company.GetRent(ownedCount));
            bankService.AddMoney(playerId, company.BranchPrice);
            companyLevel = company.RentLevel;

            // тут можно вызвать BankService.AddMoney(...)
        }
        return companyLevel;

    }
    public void UpdateBranchUI(int companyId, int playerId,int newRentLevel)
    {

        var company = companyRepository.GetCompanyById(companyId);
        if (company == null) return;
        company.RentLevel = newRentLevel;
        var ui = companyUIService.GetCompanyUI(company.Id);
      
        ui.UpdateBranchStars(company.RentLevel);

        var ownedCount = companyRepository.CountOwnedByPlayer(company.OwnerId, company.Type);
        ui.SetRentText(company.GetRent(ownedCount));
        HideAllBranchButtonsByGroup(playerId, company.Group);

    }
   
    private void HideAllBranchButtonsByGroup(int currentPlayerId, CompanyGroup group)
    {
        foreach (var c in companyRepository.GetByGroup(group))
        {
            var ui = companyUIService.GetCompanyUI(c.Id);
            ui?.HideAllBranchButtons();
        }
    }
}
