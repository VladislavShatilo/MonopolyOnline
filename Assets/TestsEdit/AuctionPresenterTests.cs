using NUnit.Framework;
using Moq;
using System;
public class AuctionPresenterTests
{
    private Mock<IAuctionWindow> mockAuctionWindow;
    private Mock<ILocalPlayerService> mockLocalPlayerService;
    private Mock<IPhotonAuctionManager> mockPhotonAuctionManager;
    private Mock<IPlayerRepository> mockPlayerRepository;
    private Mock<ICompanyRepository> mockCompanyRepository;
    private Mock<IEventBus> mockEventBus;
    private AuctionPresenter auctionPresenter;

    [SetUp]
    public void SetUp()
    {
        mockAuctionWindow = new Mock<IAuctionWindow>();
        mockLocalPlayerService = new Mock<ILocalPlayerService>();
        mockPhotonAuctionManager = new Mock<IPhotonAuctionManager>();
        mockPlayerRepository = new Mock<IPlayerRepository>();
        mockCompanyRepository = new Mock<ICompanyRepository>();
        mockEventBus = new Mock<IEventBus>();

        auctionPresenter = new AuctionPresenter();
        auctionPresenter.Construct(
            mockAuctionWindow.Object,
            mockLocalPlayerService.Object,
            mockPhotonAuctionManager.Object,
            mockPlayerRepository.Object,
            mockCompanyRepository.Object,
            mockEventBus.Object
        );
    }

    [Test]
    public void Initialize_ShouldSubscribeToEventsAndSetActions()
    {
        // Act
        auctionPresenter.Initialize();

        // Assert
        mockEventBus.Verify(bus => bus.Subscribe<AuctionPromptBidEvent>(It.IsAny<Action<AuctionPromptBidEvent>>()), Times.Once);
        mockEventBus.Verify(bus => bus.Subscribe<AuctionEndEvent>(It.IsAny<Action<AuctionEndEvent>>()), Times.Once);
        mockAuctionWindow.Verify(window => window.SetPlayAction(It.IsAny<Action<int>>()), Times.Once);
        mockAuctionWindow.Verify(window => window.SetPassAction(It.IsAny<Action<int>>()), Times.Once);
    }

    [Test]
    public void Dispose_ShouldUnsubscribeFromEvents()
    {
        // Arrange
        auctionPresenter.Initialize();

        // Act
        auctionPresenter.Dispose();

        // Assert
        mockEventBus.Verify(bus => bus.Unsubscribe<AuctionPromptBidEvent>(It.IsAny<Action<AuctionPromptBidEvent>>()), Times.Once);
        mockEventBus.Verify(bus => bus.Unsubscribe<AuctionEndEvent>(It.IsAny<Action<AuctionEndEvent>>()), Times.Once);
    }

    [Test]
    public void OnPlayClicked_ShouldCallPlayerBidRequest()
    { // Arrange
        int playerId = 1;

        Action<int> playAction = null;
        mockAuctionWindow.Setup(w => w.SetPlayAction(It.IsAny<Action<int>>()))
                         .Callback<Action<int>>(a => playAction = a);

        auctionPresenter.Initialize();

        // Act
        playAction?.Invoke(playerId);

        // Assert
        mockPhotonAuctionManager.Verify(m => m.PlayerBidRequest(playerId), Times.Once);
    }

    [Test]
    public void OnPassClicked_ShouldCallPlayerPassRequest()
    {
        // Arrange
        int playerId = 1;

        Action<int> passAction = null;
        mockAuctionWindow.Setup(w => w.SetPassAction(It.IsAny<Action<int>>()))
                         .Callback<Action<int>>(a => passAction = a);

        auctionPresenter.Initialize();

        // Act
        passAction?.Invoke(playerId);

        // Assert
        mockPhotonAuctionManager.Verify(m => m.PlayerPassRequest(playerId), Times.Once);
    }

    [Test]
    public void OnAuctionPromptBid_ShouldShowAuctionWindow_WhenPlayerIdMatchesLocalId()
    {
        // Arrange
        int localPlayerId = 1;
        int companyId = 2;
        int bid = 100;

        var auctionEvent = new AuctionPromptBidEvent(localPlayerId, bid, companyId);
        var player = new PlayerData("player1", 500, localPlayerId, null);
        var company = new Company(companyId, new CompanyData());

        mockLocalPlayerService.Setup(s => s.GetLocalPlayerId()).Returns(localPlayerId);
        mockPlayerRepository.Setup(r => r.GetPlayerById(localPlayerId)).Returns(player);
        mockCompanyRepository.Setup(r => r.GetCompanyById(companyId)).Returns(company);

        Action<AuctionPromptBidEvent> capturedHandler = null;
        mockEventBus.Setup(bus => bus.Subscribe(It.IsAny<Action<AuctionPromptBidEvent>>()))
                    .Callback<Action<AuctionPromptBidEvent>>(h => capturedHandler = h);

        auctionPresenter.Initialize();

        // Act
        capturedHandler?.Invoke(auctionEvent);

        // Assert
        mockAuctionWindow.Verify(w => w.Show(localPlayerId, company.Name, bid, player.Money), Times.Once);
    }

    [Test]
    public void OnAuctionPromptBid_ShouldHideAuctionWindow_WhenPlayerIdDoesNotMatchLocalId()
    {
        // Arrange
        int localPlayerId = 1;
        int companyId = 2;
        int bid = 100;
        var auctionEvent = new AuctionPromptBidEvent(2, bid, companyId); // PlayerId = 2, не совпадает с localPlayerId = 1

        mockLocalPlayerService.Setup(service => service.GetLocalPlayerId()).Returns(localPlayerId);

        Action<AuctionPromptBidEvent> capturedHandler = null;
        mockEventBus.Setup(bus => bus.Subscribe(It.IsAny<Action<AuctionPromptBidEvent>>()))
                    .Callback<Action<AuctionPromptBidEvent>>(h => capturedHandler = h);

        // Act
        auctionPresenter.Initialize();
        capturedHandler?.Invoke(auctionEvent);

        // Assert
        mockAuctionWindow.Verify(window => window.Hide(), Times.Once);
    }
}
