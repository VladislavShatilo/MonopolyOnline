using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;


public class Company
{
    public int Id { get; private set; }
  
    // Игровые данные
    public bool IsBought { get; set; }
    public int OwnerId { get; set; }
    public int RentLevel { get; set; }
    public CompanyData CompanyBranchData { get; set; }
    public Company(int id, CompanyData companyBranchData)
      
    {
        Id = id;
        CompanyBranchData = companyBranchData;  
        IsBought = false;
        OwnerId = -1;
        RentLevel = 0;
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

    public void AddComponyData(int id, CompanyData companyBranchData)
    {
        companies.Add(new Company(id, companyBranchData));
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
