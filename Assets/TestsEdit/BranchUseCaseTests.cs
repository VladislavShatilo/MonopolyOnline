using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;

[TestFixture]
public class BranchUseCaseFullTests
{
    private BranchUseCase useCase;
    private Mock<IBranchService> branchService;
    private Mock<ICompanyUIService> companyUIService;
    private Mock<IBankService> bankService;
    private Mock<ICompanyRepository> companyRepository;
    private Company company;
    private Mock<IUICompanyCellView> companyUI;

    [SetUp]
    public void Setup()
    {
        branchService = new Mock<IBranchService>();
        companyUIService = new Mock<ICompanyUIService>();
        bankService = new Mock<IBankService>();
        companyRepository = new Mock<ICompanyRepository>();

        var companyData = new CompanyData
        {
            rent = new int[4] { 100, 100, 100, 100 },
            branchPrice = 100
        };

        company = new Company(1, companyData)
        {
            BranchPrice = 500,
            RentLevel = 1,
            Type = CompanyType.Company,
            Group = CompanyGroup.Clothes,
            OwnerId = 1
        };

        companyUI = new Mock<IUICompanyCellView>();
        companyUIService.Setup(u => u.GetCompanyUI(1)).Returns(companyUI.Object);

        useCase = new BranchUseCase();
        useCase.Construct(branchService.Object, companyUIService.Object, bankService.Object, companyRepository.Object);
    }

    #region BuyBranch Tests

    [Test]
    public void BuyBranch_ShouldRemoveMoneyAndReturnNewLevel_WhenSuccess()
    {
        branchService.Setup(s => s.TryBuyBranch(1, 1, out company)).Returns(true);

        int level = useCase.BuyBranch(1, 1);

        Assert.AreEqual(1, level);
        bankService.Verify(b => b.RemoveMoney(1, 500), Times.Once);
    }

    [Test]
    public void BuyBranch_ShouldReturnZero_WhenFail()
    {
        branchService.Setup(s => s.TryBuyBranch(1, 1, out company)).Returns(false);

        int level = useCase.BuyBranch(1, 1);

        Assert.AreEqual(0, level);
        bankService.Verify(b => b.RemoveMoney(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
    }

    #endregion

    #region SellBranch Tests

    [Test]
    public void SellBranch_ShouldAddMoneyAndUpdateUI_WhenSuccess()
    {
        branchService.Setup(s => s.TrySellBranch(1, 1, out company)).Returns(true);
        companyRepository.Setup(r => r.CountOwnedByPlayer(1, company.Type)).Returns(1);

        int level = useCase.SellBranch(1, 1);

        Assert.AreEqual(1, level);
        bankService.Verify(b => b.AddMoney(1, 500), Times.Once);
        companyUI.Verify(u => u.UpdateBranchStars(1), Times.Once);
        companyUI.Verify(u => u.SetRentText(company.GetRent(1,0)), Times.Once);
    }

    [Test]
    public void SellBranch_ShouldReturnZero_WhenFail()
    {
        branchService.Setup(s => s.TrySellBranch(1, 1, out company)).Returns(false);

        int level = useCase.SellBranch(1, 1);

        Assert.AreEqual(0, level);
        bankService.Verify(b => b.AddMoney(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
        companyUI.Verify(u => u.UpdateBranchStars(It.IsAny<int>()), Times.Never);
        companyUI.Verify(u => u.SetRentText(It.IsAny<int>()), Times.Never);
    }

    [Test]
    public void SellBranch_ShouldThrow_WhenUIIsNull()
    {
        branchService.Setup(s => s.TrySellBranch(1, 1, out company)).Returns(true);
        companyUIService.Setup(u => u.GetCompanyUI(1)).Returns((IUICompanyCellView)null);

        Assert.Throws<InvalidOperationException>(() => useCase.SellBranch(1, 1));
    }

    [Test]
    public void SellBranch_ShouldThrow_WhenOwnedCountNegative()
    {
        branchService.Setup(s => s.TrySellBranch(1, 1, out company)).Returns(true);
        companyUIService.Setup(u => u.GetCompanyUI(1)).Returns(companyUI.Object);
        companyRepository.Setup(r => r.CountOwnedByPlayer(company.OwnerId, company.Type)).Returns(-1);

        Assert.Throws<InvalidOperationException>(() => useCase.SellBranch(1, 1));
    }

    #endregion

    #region UpdateBranchUI Tests

    [Test]
    public void UpdateBranchUI_ShouldUpdateUIAndHideButtons()
    {
        var company2 = new Company(2, new CompanyData()) { Group = CompanyGroup.Clothes };
        var ui2 = new Mock<IUICompanyCellView>();

        companyRepository.Setup(r => r.GetCompanyById(1)).Returns(company);
        companyRepository.Setup(r => r.CountOwnedByPlayer(company.OwnerId, company.Type)).Returns(1);
        companyRepository.Setup(r => r.GetByGroup(CompanyGroup.Clothes)).Returns(new List<Company> { company, company2 });
        companyUIService.Setup(u => u.GetCompanyUI(1)).Returns(companyUI.Object);
        companyUIService.Setup(u => u.GetCompanyUI(2)).Returns(ui2.Object);

        useCase.UpdateBranchUI(1, 1, 3);

        Assert.AreEqual(3, company.RentLevel);
        companyUI.Verify(u => u.UpdateBranchStars(3), Times.Once);
        companyUI.Verify(u => u.SetRentText(company.GetRent(1, 0)), Times.Once);
        companyUI.Verify(u => u.HideAllBranchButtons(), Times.Once);
        ui2.Verify(u => u.HideAllBranchButtons(), Times.Once);
    }

    [Test]
    public void UpdateBranchUI_ShouldThrow_WhenCompanyIsNull()
    {
        companyRepository.Setup(r => r.GetCompanyById(2)).Returns((Company)null);

        Assert.Throws<InvalidOperationException>(() => useCase.UpdateBranchUI(2, 1, 3));
    }

    [Test]
    public void UpdateBranchUI_ShouldThrow_WhenUIIsNull()
    {
        companyRepository.Setup(r => r.GetCompanyById(1)).Returns(company);
        companyUIService.Setup(u => u.GetCompanyUI(1)).Returns((IUICompanyCellView)null);

        Assert.Throws<InvalidOperationException>(() => useCase.UpdateBranchUI(1, 1, 3));
    }

    [Test]
    public void UpdateBranchUI_ShouldThrow_WhenOwnedCountNegative()
    {
        companyRepository.Setup(r => r.GetCompanyById(1)).Returns(company);
        companyUIService.Setup(u => u.GetCompanyUI(1)).Returns(companyUI.Object);
        companyRepository.Setup(r => r.CountOwnedByPlayer(company.OwnerId, company.Type)).Returns(-5);

        Assert.Throws<InvalidOperationException>(() => useCase.UpdateBranchUI(1, 1, 3));
    }

    #endregion
}
