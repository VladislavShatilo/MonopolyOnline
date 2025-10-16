using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;

public class MortgageServiceTests
{
    private MortgageService service;
    private Mock<ICompanyRepository> companyRepoMock;
    private Mock<IBankService> bankServiceMock;
    private Mock<IEventBus> eventBusMock;
    private GameSettings settings;

    [SetUp]
    public void Setup()
    {
        companyRepoMock = new Mock<ICompanyRepository>();
        bankServiceMock = new Mock<IBankService>();
        eventBusMock = new Mock<IEventBus>();
        settings = new GameSettings
        {
            mortgageTurns = 3
        };

        service = new MortgageService();
        service.Construct(companyRepoMock.Object, bankServiceMock.Object, eventBusMock.Object, settings);
    }

    // --- MORTGAGE COMPANY TESTS ---

    [Test]
    public void MortgageCompany_ShouldMortgage_WhenValid()
    {
        var company = new Company(1, new CompanyData());
        company.OwnerId = 5;
        company.IsMortgaged = false;
        company.MortgagePrice = 200;

        companyRepoMock.Setup(r => r.GetCompanyById(1)).Returns(company);

        service.MortgageCompany(1, 5);

        Assert.IsTrue(company.IsMortgaged);
        Assert.AreEqual(settings.mortgageTurns, company.MortgageTurnsLeft);
        bankServiceMock.Verify(b => b.AddMoney(5, 200), Times.Once);
        eventBusMock.Verify(e => e.Publish(It.IsAny<CompanyMortgagedEvent>()), Times.Once);
    }

    [Test]
    public void MortgageCompany_ShouldNotMortgage_WhenCompanyNull()
    {
        companyRepoMock.Setup(r => r.GetCompanyById(1)).Returns((Company)null);

       
        Assert.Throws<InvalidOperationException>(() => service.MortgageCompany(1, 5));

    }

    [Test]
    public void MortgageCompany_ShouldNotMortgage_WhenOwnerMismatch()
    {
        var company = new Company(1, new CompanyData());
        company.OwnerId = 2;
        company.IsMortgaged = false;

        companyRepoMock.Setup(r => r.GetCompanyById(1)).Returns(company);

        service.MortgageCompany(1, 5);

        Assert.IsFalse(company.IsMortgaged);
        bankServiceMock.VerifyNoOtherCalls();
        eventBusMock.VerifyNoOtherCalls();
    }

    [Test]
    public void MortgageCompany_ShouldNotMortgage_WhenAlreadyMortgaged()
    {
        var company = new Company(1, new CompanyData());
        company.OwnerId = 5;
        company.IsMortgaged = true;

        companyRepoMock.Setup(r => r.GetCompanyById(1)).Returns(company);

        service.MortgageCompany(1, 5);

        bankServiceMock.VerifyNoOtherCalls();
        eventBusMock.VerifyNoOtherCalls();
    }

    // --- BUYOUT COMPANY TESTS ---

    [Test]
    public void BuyoutCompany_ShouldBuyout_WhenValid()
    {
        var company = new Company(2, new CompanyData());
        company.OwnerId = 5;
        company.IsMortgaged = true;
        company.BuyoutPrice = 300;

        companyRepoMock.Setup(r => r.GetCompanyById(2)).Returns(company);

        service.BuyoutCompany(2, 5);

        Assert.IsFalse(company.IsMortgaged);
        Assert.AreEqual(0, company.MortgageTurnsLeft);
        bankServiceMock.Verify(b => b.RemoveMoney(5, 300), Times.Once);
        eventBusMock.Verify(e => e.Publish(It.IsAny<CompanyBoughtBackEvent>()), Times.Once);
    }

    [Test]
    public void BuyoutCompany_ShouldNotBuyout_WhenCompanyNull()
    {
        companyRepoMock.Setup(r => r.GetCompanyById(2)).Returns((Company)null);

       
        Assert.Throws<InvalidOperationException>(() => service.BuyoutCompany(2, 5));
      
    }

    [Test]
    public void BuyoutCompany_ShouldNotBuyout_WhenOwnerMismatch()
    {
        var company = new Company(2, new CompanyData());
        company.OwnerId = 10;
        company.IsMortgaged = true;

        companyRepoMock.Setup(r => r.GetCompanyById(2)).Returns(company);

        service.BuyoutCompany(2, 5);

        bankServiceMock.VerifyNoOtherCalls();
        eventBusMock.VerifyNoOtherCalls();
    }

    [Test]
    public void BuyoutCompany_ShouldNotBuyout_WhenNotMortgaged()
    {
        var company = new Company(2, new CompanyData());
        company.OwnerId = 5;
        company.IsMortgaged = false;

        companyRepoMock.Setup(r => r.GetCompanyById(2)).Returns(company);

        service.BuyoutCompany(2, 5);

        bankServiceMock.VerifyNoOtherCalls();
        eventBusMock.VerifyNoOtherCalls();
    }

    // --- TICK TURN TESTS ---

    [Test]
    public void TickTurn_ShouldDecreaseMortgageTurns_AndPublishUIEvent()
    {
        var company = new Company(1, new CompanyData());
        company.IsMortgaged = true;
        company.MortgageTurnsLeft = 2;

        companyRepoMock.Setup(r => r.GetByOwner(5)).Returns(new List<Company> { company });

        service.TickTurn(5);

        Assert.AreEqual(1, company.MortgageTurnsLeft);
        eventBusMock.Verify(e => e.Publish(It.IsAny<CompanyTickUIEvent>()), Times.Once);
    }

