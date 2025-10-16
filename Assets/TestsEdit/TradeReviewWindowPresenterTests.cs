using Moq;
using NUnit.Framework;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;

public class TradeReviewWindowPresenterTests
{
    private Mock<ITradeReviewWindow> windowMock;
    private Mock<IEventBus> eventBusMock;
    private Mock<ILocalPlayerService> localPlayerServiceMock;
    private Mock<IPhotonTradeManager> photonTradeManagerMock;
    private TradeReviewWindowPresenter presenter;

    private PlayerData localPlayer;
    private PlayerData otherPlayer;

    [SetUp]
    public void SetUp()
    {
        windowMock = new Mock<ITradeReviewWindow>();
        eventBusMock = new Mock<IEventBus>();
        localPlayerServiceMock = new Mock<ILocalPlayerService>();
        photonTradeManagerMock = new Mock<IPhotonTradeManager>();

        presenter = new TradeReviewWindowPresenter();
        presenter.Construct(windowMock.Object, eventBusMock.Object, localPlayerServiceMock.Object, photonTradeManagerMock.Object);

        localPlayer = new PlayerData("Local", 1000, 1, null);
        otherPlayer = new PlayerData("Other", 1000, 2, null);
    }

    [Test]
    public void Initialize_Should_SubscribeEvents_And_SetActions()
    {
        // Act
        presenter.Initialize();

        // Assert
        eventBusMock.Verify(e => e.Subscribe<TradeProposalReceivedEvent>(It.IsAny<Action<TradeProposalReceivedEvent>>()), Times.Once);
        eventBusMock.Verify(e => e.Subscribe<TradeEndedEvent>(It.IsAny<Action<TradeEndedEvent>>()), Times.Once);

        windowMock.Verify(w => w.SetAcceptAction(It.IsAny<Action>()), Times.Once);
        windowMock.Verify(w => w.SetCancelAction(It.IsAny<Action>()), Times.Once);
    }

    [Test]
    public void Dispose_Should_UnsubscribeEvents()
    {
        presenter.Initialize();
        eventBusMock.Invocations.Clear();

        presenter.Dispose();

        eventBusMock.Verify(e => e.Unsubscribe<TradeProposalReceivedEvent>(It.IsAny<Action<TradeProposalReceivedEvent>>()), Times.Once);
        eventBusMock.Verify(e => e.Unsubscribe<TradeEndedEvent>(It.IsAny<Action<TradeEndedEvent>>()), Times.Once);
    }

    [Test]
    public void ShowTradeReviewWindow_Should_ShowForLocalPlayer_WhenOfferToLocal()
    {
        presenter.Initialize();
        localPlayerServiceMock.Setup(s => s.GetLocalPlayerId()).Returns(localPlayer.Id);

        var offer = new TradeOffer(otherPlayer, localPlayer);
        var e = new TradeProposalReceivedEvent(offer, otherPlayer.Id, localPlayer.Id);

        var method = typeof(TradeReviewWindowPresenter)
            .GetMethod("ShowTradeReviewWindow", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        method.Invoke(presenter, new object[] { e });

        windowMock.Verify(w => w.Show(true, offer), Times.Once);
    }

    [Test]
    public void ShowTradeReviewWindow_Should_NotShow_WhenOfferFromLocal()
    {
        presenter.Initialize();
        localPlayerServiceMock.Setup(s => s.GetLocalPlayerId()).Returns(localPlayer.Id);

        var offer = new TradeOffer(localPlayer, otherPlayer);
        var e = new TradeProposalReceivedEvent(offer, otherPlayer.Id, localPlayer.Id);

        var method = typeof(TradeReviewWindowPresenter)
            .GetMethod("ShowTradeReviewWindow", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        method.Invoke(presenter, new object[] { e });

        windowMock.Verify(w => w.Show(It.IsAny<bool>(), It.IsAny<TradeOffer>()), Times.Never);
    }

    [Test]
    public void ShowTradeReviewWindow_Should_ShowForOther_WhenOfferToOther()
    {
        presenter.Initialize();
        localPlayerServiceMock.Setup(s => s.GetLocalPlayerId()).Returns(localPlayer.Id);

        var offer = new TradeOffer(otherPlayer, new PlayerData("Another", 1000, 3, null));
        var e = new TradeProposalReceivedEvent(offer, otherPlayer.Id, 3);

        var method = typeof(TradeReviewWindowPresenter)
            .GetMethod("ShowTradeReviewWindow", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        method.Invoke(presenter, new object[] { e });

        windowMock.Verify(w => w.Show(false, offer), Times.Once);
    }

    [Test]
    public void HideWindowsOnTradeEnded_Should_HideWindow()
    {
        presenter.Initialize();
        var e = new TradeEndedEvent(true, otherPlayer.Id, localPlayer.Id);

        var method = typeof(TradeReviewWindowPresenter)
            .GetMethod("HideWindowsOnTradeEnded", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        method.Invoke(presenter, new object[] { e });

        windowMock.Verify(w => w.Hide(), Times.Once);
    }

    [Test]
    public void AcceptTrade_Should_CallCompleteTradeWithTrue()
    {
        presenter.Initialize();
        var method = typeof(TradeReviewWindowPresenter)
            .GetMethod("AcceptTrade", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        method.Invoke(presenter, null);

        photonTradeManagerMock.Verify(p => p.CompleteTrade(true), Times.Once);
    }

    [Test]
    public void CancelTrade_Should_CallCompleteTradeWithFalse()
    {
        presenter.Initialize();
        var method = typeof(TradeReviewWindowPresenter)
            .GetMethod("CancelTrade", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        method.Invoke(presenter, null);

        photonTradeManagerMock.Verify(p => p.CompleteTrade(false), Times.Once);
    }
    [Test]
    public void Construct_Should_ThrowArgumentNullException_WhenParametersNull()
    {
        Assert.Throws<ArgumentNullException>(() => new TradeReviewWindowPresenter().Construct(null, eventBusMock.Object, localPlayerServiceMock.Object, photonTradeManagerMock.Object));
        Assert.Throws<ArgumentNullException>(() => new TradeReviewWindowPresenter().Construct(windowMock.Object, null, localPlayerServiceMock.Object, photonTradeManagerMock.Object));
        Assert.Throws<ArgumentNullException>(() => new TradeReviewWindowPresenter().Construct(windowMock.Object, eventBusMock.Object, null, photonTradeManagerMock.Object));
        Assert.Throws<ArgumentNullException>(() => new TradeReviewWindowPresenter().Construct(windowMock.Object, eventBusMock.Object, localPlayerServiceMock.Object, null));
    }

    [Test]
    public void ShowTradeReviewWindow_Should_ShowForOther_WhenLocalNotInvolved()
    {
        presenter.Initialize();
        localPlayerServiceMock.Setup(s => s.GetLocalPlayerId()).Returns(localPlayer.Id);

        var offer = new TradeOffer(new PlayerData("X", 1000, 10, null), new PlayerData("Y", 1000, 11, null));
        var e = new TradeProposalReceivedEvent(offer, 10, 11);

        var method = typeof(TradeReviewWindowPresenter)
            .GetMethod("ShowTradeReviewWindow", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        method.Invoke(presenter, new object[] { e });

        windowMock.Verify(w => w.Show(false, offer), Times.Once);
    }

}
