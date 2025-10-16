using NUnit.Framework;
using System;

[TestFixture]
public class CompanyFullTests
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
            rent = new int[] { 10, 20, 30 },
            group = CompanyGroup.Clothes
        };

        fieldData = new FieldCompanyData
        {
            group = CompanyGroup.FastFood,
            pledgePrice = 15,
            buyoutPrice = 100,
            name = "FieldCompany",
            price = 50,
            rentField = new int[] { 5, 10, 15 }
        };

        diceData = new DiceCompanyData
        {
            group = CompanyGroup.Hotels,
            pledgePrice = 15,
            buyoutPrice = 100,
            name = "DiceCompany",
            price = 50,
            rentMultiplier = new int[] { 1, 2, 3 }
        };
    }

    #region ConstructorTests

    [Test]
    public void Constructor_CompanyData_ShouldInitializeAllFields()
    {
        var company = new Company(1, companyData);
        Assert.AreEqual(1, company.Id);
        Assert.AreEqual("TestCompany", company.Name);
        Assert.AreEqual(100, company.Price);
        Assert.AreEqual(50, company.BranchPrice);
        Assert.AreEqual(20, company.MortgagePrice);
        Assert.AreEqual(150, company.BuyoutPrice);
        Assert.AreEqual(CompanyType.Company, company.Type);
        Assert.AreEqual(CompanyGroup.Clothes, company.Group);
    }

    [Test]
    public void Constructor_FieldCompanyData_ShouldInitializeAllFields()
    {
        var company = new Company(2, fieldData);
        Assert.AreEqual(2, company.Id);
        Assert.AreEqual("FieldCompany", company.Name);
        Assert.AreEqual(50, company.Price);
        Assert.AreEqual(15, company.MortgagePrice);
        Assert.AreEqual(100, company.BuyoutPrice);
        Assert.AreEqual(CompanyType.FieldCompany, company.Type);
        Assert.AreEqual(CompanyGroup.FastFood, company.Group);
    }

    [Test]
    public void Constructor_DiceCompanyData_ShouldInitializeAllFields()
    {
        var company = new Company(3, diceData);
        Assert.AreEqual(3, company.Id);
        Assert.AreEqual("DiceCompany", company.Name);
        Assert.AreEqual(50, company.Price);
        Assert.AreEqual(15, company.MortgagePrice);
        Assert.AreEqual(100, company.BuyoutPrice);
        Assert.AreEqual(CompanyType.DiceCompany, company.Type);
        Assert.AreEqual(CompanyGroup.Hotels, company.Group);
    }

    [Test]
    public void Constructor_ShouldThrow_WhenDataNull()
    {
        Assert.Throws<ArgumentNullException>(() => new Company(1, (CompanyData)null));
        Assert.Throws<ArgumentNullException>(() => new Company(1, (FieldCompanyData)null));
        Assert.Throws<ArgumentNullException>(() => new Company(1, (DiceCompanyData)null));
    }

    #endregion

    #region OwnershipTests

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
    public void ResetData_ShouldClearOwnership()
    {
        var company = new Company(1, companyData);
        company.Buy(5);
        company.ResetData();
        Assert.IsFalse(company.IsBought);
        Assert.AreEqual(-1, company.OwnerId);
        Assert.AreEqual(0, company.RentLevel);
    }

    [Test]
    public void TransferTo_ShouldChangeOwnerAndResetRent()
    {
        var company = new Company(1, companyData);
        company.Buy(5);
        company.RentLevel = 2;
        company.TransferTo(7);
        Assert.AreEqual(7, company.OwnerId);
        Assert.AreEqual(0, company.RentLevel);
    }

    [Test]
    public void TransferTo_ShouldDoNothingIfNotBought()
    {
        var company = new Company(1, companyData);
        company.TransferTo(7);
        Assert.AreEqual(-1, company.OwnerId);
        Assert.IsFalse(company.IsBought);
    }

    #endregion

    #region GetRentTests

    [Test]
    public void GetRent_Company_ShouldReturnCorrectValue()
    {
        var company = new Company(1, companyData);
        company.RentLevel = 0;
        Assert.AreEqual(10, company.GetRent(0));
        company.RentLevel = 2;
        Assert.AreEqual(30, company.GetRent(0));
    }

    [Test]
    public void GetRent_FieldCompany_ShouldReturnCorrectValue()
    {
        var company = new Company(1, fieldData);
        Assert.AreEqual(5, company.GetRent(1));
        Assert.AreEqual(10, company.GetRent(2));
        Assert.AreEqual(15, company.GetRent(3));
    }

    [Test]
    public void GetRent_DiceCompany_ShouldReturnCorrectValue()
    {
        var company = new Company(1, diceData);
        Assert.AreEqual(1 * 3, company.GetRent(1, 3));
        Assert.AreEqual(2 * 2, company.GetRent(2, 2));
        Assert.AreEqual(3 * 4, company.GetRent(3, 4));
    }

    [Test]
    public void GetRent_ShouldThrowOnInvalidIndex()
    {
        var company = new Company(1, fieldData);
        Assert.Throws<IndexOutOfRangeException>(() => company.GetRent(0));
        var diceCompany = new Company(1, diceData);
        Assert.Throws<IndexOutOfRangeException>(() => diceCompany.GetRent(0, 1));
    }

    #endregion
}
