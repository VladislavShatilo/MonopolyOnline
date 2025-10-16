using NUnit.Framework;
using Moq;
using System;

[TestFixture]
public class BranchServiceFullTests
{
    private BranchService branchService;
    private Mock<ICompanyRepository> companyRepository;
    private Mock<IPlayerRepository> playerRepository;
    private GameSettings gameSettings;

    private Company company;
    private PlayerData player;

    [SetUp]
    public void Setup()
    {
        companyRepository = new Mock<ICompanyRepository>();
        playerRepository = new Mock<IPlayerRepository>();
        gameSettings = new GameSettings { maxBranchLevel = 3 };

        branchService = new BranchService();
        branchService.Construct(companyRepository.Object, playerRepository.Object, gameSettings);

        player = new PlayerData("player1", 500, 1, null);
        CompanyData companyData = new CompanyData();
        company = new Company(1, companyData);
        company.OwnerId = 1;
        company.RentLevel = 1;

        companyRepository.Setup(r => r.GetCompanyById(1)).Returns(company);
        playerRepository.Setup(r => r.GetPlayerById(1)).Returns(player);
    }

    #region TryBuyBranch Tests

    [Test]
    public void TryBuyBranch_ShouldIncreaseRentLevel_WhenValid()
    {
        bool result = branchService.TryBuyBranch(1, 1, out var resultCompany);

        Assert.IsTrue(result);
        Assert.AreEqual(2, resultCompany.RentLevel);
    }

    [Test]
    public void TryBuyBranch_ShouldIncreaseToMaxLevel_WhenAtMaxMinusOne()
    {
        company.RentLevel = gameSettings.maxBranchLevel - 1;

        bool result = branchService.TryBuyBranch(1, 1, out var resultCompany);

        Assert.IsTrue(result);
        Assert.AreEqual(gameSettings.maxBranchLevel, resultCompany.RentLevel);
    }

    [Test]
    public void TryBuyBranch_ShouldFail_WhenMaxLevelReached()
    {
        company.RentLevel = gameSettings.maxBranchLevel;

        bool result = branchService.TryBuyBranch(1, 1, out var resultCompany);

        Assert.IsFalse(result);
        Assert.AreEqual(gameSettings.maxBranchLevel, resultCompany.RentLevel);
    }

    [Test]
    public void TryBuyBranch_ShouldFail_WhenOwnerMismatch()
    {
        company.OwnerId = 2;

        bool result = branchService.TryBuyBranch(1, 1, out var resultCompany);

        Assert.IsFalse(result);
        Assert.AreEqual(1, resultCompany.RentLevel);
    }

    [Test]
    public void TryBuyBranch_ShouldThrow_WhenCompanyNull()
    {
        companyRepository.Setup(r => r.GetCompanyById(99)).Returns((Company)null);

        Assert.Throws<InvalidOperationException>(() =>
            branchService.TryBuyBranch(99, 1, out var resultCompany));
    }

    [Test]
    public void TryBuyBranch_ShouldThrow_WhenPlayerNull()
    {
        playerRepository.Setup(r => r.GetPlayerById(99)).Returns((PlayerData)null);

        Assert.Throws<InvalidOperationException>(() =>
            branchService.TryBuyBranch(1, 99, out var resultCompany));
    }

    #endregion

    #region TrySellBranch Tests

    [Test]
    public void TrySellBranch_ShouldDecreaseRentLevel_WhenValid()
    {
        company.RentLevel = 2;

        bool result = branchService.TrySellBranch(1, 1, out var resultCompany);

        Assert.IsTrue(result);
        Assert.AreEqual(1, resultCompany.RentLevel);
    }

    [Test]
    public void TrySellBranch_ShouldFail_WhenRentLevelZero()
    {
        company.RentLevel = 0;

        bool result = branchService.TrySellBranch(1, 1, out var resultCompany);

        Assert.IsFalse(result);
        Assert.AreEqual(0, resultCompany.RentLevel);
    }

    [Test]
    public void TrySellBranch_ShouldFail_WhenOwnerMismatch()
    {
        company.OwnerId = 2;
        company.RentLevel = 2;

        bool result = branchService.TrySellBranch(1, 1, out var resultCompany);

        Assert.IsFalse(result);
        Assert.AreEqual(2, resultCompany.RentLevel);
    }

    [Test]
    public void TrySellBranch_ShouldThrow_WhenCompanyNull()
    {
        companyRepository.Setup(r => r.GetCompanyById(99)).Returns((Company)null);

        Assert.Throws<InvalidOperationException>(() =>
            branchService.TrySellBranch(99, 1, out var resultCompany));
    }

    [Test]
    public void TrySellBranch_ShouldThrow_WhenPlayerNull()
    {
        playerRepository.Setup(r => r.GetPlayerById(99)).Returns((PlayerData)null);

        Assert.Throws<InvalidOperationException>(() =>
            branchService.TrySellBranch(1, 99, out var resultCompany));
    }

    [Test]
    public void TrySellBranch_AtZeroLevel_ShouldReturnSameCompany()
    {
        company.RentLevel = 0;

        bool result = branchService.TrySellBranch(1, 1, out var resultCompany);

        Assert.IsFalse(result);
        Assert.AreEqual(0, resultCompany.RentLevel);
        Assert.AreSame(company, resultCompany);
    }

    #endregion
}
