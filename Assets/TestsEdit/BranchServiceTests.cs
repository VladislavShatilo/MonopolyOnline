using NUnit.Framework;
using Moq;

[TestFixture]
public class BranchServiceTests
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

    [Test]
    public void TryBuyBranch_ShouldIncreaseRentLevel_WhenValid()
    {
        bool result = branchService.TryBuyBranch(1, 1, out var resultCompany);

        Assert.IsTrue(result);
        Assert.AreEqual(2, resultCompany.RentLevel);
    }

    [Test]
    public void TryBuyBranch_ShouldFail_WhenCompanyIsNull()
    {
        companyRepository.Setup(r => r.GetCompanyById(2)).Returns((Company)null);

        bool result = branchService.TryBuyBranch(2, 1, out var resultCompany);

        Assert.IsFalse(result);
        Assert.IsNull(resultCompany);
    }

    [Test]
    public void TryBuyBranch_ShouldFail_WhenPlayerIsNull()
    {
        playerRepository.Setup(r => r.GetPlayerById(2)).Returns((PlayerData)null);

        bool result = branchService.TryBuyBranch(1, 2, out var resultCompany);

        Assert.IsFalse(result);
        Assert.AreEqual(company, resultCompany); // компания всё равно возвращается
    }

    [Test]
    public void TryBuyBranch_ShouldFail_WhenOwnerMismatch()
    {
        company.OwnerId = 2;

        bool result = branchService.TryBuyBranch(1, 1, out var resultCompany);

        Assert.IsFalse(result);
        Assert.AreEqual(1, resultCompany.RentLevel); // уровень не меняется
    }

    [Test]
    public void TryBuyBranch_ShouldFail_WhenMaxLevelReached()
    {
        company.RentLevel = 3;

        bool result = branchService.TryBuyBranch(1, 1, out var resultCompany);

        Assert.IsFalse(result);
        Assert.AreEqual(3, resultCompany.RentLevel);
    }

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
    public void TrySellBranch_ShouldFail_WhenCompanyIsNull()
    {
        companyRepository.Setup(r => r.GetCompanyById(2)).Returns((Company)null);

        bool result = branchService.TrySellBranch(2, 1, out var resultCompany);

        Assert.IsFalse(result);
        Assert.IsNull(resultCompany);
    }
}
