using Photon.Realtime;
using System;
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
        this.companyRepository = companyRepository ?? throw new ArgumentNullException(nameof(companyRepository));
        this.playerRepository = playerRepository ?? throw new ArgumentNullException(nameof(playerRepository));
        this.gameSettings = gameSettings ?? throw new ArgumentNullException(nameof(gameSettings));
    }

    #endregion LIFE_CYCLE

    #region PUBLIC_METHODS

    public bool TryBuyBranch(int companyId, int playerId, out Company company)
    {
        company = companyRepository.GetCompanyById(companyId) ?? throw new InvalidOperationException(nameof(company));
        var player = playerRepository.GetPlayerById(playerId) ?? throw new InvalidOperationException(nameof(TryBuyBranch));

        if (company.OwnerId != player.Id) return false;
        if (company.RentLevel >= gameSettings.maxBranchLevel) return false;

        company.RentLevel++;
        return true;
    }

    public bool TrySellBranch(int companyId, int playerId, out Company company)
    {
        company = companyRepository.GetCompanyById(companyId) ?? throw new InvalidOperationException(nameof(company));
        var player = playerRepository.GetPlayerById(playerId) ?? throw new InvalidOperationException(nameof(TryBuyBranch));

        if (company.OwnerId != player.Id) return false;
        if (company.RentLevel <= 0) return false;

        company.RentLevel--;
        return true;
    }

    #endregion PUBLIC_METHODS
}
