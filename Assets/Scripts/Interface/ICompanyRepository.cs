using System.Collections.Generic;

public interface ICompanyRepository
{
    Company GetCompanyById(int id);
    IEnumerable<Company> GetAll();
    int  CountOwnedByPlayer(int playerId, CompanyType type);

    void Save(Company company);
    void ResetAll();
}