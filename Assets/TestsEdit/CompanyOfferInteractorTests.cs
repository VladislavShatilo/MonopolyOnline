using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;

[TestFixture]
public class CompanyOfferInteractorTests
{
    private CompanyOfferInteractor interactor;
    private Mock<ITradeService> tradeServiceMock;
    private Mock<ICompanyRepository> companyRepositoryMock;
    private Company company;
    private TradeOffer offer;
    private PlayerData fromPlayer;
    private PlayerData toPlayer;

    [SetUp]
    public void Setup()
    {
        tradeServiceMock = new Mock<ITradeService>();
        companyRepositoryMock = new Mock<ICompanyRepository>();

        // Игроки
        fromPlayer = new PlayerData("Player1", 1000, 1, null);
        toPlayer = new PlayerData("Player2", 1000, 2, null);

        // Компания
        company = new Company(1, new CompanyData
        {
            name = "TestCompany",
            price = 1000,
            pledgePrice = 100,
            buyoutPrice = 200,
            branchPrice = 50,
            group = CompanyGroup.Clothes
        });
        company.OwnerId = fromPlayer.Id;

        companyRepositoryMock.Setup(c => c.GetCompanyById(1)).Returns(company);

        // TradeOffer
        offer = new TradeOffer(fromPlayer, toPlayer);
        offer.SetFromCompanies(new List<Company>());
        offer.SetToCompanies(new List<Company>());

        tradeServiceMock.Setup(ts => ts.CurrentOffer).Returns(offer);
        tradeServiceMock.Setup(ts => ts.IsTradeActive).Returns(true);

        interactor = new CompanyOfferInteractor(tradeServiceMock.Object, companyRepositoryMock.Object);
    }
    [Test]
    public void TryToggleCompanyInOffer_ShouldThrow_WhenCompanyDoesNotExist()
    {
        companyRepositoryMock.Setup(c => c.GetCompanyById(99)).Returns((Company)null);

        Assert.Throws<InvalidOperationException>(() => interactor.TryToggleCompanyInOffer(99));

        tradeServiceMock.Verify(ts => ts.AddCompanyToOffer(It.IsAny<int>(), It.IsAny<Company>()), Times.Never);
        tradeServiceMock.Verify(ts => ts.RemoveCompanyFromOffer(It.IsAny<int>(), It.IsAny<Company>()), Times.Never);
    }

    [Test]
    public void TryToggleCompanyInOffer_ShouldReturnFalse_WhenTradeIsNotActive()
    {
        tradeServiceMock.Setup(ts => ts.IsTradeActive).Returns(false);

        var result = interactor.TryToggleCompanyInOffer(1);

        Assert.IsFalse(result);
        tradeServiceMock.Verify(ts => ts.AddCompanyToOffer(It.IsAny<int>(), It.IsAny<Company>()), Times.Never);
        tradeServiceMock.Verify(ts => ts.RemoveCompanyFromOffer(It.IsAny<int>(), It.IsAny<Company>()), Times.Never);
    }

    [Test]
    public void TryToggleCompanyInOffer_ShouldAddCompany_WhenNotAlreadyInOffer()
    {
        var result = interactor.TryToggleCompanyInOffer(1);

        Assert.IsTrue(result);
        tradeServiceMock.Verify(ts => ts.AddCompanyToOffer(company.OwnerId, company), Times.Once);
    }

    [Test]
    public void TryToggleCompanyInOffer_ShouldRemoveCompany_WhenAlreadyInOffer()
    {
        // Добавим компанию в текущий оффер
        offer.SetFromCompanies(new List<Company> { company });

        var result = interactor.TryToggleCompanyInOffer(1);

        Assert.IsTrue(result);
        tradeServiceMock.Verify(ts => ts.RemoveCompanyFromOffer(company.OwnerId, company), Times.Once);
    }

    [Test]
    public void TryToggleCompanyInOffer_ShouldReturnFalse_WhenCompanyOwnerNotInOfferPlayers()
    {
        // Сделаем компанию чужой
        company.OwnerId = 99;

        var result = interactor.TryToggleCompanyInOffer(1);

        Assert.IsFalse(result);
        tradeServiceMock.Verify(ts => ts.AddCompanyToOffer(It.IsAny<int>(), It.IsAny<Company>()), Times.Never);
        tradeServiceMock.Verify(ts => ts.RemoveCompanyFromOffer(It.IsAny<int>(), It.IsAny<Company>()), Times.Never);
    }

    [Test]
    public void Constructor_ShouldThrow_WhenTradeServiceIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => new CompanyOfferInteractor(null, companyRepositoryMock.Object));
    }

    [Test]
    public void Constructor_ShouldThrow_WhenCompanyRepositoryIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => new CompanyOfferInteractor(tradeServiceMock.Object, null));
    }

    [Test]
    public void TryToggleCompanyInOffer_ShouldThrow_WhenCompanyNotFound()
    {
        companyRepositoryMock.Setup(c => c.GetCompanyById(It.IsAny<int>())).Returns((Company)null);
        Assert.Throws<InvalidOperationException>(() => interactor.TryToggleCompanyInOffer(99));
    }
}
