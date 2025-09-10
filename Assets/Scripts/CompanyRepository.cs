using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class CompanyRepository : ICompanyRepository
{
    private readonly Dictionary<int, Company> companies = new();

    [Inject]
    public void Construct(BoardConfig boardConfig)
    {
        foreach (var cell in boardConfig.cells)
        {
            if (cell.cellType == CellType.Company && cell.companyData != null)
            {
                var company = new Company(
                    cell.index,
                   cell.companyData
                );
                companies[cell.index] = company;
            }
            else if (cell.cellType == CellType.FieldCompany && cell.fieldCompanyData != null)
            {
                var company = new Company(
                    cell.index,
                    cell.fieldCompanyData
                );
                companies[cell.index] = company;
            }
            else if (cell.cellType == CellType.DiceCompany && cell.diceCompanyData != null)
            {
                var company = new Company(
                    cell.index,
                    cell.diceCompanyData
                );
                companies[cell.index] = company;
            }
        }
    }

    public Company GetCompanyById(int id) => companies.ContainsKey(id) ? companies[id] : null;

    public IEnumerable<Company> GetAll() => companies.Values;

    public void Save(Company company)
    {
        companies[company.Id] = company;
    }

    public void ResetAll()
    {
        foreach (var company in companies.Values)
        {
            company.ResetData();
        }
    }
}
