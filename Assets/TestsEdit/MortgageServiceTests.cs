using System.Collections.Generic;
using Moq;
using NUnit.Framework;

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

        service.MortgageCompany(1, 5);

        bankServiceMock.VerifyNoOtherCalls();
        eventBusMock.VerifyNoOtherCalls();
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

        service.BuyoutCompany(2, 5);

        bankServiceMock.VerifyNoOtherCalls();
        eventBusMock.VerifyNoOtherCalls();
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
}
