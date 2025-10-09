using Moq;
using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

[TestFixture]
public class BranchUseCaseTests
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
            branchPrice = 100      // пример
        };

        company = new Company(1, companyData);
        company.BranchPrice = 500;
        company.RentLevel = 1;
        company.Type = CompanyType.Company;
        company.Group = CompanyGroup.Clothes;
        company.OwnerId = 1;

        companyUI = new Mock<IUICompanyCellView>();
        companyUIService.Setup(u => u.GetCompanyUI(1)).Returns(companyUI.Object);

        useCase = new BranchUseCase();
        useCase.Construct(branchService.Object, companyUIService.Object, bankService.Object, companyRepository.Object);
    }

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
    public void UpdateBranchUI_ShouldUpdateUIAndHideButtons()
    {
        companyRepository.Setup(r => r.GetCompanyById(1)).Returns(company);
        companyRepository.Setup(r => r.CountOwnedByPlayer(company.OwnerId, company.Type)).Returns(1);
        companyRepository.Setup(r => r.GetByGroup(CompanyGroup.Clothes)).Returns(new List<Company> { company });

        useCase.UpdateBranchUI(1, 1, 3);

        Assert.AreEqual(3, company.RentLevel);
        companyUI.Verify(u => u.UpdateBranchStars(3), Times.Once);
        companyUI.Verify(u => u.SetRentText(company.GetRent(1,0)), Times.Once);
        companyUI.Verify(u => u.HideAllBranchButtons(), Times.Once);
    }

    [Test]
    public void UpdateBranchUI_ShouldDoNothing_WhenCompanyIsNull()
    {
        companyRepository.Setup(r => r.GetCompanyById(2)).Returns((Company)null);

        useCase.UpdateBranchUI(2, 1, 3);

        companyUI.Verify(u => u.UpdateBranchStars(It.IsAny<int>()), Times.Never);
        companyUI.Verify(u => u.SetRentText(It.IsAny<int>()), Times.Never);
        companyUI.Verify(u => u.HideAllBranchButtons(), Times.Never);
    }
}
