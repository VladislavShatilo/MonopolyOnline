using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;

[TestFixture]
public class CompanyRepositoryTests
{
    private CompanyRepository repository;

    [SetUp]
    public void Setup()
    {
        // Создаем конфиг с разными типами компаний
        var boardConfig = new BoardConfig
        {
            cells = new List<CellData>
            {
                new CellData
                {
                    index = 1,
                    cellType = CellType.Company,
                    companyData = new CompanyData
                    {
                        name = "C1",
                        price = 100,
                        pledgePrice = 10,
                        buyoutPrice = 20,
                        branchPrice = 5,
                        group = CompanyGroup.Clothes
                    }
                },
                new CellData
                {
                    index = 2,
                    cellType = CellType.FieldCompany,
                    fieldCompanyData = new FieldCompanyData
                    {
                        name = "FC1",
                        price = 200,
                        pledgePrice = 20,
                        buyoutPrice = 40,
                        group = CompanyGroup.Games,
                        rentField = new int[]{10, 20}
                    }
                },
                new CellData
                {
                    index = 3,
                    cellType = CellType.DiceCompany,
                    diceCompanyData = new DiceCompanyData
                    {
                        name = "DC1",
                        price = 300,
                        pledgePrice = 30,
                        buyoutPrice = 60,
                        group = CompanyGroup.Hotels,
                        rentMultiplier = new int[]{1,2}
                    }
                }
            }
        };

        repository = new CompanyRepository();
        repository.Construct(boardConfig);
    }

    [Test]
    public void GetCompanyById_ShouldReturnCorrectCompany_WhenExists()
    {
        var company = repository.GetCompanyById(1);
        Assert.NotNull(company);
        Assert.AreEqual("C1", company.Name);
        Assert.AreEqual(CompanyType.Company, company.Type);
    }

    [Test]
    public void GetCompanyById_ShouldReturnNull_WhenNotExists()
    {
        var company = repository.GetCompanyById(99);
        Assert.IsNull(company);
    }

    [Test]
    public void CountOwnedByPlayer_ShouldReturnCorrectCount()
    {
        var company1 = repository.GetCompanyById(1);
        company1.OwnerId = 5;
        var company2 = repository.GetCompanyById(2);
        company2.OwnerId = 5;
        var company3 = repository.GetCompanyById(3);
        company3.OwnerId = 5;

        Assert.AreEqual(1, repository.CountOwnedByPlayer(5, CompanyType.Company));
        Assert.AreEqual(1, repository.CountOwnedByPlayer(5, CompanyType.FieldCompany));
        Assert.AreEqual(1, repository.CountOwnedByPlayer(5, CompanyType.DiceCompany));
        Assert.AreEqual(0, repository.CountOwnedByPlayer(1, CompanyType.Company));
    }

    [Test]
    public void GetAll_ShouldReturnAllCompanies()
    {
        var all = repository.GetAll().ToList();
        Assert.AreEqual(3, all.Count);
        CollectionAssert.AreEquivalent(new[] { 1, 2, 3 }, all.Select(c => c.Id));
    }

    [Test]
    public void Save_ShouldAddOrUpdateCompany()
    {
        var newCompany = new Company(99, new CompanyData
        {
            name = "New",
            price = 50,
            pledgePrice = 5,
            buyoutPrice = 10,
            branchPrice = 2,
            group = CompanyGroup.Games
        });

        repository.Save(newCompany);

        var saved = repository.GetCompanyById(99);
        Assert.NotNull(saved);
        Assert.AreEqual("New", saved.Name);

        // Изменение существующей компании
        saved.Name = "Updated";
        repository.Save(saved);
        var updated = repository.GetCompanyById(99);
        Assert.AreEqual("Updated", updated.Name);
    }

    [Test]
    public void ResetAll_ShouldResetCompanyData()
    {
        var company1 = repository.GetCompanyById(1);
        company1.Buy(1);
        company1.RentLevel = 2;

        repository.ResetAll();

        var resetCompany = repository.GetCompanyById(1);
        Assert.IsFalse(resetCompany.IsBought);
        Assert.AreEqual(-1, resetCompany.OwnerId);
        Assert.AreEqual(0, resetCompany.RentLevel);
    }

    [Test]
    public void GetByGroup_ShouldReturnCorrectCompanies()
    {
        var clothesCompanies = repository.GetByGroup(CompanyGroup.Clothes).ToList();
        Assert.AreEqual(1, clothesCompanies.Count);
        Assert.AreEqual("C1", clothesCompanies[0].Name);

        var foodCompanies = repository.GetByGroup(CompanyGroup.Games).ToList();
        Assert.AreEqual( foodCompanies[0].Name, "FC1");

        var transportCompanies = repository.GetByGroup(CompanyGroup.Hotels).ToList();
        Assert.AreEqual( transportCompanies[0].Name, "DC1");
    }

    [Test]
    public void GetByOwner_ShouldReturnCorrectCompanies()
    {
        var company1 = repository.GetCompanyById(1);
        company1.OwnerId = 5;

        var owned = repository.GetByOwner(5).ToList();
        Assert.AreEqual(1, owned.Count);
        Assert.AreEqual(1, owned[0].Id);
    }
}
