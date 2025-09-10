using System.Collections.Generic;

public interface ICompanyRepository
{
    Company GetCompanyById(int id);
    IEnumerable<Company> GetAll();

    void Save(Company company);
    void ResetAll();
}