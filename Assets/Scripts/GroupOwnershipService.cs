using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;

public class GroupOwnershipService : IGroupOwnershipService
{
    private ICompanyRepository companyRepository;

    #region LIFE_CYCLE

    [Inject]
    public void Construct(ICompanyRepository companyRepository)
    {
        this.companyRepository = companyRepository;
    }

    #endregion LIFE_CYCLE

    #region PUBLIC_METHODS

    public bool PlayerOwnsWholeGroup(CompanyGroup group, int playerId)
    {
        var companiesInGroup = companyRepository.GetByGroup(group).ToList();

        if (companiesInGroup == null || companiesInGroup.Count == 0)
            return false;

        foreach (var company in companiesInGroup)
        {
            if (company.OwnerId != playerId || company.IsMortgaged)
                return false;
        }

        return true;
    }

    #endregion PUBLIC_METHODS
}