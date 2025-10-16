using Moq;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;

[TestFixture]
public class TurnCompanyUIUseCaseTests
{
    private TurnCompanyUIUseCase useCase;
    private Mock<ICompanyRepository> companyRepoMock;
    private Mock<IGroupOwnershipService> groupOwnershipMock;
    private Mock<ILocalPlayerService> localPlayerMock;

    [SetUp]
    public void SetUp()
    {
        companyRepoMock = new Mock<ICompanyRepository>();
        groupOwnershipMock = new Mock<IGroupOwnershipService>();
        localPlayerMock = new Mock<ILocalPlayerService>();

        useCase = new TurnCompanyUIUseCase();
        useCase.Construct(companyRepoMock.Object, groupOwnershipMock.Object, localPlayerMock.Object);
    }

    [Test]
    public void GetAvailableActions_ShouldReturnNone_WhenCompanyNotBought()
    {
        // Arrange
        var company = new Company(1, new CompanyData()) { IsBought = false, OwnerId = 1 };
        companyRepoMock.Setup(r => r.GetAll()).Returns(new List<Company> { company });
        localPlayerMock.Setup(p => p.GetLocalPlayerId()).Returns(1);

        // Act
        var actions = useCase.GetAvailableActions(1).ToList();

        // Assert
        Assert.AreEqual(1, actions.Count);
        Assert.AreEqual(CompanyActionType.None, actions[0].ActionType);
    }

    [Test]
    public void GetAvailableActions_ShouldReturnMortgage_WhenCompanyBoughtButNotWholeGroup()
    {
        var company = new Company(1, new CompanyData()) { IsBought = true, OwnerId = 1, Type = CompanyType.Company, IsMortgaged = false, Group = CompanyGroup.Airlines };
        companyRepoMock.Setup(r => r.GetAll()).Returns(new List<Company> { company });
        groupOwnershipMock.Setup(g => g.PlayerOwnsWholeGroup(CompanyGroup.Airlines, 1)).Returns(false);
        localPlayerMock.Setup(p => p.GetLocalPlayerId()).Returns(1);

        var actions = useCase.GetAvailableActions(1).ToList();

        Assert.AreEqual(1, actions.Count);
        Assert.AreEqual(CompanyActionType.Mortgage, actions[0].ActionType);
    }

    [Test]
    public void GetAvailableActions_ShouldReturnBuyout_WhenCompanyBoughtButNotWholeGroup_AndMortgaged()
    {
        var company = new Company(1, new CompanyData()) { IsBought = true, OwnerId = 1, Type = CompanyType.Company, IsMortgaged = true, Group = CompanyGroup.Clothes };
        companyRepoMock.Setup(r => r.GetAll()).Returns(new List<Company> { company });
        groupOwnershipMock.Setup(g => g.PlayerOwnsWholeGroup(CompanyGroup.Clothes, 1)).Returns(false);
        localPlayerMock.Setup(p => p.GetLocalPlayerId()).Returns(1);

        var actions = useCase.GetAvailableActions(1).ToList();

        Assert.AreEqual(1, actions.Count);
        Assert.AreEqual(CompanyActionType.Buyout, actions[0].ActionType);
    }

    [Test]
    public void GetAvailableActions_ShouldReturnManageBranches_WhenOwnsWholeGroup_NoMortgaged()
    {
        var company = new Company(1, new CompanyData()) { IsBought = true, OwnerId = 1, Type = CompanyType.Company, IsMortgaged = false, Group = CompanyGroup.Cars };
        var groupCompanies = new List<Company> { company };

        companyRepoMock.Setup(r => r.GetAll()).Returns(new List<Company> { company });
        companyRepoMock.Setup(r => r.GetByGroup(CompanyGroup.Cars)).Returns(groupCompanies);
        groupOwnershipMock.Setup(g => g.PlayerOwnsWholeGroup(CompanyGroup.Cars, 1)).Returns(true);
        localPlayerMock.Setup(p => p.GetLocalPlayerId()).Returns(1);

        var actions = useCase.GetAvailableActions(1).ToList();

        Assert.AreEqual(1, actions.Count);
        Assert.AreEqual(CompanyActionType.ManageBranches, actions[0].ActionType);
    }

