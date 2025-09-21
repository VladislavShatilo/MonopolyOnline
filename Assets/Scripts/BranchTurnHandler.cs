using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BranchTurnHandler : IBranchTurnHandler
{
    private readonly ICompanyRepository companyRepository;
    private readonly ICompanyUIService companyUIService;
    private readonly BranchRules branchRules;
    private readonly IGroupOwnershipService groupOwnershipService; // сервис, провер€ющий владение группой

    public BranchTurnHandler(
        ICompanyRepository companyRepository,
        ICompanyUIService companyUIService,
        BranchRules branchRules,
        IGroupOwnershipService groupOwnershipService)
    {
        this.companyRepository = companyRepository;
        this.companyUIService = companyUIService;
        this.branchRules = branchRules;
        this.groupOwnershipService = groupOwnershipService;
    }

    public void OnTurnStart(int playerId, int localPlayerId)
    {
        foreach (var company in companyRepository.GetAll())
        {
            var ui = companyUIService.GetCompanyUI(company.Id);
            if (ui == null || company == null)
                continue;

            bool ownsGroup = groupOwnershipService.PlayerOwnsWholeGroup(company.Group, playerId);

            if (playerId != localPlayerId || !branchRules.CanShowBranchButtons(company, playerId, ownsGroup))
            {
                ui.HideAllBranchButtons();
                continue;
            }

            ShowBranchButtonsForLevel(ui, company.RentLevel);
        }
    }

    private void ShowBranchButtonsForLevel(UICompanyCell ui, int level)
    {
        switch (level)
        {
            case 0: ui.ShowBuyFirstBranchButton(); break;
            case 5: ui.ShowSellFirstButton(); break;
            default: ui.ShowBuySellButtons(); break;
        }
    }
}
