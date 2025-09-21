using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class BranchService : IBranchService
{
    private ICompanyRepository companyRepository;
    private IPlayerRepository playerRepository;

    [Inject]
    public void Construct(ICompanyRepository companyRepository, IPlayerRepository playerRepository)
    {
        this.companyRepository = companyRepository;
        this.playerRepository = playerRepository;
    }

    public bool TryBuyBranch(int companyId, int playerId, out Company company)
    {
        company = companyRepository.GetCompanyById(companyId);
        var player = playerRepository.GetPlayerById(playerId);

        if (company == null || player == null) return false;
        if (company.OwnerId != player.Id) return false;
        if (company.RentLevel >= 5) return false;

        company.RentLevel++;
        return true;
    }

    public bool TrySellBranch(int companyId, int playerId, out Company company)
    {
        company = companyRepository.GetCompanyById(companyId);
        var player = playerRepository.GetPlayerById(playerId);

        if (company == null || player == null) return false;
        if (company.OwnerId != player.Id) return false;
        if (company.RentLevel <= 0) return false;

        company.RentLevel--;
        return true;
    }
}
