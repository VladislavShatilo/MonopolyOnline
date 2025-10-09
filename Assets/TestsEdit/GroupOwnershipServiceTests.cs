using NUnit.Framework;
using Moq;
using System.Collections.Generic;
using System.Linq;

public class GroupOwnershipServiceTests
{
    private Mock<ICompanyRepository> companyRepoMock;
    private GroupOwnershipService service;

    [SetUp]
    public void Setup()
    {
        companyRepoMock = new Mock<ICompanyRepository>();
        service = new GroupOwnershipService();
        service.Construct(companyRepoMock.Object);
    }

    [Test]
    public void PlayerOwnsWholeGroup_AllOwnedAndNotMortgaged_ReturnsTrue()
    {
        // Arrange
        var group = new CompanyGroup();
        var playerId = 1;
        var companies = new List<Company>
        {
            new Company(0, new CompanyData()) { OwnerId = playerId, IsMortgaged = false },
            new Company(1, new CompanyData()) { OwnerId = playerId, IsMortgaged = false }
        };
        companyRepoMock.Setup(r => r.GetByGroup(group)).Returns(companies);

        // Act
        bool result = service.PlayerOwnsWholeGroup(group, playerId);

        // Assert
        Assert.IsTrue(result);
    }

    [Test]
    public void PlayerOwnsWholeGroup_OneCompanyNotOwned_ReturnsFalse()
    {
        var group = new CompanyGroup();
        var playerId = 1;
        var companies = new List<Company>
        {
            new Company(0, new CompanyData()) { OwnerId = playerId },
            new Company(1, new CompanyData()) { OwnerId = 2 } // не игрок
        };
        companyRepoMock.Setup(r => r.GetByGroup(group)).Returns(companies);

        bool result = service.PlayerOwnsWholeGroup(group, playerId);

        Assert.IsFalse(result);
    }

    [Test]
    public void PlayerOwnsWholeGroup_OneCompanyMortgaged_ReturnsFalse()
    {
        var group = new CompanyGroup();
        var playerId = 1;
        var companies = new List<Company>
        {
            new Company(0, new CompanyData()) { OwnerId = playerId },
            new Company(1, new CompanyData()) { OwnerId = playerId, IsMortgaged = true }
        };
        companyRepoMock.Setup(r => r.GetByGroup(group)).Returns(companies);

        bool result = service.PlayerOwnsWholeGroup(group, playerId);

        Assert.IsFalse(result);
    }

    [Test]
    public void PlayerOwnsWholeGroup_NoCompaniesInGroup_ReturnsFalse()
    {
        var group = new CompanyGroup();
        var playerId = 1;
        companyRepoMock.Setup(r => r.GetByGroup(group)).Returns(new List<Company>());

        bool result = service.PlayerOwnsWholeGroup(group, playerId);

        Assert.IsFalse(result);
    }

    [Test]
    public void PlayerOwnsWholeGroup_NullReturnedFromRepo_ReturnsFalse()
    {
        var group = new CompanyGroup();
        var playerId = 1;
        companyRepoMock.Setup(r => r.GetByGroup(group)).Returns((IEnumerable<Company>)null);

        bool result = service.PlayerOwnsWholeGroup(group, playerId);

        Assert.IsFalse(result);
    }
}
