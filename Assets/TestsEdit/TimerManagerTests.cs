using Moq;
using NUnit.Framework;
using System;

[TestFixture]
public class TimerManagerTests
{
    private TimerManager timerManager;
    private Mock<ITimeProvider> timeProviderMock;

    [SetUp]
    public void SetUp()
    {
        timeProviderMock = new Mock<ITimeProvider>();
        timerManager = new TimerManager(timeProviderMock.Object);
    }

    [Test]
    public void StartTurnTimer_Should_StartTimer()
    {
        int playerId = 1;
        double duration = 5.0;

        timeProviderMock.Setup(tp => tp.Now).Returns(0);
        timerManager.StartTurnTimer(playerId, duration);

        var tick = timerManager.Tick();

        Assert.IsNotNull(tick);
        Assert.AreEqual(TimerType.Turn, tick?.type);
        Assert.AreEqual(playerId, tick?.playerId);
        Assert.IsTrue(tick?.isActive);
        Assert.AreEqual(duration, tick?.timeLeft);
    }

    [Test]
    public void StartAuctionTimer_Should_StartTimer()
    {
        int playerId = 2;
        double duration = 10.0;

        timeProviderMock.Setup(tp => tp.Now).Returns(0);
        timerManager.StartAuctionTimer(playerId, duration);

        var tick = timerManager.Tick();

        Assert.IsNotNull(tick);
        Assert.AreEqual(TimerType.Auction, tick?.type);
        Assert.AreEqual(playerId, tick?.playerId);
        Assert.IsTrue(tick?.isActive);
        Assert.AreEqual(duration, tick?.timeLeft);
    }

    [Test]
    public void StartTradeTimer_Should_StartTimer()
    {
        int playerId = 3;
        double duration = 8.0;

        timeProviderMock.Setup(tp => tp.Now).Returns(0);
        timerManager.StartTradeTimer(playerId, duration);

        var tick = timerManager.Tick();

        Assert.IsNotNull(tick);
        Assert.AreEqual(TimerType.Trade, tick?.type);
        Assert.AreEqual(playerId, tick?.playerId);
        Assert.IsTrue(tick?.isActive);
        Assert.AreEqual(duration, tick?.timeLeft);
    }

    [Test]
    public void Tick_Should_DecreaseTimeLeft_WhenTimeAdvances()
    {
        int playerId = 1;
        double duration = 5.0;

        timeProviderMock.Setup(tp => tp.Now).Returns(0);
        timerManager.StartTurnTimer(playerId, duration);

        var tick1 = timerManager.Tick();

        timeProviderMock.Setup(tp => tp.Now).Returns(2); // прошло 2 секунды
        var tick2 = timerManager.Tick();

        Assert.IsNotNull(tick1);
        Assert.IsNotNull(tick2);
        Assert.Less(tick2.Value.timeLeft, tick1.Value.timeLeft);
        Assert.IsTrue(tick2.Value.isActive);
    }

    [Test]
    public void Tick_Should_SetExpired_WhenTimeRunsOut()
    {
        int playerId = 2;
        double duration = 1.0;

        timeProviderMock.Setup(tp => tp.Now).Returns(0);
        timerManager.StartTurnTimer(playerId, duration);

        timeProviderMock.Setup(tp => tp.Now).Returns(2); // прошло больше времени
        var tick = timerManager.Tick();

        Assert.IsNotNull(tick);
        Assert.IsTrue(tick.Value.expired);
        Assert.IsFalse(tick.Value.isActive);
        Assert.AreEqual(0f, tick.Value.timeLeft);
    }

    [Test]
    public void StartNewTimer_Should_ReplacePreviousTimer_OfSameType()
    {
        int playerId1 = 1;
        int playerId2 = 2;
        double duration = 5.0;

        timeProviderMock.Setup(tp => tp.Now).Returns(0);
        timerManager.StartTurnTimer(playerId1, duration);

        timerManager.StartTurnTimer(playerId2, duration);

        var tick = timerManager.Tick();

        Assert.IsNotNull(tick);
        Assert.AreEqual(playerId2, tick.Value.playerId); // предыдущий игрок заменён
        Assert.AreEqual(TimerType.Turn, tick.Value.type);
        Assert.AreEqual(duration, tick.Value.timeLeft);
    }

    [Test]
    public void Tick_Should_ReturnNull_IfNoTimerStarted()
    {
        var tick = timerManager.Tick();

        Assert.IsNull(tick);
    }
}
