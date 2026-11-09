using Photon.Realtime;
using System;
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

    #region LIFE_CYCLE

    [Inject]
    public void Construct(IBranchService branchService, ICompanyUIService companyUIService, IBankService bankService, ICompanyRepository companyRepository)
    {
        this.branchService = branchService ?? throw new ArgumentNullException(nameof(branchService));
        this.companyUIService = companyUIService ?? throw new ArgumentNullException(nameof(companyUIService));
        this.bankService = bankService ?? throw new ArgumentNullException(nameof(bankService));
        this.companyRepository = companyRepository ?? throw new ArgumentNullException(nameof(companyRepository));
    }

    #endregion LIFE_CYCLE

    #region PUBLIC_METHODS

    public int BuyBranch(int companyId, int playerId)
    {
        int companyLevel = 0;
        if (branchService.TryBuyBranch(companyId, playerId, out var company))
        {
            bankService.RemoveMoney(playerId, company.BranchPrice);
            companyLevel = company.RentLevel;
        }
        return companyLevel;
    }

    public int SellBranch(int companyId, int playerId)
    {
        int companyLevel = 0;

        if (branchService.TrySellBranch(companyId, playerId, out var company))
        {
            var ui = companyUIService.GetCompanyUI(company.Id) ?? throw new InvalidOperationException(nameof(SellBranch));
            ui.UpdateBranchStars(company.RentLevel);

            var ownedCount = companyRepository.CountOwnedByPlayer(company.OwnerId, company.Type);
            if (ownedCount < 0)
            {
                throw new InvalidOperationException(nameof(ownedCount));
            }

            ui.SetRentText(company.GetRent(ownedCount));
            bankService.AddMoney(playerId, company.BranchPrice);
            companyLevel = company.RentLevel;
        }
        return companyLevel;
    }

    public void UpdateBranchUI(int companyId, int playerId, int newRentLevel)
    {
        var company = companyRepository.GetCompanyById(companyId) ?? throw new InvalidOperationException(nameof(UpdateBranchUI));
        company.RentLevel = newRentLevel;
        var ui = companyUIService.GetCompanyUI(company.Id) ?? throw new InvalidOperationException(nameof(SellBranch));
        ui.UpdateBranchStars(company.RentLevel);

        var ownedCount = companyRepository.CountOwnedByPlayer(company.OwnerId, company.Type);
        if (ownedCount < 0)
        {
            throw new InvalidOperationException(nameof(ownedCount));
        }

        ui.SetRentText(company.GetRent(ownedCount));
        HideAllBranchButtonsByGroup(playerId, company.Group);
    }

    #endregion PUBLIC_METHODS

    #region PRIVATE_METHODS

    private void HideAllBranchButtonsByGroup(int currentPlayerId, CompanyGroup group)
    {
        foreach (var c in companyRepository.GetByGroup(group))
        {
            var ui = companyUIService.GetCompanyUI(c.Id) ?? throw new InvalidOperationException(nameof(HideAllBranchButtonsByGroup));

            ui.HideAllBranchButtons();
        }
    }

    #endregion PRIVATE_METHODS
}