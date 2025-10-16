using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Reflection;

[TestFixture]
public class BuyCompanyPresenterTests
{
    private BuyCompanyPresenter presenter;
    private Mock<ILocalPlayerService> localPlayerService;
    private Mock<IBuyWindow> buyWindow;
    private Mock<IPhotonCompanyManager> photonCompanyManager;
    private Mock<IPhotonAuctionManager> photonAuctionManager;
    private Mock<ICompanyRepository> companyRepository;
    private Mock<IEventBus> eventBus;

    private Company company;

    [SetUp]
    public void Setup()
    {
        localPlayerService = new Mock<ILocalPlayerService>();
        buyWindow = new Mock<IBuyWindow>();
        photonCompanyManager = new Mock<IPhotonCompanyManager>();
        photonAuctionManager = new Mock<IPhotonAuctionManager>();
        companyRepository = new Mock<ICompanyRepository>();
        eventBus = new Mock<IEventBus>();

        var companyData = new CompanyData();
      
        company = new Company(5, companyData)
        {
            Price = 1000

        };

        presenter = new BuyCompanyPresenter();
        presenter.Construct(
            localPlayerService.Object,
            buyWindow.Object,
            photonCompanyManager.Object,
            photonAuctionManager.Object,
            companyRepository.Object,
            eventBus.Object
        );
    }

    #region Initialization / Dispose

    [Test]
    public void Initialize_ShouldSubscribeAndSetActions()
    {
        presenter.Initialize();

        eventBus.Verify(e => e.Subscribe<OfferPurchaseEvent>(It.IsAny<Action<OfferPurchaseEvent>>()), Times.Once);
        buyWindow.Verify(b => b.SetBuyAction(It.IsAny<Action<int>>()), Times.Once);
        buyWindow.Verify(b => b.SetAuctionAction(It.IsAny<Action<int>>()), Times.Once);
    }

    [Test]
    public void Dispose_ShouldUnsubscribeEvent()
    {
        presenter.Dispose();

        eventBus.Verify(e => e.Unsubscribe<OfferPurchaseEvent>(It.IsAny<Action<OfferPurchaseEvent>>()), Times.Once);
    }

    #endregion

    #region TryBuyCompany

    [Test]
    public void TryBuyCompany_ShouldCallRequestBuyCompany()
    {
        localPlayerService.Setup(l => l.GetLocalPlayerId()).Returns(42);

        presenter.TryBuyCompany(5);

        photonCompanyManager.Verify(p => p.RequestBuyCompany(5, 42, BuyReason.Buy), Times.Once);
    }

    #endregion

    #region StartAuctionRequest

    [Test]
    public void StartAuctionRequest_ShouldCallStartAuctionRequest_WhenCompanyExists()
    {
        localPlayerService.Setup(l => l.GetLocalPlayerId()).Returns(42);
        companyRepository.Setup(r => r.GetCompanyById(5)).Returns(company);

        presenter.StartAuctionRequest(5);

        photonAuctionManager.Verify(p => p.StartAuctionRequest(42, 5, company.Price), Times.Once);
    }

    [Test]
    public void StartAuctionRequest_ShouldThrow_WhenCompanyIsNull()
    {
        localPlayerService.Setup(l => l.GetLocalPlayerId()).Returns(42);
        companyRepository.Setup(r => r.GetCompanyById(5)).Returns((Company)null);

        Assert.Throws<InvalidOperationException>(() => presenter.StartAuctionRequest(5));
    }

    #endregion

    #region BuyWindowShow (private)

    [Test]
    public void BuyWindowShow_ShouldShowWindow_WhenPlayerIsLocal()
    {
        localPlayerService.Setup(l => l.GetLocalPlayerId()).Returns(1);
        var evt = new OfferPurchaseEvent(5,1, 500, true);

        presenter.GetType().GetMethod("BuyWindowShow", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .Invoke(presenter, new object[] { evt });


        // Assert
       buyWindow.Verify(b => b.Show(1,5, 500, true), Times.Once);

    }
    [Test]
    public void BuyWindowShow_ShouldHideWindow_WhenPlayerIsNotLocal()
    {
        localPlayerService.Setup(l => l.GetLocalPlayerId()).Returns(1);
        var evt = new OfferPurchaseEvent(5,2, 500, true);

        presenter.GetType().GetMethod("BuyWindowShow", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .Invoke(presenter, new object[] { evt });

        buyWindow.Verify(b => b.Hide(), Times.Once);
    }

    [Test]
    public void BuyWindowShow_ShouldThrow_WhenEventIsNull()
    {
        Assert.Throws<TargetInvocationException>(() =>
            presenter.GetType().GetMethod("BuyWindowShow", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .Invoke(presenter, new object[] { null }));
    }

    #endregion
}
