using Moq;
using NUnit.Framework;
using System;
using System.ComponentModel.Design;

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

    [SetUp]
    public void Setup()
    {
        localPlayerService = new Mock<ILocalPlayerService>();
        buyWindow = new Mock<IBuyWindow>();
        photonCompanyManager = new Mock<IPhotonCompanyManager>();
        photonAuctionManager = new Mock<IPhotonAuctionManager>();
        companyRepository = new Mock<ICompanyRepository>();
        eventBus = new Mock<IEventBus>();

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

    [Test]
    public void Initialize_ShouldSubscribeToEventAndSetActions()
    {
        presenter.Initialize();

        eventBus.Verify(e => e.Subscribe<OfferPurchaseEvent>(It.IsAny<Action<OfferPurchaseEvent>>()), Times.Once);
        buyWindow.Verify(b => b.SetBuyAction(It.IsAny<Action<int>>()), Times.Once);
        buyWindow.Verify(b => b.SetAuctionAction(It.IsAny<Action<int>>()), Times.Once);
    }

    [Test]
    public void Dispose_ShouldUnsubscribeFromEvent()
    {
        presenter.Dispose();

        eventBus.Verify(e => e.Unsubscribe<OfferPurchaseEvent>(It.IsAny<Action<OfferPurchaseEvent>>()), Times.Once);
    }

    [Test]
    public void TryBuyCompany_ShouldCallRequestBuyCompany()
    {
        localPlayerService.Setup(l => l.GetLocalPlayerId()).Returns(42);

        presenter.TryBuyCompany(5);

        photonCompanyManager.Verify(p => p.RequestBuyCompany(5, 42, BuyReason.Buy), Times.Once);
    }

    [Test]
    public void StartAuctionRequest_ShouldCallStartAuctionRequest()
    {
        localPlayerService.Setup(l => l.GetLocalPlayerId()).Returns(42);
        var company = new Company(5, new CompanyData());
        company.Price = 1000;
        companyRepository.Setup(r => r.GetCompanyById(5)).Returns(company);

        presenter.StartAuctionRequest(5);

        photonAuctionManager.Verify(p => p.StartAuctionRequest(42, 5, 1000), Times.Once);
    }

    [Test]
    public void BuyWindowShow_ShouldShowWindow_WhenPlayerIsLocal()
    {
        localPlayerService.Setup(l => l.GetLocalPlayerId()).Returns(1);
        var evt = new OfferPurchaseEvent(3, 1, 500, true);

        // вызываем приватный метод через reflection или сделаем его internal + InternalsVisibleTo
        presenter.GetType().GetMethod("BuyWindowShow", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .Invoke(presenter, new object[] { evt });

        buyWindow.Verify(b => b.Show(1, 3, 500, true), Times.Once);
    }

    [Test]
    public void BuyWindowShow_ShouldHideWindow_WhenPlayerIsNotLocal()
    {
        localPlayerService.Setup(l => l.GetLocalPlayerId()).Returns(1);
        var evt = new OfferPurchaseEvent(2, 2, 500, true);

        presenter.GetType().GetMethod("BuyWindowShow", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .Invoke(presenter, new object[] { evt });

        buyWindow.Verify(b => b.Hide(), Times.Once);
    }

    [Test]
    public void HideTurn_ShouldHideWindow()
    {
        presenter.HideTurn();

        buyWindow.Verify(b => b.Hide(), Times.Once);
    }

    [Test]
    public void ShowTurnFor_ShouldHideWindow()
    {
        presenter.ShowTurnFor(42);

        buyWindow.Verify(b => b.Hide(), Times.Once);
    }
}
