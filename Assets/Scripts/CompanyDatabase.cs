using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
public enum CompanyType
{
    Company,
    FieldCompany,
    DiceCompany

}

public class Company
{
    public int Id { get; private set; }
  
    // Игровые данные
    public bool IsBought { get; set; }
    public int OwnerId { get; set; }
    public int RentLevel { get; set; }
    public  CompanyType Type { get; set; }
    public CompanyData CompanyData { get; set; }
    public FieldCompanyData FieldCompanyData { get; set; }
    public DiceCompanyData DiceCompanyData { get; set; }
    public CompanyGroup Group { get; set; }
    public Company(int id, CompanyData companyData)
      
    {
        Id = id;
        IsBought = false;
        OwnerId = -1;
        RentLevel = 0;
        Type = CompanyType.Company;
        CompanyData = companyData;
        FieldCompanyData = null;
        DiceCompanyData = null;
        Group = companyData.group;
        
      
    }
    public Company(int id, FieldCompanyData fieldCompanyData)
    {
        Id = id;
        IsBought = false;
        OwnerId = -1;
        RentLevel = 0;
        Type = CompanyType.FieldCompany;
        CompanyData = null;
        FieldCompanyData = fieldCompanyData;
        DiceCompanyData = null;
        Group = fieldCompanyData.group;


    }
    public Company(int id, DiceCompanyData diceCompanyData)
    {
        Id = id;
        IsBought = false;
        OwnerId = -1;
        RentLevel = 0;
        Type = CompanyType.DiceCompany;
        CompanyData = null;
        FieldCompanyData = null;
        DiceCompanyData = diceCompanyData;
        Group = diceCompanyData.group;


    }
    public void ResetData()
    {
        IsBought = false;
        OwnerId = -1;
        RentLevel = 0;
    }
}
public class CompanyDatabase
{
    private static CompanyDatabase _instance;
    public static CompanyDatabase Instance => _instance ??= new CompanyDatabase();

    private List<Company> companies = new List<Company>();

    public void AddComponyData(int id,  CompanyData companyData)
    {
        companies.Add(new Company(id, companyData));
    }
    public void AddComponyData(int id, FieldCompanyData fieldCompanyData)
    {
        companies.Add(new Company(id, fieldCompanyData));
    }
    public void AddComponyData(int id,  DiceCompanyData diceCompanyData)
    {
        companies.Add(new Company(id, diceCompanyData));
    }
    public List<Company> GetAllCompanies() => companies;

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
