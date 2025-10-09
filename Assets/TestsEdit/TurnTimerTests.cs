using NUnit.Framework;
using UnityEngine;

[TestFixture]
public class TurnTimerTests
{
    private TurnTimer timer;

    [SetUp]
    public void SetUp()
    {
        timer = new TurnTimer();
    }

    [Test]
    public void Start_ShouldInitializeTimer()
    {
        timer.Start(TimerType.Turn, 1, 0, 10);

        var tick = timer.Tick(0);
        Assert.IsNotNull(tick);
        Assert.AreEqual(TimerType.Turn, tick.Value.type);
        Assert.AreEqual(1, tick.Value.playerId);
        Assert.AreEqual(10f, tick.Value.timeLeft);
        Assert.IsTrue(tick.Value.isActive);
        Assert.IsFalse(tick.Value.expired);
    }

    [Test]
    public void Tick_ShouldReturnCorrectTimeLeft()
    {
        timer.Start(TimerType.Trade, 2, 5, 10);

        var tick = timer.Tick(10); // прошло 5 секунд
        Assert.IsNotNull(tick);
        Assert.AreEqual(5f, tick.Value.timeLeft);
        Assert.IsTrue(tick.Value.isActive);
        Assert.IsFalse(tick.Value.expired);
    }

    [Test]
    public void Tick_ShouldExpireTimer_WhenTimeOver()
    {
        timer.Start(TimerType.Auction, 3, 0, 10);

        var tick = timer.Tick(12); // прошло больше времени
        Assert.IsNotNull(tick);
        Assert.AreEqual(0f, tick.Value.timeLeft);
        Assert.IsFalse(tick.Value.isActive);
        Assert.IsTrue(tick.Value.expired);
    }

    [Test]
    public void Tick_ShouldReturnNull_WhenTimerNotActive()
    {
        // таймер не запущен
        var tick = timer.Tick(0);
        Assert.IsNull(tick);
    }

    [Test]
    public void Stop_ShouldDeactivateTimer()
    {
        timer.Start(TimerType.Turn, 1, 0, 10);
        timer.Stop();

        var tick = timer.Tick(5);
        Assert.IsNull(tick);
    }

    [Test]
    public void Tick_ShouldClampTimeLeftToDuration()
    {
        timer.Start(TimerType.Turn, 1, 0, 10);
        var tick = timer.Tick(-5); // отрицательное время
        Assert.AreEqual(10f, tick.Value.timeLeft);
    }
}
