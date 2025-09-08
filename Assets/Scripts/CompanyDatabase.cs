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
    public void TransferTo(int newOwnerId)
    {
        // Сначала проверяем, что компания вообще куплена
        if (!IsBought) return;

        // Меняем владельца
        OwnerId = newOwnerId;
        PlayerData player =  playerRepository.GetPlayerById(OwnerId);
        player.OwnedCompanies.Add(this);
        EventBus.Publish(new OnUpdatePlayerCapitalEvent(player));

        IsMortgaged = IsMortgaged;
        MortgageTurnsLeft = MortgageTurnsLeft;

        var cellUI = companyUIService.GetCompanyUI(Id);
        cellUI.HandleCompanyBought(Id, newOwnerId);
        // Обновляем аренду для группы
       CompanyManager.Instance.UpdateRent(this);
        // MortgageTurnsLeft = 0;

        // При передаче можно сбросить уровень ренты (по правилам твоей игры)
        RentLevel = 0;
    }
}

public class CompanyDatabase
{
    private static CompanyDatabase _instance;
    public static CompanyDatabase Instance => _instance ??= new CompanyDatabase();

    private readonly List<Company> companies = new List<Company>();

    public IReadOnlyList<Company> Companies => companies.AsReadOnly();

    public bool AddCompanyData(int id, CompanyData companyData)
    {
        if (GetCompanyById(id) != null) return false;
        companies.Add(new Company(id, companyData));
        return true;
    }

    public bool AddCompanyData(int id, FieldCompanyData fieldCompanyData)
    {
        if (GetCompanyById(id) != null) return false;
        companies.Add(new Company(id, fieldCompanyData));
        return true;
    }

    public bool AddCompanyData(int id, DiceCompanyData diceCompanyData)
    {
        if (GetCompanyById(id) != null) return false;
        companies.Add(new Company(id, diceCompanyData));
        return true;
    }

    public Company GetCompanyById(int id)
    {
        return companies.Find(c => c.Id == id);
    }

    public void ResetCompaniesForPlayer(int playerId)
    {
        foreach (var company in companies)
        {
            if (company.OwnerId == playerId)
                company.ResetData();
        }
    }

    public void ResetAllCompanies()
    {
        foreach (var company in companies)
            company.ResetData();
    }
}
