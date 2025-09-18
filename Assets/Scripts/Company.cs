using System;

public enum CompanyType
{
    Company,
    FieldCompany,
    DiceCompany
}

public class Company
{
    public int Id { get; }
    public bool IsBought { get; private set; }
    public string Name { get; }
    public int Price { get; }
    public int OwnerId { get; private set; } = -1;
    public int RentLevel { get; private set; } = 0;
    public bool IsMortgaged { get; private set; }
    public int MortgageTurnsLeft { get; private set; }
    public int MortgagePrice { get; }
    public int BuyoutPrice { get; }
    public CompanyType Type { get; }
    public CompanyGroup Group { get; }

    private readonly CompanyData companyData;
    private readonly FieldCompanyData fieldCompanyData;
    private readonly DiceCompanyData diceCompanyData;

    public Company(int id, CompanyData data)
    {
        Id = id;
        Type = CompanyType.Company;
        companyData = data ?? throw new ArgumentNullException(nameof(data));
        Group = data.group;
        MortgagePrice = data.pledgePrice;
        BuyoutPrice = data.buyoutPrice;
        Name = data.name;
        Price = data.price;
    }

    public Company(int id, FieldCompanyData data)
    {
        Id = id;
        Type = CompanyType.FieldCompany;
        fieldCompanyData = data ?? throw new ArgumentNullException(nameof(data));
        Group = data.group;
        MortgagePrice = data.pledgePrice;
        BuyoutPrice = data.buyoutPrice;
        Name = data.name;
        Price = data.price;
    }

    public Company(int id, DiceCompanyData data)
    {
        Id = id;
        Type = CompanyType.DiceCompany;
        diceCompanyData = data ?? throw new ArgumentNullException(nameof(data));
        Group = data.group;
        MortgagePrice = data.pledgePrice;
        BuyoutPrice = data.buyoutPrice;
        Name = data.name;
        Price = data.price;
    }

    public void ResetData()
    {
        IsBought = false;
        OwnerId = -1;
        RentLevel = 0;
    }

    public void Buy(int playerId)
    {
        IsBought = true;
        OwnerId = playerId;
        RentLevel = 0;
    }

    public void TransferTo(int newOwnerId)
    {
        if (!IsBought) return;
        OwnerId = newOwnerId;
        RentLevel = 0; // сброс уровня при передаче
    }

    public int GetRent(int ownedCount, int diceSum = 0)
    {
        return Type switch
        {
            CompanyType.Company => companyData.rent[RentLevel],
            CompanyType.FieldCompany => fieldCompanyData.rentField[ownedCount - 1],
            CompanyType.DiceCompany => diceCompanyData.rentMultiplier[ownedCount - 1] * diceSum,
            _ => 0
        };
    }
}
