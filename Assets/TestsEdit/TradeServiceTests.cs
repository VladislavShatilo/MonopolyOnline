using Moq;
using NUnit.Framework;

[TestFixture]
public class TradeServiceTests
{
    private Mock<IPhotonTradeManager> photonTradeManagerMock;
    private Mock<IBankService> bankServiceMock;
    private Mock<IEventBus> eventBusMock;
    private Mock<IPlayerRepository> playerRepositoryMock;
    private Mock<ITimerManager> timerManagerMock;
    private Mock<ITurnPresenter> turnPresenterMock;
    private Mock<IPhotonNetworkWrapper> photonNetworkWrapperMock;

    private TradeService tradeService;
    private GameSettings gameSettings;
    private PlayerData playerA;
    private PlayerData playerB;

    [SetUp]
    public void SetUp()
    {
        photonTradeManagerMock = new Mock<IPhotonTradeManager>();
        bankServiceMock = new Mock<IBankService>();
        eventBusMock = new Mock<IEventBus>();
        playerRepositoryMock = new Mock<IPlayerRepository>();
        timerManagerMock = new Mock<ITimerManager>();
        turnPresenterMock = new Mock<ITurnPresenter>();
        photonNetworkWrapperMock = new Mock<IPhotonNetworkWrapper>();

        gameSettings = new GameSettings { turnTime = 30, tradeTime = 15 };

        tradeService = new TradeService();
        tradeService.Construct(
            photonTradeManagerMock.Object,
            bankServiceMock.Object,
            eventBusMock.Object,
            playerRepositoryMock.Object,
            timerManagerMock.Object,
            gameSettings,
            turnPresenterMock.Object,
            photonNetworkWrapperMock.Object
        );

        playerA = new PlayerData("A", 1000, 1, null);
        playerB = new PlayerData("B", 1000, 2, null);

        playerRepositoryMock.Setup(p => p.GetPlayerById(1)).Returns(playerA);
        playerRepositoryMock.Setup(p => p.GetPlayerById(2)).Returns(playerB);
    }

    [Test]
    public void StartTrade_ShouldInitializeTradeAndPublishEvent()
    {
        tradeService.StartTrade(1, 2);

        Assert.IsTrue(tradeService.IsTradeActive);
        Assert.AreEqual(playerA, tradeService.CurrentOffer.FromPlayerData);
        Assert.AreEqual(playerB, tradeService.CurrentOffer.ToPlayerData);

        eventBusMock.Verify(e => e.Publish(It.IsAny<TradeStartedEvent>()), Times.Once);
    }

    [Test]
    public void CancelTrade_ShouldResetState()
    {
        tradeService.StartTrade(1, 2);
        tradeService.CancelTrade();

        Assert.IsFalse(tradeService.IsTradeActive);
        Assert.IsNull(tradeService.CurrentOffer);
    }

    [Test]
    public void AddCompanyToOffer_ShouldAddCompanyToCorrectPlayer()
    {
        tradeService.StartTrade(1, 2);
        var company = new Company(101, new CompanyData()) { Price = 500 };

        tradeService.AddCompanyToOffer(1, company);

        Assert.Contains(company, tradeService.CurrentOffer.FromCompanies);
        eventBusMock.Verify(e => e.Publish(It.IsAny<TradeUpdatedEvent>()), Times.Once);
    }

    [Test]
    public void RemoveCompanyFromOffer_ShouldRemoveCompanyFromCorrectPlayer()
    {
        tradeService.StartTrade(1, 2);
        var company = new Company(101, new CompanyData()) { Price = 500 };
        tradeService.AddCompanyToOffer(1, company);

        tradeService.RemoveCompanyFromOffer(1, company);

        Assert.IsEmpty(tradeService.CurrentOffer.FromCompanies);
        eventBusMock.Verify(e => e.Publish(It.IsAny<TradeUpdatedEvent>()), Times.Exactly(2));
    }

    [Test]
    public void OnTradeProposalReceived_ShouldSetCurrentOfferAndStartTimer()
    {
        var offer = new TradeOffer(playerA, playerB);

        timerManagerMock.Setup(t => t.Tick())
            .Returns((TimerType.Trade, 1, 10f, true, false) as (TimerType, int, float, bool, bool)?);

        tradeService.OnTradeProposalReceived(offer);

        Assert.IsTrue(tradeService.IsTradeActive);
        Assert.AreEqual(offer, tradeService.CurrentOffer);

        timerManagerMock.Verify(t => t.StartTradeTimer(playerB.Id, gameSettings.tradeTime), Times.Once);
        eventBusMock.Verify(e => e.Publish(It.IsAny<TradeProposalReceivedEvent>()), Times.Once);
    }

    [Test]
    public void OnTradeCompleted_ShouldApplyTrade_WhenAccepted()
    {
        var offer = new TradeOffer(playerA, playerB)
        {
            FromMoney = 100,
            ToMoney = 200
        };

        var companyA = new Company(1, new CompanyData()) { Price = 300 };
        offer.FromCompanies.Add(companyA);

        var companyB = new Company(2, new CompanyData()) { Price = 400 };
        offer.ToCompanies.Add(companyB);

        tradeService.OnTradeProposalReceived(offer);

        photonNetworkWrapperMock.Setup(p => p.IsMasterClient).Returns(true); // גלוסעמ PhotonNetwork

        tradeService.OnTradeCompleted(true);

        bankServiceMock.Verify(b => b.RemoveMoney(playerA.Id, 100), Times.Once);
        bankServiceMock.Verify(b => b.AddMoney(playerB.Id, 100), Times.Once);
        bankServiceMock.Verify(b => b.RemoveMoney(playerB.Id, 200), Times.Once);
        bankServiceMock.Verify(b => b.AddMoney(playerA.Id, 200), Times.Once);

        eventBusMock.Verify(e => e.Publish(It.IsAny<CompanyBoughtEvent>()), Times.Exactly(2));
        eventBusMock.Verify(e => e.Publish(It.IsAny<HideButtonsTradeEvent>()), Times.Exactly(2));
        eventBusMock.Verify(e => e.Publish(It.IsAny<TradeEndedEvent>()), Times.Once);

        Assert.IsFalse(tradeService.IsTradeActive);
        Assert.IsNull(tradeService.CurrentOffer);
    }

    [Test]
    public void TimerExpiredEvent_ShouldCallSendTradeResult_WhenTradeTimer()
    {
        tradeService.StartTrade(1, 2);
        var e = new TimerExpiredEvent (TimerType.Trade,2);

        tradeService.TimerExpiredEvent(e);

        photonTradeManagerMock.Verify(p => p.SendTradeResult(false), Times.Once);
    }
}
