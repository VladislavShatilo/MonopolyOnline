using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class MortgageService : IMortgageService
{
    private GameSettings settings;
    private ICompanyRepository companyRepository;
    private IBankService bankService;
    private IEventBus eventBus;

    [Inject]
    public void Construct(ICompanyRepository companyRepository, IBankService bankService, IEventBus eventBus, GameSettings settings)
    {
        this.companyRepository = companyRepository;
        this.bankService = bankService;
        this.eventBus = eventBus;
        this.settings = settings;
    }

    public void MortgageCompany(int companyId, int playerId)
    {
        var company = companyRepository.GetCompanyById(companyId);
        if (company == null || company.OwnerId != playerId || company.IsMortgaged) return;

        company.IsMortgaged = true;
        company.MortgageTurnsLeft = settings.mortgageTurns;
        bankService.AddMoney(playerId, company.MortgagePrice);
        eventBus.Publish(new CompanyMortgagedEvent(playerId, companyId, settings.mortgageTurns));

    }

    public void BuyoutCompany(int companyId, int playerId)
    {
        var company = companyRepository.GetCompanyById(companyId);
        if (company == null || company.OwnerId != playerId || !company.IsMortgaged) return;

        company.IsMortgaged = false;
        company.MortgageTurnsLeft = 0;
        bankService.RemoveMoney(playerId, company.BuyoutPrice);
        eventBus.Publish(new CompanyBoughtBackEvent(playerId, companyId));

    }

    public void TickTurn(int playerId)
    {
        var companies = companyRepository.GetByOwner(playerId);
        foreach (var company in companies)
        {
            if (!company.IsMortgaged) continue;
            company.MortgageTurnsLeft--;
            eventBus.Publish(new CompanyTickUIEvent(company.Id, company.MortgageTurnsLeft));
            if (company.MortgageTurnsLeft <= 0)
            {
                company.IsMortgaged = false;
                company.IsBought = false;
                company.OwnerId = -1;

                eventBus.Publish(new CompanyFreedFromMortgageEvent(company.Id));
            }
        }
    }
}
public class CompanyMortgagedEvent
{
    public int PlayerId { get; }
    public int CompanyId { get; }
    public int Turns { get; }

    public CompanyMortgagedEvent(int playerId, int companyId, int turns)
    {
        PlayerId = playerId;
        CompanyId = companyId;
        Turns = turns;
    }
}

public class CompanyBoughtBackEvent
{
    public int PlayerId { get; }
    public int CompanyId { get; }
    public CompanyBoughtBackEvent(int playerId, int companyId)
    {
        PlayerId = playerId;
        CompanyId = companyId;
    }
}

public class CompanyFreedFromMortgageEvent
{
    public int CompanyId { get; }

    public CompanyFreedFromMortgageEvent(int companyId)
    {
        CompanyId = companyId;
    }
}