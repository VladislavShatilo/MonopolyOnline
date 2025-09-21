using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;
public enum BuyReason
{
    Buy,
    Auction
}

public class CompanyService : ICompanyService,IInitializable,IDisposable
{
    private ICompanyRepository companyRepository;
    private IBankService bank;
    private IPhotonTurnManager photonTurnManager;
    private ICompanySyncService companySyncService;
    private IPlayerRepository playerRepository;
    private IEventBus eventBus;

    [Inject]
    public void Construct(ICompanyRepository companyRepository, IBankService bank, IPhotonTurnManager photonTurnManager, ICompanySyncService companySyncService,
        IPlayerRepository playerRepository, IEventBus eventBus)
    {
        this.companyRepository = companyRepository;
        this.bank = bank;
        this.photonTurnManager = photonTurnManager;
        this.companySyncService = companySyncService;
        this.playerRepository = playerRepository;
        this.eventBus = eventBus;
    }
    void IInitializable.Initialize()
    {
        eventBus.Subscribe<EndAuctionWithWinnerEvent>(AuctionBuyCompany);
    }
    void IDisposable.Dispose()
    {
        eventBus.Unsubscribe<EndAuctionWithWinnerEvent>(AuctionBuyCompany);
    }
    public void HandleCell(int cellIndex, int playerId)
    {
        var company = companyRepository.GetCompanyById(cellIndex);
        if (company == null) return;

        if (!company.IsBought)
        {
            bool canAfford = bank.HasEnoughMoney(playerId, company.Price);
            eventBus.Publish(new OfferPurchaseEvent(cellIndex, playerId,company.Price, canAfford));

        }
        else if (company.OwnerId != playerId)
        {

            eventBus.Publish(new OfferRentEvent(cellIndex, playerId, CalculateRent(company)));

        }
        else
        {
            photonTurnManager.RequestEndTurn();
        }
    }
    private void AuctionBuyCompany(EndAuctionWithWinnerEvent e)
    {
       
        TryBuyCompany(e.CompanyId, e.WinnerId,e.FinalPrice, BuyReason.Auction);
    }
    public void TryBuyCompany(int cellIndex, int playerId, int price,BuyReason reason)
    {
        var company = companyRepository.GetCompanyById(cellIndex);

     
            if (company == null || company.IsBought) return;
        if (reason == BuyReason.Buy)
        {
             price = company.Price;
        }
        if (!bank.HasEnoughMoney(playerId, price)) return;

        company.Buy(playerId);
        bank.RemoveMoney(playerId, price);
        eventBus.Publish(new CompanyBoughtEvent(cellIndex, playerId, price, reason));
        photonTurnManager.RequestEndTurn();
        companySyncService.SyncCompanyBought(cellIndex,playerId,price, reason);

    }

    public void TryPayRent(int cellIndex, int playerId)
    {

        var company = companyRepository.GetCompanyById(cellIndex);
        if (company == null || !company.IsBought) return;

        int rent = CalculateRent(company);
        if (!bank.HasEnoughMoney(playerId, rent)) return;

        bank.TransferMoney(playerId, company.OwnerId, rent);
        eventBus.Publish(new RentPaidEvent(cellIndex, playerId, company.OwnerId, rent));
        photonTurnManager.RequestEndTurn();

        companySyncService.SyncRentPaid(cellIndex, playerId, company.OwnerId, rent);

    }
    public void TransferCompany(int companyId, int newOwnerId)
    {
        var company = companyRepository.GetCompanyById(companyId);
        company.TransferTo(newOwnerId);

        var player = playerRepository.GetPlayerById(newOwnerId);
        player.OwnedCompanies.Add(company);

        eventBus.Publish(new OnCompanyTransferredEvent(companyId, newOwnerId));
    }

    public int CalculateRent(Company company, int diceSum = 0)
    {
        var ownedCount = companyRepository.CountOwnedByPlayer(company.OwnerId, company.Type);
        return company.GetRent(ownedCount, diceSum);
    }


}
public class OfferPurchaseEvent
{
    public int CellIndex;
    public int PlayerId;
    public int Price;
    public bool CanAfford;
    public OfferPurchaseEvent(int cellIndex, int playerId, int price,bool canAfford)
    {
        CellIndex = cellIndex;
        PlayerId = playerId;
        Price = price;
        CanAfford = canAfford;
    }
}
public class OfferRentEvent
{
    public int CellIndex;
    public int PlayerId;
    public int Rent;
    public OfferRentEvent(int cellIndex, int playerId, int rent)
    {
        CellIndex = cellIndex;
        PlayerId = playerId;
        Rent = rent;
    }
}
public class CompanyBoughtEvent
{
    public int CellIndex;
    public int PlayerId;
    public int Price;
    public BuyReason Reason;
    public CompanyBoughtEvent(int cellIndex, int playerId, int price, BuyReason reason)
    {
        CellIndex =cellIndex;
        PlayerId = playerId;
        Price = price;
        Reason = reason;
    }
}
public class RentPaidEvent
{
    public int CellIndex;
    public int PlayerId;
    public int Owner;
    public int Rent;
    public RentPaidEvent(int cellIndex,int playerId,int owner, int rent)
    {
        CellIndex = cellIndex;
        PlayerId = playerId;
        Owner = owner;
        Rent = rent;
    }
}
public class OnCompanyTransferredEvent
{
    public int CompanyId;
    public int NewOwnerId;
    public OnCompanyTransferredEvent(int companyId, int newOwnerId)
    {
        CompanyId = companyId;
        NewOwnerId = newOwnerId;
    }
}