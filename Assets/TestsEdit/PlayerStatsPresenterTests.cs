using NUnit.Framework;
using Moq;
using System;
using Photon.Realtime;
public class PlayerStatsPresenterTests
{
    private Mock<IPlayerStatsView> mockView;
    private Mock<IEventBus> mockEventBus;
    private Mock<ILocalPlayerService> mockLocalPlayerService;
    private Mock<IPhotonTradeManager> mockTradeManager;
    private Mock<IPhotonLoanManager> mockLoanManager;
    private PlayerStatsPresenter presenter;
    private PlayerData playerData;

    [SetUp]
    public void SetUp()
    {
        mockView = new Mock<IPlayerStatsView>();
        mockEventBus = new Mock<IEventBus>();
        mockLocalPlayerService = new Mock<ILocalPlayerService>();
        mockTradeManager = new Mock<IPhotonTradeManager>();
        mockLoanManager = new Mock<IPhotonLoanManager>();
        var phPlayer = new Photon.Realtime.Player("1", 1);
        
        playerData = new PlayerData("TestPlayer", 1000, 1,null, phPlayer)
        {
            VisibleCapital = 2000,
            LiquidAssets = 1500,
            HasLoan = false,
        };

        presenter = new PlayerStatsPresenter(mockView.Object, mockLoanManager.Object, mockEventBus.Object, mockLocalPlayerService.Object, mockTradeManager.Object);
    }

    #region CONSTRUCTOR

    [Test]
    public void Constructor_NullDependencies_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new PlayerStatsPresenter(null, mockLoanManager.Object, mockEventBus.Object, mockLocalPlayerService.Object, mockTradeManager.Object));
        Assert.Throws<ArgumentNullException>(() => new PlayerStatsPresenter(mockView.Object, mockLoanManager.Object, null, mockLocalPlayerService.Object, mockTradeManager.Object));
        Assert.Throws<ArgumentNullException>(() => new PlayerStatsPresenter(mockView.Object, mockLoanManager.Object, mockEventBus.Object, null, mockTradeManager.Object));
        Assert.Throws<ArgumentNullException>(() => new PlayerStatsPresenter(mockView.Object, mockLoanManager.Object, mockEventBus.Object, mockLocalPlayerService.Object, null));
    }

    [Test]
    public void Constructor_BindsButtons()
    {
        mockView.Verify(v => v.BindTradeAction(It.IsAny<Action>()), Times.Once);
        mockView.Verify(v => v.BindTakeLoanAction(It.IsAny<Action>()), Times.Once);
        mockView.Verify(v => v.BindPayLoanAction(It.IsAny<Action>()), Times.Once);
    }

    #endregion

    #region INIT

    [Test]
    public void Init_NullData_DoesNothing()
    {
        presenter.Init(null);

        // Проверяем, что методы установки данных на view не вызываются
        mockView.Verify(v => v.SetName(It.IsAny<string>()), Times.Never);
        mockView.Verify(v => v.SetMoney(It.IsAny<int>()), Times.Never);
        mockView.Verify(v => v.SetCapital(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
        mockView.Verify(v => v.SetLeaveButtonVisible(It.IsAny<bool>()), Times.Never);
        mockView.Verify(v => v.SetLoanButtonsVisible(It.IsAny<bool>(), It.IsAny<bool>()), Times.Never);
        mockView.Verify(v => v.SetTradeButtonVisible(It.IsAny<bool>()), Times.Never);
    }

    [Test]
    public void Init_SetsAllViewProperties_AndSubscribesEvents()
    {
        presenter.Init(playerData);

        mockView.Verify(v => v.SetName(playerData.Name));
        mockView.Verify(v => v.SetMoney(playerData.Money));
        mockView.Verify(v => v.SetCapital(playerData.VisibleCapital, playerData.LiquidAssets));
        mockView.Verify(v => v.SetLeaveButtonVisible(playerData.photonPlayer.IsLocal));
        mockView.Verify(v => v.SetLoanButtonsVisible(false, false));
        mockView.Verify(v => v.SetTradeButtonVisible(false));
        mockView.Verify(v => v.SetAuctionHighlightVisible(false));
        mockView.Verify(v => v.SetTurnHighlightVisible(false));
        mockView.Verify(v => v.SetLoanContainerVisible(false));
        mockView.Verify(v => v.SetTimerGOVisible(false));

        mockEventBus.Verify(e => e.Subscribe<OnUpdatePlayerMoneyEvent>(It.IsAny<Action<OnUpdatePlayerMoneyEvent>>()), Times.Once);
        mockEventBus.Verify(e => e.Subscribe<TurnStartEvent>(It.IsAny<Action<TurnStartEvent>>()), Times.Once);
    }

    #endregion

    #region CALLBACKS

    [Test]
    public void OnMoneyUpdate_UpdatesMoney_WhenPlayerIdMatches()
    {
        presenter.Init(playerData);
        var player = new PlayerData("TestPlayer", 500, 1, null);
        var evt = new OnUpdatePlayerMoneyEvent(player);

        // вызываем через подписку вручную
        presenter.GetType().GetMethod("OnMoneyUpdate", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .Invoke(presenter, new object[] { evt });

        mockView.Verify(v => v.SetMoney(500), Times.Once);
    }

    [Test]
    public void OnTimerUpdated_UpdatesTurnTimerCorrectly()
    {
        presenter.Init(playerData);
        var evt = new TimerUpdatedEvent(TimerType.Turn, playerData.Id, 10, true);

        presenter.GetType().GetMethod("OnTurnTimerUpdated", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .Invoke(presenter, new object[] { evt });

        mockView.Verify(v => v.SetTimer(true, 10, true, false), Times.Once);
    }

    [Test]
    public void OnAuctionTimerUpdated_UpdatesAuctionTimerCorrectly()
    {
        presenter.Init(playerData);
        var evt = new TimerUpdatedEvent(TimerType.Auction, playerData.Id, 20, true);


        presenter.GetType().GetMethod("OnAuctionTimerUpdated", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .Invoke(presenter, new object[] { evt });

        mockView.Verify(v => v.SetTimer(true, 20, false, true), Times.Once);
    }

    [Test]
    public void OnTradeClick_CallsSendTradeRequest()
    {
        presenter.Init(playerData);

        presenter.GetType().GetMethod("OnTradeClick", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .Invoke(presenter, null);

        mockTradeManager.Verify(m => m.SendTradeRequest(It.IsAny<int>(), playerData.Id), Times.Once);
    }

    [Test]
    public void Dispose_UnsubscribesEvents()
    {
        presenter.Init(playerData);
        presenter.Dispose();

        mockEventBus.Verify(e => e.Unsubscribe<OnUpdatePlayerMoneyEvent>(It.IsAny<Action<OnUpdatePlayerMoneyEvent>>()), Times.Once);
        mockEventBus.Verify(e => e.Unsubscribe<TurnStartEvent>(It.IsAny<Action<TurnStartEvent>>()), Times.Once);
    }

    #endregion
}
