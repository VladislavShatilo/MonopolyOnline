using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;
public enum CompanyActionType
{
    None,
    Mortgage,
    Buyout,
    ManageBranches
}
public class TurnCompanyUIUseCase: ITurnCompanyUIUseCase
{
    private ICompanyRepository companyRepository;
    private IGroupOwnershipService groupOwnershipService; // сервис, провер€ющий владение группой
    private ILocalPlayerService localPlayerService;

    #region LIFE_CYCLE

    [Inject]
    public void Construct(ICompanyRepository companyRepository, IGroupOwnershipService groupOwnershipService, ILocalPlayerService localPlayerService)
    {
        this.companyRepository = companyRepository ?? throw new ArgumentNullException(nameof(companyRepository));
        this.groupOwnershipService = groupOwnershipService ?? throw new ArgumentNullException(nameof(groupOwnershipService));
        this.localPlayerService = localPlayerService ?? throw new ArgumentNullException(nameof(localPlayerService));
    }

    #endregion LIFE_CYCLE

    #region PUBLIC_METHODS

    public IEnumerable<CompanyUIAction> GetAvailableActions(int currentPlayerId)
    {
        var actions = new List<CompanyUIAction>();

        foreach (var company in companyRepository.GetAll())
        {
            if (!company.IsBought || company.OwnerId != currentPlayerId || currentPlayerId != localPlayerService.GetLocalPlayerId())
            {
                actions.Add(new CompanyUIAction(company.Id, CompanyActionType.None));
                continue;
            }

            bool ownsGroup = company.Type == CompanyType.Company &&
                             groupOwnershipService.PlayerOwnsWholeGroup(company.Group, currentPlayerId);

            if (!ownsGroup)
            {
                if (!company.IsMortgaged)
                    actions.Add(new CompanyUIAction(company.Id, CompanyActionType.Mortgage));
                else
                    actions.Add(new CompanyUIAction(company.Id, CompanyActionType.Buyout));
            }
            else
            {
                var groupCompanies = companyRepository.GetByGroup(company.Group);
                bool hasMortgagedInGroup = groupCompanies.Any(c => c.IsMortgaged);

                if (hasMortgagedInGroup)
                {
                    if (!company.IsMortgaged)
                        actions.Add(new CompanyUIAction(company.Id, CompanyActionType.Mortgage));
                    else
                        actions.Add(new CompanyUIAction(company.Id, CompanyActionType.Buyout));
                }
                else
                {
                    actions.Add(new CompanyUIAction(company.Id, CompanyActionType.ManageBranches));
                }
            }
        }

        return actions;
    }

    #endregion PUBLIC_METHODS

}
