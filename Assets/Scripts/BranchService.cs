using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class BranchService : IBranchService
{
    private ICompanyRepository companyRepository;
    private IPlayerRepository playerRepository;
    private GameSettings gameSettings;

    #region LIFE_CYCLE

    [Inject]
    public void Construct(ICompanyRepository companyRepository, IPlayerRepository playerRepository, GameSettings gameSettings)
    {
        this.companyRepository = companyRepository;
        this.playerRepository = playerRepository;
        this.gameSettings = gameSettings;
    }

    #endregion LIFE_CYCLE

    #region PUBLIC_METHODS

    public bool TryBuyBranch(int companyId, int playerId, out Company company)
    {
        company = companyRepository.GetCompanyById(companyId);
        var player = playerRepository.GetPlayerById(playerId);

        if (company == null || player == null) return false;
        if (company.OwnerId != player.Id) return false;
        if (company.RentLevel >= gameSettings.maxBranchLevel) return false;

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

    #endregion PUBLIC_METHODS
}