    [Test]
    public void TickTurn_ShouldFreeCompany_WhenTurnsReachZero()
    {
        var company = new Company(1, new CompanyData());
        company.IsMortgaged = true;
        company.MortgageTurnsLeft = 1;
        company.OwnerId = 5;
        company.IsBought = true;

        companyRepoMock.Setup(r => r.GetByOwner(5)).Returns(new List<Company> { company });

        service.TickTurn(5);

        Assert.IsFalse(company.IsMortgaged);
        Assert.IsFalse(company.IsBought);
        Assert.AreEqual(-1, company.OwnerId);

        eventBusMock.Verify(e => e.Publish(It.IsAny<CompanyFreedFromMortgageEvent>()), Times.Once);
    }

    [Test]
    public void TickTurn_ShouldSkip_WhenNotMortgaged()
    {
        var company = new Company(3, new CompanyData());
        company.IsMortgaged = false;

        companyRepoMock.Setup(r => r.GetByOwner(5)).Returns(new List<Company> { company });

        service.TickTurn(5);

        Assert.IsFalse(company.IsMortgaged);
        eventBusMock.Verify(e => e.Publish(It.IsAny<object>()), Times.Never);
    }
    [Test]
    public void TickTurn_MultipleCompanies_SomeMortgaged_SomeNot()
    {
        var company1 = new Company(1, new CompanyData()) { IsMortgaged = true, MortgageTurnsLeft = 2, OwnerId = 5 };
        var company2 = new Company(2, new CompanyData()) { IsMortgaged = false, OwnerId = 5 };
        var company3 = new Company(3, new CompanyData()) { IsMortgaged = true, MortgageTurnsLeft = 1, OwnerId = 5 };

        companyRepoMock.Setup(r => r.GetByOwner(5)).Returns(new List<Company> { company1, company2, company3 });

        service.TickTurn(5);

        // Проверяем уменьшение залога только для mortgaged компаний
        Assert.AreEqual(1, company1.MortgageTurnsLeft);
        Assert.AreEqual(0, company3.MortgageTurnsLeft);

        // Проверяем, что компания3 освобождена
        Assert.IsFalse(company3.IsMortgaged);
        Assert.IsFalse(company3.IsBought);
        Assert.AreEqual(-1, company3.OwnerId);

        // Проверяем события
        eventBusMock.Verify(e => e.Publish(It.IsAny<CompanyTickUIEvent>()), Times.Exactly(2));
        eventBusMock.Verify(e => e.Publish(It.IsAny<CompanyFreedFromMortgageEvent>()), Times.Once);
    }

    // --- TICK TURN: GetByOwner возвращает null ---
    [Test]
    public void TickTurn_GetByOwnerReturnsNull_Throws()
    {
        companyRepoMock.Setup(r => r.GetByOwner(5)).Returns((List<Company>)null);

        Assert.Throws<InvalidOperationException>(() => service.TickTurn(5));
    }

    // --- MORTGAGE COMPANY: MortgagePrice = 0 ---
    [Test]
    public void MortgageCompany_MortgagePriceZero_AddsMoneyZero()
    {
        var company = new Company(1, new CompanyData()) { OwnerId = 5, IsMortgaged = false, MortgagePrice = 0 };
        companyRepoMock.Setup(r => r.GetCompanyById(1)).Returns(company);

        service.MortgageCompany(1, 5);

        Assert.IsTrue(company.IsMortgaged);
        bankServiceMock.Verify(b => b.AddMoney(5, 0), Times.Once);
        eventBusMock.Verify(e => e.Publish(It.IsAny<CompanyMortgagedEvent>()), Times.Once);
    }

    // --- BUYOUT COMPANY: BuyoutPrice = 0 ---
    [Test]
    public void BuyoutCompany_BuyoutPriceZero_RemovesZeroMoney()
    {
        var company = new Company(2, new CompanyData()) { OwnerId = 5, IsMortgaged = true, BuyoutPrice = 0 };
        companyRepoMock.Setup(r => r.GetCompanyById(2)).Returns(company);

        service.BuyoutCompany(2, 5);

        Assert.IsFalse(company.IsMortgaged);
        bankServiceMock.Verify(b => b.RemoveMoney(5, 0), Times.Once);
        eventBusMock.Verify(e => e.Publish(It.IsAny<CompanyBoughtBackEvent>()), Times.Once);
    }

    // --- TICK TURN: Список содержит null ---
    [Test]
    public void TickTurn_ListContainsNull_ContinuesWithoutException()
    {
        var company1 = new Company(1, new CompanyData()) { IsMortgaged = true, MortgageTurnsLeft = 2, OwnerId = 5 };
        Company company2 = null;

        companyRepoMock.Setup(r => r.GetByOwner(5)).Returns(new List<Company> { company1, company2 });

        Assert.DoesNotThrow(() => service.TickTurn(5));

        Assert.AreEqual(1, company1.MortgageTurnsLeft);
        eventBusMock.Verify(e => e.Publish(It.IsAny<CompanyTickUIEvent>()), Times.Once);
    }
}
