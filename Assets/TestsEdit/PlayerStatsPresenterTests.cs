using NUnit.Framework;
using Moq;
using System;

public class PlayerStatsPresenterTests
{
    private Mock<IPlayerStatsView> viewMock;
    private Mock<IEventBus> eventBusMock;
    private Mock<ILocalPlayerService> localPlayerServiceMock;
    private Mock<IPhotonTradeManager> photonTradeManagerMock;
    private Mock<IPhotonLoanManager> photonLoanManagerMock;
    private PlayerStatsPresenter presenter;
    private PlayerData playerData;

    [SetUp]
    public void SetUp()
    {
        viewMock = new Mock<IPlayerStatsView>();
        eventBusMock = new Mock<IEventBus>();
        localPlayerServiceMock = new Mock<ILocalPlayerService>();
        photonTradeManagerMock = new Mock<IPhotonTradeManager>();
        photonLoanManagerMock = new Mock<IPhotonLoanManager>();

        presenter = new PlayerStatsPresenter(
            viewMock.Object,
            photonLoanManagerMock.Object,
            eventBusMock.Object,
            localPlayerServiceMock.Object,
            photonTradeManagerMock.Object
        );

        // —оздаем игрока
        var photonPlayer = new Photon.Realtime.Player("TestPlayer", 1);
        playerData = new PlayerData("TestPlayer", 500, 1, null, photonPlayer);

        // ¬ажно: VisibleCapital и LiquidAssets не задаем напр€мую, чтобы тесты были независимы
    }

    [Test]
    public void Init_Should_SetupViewProperly()
    {
        // Arrange
        localPlayerServiceMock.Setup(s => s.GetLocalPlayerId()).Returns(1);
        playerData.photonPlayer = new Photon.Realtime.Player("TestPlayer", 1); // локальный игрок

        // Act
        presenter.Init(playerData);

        // Assert
        viewMock.Verify(v => v.SetName("TestPlayer"), Times.Once);
        viewMock.Verify(v => v.SetMoney(500), Times.Once);
        viewMock.Verify(v => v.SetCapital(500, 500), Times.Once); // Default значени€ из конструктора
        viewMock.Verify(v => v.SetLeaveButtonVisible(true), Times.Once); // ƒл€ локального игрока
        viewMock.Verify(v => v.SetLoanButtonsVisible(false, false), Times.Once);
        viewMock.Verify(v => v.SetTradeButtonVisible(false), Times.Once);
        viewMock.Verify(v => v.SetAuctionHighlightVisible(false), Times.Once);
        viewMock.Verify(v => v.SetTurnHighlightVisible(false), Times.Once);
        viewMock.Verify(v => v.SetLoanContainerVisible(false), Times.Once);
        viewMock.Verify(v => v.SetTimerGOVisible(false), Times.Once);
    }

    [Test]
    public void Dispose_Should_UnsubscribeEvents()
    {
        presenter.Init(playerData);
        eventBusMock.Invocations.Clear();

        presenter.Dispose();

        eventBusMock.Verify(e => e.Unsubscribe<OnUpdatePlayerMoneyEvent>(It.IsAny<Action<OnUpdatePlayerMoneyEvent>>()), Times.Once);
        eventBusMock.Verify(e => e.Unsubscribe<TurnStartEvent>(It.IsAny<Action<TurnStartEvent>>()), Times.Once);
    }

    [Test]
    public void OnMoneyUpdate_Should_UpdateMoney_When_PlayerMatches()
    {
        presenter.Init(playerData);
        var updatedPlayer = new PlayerData("p1", 999, 1, null);
        var e = new OnUpdatePlayerMoneyEvent(updatedPlayer);

        var method = typeof(PlayerStatsPresenter).GetMethod("OnMoneyUpdate",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        method.Invoke(presenter, new object[] { e });

        viewMock.Verify(v => v.SetMoney(999), Times.Once);
    }

    [Test]
    public void OnMoneyUpdate_ShouldNot_UpdateMoney_When_OtherPlayer()
    {
        presenter.Init(playerData);
        var e = new OnUpdatePlayerMoneyEvent(new PlayerData("p2", 777, 2, null));

        var method = typeof(PlayerStatsPresenter).GetMethod("OnMoneyUpdate",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        method.Invoke(presenter, new object[] { e });

        viewMock.Verify(v => v.SetMoney(It.IsAny<int>()), Times.Never);
    }

    [Test]
    public void OnTurnStarted_Should_ShowLoanButtons_ForLocalPlayer_WithoutLoan()
    {
        presenter.Init(playerData);
        localPlayerServiceMock.Setup(s => s.GetLocalPlayerId()).Returns(1);
        playerData.HasLoan = false;

        var e = new TurnStartEvent(1);
        var method = typeof(PlayerStatsPresenter).GetMethod("OnTurnStarted",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        method.Invoke(presenter, new object[] { e });

        viewMock.Verify(v => v.SetLoanButtonsVisible(true, false), Times.Once);
        viewMock.Verify(v => v.SetTradeButtonVisible(false), Times.Once);
    }

    [Test]
    public void OnTurnStarted_Should_ShowPayLoanButton_ForLocalPlayer_WithLoan()
    {
        presenter.Init(playerData);
        localPlayerServiceMock.Setup(s => s.GetLocalPlayerId()).Returns(1);
        playerData.HasLoan = true;

        var e = new TurnStartEvent(1);
        var method = typeof(PlayerStatsPresenter).GetMethod("OnTurnStarted",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        method.Invoke(presenter, new object[] { e });

        viewMock.Verify(v => v.SetLoanButtonsVisible(false, true), Times.Once);
    }

    [Test]
    public void OnTradeClick_Should_SendTradeRequest()
    {
        presenter.Init(playerData);
        localPlayerServiceMock.Setup(s => s.GetLocalPlayerId()).Returns(99);

        var method = typeof(PlayerStatsPresenter).GetMethod("OnTradeClick",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        method.Invoke(presenter, null);

        photonTradeManagerMock.Verify(p => p.SendTradeRequest(99, playerData.Id), Times.Once);
    }

    [Test]
    public void OnLoanUpdated_Should_UpdateLoanUI_ForSamePlayer()
    {
        presenter.Init(playerData);

        playerData.HasLoan = true;
        playerData.LoanTurnsLeft = 3;
        playerData.photonPlayer = new Photon.Realtime.Player("TestPlayer", 1);
        var e = new OnTakeLoanEvent(playerData);

        var method = typeof(PlayerStatsPresenter).GetMethod("OnLoanUpdated",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        method.Invoke(presenter, new object[] { e });

        viewMock.Verify(v => v.SetLoan(true, 3, true), Times.Once);
    }

    [Test]
    public void SubscribeEvents_ShouldOnly_SubscribeOnce()
    {
        presenter.Init(playerData);
        eventBusMock.Invocations.Clear();

        var method = typeof(PlayerStatsPresenter).GetMethod("SubscribeEvents",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        method.Invoke(presenter, null);

        // ѕровер€ем, что повторна€ подписка не была выполнена
        eventBusMock.Verify(e => e.Subscribe<OnUpdatePlayerMoneyEvent>(It.IsAny<Action<OnUpdatePlayerMoneyEvent>>()), Times.Never);
    }
}
