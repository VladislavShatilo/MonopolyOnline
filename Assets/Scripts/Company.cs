using System;

public enum CompanyType
{
    Company,
    FieldCompany,
    DiceCompany
}

public class Company
{
    public int Id { get; set; }
    public bool IsBought { get; set; }
    public string Name { get; set; }
    public int Price { get; set; }
    public int OwnerId { get; set; } = -1;
    public int RentLevel { get;  set; } = 0;
    public bool IsMortgaged { get; set; }
    public int MortgageTurnsLeft { get; set; }
    public int MortgagePrice { get; set; }
    public int BuyoutPrice { get; set; }
    public int BranchPrice { get; set; }
    public CompanyType Type { get; set; }
    public CompanyGroup Group { get; set; }

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
        BranchPrice = data.branchPrice;
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
