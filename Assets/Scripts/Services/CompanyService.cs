using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public enum BuyReason
{
    Buy,
    Auction
}

public class CompanyService : ICompanyService
{
    private readonly ICompanyRepository companyRepository;
    private readonly IBankService bank;
    private readonly ITurnService turnService;

    public CompanyService(ICompanyRepository companyRepository, IBankService bank, ITurnService turnService)
    {
        this.companyRepository = companyRepository;
        this.bank = bank;
        this.turnService = turnService;
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
            EventBus.Publish(new OfferRentEvent(cellIndex, playerId));

        }
        else
        {
            turnService.EndTurn();
        }
    }

    public void TryBuyCompany(int cellIndex, int playerId, BuyReason reason)
    {
        Debug.Log(cellIndex + "   " + playerId);
        var company = companyRepository.GetCompanyById(cellIndex);
        if (company == null || company.IsBought) return;

        int price = company.Price;
        if (!bank.HasEnoughMoney(playerId, price)) return;

        company.Buy(playerId);
        bank.RemoveMoney(playerId, price);
        EventBus.Publish(new CompanyBoughtEvent(cellIndex, playerId, price, reason));
    }

    public void TryPayRent(int cellIndex, int playerId)
    {
        var company = companyRepository.GetCompanyById(cellIndex);
        if (company == null || !company.IsBought) return;

        int rent = GetRent(company);
        if (!bank.HasEnoughMoney(playerId, rent)) return;

        bank.TransferMoney(playerId, company.OwnerId, rent);
        EventBus.Publish(new RentPaidEvent(cellIndex, playerId, company.OwnerId, rent));
    }
    private int GetRent(Company company)
    {
        switch (company.Type)
        {
            case CompanyType.Company:
                return company.CompanyData.rent[company.RentLevel];
            case CompanyType.FieldCompany:
                int ownedFieldCount = CountOwnedByPlayer(company.OwnerId, CompanyType.FieldCompany);
                return company.FieldCompanyData.rentField[ownedFieldCount - 1];
            case CompanyType.DiceCompany:
                int ownedDiceCount = CountOwnedByPlayer(company.OwnerId, CompanyType.DiceCompany);
                return company.DiceCompanyData.rentMultiplier[ownedDiceCount - 1] /** TurnManager.Instance.DiceSum*/;
               
        }
        return 0;
    }
    private int CountOwnedByPlayer(int playerId, CompanyType companyType)
    {
        int count = 0;

        List<Company> companiesList = new List<Company>(companyRepository.GetAll());
        for (int i = 0; i < companiesList.Count; i++)
        {
            if (companiesList[i].IsBought && companiesList[i].OwnerId == playerId && companiesList[i].Type == companyType)
                count++;
        }

        return count;
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
    public OfferRentEvent(int cellIndex, int playerId)
    {
        CellIndex = cellIndex;
        PlayerId = playerId;
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