    [Test]
    public void GetAvailableActions_ShouldReturnMortgage_WhenOwnsWholeGroup_ButOtherMortgaged()
    {
        var company = new Company(1, new CompanyData()) { IsBought = true, OwnerId = 1, Type = CompanyType.Company, IsMortgaged = false, Group = CompanyGroup.SocialMedia };
        var mortgagedCompany = new Company(2, new CompanyData()) { IsBought = true, OwnerId = 1, Type = CompanyType.Company, IsMortgaged = true, Group = CompanyGroup.SocialMedia };
        var groupCompanies = new List<Company> { company, mortgagedCompany };

        companyRepoMock.Setup(r => r.GetAll()).Returns(new List<Company> { company });
        companyRepoMock.Setup(r => r.GetByGroup(CompanyGroup.SocialMedia)).Returns(groupCompanies);
        groupOwnershipMock.Setup(g => g.PlayerOwnsWholeGroup(CompanyGroup.SocialMedia, 1)).Returns(true);
        localPlayerMock.Setup(p => p.GetLocalPlayerId()).Returns(1);

        var actions = useCase.GetAvailableActions(1).ToList();

        Assert.AreEqual(1, actions.Count);
        Assert.AreEqual(CompanyActionType.Mortgage, actions[0].ActionType);
    }

    [Test]
    public void GetAvailableActions_ShouldReturnBuyout_WhenOwnsWholeGroup_ButCompanyIsMortgaged()
    {
        var company = new Company(1, new CompanyData()) { IsBought = true, OwnerId = 1, Type = CompanyType.Company, IsMortgaged = true, Group = CompanyGroup.FastFood };
        var mortgagedCompany = new Company(2, new CompanyData()) { IsBought = true, OwnerId = 1, Type = CompanyType.Company, IsMortgaged = false, Group = CompanyGroup.FastFood };
        var groupCompanies = new List<Company> { company, mortgagedCompany };

        companyRepoMock.Setup(r => r.GetAll()).Returns(new List<Company> { company });
        companyRepoMock.Setup(r => r.GetByGroup(CompanyGroup.FastFood)).Returns(groupCompanies);
        groupOwnershipMock.Setup(g => g.PlayerOwnsWholeGroup(CompanyGroup.FastFood, 1)).Returns(true);
        localPlayerMock.Setup(p => p.GetLocalPlayerId()).Returns(1);

        var actions = useCase.GetAvailableActions(1).ToList();

        Assert.AreEqual(1, actions.Count);
        Assert.AreEqual(CompanyActionType.Buyout, actions[0].ActionType);
    }
   

    [Test]
    public void GetAvailableActions_ShouldReturnMortgage_WhenGroupHasMortgagedCompany()
    {
        // Arrange
        var company = new Company(1, new CompanyData())
        {
            IsBought = true,
            OwnerId = 1,
            Type = CompanyType.Company,
            IsMortgaged = false,
            Group = CompanyGroup.SocialMedia
        };

        var mortgagedCompany = new Company(2, new CompanyData())
        {
            IsBought = true,
            OwnerId = 1,
            Type = CompanyType.Company,
            IsMortgaged = true,
            Group = CompanyGroup.SocialMedia
        };

        companyRepoMock.Setup(r => r.GetAll()).Returns(new List<Company> { company });
        companyRepoMock.Setup(r => r.GetByGroup(CompanyGroup.SocialMedia))
            .Returns(new List<Company> { company, mortgagedCompany });

        groupOwnershipMock.Setup(g => g.PlayerOwnsWholeGroup(CompanyGroup.SocialMedia, 1)).Returns(true);
        localPlayerMock.Setup(p => p.GetLocalPlayerId()).Returns(1);

        // Act
        var actions = useCase.GetAvailableActions(1).ToList();

        // Assert
        Assert.AreEqual(1, actions.Count);
        Assert.AreEqual(CompanyActionType.Mortgage, actions[0].ActionType);
    }

}
