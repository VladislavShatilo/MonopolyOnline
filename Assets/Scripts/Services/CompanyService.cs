using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Zenject;
public enum BuyReason
{
    Buy,
    Auction
}

public class CompanyService : ICompanyService
{
    private  ICompanyRepository companyRepository;
    private  IBankService bank;
    private IPhotonTurnManager photonTurnManager;
    private ICompanySyncService companySyncService;

    [Inject]
    public void Construct(ICompanyRepository companyRepository, IBankService bank, IPhotonTurnManager photonTurnManager, ICompanySyncService companySyncService)
    {
        this.companyRepository = companyRepository;
        this.bank = bank;
        this.photonTurnManager = photonTurnManager;
        this.companySyncService = companySyncService;
    }

    public void HandleCell(int cellIndex, int playerId)
    {
        var company = companyRepository.GetCompanyById(cellIndex);
        if (company == null) return;

        if (!company.IsBought)
        {
            bool canAfford = bank.HasEnoughMoney(playerId, company.Price);
            EventBus.Publish(new OfferPurchaseEvent(cellIndex, playerId,company.Price, canAfford));

        }
        else if (company.OwnerId != playerId)
        {

            EventBus.Publish(new OfferRentEvent(cellIndex, playerId,company.GetRent()));

        }
        else
        {
            photonTurnManager.RequestEndTurn();
        }
    }

    public void TryBuyCompany(int cellIndex, int playerId, BuyReason reason)
    {
        var company = companyRepository.GetCompanyById(cellIndex);
        if (company == null || company.IsBought) return;

        int price = company.Price;
        if (!bank.HasEnoughMoney(playerId, price)) return;

        company.Buy(playerId);
        bank.RemoveMoney(playerId, price);
        EventBus.Publish(new CompanyBoughtEvent(cellIndex, playerId, price, reason));
        photonTurnManager.RequestEndTurn();
        companySyncService.SyncCompanyBought(cellIndex,playerId,price, reason);
    }

    public void TryPayRent(int cellIndex, int playerId)
    {
        var company = companyRepository.GetCompanyById(cellIndex);
        if (company == null || !company.IsBought) return;

        int rent = company.GetRent();
        if (!bank.HasEnoughMoney(playerId, rent)) return;

        bank.TransferMoney(playerId, company.OwnerId, rent);
        EventBus.Publish(new RentPaidEvent(cellIndex, playerId, company.OwnerId, rent));
        companySyncService.SyncRentPaid(cellIndex, playerId, company.OwnerId, rent);

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