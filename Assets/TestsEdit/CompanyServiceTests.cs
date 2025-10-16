using Moq;
using NUnit.Framework;
using System;

[TestFixture]
public class CompanyServiceTests
{
    private Mock<ICompanyRepository> companyRepoMock;
    private Mock<IBankService> bankMock;
    private Mock<IPhotonTurnManager> turnManagerMock;
    private Mock<ICompanySyncService> syncMock;
    private Mock<IPlayerRepository> playerRepoMock;
    private Mock<IEventBus> eventBusMock;
    private Mock<IPhotonNetworkWrapper> photonMock;

    private CompanyService service;

    [SetUp]
    public void SetUp()
    {
        companyRepoMock = new Mock<ICompanyRepository>();
        bankMock = new Mock<IBankService>();
        turnManagerMock = new Mock<IPhotonTurnManager>();
        syncMock = new Mock<ICompanySyncService>();
        playerRepoMock = new Mock<IPlayerRepository>();
        eventBusMock = new Mock<IEventBus>();
        photonMock = new Mock<IPhotonNetworkWrapper>();

        service = new CompanyService();
        service.Construct(
            companyRepoMock.Object,
            bankMock.Object,
            turnManagerMock.Object,
            syncMock.Object,
            playerRepoMock.Object,
            eventBusMock.Object,
            photonMock.Object
        );
    }

    // ---------------- HandleCell ----------------

    [Test]
    public void HandleCell_ShouldThrow_WhenCompanyNotFound()
    {
        companyRepoMock.Setup(r => r.GetCompanyById(5)).Returns((Company)null);
        Assert.Throws<InvalidOperationException>(() => service.HandleCell(5, 1));
    }

    [Test]
    public void HandleCell_ShouldPublishOfferPurchase_WhenCompanyNotBoughtAndCanAfford()
    {
        var company = new Company(1, new CompanyData()) { IsBought = false, Price = 100 };
        companyRepoMock.Setup(r => r.GetCompanyById(1)).Returns(company);
        bankMock.Setup(b => b.HasEnoughMoney(10, 100)).Returns(true);

        service.HandleCell(1, 10);

        eventBusMock.Verify(e => e.Publish(It.Is<OfferPurchaseEvent>(
            ev => ev.CellIndex == 1 && ev.PlayerId == 10 && ev.CanAfford)), Times.Once);
    }

    [Test]
    public void HandleCell_ShouldPublishOfferRent_WhenCompanyOwnedByAnotherPlayer()
    {
        CompanyData companyData = new CompanyData() { rent = new int[1] { 200 } };
        var company = new Company(1, companyData);
        company.Id = 1;
        company.IsBought = true;
        company.OwnerId = 99;
        company.Type = CompanyType.Company;
        company.IsMortgaged = false;
        company.RentLevel = 0;

        companyRepoMock.Setup(r => r.GetCompanyById(1)).Returns(company);
        companyRepoMock.Setup(r => r.CountOwnedByPlayer(99, company.Type)).Returns(1);
        playerRepoMock.Setup(p => p.GetPlayerById(10)).Returns(new PlayerData("P", 1000, 10, null) { LastDiceSum = 7 });

        service.HandleCell(1, 10);

        eventBusMock.Verify(e => e.Publish(It.IsAny<OfferRentEvent>()), Times.Once);
    }

    [Test]
    public void HandleCell_ShouldEndTurn_WhenPlayerOwnsCompany()
    {
        var company = new Company(2, new CompanyData());
        company.IsBought = true;
        company.OwnerId = 10;
        company.IsMortgaged = false;

        companyRepoMock.Setup(r => r.GetCompanyById(2)).Returns(company);
        photonMock.Setup(p => p.IsMasterClient).Returns(true);

        service.HandleCell(2, 10);

        turnManagerMock.Verify(t => t.RequestEndTurn(), Times.Once);
    }

    // ---------------- TryBuyCompany ----------------

    [Test]
    public void TryBuyCompany_ShouldBuyAndSync_WhenEnoughMoney()
    {
        var company = new Company(1, new CompanyData());
        company.IsBought = false;
        company.Price = 500;

        companyRepoMock.Setup(r => r.GetCompanyById(1)).Returns(company);
        bankMock.Setup(b => b.HasEnoughMoney(7, 500)).Returns(true);

        service.TryBuyCompany(1, 7, 999, BuyReason.Buy);

        bankMock.Verify(b => b.RemoveMoney(7, 500), Times.Once);
        syncMock.Verify(s => s.SyncCompanyBought(1, 7, 500), Times.Once);
        eventBusMock.Verify(e => e.Publish(It.Is<CompanyBoughtEvent>(ev => ev.CellIndex == 1 && ev.PlayerId == 7)), Times.Once);
        turnManagerMock.Verify(t => t.RequestEndTurn(), Times.Once);
    }

