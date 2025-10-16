using Moq;
using NUnit.Framework;
using System;

[TestFixture]
public class TradeWindowPresenterTests
{
    private Mock<ITradeWindow> tradeWindowMock;
    private Mock<ITurnWindow> turnWindowMock;
    private Mock<IEventBus> eventBusMock;
    private Mock<ILocalPlayerService> localPlayerServiceMock;
    private Mock<IPhotonTradeManager> photonTradeManagerMock;

    private TradeWindowPresenter presenter;
    private PlayerData playerA;
    private PlayerData playerB;
    private TradeOffer tradeOffer;

    [SetUp]
    public void SetUp()
    {
        tradeWindowMock = new Mock<ITradeWindow>();
        turnWindowMock = new Mock<ITurnWindow>();
        eventBusMock = new Mock<IEventBus>();
        localPlayerServiceMock = new Mock<ILocalPlayerService>();
        photonTradeManagerMock = new Mock<IPhotonTradeManager>();

        presenter = new TradeWindowPresenter();
        presenter.Construct(
            tradeWindowMock.Object,
            localPlayerServiceMock.Object,
            eventBusMock.Object,
            photonTradeManagerMock.Object,
            turnWindowMock.Object
        );

        playerA = new PlayerData("A", 1000, 1, null);
        playerB = new PlayerData("B", 1000, 2, null);
        tradeOffer = new TradeOffer(playerA, playerB);

        localPlayerServiceMock.Setup(x => x.GetLocalPlayerId()).Returns(1);
    }

    [Test]
    public void Initialize_ShouldSubscribeToEventsAndSetActions()
    {
        presenter.Initialize();

        eventBusMock.Verify(e => e.Subscribe<TradeStartedEvent>(It.IsAny<Action<TradeStartedEvent>>()), Times.Once);
        eventBusMock.Verify(e => e.Subscribe<TradeUpdatedEvent>(It.IsAny<Action<TradeUpdatedEvent>>()), Times.Once);

        tradeWindowMock.Verify(t => t.SetCloseAction(It.IsAny<Action>()), Times.Once);
        tradeWindowMock.Verify(t => t.SetOfferAction(It.IsAny<Action>()), Times.Once);
    }

    [Test]
    public void Dispose_ShouldUnsubscribeFromEvents()
    {
        presenter.Dispose();

        eventBusMock.Verify(e => e.Unsubscribe<TradeStartedEvent>(It.IsAny<Action<TradeStartedEvent>>()), Times.Once);
        eventBusMock.Verify(e => e.Unsubscribe<TradeUpdatedEvent>(It.IsAny<Action<TradeUpdatedEvent>>()), Times.Once);
    }

    [Test]
    public void ShowTradeWindow_ShouldShowWindow_WhenLocalPlayerIsSender()
    {
        var tradeEvent = new TradeStartedEvent(playerA.Id, playerB.Id, tradeOffer);

        presenter.Initialize();
        // Вызов приватного метода через событие
        presenter.GetType().GetMethod("ShowTradeWindow", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .Invoke(presenter, new object[] { tradeEvent });

        tradeWindowMock.Verify(t => t.Show(tradeOffer), Times.Once);
        Assert.AreEqual(tradeOffer, presenter.GetType().GetField("currentOffer", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(presenter));
    }

    [Test]
    public void ShowTradeWindow_ShouldHideWindow_WhenLocalPlayerIsNotSender()
    {
        localPlayerServiceMock.Setup(x => x.GetLocalPlayerId()).Returns(99);
        var tradeEvent = new TradeStartedEvent(playerA.Id, playerB.Id, tradeOffer);

        presenter.Initialize();
        presenter.GetType().GetMethod("ShowTradeWindow", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .Invoke(presenter, new object[] { tradeEvent });

        tradeWindowMock.Verify(t => t.Hide(), Times.Once);
    }

    [Test]
    public void UpdateTrade_ShouldCallUpdateOnTradeWindow()
    {
        var updateEvent = new TradeUpdatedEvent(tradeOffer);

        presenter.Initialize();
        presenter.GetType().GetMethod("UpdateTrade", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .Invoke(presenter, new object[] { updateEvent });

        tradeWindowMock.Verify(t => t.UpdateTrade(tradeOffer), Times.Once);
    }

    [Test]
    public void CloseTrade_ShouldHideTradeWindow()
    {
        presenter.Initialize();
        presenter.GetType().GetMethod("CloseTrade", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .Invoke(presenter, null);

        tradeWindowMock.Verify(t => t.Hide(), Times.Once);
    }

    [Test]
    public void OfferTrade_ShouldSendTradeOfferAndHideWindows()
    {
        presenter.Initialize();
        // Установим currentOffer вручную
        presenter.GetType().GetField("currentOffer", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .SetValue(presenter, tradeOffer);

        presenter.GetType().GetMethod("OfferTrade", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .Invoke(presenter, null);

        photonTradeManagerMock.Verify(p => p.SendTradeOffer(tradeOffer), Times.Once);
        tradeWindowMock.Verify(t => t.Hide(), Times.Once);
        turnWindowMock.Verify(t => t.Hide(), Times.Once);
    }
    [Test]
    public void OfferTrade_ShouldNotThrow_WhenCurrentOfferIsNull()
    {
        presenter.GetType().GetField("currentOffer", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .SetValue(presenter, null);

        presenter.GetType().GetMethod("OfferTrade", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .Invoke(presenter, null);

        tradeWindowMock.Verify(t => t.Hide(), Times.Once);
        turnWindowMock.Verify(t => t.Hide(), Times.Once);
        photonTradeManagerMock.Verify(p => p.SendTradeOffer(It.IsAny<TradeOffer>()), Times.Once);
    }

   

    [Test]
    public void UpdateTrade_ShouldNotThrow_WhenOfferIsNull()
    {
        var updateEvent = new TradeUpdatedEvent(null);

        Assert.DoesNotThrow(() =>
        {
            presenter.GetType().GetMethod("UpdateTrade", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .Invoke(presenter, new object[] { updateEvent });
        });

        tradeWindowMock.Verify(t => t.UpdateTrade(null), Times.Once);
    }

    [Test]
    public void ShowTradeWindow_ShouldUpdateCurrentOffer_OnMultipleCalls()
    {
        var offer1 = new TradeOffer(new PlayerData("A", 1000, 1, null), new PlayerData("B", 1000, 2, null));
        var offer2 = new TradeOffer(new PlayerData("A", 1000, 1, null), new PlayerData("C", 1000, 3, null));

        var tradeEvent1 = new TradeStartedEvent(1, 2, offer1);
        var tradeEvent2 = new TradeStartedEvent(1, 3, offer2);

        presenter.GetType().GetMethod("ShowTradeWindow", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .Invoke(presenter, new object[] { tradeEvent1 });

        presenter.GetType().GetMethod("ShowTradeWindow", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .Invoke(presenter, new object[] { tradeEvent2 });

        // Проверяем, что currentOffer обновился
        var currentOffer = (TradeOffer)presenter.GetType().GetField("currentOffer", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(presenter);
        Assert.AreEqual(offer2, currentOffer);

        // Метод Show должен был вызван дважды
        tradeWindowMock.Verify(t => t.Show(It.IsAny<TradeOffer>()), Times.Exactly(2));
    }
}
