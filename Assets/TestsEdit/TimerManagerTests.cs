using NUnit.Framework;
using Moq;
using Photon.Pun;
using System;

[TestFixture]
public class TimerManagerTests
{
    private TimerManager timerManager;

    [SetUp]
    public void SetUp()
    {
        timerManager = new TimerManager();
    }

    [Test]
    public void StartTurnTimer_Should_StartTimer()
    {
        // Arrange
        int playerId = 1;
        double duration = 5.0;

        // Act
        timerManager.StartTurnTimer(playerId, duration);
        var tick = timerManager.Tick();

        // Assert
        Assert.IsNotNull(tick);
        Assert.AreEqual(TimerType.Turn, tick?.type);
        Assert.AreEqual(playerId, tick?.playerId);
        Assert.IsTrue(tick?.isActive);
    }

    [Test]
    public void StartAuctionTimer_Should_StartTimer()
    {
        int playerId = 2;
        double duration = 10.0;

        timerManager.StartAuctionTimer(playerId, duration);
        var tick = timerManager.Tick();

        Assert.IsNotNull(tick);
        Assert.AreEqual(TimerType.Auction, tick?.type);
        Assert.AreEqual(playerId, tick?.playerId);
        Assert.IsTrue(tick?.isActive);
    }

    [Test]
    public void StartTradeTimer_Should_StartTimer()
    {
        int playerId = 3;
        double duration = 8.0;

        timerManager.StartTradeTimer(playerId, duration);
        var tick = timerManager.Tick();

        Assert.IsNotNull(tick);
        Assert.AreEqual(TimerType.Trade, tick?.type);
        Assert.AreEqual(playerId, tick?.playerId);
        Assert.IsTrue(tick?.isActive);
    }
}
