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
        int playerId2 = 2;

        Action<int> playAction = null;
        mockAuctionWindow.Setup(w => w.SetPlayAction(It.IsAny<Action<int>>()))
                         .Callback<Action<int>>(a => playAction = a);

        auctionPresenter.Initialize();

        // Act
        playAction?.Invoke(playerId);

        // Assert
        mockPhotonAuctionManager.Verify(m => m.PlayerBidRequest(playerId), Times.Once);
        mockPhotonAuctionManager.Verify(m => m.PlayerBidRequest(playerId2), Times.Never);

    }

    [Test]
    public void OnPassClicked_ShouldCallPlayerPassRequest()
    {
        // Arrange
        int playerId = 1;
        int playerId2 = 2;

        Action<int> passAction = null;
        mockAuctionWindow.Setup(w => w.SetPassAction(It.IsAny<Action<int>>()))
                         .Callback<Action<int>>(a => passAction = a);

        auctionPresenter.Initialize();

        // Act
        passAction?.Invoke(playerId);

        // Assert
        mockPhotonAuctionManager.Verify(m => m.PlayerPassRequest(playerId), Times.Once);
        mockPhotonAuctionManager.Verify(m => m.PlayerPassRequest(playerId2), Times.Never);
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
    [Test]
    public void OnAuctionPromptBid_ShouldThrow_WhenPlayerIsNull()
    {
        // Arrange
        int localPlayerId = 1;
        int companyId = 2;
        int bid = 100;

        var auctionEvent = new AuctionPromptBidEvent(localPlayerId, bid, companyId);
     
        var company = new Company(companyId, new CompanyData());

        mockLocalPlayerService.Setup(s => s.GetLocalPlayerId()).Returns(localPlayerId);
        mockPlayerRepository.Setup(r => r.GetPlayerById(localPlayerId)).Returns((PlayerData)null);
        mockCompanyRepository.Setup(r => r.GetCompanyById(companyId)).Returns(company);

        Action<AuctionPromptBidEvent> capturedHandler = null;
        mockEventBus.Setup(bus => bus.Subscribe(It.IsAny<Action<AuctionPromptBidEvent>>()))
                    .Callback<Action<AuctionPromptBidEvent>>(h => capturedHandler = h);

        auctionPresenter.Initialize();

        // Act
        TestDelegate act = () => capturedHandler?.Invoke(auctionEvent);

        Assert.Throws<InvalidOperationException>(act);
    }
    [Test]
    public void OnAuctionPromptBid_ShouldThrow_WhenCompanyIsNull()
    {
        // Arrange
        int localPlayerId = 1;
        int companyId = 2;
        int bid = 100;

        var auctionEvent = new AuctionPromptBidEvent(localPlayerId, bid, companyId);
        var player = new PlayerData("player1", 500, localPlayerId, null);
      
        mockLocalPlayerService.Setup(s => s.GetLocalPlayerId()).Returns(localPlayerId);
        mockPlayerRepository.Setup(r => r.GetPlayerById(localPlayerId)).Returns(player);
        mockCompanyRepository.Setup(r => r.GetCompanyById(companyId)).Returns((Company)null);

        Action<AuctionPromptBidEvent> capturedHandler = null;
        mockEventBus.Setup(bus => bus.Subscribe(It.IsAny<Action<AuctionPromptBidEvent>>()))
                    .Callback<Action<AuctionPromptBidEvent>>(h => capturedHandler = h);

        auctionPresenter.Initialize();

        // Act

        TestDelegate act = () => capturedHandler?.Invoke(auctionEvent);

        Assert.Throws<InvalidOperationException>(act);
    }
   

    [Test]
    public void OnAuctionEndEvent_ShouldHideWindow()
    {
        Action<AuctionEndEvent> capturedHandler = null;
        mockEventBus.Setup(bus => bus.Subscribe(It.IsAny<Action<AuctionEndEvent>>()))
                    .Callback<Action<AuctionEndEvent>>(h => capturedHandler = h);

        auctionPresenter.Initialize();

        capturedHandler?.Invoke(new AuctionEndEvent());

        mockAuctionWindow.Verify(w => w.Hide(), Times.Once);
    }
    [TestCase(null, "auctionWindow")]
    [TestCase(typeof(ILocalPlayerService), "localPlayerService")]
    [TestCase(typeof(IPhotonAuctionManager), "photonAuctionManager")]
    [TestCase(typeof(IPlayerRepository), "playerRepository")]
    [TestCase(typeof(ICompanyRepository), "companyRepository")]
    [TestCase(typeof(IEventBus), "eventBus")]
    public void Construct_ShouldThrowArgumentNullException_WhenDependencyIsNull(Type? nullDependencyType, string expectedParamName)
    {
        // Arrange: создаем mock-и всех зависимостей
        var auctionWindow = nullDependencyType == null ? null : Mock.Of<IAuctionWindow>();
        var localPlayerService = nullDependencyType == typeof(ILocalPlayerService) ? null : Mock.Of<ILocalPlayerService>();
        var photonAuctionManager = nullDependencyType == typeof(IPhotonAuctionManager) ? null : Mock.Of<IPhotonAuctionManager>();
        var playerRepository = nullDependencyType == typeof(IPlayerRepository) ? null : Mock.Of<IPlayerRepository>();
        var companyRepository = nullDependencyType == typeof(ICompanyRepository) ? null : Mock.Of<ICompanyRepository>();
        var eventBus = nullDependencyType == typeof(IEventBus) ? null : Mock.Of<IEventBus>();

        var presenter = new AuctionPresenter();

        // Act + Assert
        var ex = Assert.Throws<ArgumentNullException>(() =>
            presenter.Construct(
                (IAuctionWindow)auctionWindow,
                (ILocalPlayerService)localPlayerService,
                (IPhotonAuctionManager)photonAuctionManager,
                (IPlayerRepository)playerRepository,
                (ICompanyRepository)companyRepository,
                (IEventBus)eventBus
            ));

        Assert.That(ex.ParamName, Is.EqualTo(expectedParamName));
    }

}