    [Test]
    public void TryBuyCompany_ShouldNotBuy_WhenNotEnoughMoney()
    {
        var company = new Company(2, new CompanyData());
        company.IsBought = false;
        company.Price = 1000;

        companyRepoMock.Setup(r => r.GetCompanyById(2)).Returns(company);
        bankMock.Setup(b => b.HasEnoughMoney(5, 1000)).Returns(false);

        service.TryBuyCompany(2, 5, 1000, BuyReason.Buy);

        bankMock.Verify(b => b.RemoveMoney(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
        syncMock.Verify(s => s.SyncCompanyBought(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()), Times.Never);
    }

    [Test]
    public void TryBuyCompany_ShouldDoNothing_WhenAlreadyBought()
    {
        var company = new Company(3, new CompanyData());
        company.IsBought = true;

        companyRepoMock.Setup(r => r.GetCompanyById(3)).Returns(company);
        service.TryBuyCompany(3, 1, 100, BuyReason.Buy);

        bankMock.Verify(b => b.RemoveMoney(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
        syncMock.Verify(s => s.SyncCompanyBought(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()), Times.Never);
        eventBusMock.VerifyNoOtherCalls();
    }

    // ---------------- TryPayRent ----------------

    [Test]
    public void TryPayRent_ShouldTransferMoney_WhenEnoughMoney()
    {
        CompanyData companyData = new CompanyData() { rent = new int[1] { 200 } };
        var company = new Company(4, companyData);
        company.IsBought = true;
        company.OwnerId = 99;
        company.RentLevel = 0;


        companyRepoMock.Setup(r => r.GetCompanyById(4)).Returns(company);
        playerRepoMock.Setup(p => p.GetPlayerById(10)).Returns(new PlayerData("P", 500, 10, null) { LastDiceSum = 6 });
        bankMock.Setup(b => b.HasEnoughMoney(10, 200)).Returns(true);
        companyRepoMock.Setup(r => r.CountOwnedByPlayer(99, company.Type)).Returns(1);

        service.TryPayRent(4, 10);

        bankMock.Verify(b => b.TransferMoney(10, 99, 200), Times.Once);
        syncMock.Verify(s => s.SyncRentPaid(4, 10, 99, 200), Times.Once);
        turnManagerMock.Verify(t => t.RequestEndTurn(), Times.Once);
        eventBusMock.Verify(e => e.Publish(It.IsAny<RentPaidEvent>()), Times.Once);
    }

    [Test]
    public void TryPayRent_ShouldNotPay_WhenNotEnoughMoney()
    {
        CompanyData companyData = new CompanyData() { rent = new int[1] { 500 } };
        var company = new Company(5, companyData);
        company.IsBought = true;
        company.OwnerId = 99;
        company.RentLevel = 0;

        companyRepoMock.Setup(r => r.GetCompanyById(5)).Returns(company);
        playerRepoMock.Setup(p => p.GetPlayerById(7)).Returns(new PlayerData("A", 100, 7, null));
        bankMock.Setup(b => b.HasEnoughMoney(7, 500)).Returns(false);

        service.TryPayRent(5, 7);

        bankMock.Verify(b => b.TransferMoney(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()), Times.Never);
        syncMock.Verify(s => s.SyncRentPaid(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()), Times.Never);
    }

    [Test]
    public void TryPayRent_ShouldNotPay_WhenCompanyNotBought()
    {
        var company = new Company(6, new CompanyData());
        company.IsBought = false;

     


        companyRepoMock.Setup(r => r.GetCompanyById(6)).Returns(company);
        playerRepoMock.Setup(p => p.GetPlayerById(8)).Returns(new PlayerData("B", 100, 8, null));

        service.TryPayRent(6, 8);

        bankMock.Verify(b => b.TransferMoney(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()), Times.Never);
        syncMock.Verify(s => s.SyncRentPaid(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()), Times.Never);
    }

    [Test]
    public void TryPayRent_ShouldThrow_WhenPlayerNotFound()
    {
        var company = new Company(7, new CompanyData());
        company.IsBought = true;


    

        companyRepoMock.Setup(r => r.GetCompanyById(7)).Returns(company);
        playerRepoMock.Setup(p => p.GetPlayerById(9)).Returns((PlayerData)null);

        Assert.Throws<InvalidOperationException>(() => service.TryPayRent(7, 9));
    }

    // ---------------- CalculateRent ----------------

    [Test]
    public void CalculateDiceRent_ShouldReturnCorrectRent()
    {
        DiceCompanyData companyData = new DiceCompanyData() { rentMultiplier = new int[1] { 100 } };
        var company = new Company(5, companyData);
        company.IsBought = true;
        company.OwnerId = 1;
        company.RentLevel = 0;
     
        company.Type = CompanyType.DiceCompany;

        companyRepoMock.Setup(r => r.CountOwnedByPlayer(1, CompanyType.DiceCompany)).Returns(1);

        var rent = service.CalculateRent(company, 8);

        Assert.AreEqual(800, rent);
    }
    [Test]
    public void CalculateFieldRent_ShouldReturnCorrectRent()
    {
        FieldCompanyData companyData = new FieldCompanyData() { rentField = new int[4] { 250,500,1000,2000 } };
        var company = new Company(5, companyData);
        company.IsBought = true;
        company.OwnerId = 1;
        company.RentLevel = 0;

        company.Type = CompanyType.FieldCompany;

        companyRepoMock.Setup(r => r.CountOwnedByPlayer(1, CompanyType.FieldCompany)).Returns(3);

        var rent = service.CalculateRent(company, 0);

        Assert.AreEqual(1000, rent);
    }
    // ---------------- Auction Event ----------------

   
}