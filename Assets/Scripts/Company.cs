using System.Collections.Generic;
using UnityEngine;
using Zenject;

public enum CompanyType
{
    Company,
    FieldCompany,
    DiceCompany
}

public class Company
{
    public int Id { get; private set; }
    public bool IsBought { get; set; } = false;
    public string Name { get; set; }
    public int Price { get; set; }
    public int OwnerId { get; set; } = -1;
    public int RentLevel { get; set; } = 0;
    public bool IsMortgaged { get; set; } = false; 
    public int MortgageTurnsLeft { get; set; } = 0;
    public int MortgagePrice { get; set; }
    public int BuyoutPrice { get; set; }

    public CompanyType Type { get; private set; }
    public CompanyData CompanyData { get; private set; }
    public FieldCompanyData FieldCompanyData { get; private set; }
    public DiceCompanyData DiceCompanyData { get; private set; }
    public CompanyGroup Group { get; private set; }
    [Inject] private IPlayerRepository playerRepository;
    [Inject] private ICompanyUIService companyUIService;
    [Inject] private ICompanyRepository companyRepository;
    public Company(int id, CompanyData companyData)
    {
        if (companyData == null) throw new System.ArgumentNullException(nameof(companyData));
        Id = id;
        Type = CompanyType.Company;
        CompanyData = companyData;
        Group = companyData.group;
        MortgagePrice = companyData.pledgePrice;
        BuyoutPrice = companyData.buyoutPrice;
        Name = companyData.name;
        Price = companyData.price;

    }

    public Company(int id, FieldCompanyData fieldCompanyData)
    {
        if (fieldCompanyData == null) throw new System.ArgumentNullException(nameof(fieldCompanyData));
        Id = id;
        Type = CompanyType.FieldCompany;
        FieldCompanyData = fieldCompanyData;
        Group = fieldCompanyData.group;
        MortgagePrice = fieldCompanyData.pledgePrice;
        BuyoutPrice = fieldCompanyData.buyoutPrice;
        Name = fieldCompanyData.name;
        Price = fieldCompanyData.price;
    }

    public Company(int id, DiceCompanyData diceCompanyData)
    {
        if (diceCompanyData == null) throw new System.ArgumentNullException(nameof(diceCompanyData));
        Id = id;
        Type = CompanyType.DiceCompany;
        DiceCompanyData = diceCompanyData;
        Group = diceCompanyData.group;
        MortgagePrice = diceCompanyData.pledgePrice;
        BuyoutPrice = diceCompanyData.buyoutPrice;
        Name = diceCompanyData.name;
        Price = diceCompanyData.price;
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
   public int GetRent()
    {
        switch (Type)
        {
            case CompanyType.Company:
                return CompanyData.rent[RentLevel];
            case CompanyType.FieldCompany:
                int ownedFieldCount = CountOwnedByPlayer(OwnerId, CompanyType.FieldCompany);
                return FieldCompanyData.rentField[ownedFieldCount - 1];
            case CompanyType.DiceCompany:
                int ownedDiceCount = CountOwnedByPlayer(OwnerId, CompanyType.DiceCompany);
                return DiceCompanyData.rentMultiplier[ownedDiceCount - 1] /** TurnManager.Instance.DiceSum*/;

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
    public void TransferTo(int newOwnerId)
    {
        // Сначала проверяем, что компания вообще куплена
        if (!IsBought) return;

        // Меняем владельца
        OwnerId = newOwnerId;
        PlayerData player =  playerRepository.GetPlayerById(OwnerId);
        player.OwnedCompanies.Add(this);
        EventBus.Publish(new OnUpdatePlayerMoneyEvent(player));

        IsMortgaged = IsMortgaged;
        MortgageTurnsLeft = MortgageTurnsLeft;

        var cellUI = companyUIService.GetCompanyUI(Id);
       // cellUI.HandleCompanyBought(Id, newOwnerId);
        // Обновляем аренду для группы
       //CompanyManager.Instance.UpdateRent(this);
        // MortgageTurnsLeft = 0;

        // При передаче можно сбросить уровень ренты (по правилам твоей игры)
        RentLevel = 0;
    }
}

