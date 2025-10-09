using NUnit.Framework;

[TestFixture]
public class CompanyTests
{
    private CompanyData companyData;
    private FieldCompanyData fieldData;
    private DiceCompanyData diceData;

    [SetUp]
    public void Setup()
    {
        companyData = new CompanyData
        {
            name = "TestCompany",
            price = 100,
            branchPrice = 50,
            pledgePrice = 20,
            buyoutPrice = 150,
            rent = new int[] { 10, 20, 30 }
        };

        fieldData = new FieldCompanyData
        {
            group = CompanyGroup.Clothes,
            pledgePrice = 15,
            buyoutPrice = 100,
            name = "Field",
            price = 50,
            rentField = new int[] { 5, 10, 15 }
        };

        diceData = new DiceCompanyData
        {
            group = CompanyGroup.Clothes,
            pledgePrice = 15,
            buyoutPrice = 100,
            name = "Dice",
            price = 50,
            rentMultiplier = new int[] { 1, 2, 3 }
        };
    }

    [Test]
    public void Constructor_ShouldInitializeCompanyProperties()
    {
        var company = new Company(1, companyData);
        Assert.AreEqual(1, company.Id);
        Assert.AreEqual("TestCompany", company.Name);
        Assert.AreEqual(100, company.Price);
        Assert.AreEqual(50, company.BranchPrice);
        Assert.AreEqual(CompanyType.Company, company.Type);
    }

    [Test]
    public void ResetData_ShouldClearOwnership()
    {
        var company = new Company(1, companyData);
        company.Buy(10);
        company.ResetData();
        Assert.IsFalse(company.IsBought);
        Assert.AreEqual(-1, company.OwnerId);
        Assert.AreEqual(0, company.RentLevel);
    }

    [Test]
    public void Buy_ShouldSetOwnerAndFlag()
    {
        var company = new Company(1, companyData);
        company.Buy(5);
        Assert.IsTrue(company.IsBought);
        Assert.AreEqual(5, company.OwnerId);
        Assert.AreEqual(0, company.RentLevel);
    }

    [Test]
    public void TransferTo_ShouldChangeOwnerAndResetRent()
    {
        var company = new Company(1, companyData);
        company.Buy(5);
        company.TransferTo(7);
        Assert.AreEqual(7, company.OwnerId);
        Assert.AreEqual(0, company.RentLevel);
    }

    [Test]
    public void GetRent_ShouldReturnCorrectValueForCompany()
    {
        var company = new Company(1, companyData);
        company.RentLevel = 1;
        Assert.AreEqual(20, company.GetRent(0));
    }

    [Test]
    public void GetRent_ShouldReturnCorrectValueForFieldCompany()
    {
        var company = new Company(1, fieldData);
        Assert.AreEqual(5, company.GetRent(1));
        Assert.AreEqual(10, company.GetRent(2));
    }

    [Test]
    public void GetRent_ShouldReturnCorrectValueForDiceCompany()
    {
        var company = new Company(1, diceData);
        Assert.AreEqual(2 * 3, company.GetRent(2, 3));
    }
}
