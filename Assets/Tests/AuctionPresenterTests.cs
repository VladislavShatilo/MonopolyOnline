using NUnit.Framework;
using Moq;
using System;
public class AuctionPresenterTests
{
    private AuctionPresenter presenter;
    private Mock<IAuctionWindow> auctionWindowMock;
    private Mock<ILocalPlayerService> localPlayerServiceMock;
    private Mock<IPhotonAuctionManager> photonAuctionManagerMock;
    private Mock<IPlayerRepository> playerRepositoryMock;
    private Mock<ICompanyRepository> companyRepositoryMock;
    private Mock<IEventBus> eventBusMock;

    [SetUp]
    public void SetUp()
    {
        auctionWindowMock = new Mock<IAuctionWindow>();
        localPlayerServiceMock = new Mock<ILocalPlayerService>();
        photonAuctionManagerMock = new Mock<IPhotonAuctionManager>();
        playerRepositoryMock = new Mock<IPlayerRepository>();
        companyRepositoryMock = new Mock<ICompanyRepository>();
        eventBusMock = new Mock<IEventBus>();

        presenter = new AuctionPresenter();
        presenter.Construct(
            auctionWindowMock.Object,
            localPlayerServiceMock.Object,
            photonAuctionManagerMock.Object,
            playerRepositoryMock.Object,
            companyRepositoryMock.Object,
            eventBusMock.Object
        );
    }

    [Test]
    public void Initialize_SubscribesToEventsAndSetsActions()
    {
        presenter.Initialize();

        eventBusMock.Verify(e => e.Subscribe<AuctionPromptBidEvent>(It.IsAny<Action<AuctionPromptBidEvent>>()), Times.Once);
        eventBusMock.Verify(e => e.Subscribe<AuctionEndEvent>(It.IsAny<Action<AuctionEndEvent>>()), Times.Once);
        auctionWindowMock.Verify(w => w.SetPlayAction(It.IsAny<Action<int>>()), Times.Once);
        auctionWindowMock.Verify(w => w.SetPassAction(It.IsAny<Action<int>>()), Times.Once);
    }

    [Test]
    public void OnPlayClicked_CallsPlayerBidRequest()
    {
        presenter.Initialize();
        var playAction = auctionWindowMock.Invocations[0].Arguments[0] as Action<int>;

        playAction.Invoke(5);

        photonAuctionManagerMock.Verify(p => p.PlayerBidRequest(5), Times.Once);
    }

    [Test]
    public void OnPassClicked_CallsPlayerPassRequest()
    {
        presenter.Initialize();
        var passAction = auctionWindowMock.Invocations[1].Arguments[0] as Action<int>;

        passAction.Invoke(10);

        photonAuctionManagerMock.Verify(p => p.PlayerPassRequest(10), Times.Once);
    }

    [Test]
    public void OnAuctionPromptBid_ShowsWindowForLocalPlayer()
    {
        presenter.Initialize();
        var localPlayer = new PlayerData ("Player1", 5000, 0,null,0);
        var company = new Company ( 0,new CompanyData() );

        localPlayerServiceMock.Setup(l => l.GetLocalPlayerId()).Returns(1);
        playerRepositoryMock.Setup(p => p.GetPlayerById(1)).Returns(localPlayer);
        companyRepositoryMock.Setup(c => c.GetCompanyById(2)).Returns(company);

        //var auctionEvent = new AuctionPromptBidEvent { PlayerId = 1, CompanyId = 2, Bid = 500 };
        //var callback = CaptureAuctionPromptBidCallback();
        //callback.Invoke(auctionEvent);

        auctionWindowMock.Verify(w => w.Show(1, "TestCo", 500, 1000), Times.Once);
    }

    [Test]
    public void OnAuctionPromptBid_HidesWindowForOtherPlayer()
    {
        presenter.Initialize();
        localPlayerServiceMock.Setup(l => l.GetLocalPlayerId()).Returns(1);

        //var auctionEvent = new AuctionPromptBidEvent { PlayerId = 2, CompanyId = 2, Bid = 500 };
        //var callback = CaptureAuctionPromptBidCallback();
        //callback.Invoke(auctionEvent);

        auctionWindowMock.Verify(w => w.Hide(), Times.Once);
    }

    private Action<AuctionPromptBidEvent> CaptureAuctionPromptBidCallback()
    {
        Action<AuctionPromptBidEvent> callback = null;
        eventBusMock.Setup(e => e.Subscribe<AuctionPromptBidEvent>(It.IsAny<Action<AuctionPromptBidEvent>>()))
            .Callback<Action<AuctionPromptBidEvent>>(c => callback = c);
        presenter.Initialize();
        return callback;
    }
}
