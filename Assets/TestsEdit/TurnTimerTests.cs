using Moq;
using NUnit.Framework;

[TestFixture]
public class TurnTimerTests
{
    private TurnTimer timer;
    private Mock<ITimeProvider> timeProviderMock;

    [SetUp]
    public void SetUp()
    {
        timeProviderMock = new Mock<ITimeProvider>();
        timer = new TurnTimer(timeProviderMock.Object);
    }

    [Test]
    public void Start_ShouldInitializeTimer()
    {
        timeProviderMock.Setup(tp => tp.Now).Returns(0);
        timer.Start(TimerType.Turn, 1, timeProviderMock.Object.Now, 10);

        var tick = timer.Tick();
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
        timeProviderMock.Setup(tp => tp.Now).Returns(5);
        timer.Start(TimerType.Trade, 2, 5, 10); // старт в 5 сек

        timeProviderMock.Setup(tp => tp.Now).Returns(10); // прошло 5 сек
        var tick = timer.Tick();

        Assert.IsNotNull(tick);
        Assert.AreEqual(5f, tick.Value.timeLeft);
        Assert.IsTrue(tick.Value.isActive);
        Assert.IsFalse(tick.Value.expired);
    }

    [Test]
    public void Tick_ShouldExpireTimer_WhenTimeOver()
    {
        timeProviderMock.Setup(tp => tp.Now).Returns(0);
        timer.Start(TimerType.Auction, 3, timeProviderMock.Object.Now, 10);

        timeProviderMock.Setup(tp => tp.Now).Returns(12); // прошло больше времени
        var tick = timer.Tick();

        Assert.IsNotNull(tick);
        Assert.AreEqual(0f, tick.Value.timeLeft);
        Assert.IsFalse(tick.Value.isActive);
        Assert.IsTrue(tick.Value.expired);
    }

    [Test]
    public void Tick_ShouldReturnNull_WhenTimerNotActive()
    {
        // таймер не запущен
        var tick = timer.Tick();
        Assert.IsNull(tick);
    }

    [Test]
    public void Tick_ShouldClampTimeLeftToDuration_WhenNegativeTime()
    {
        timeProviderMock.Setup(tp => tp.Now).Returns(-5);
        timer.Start(TimerType.Turn, 1, 0, 10);

        var tick = timer.Tick();

        Assert.IsNotNull(tick);
        Assert.AreEqual(15f, tick.Value.timeLeft); // ожидаем 15
        Assert.IsTrue(tick.Value.isActive);
    }
    [Test]
    public void Start_ShouldOverwriteExistingTimer()
    {
        timeProviderMock.Setup(tp => tp.Now).Returns(0);
        timer.Start(TimerType.Turn, 1, 0, 10);

        // Перезапуск таймера
        timer.Start(TimerType.Auction, 2, 0, 20);

        var tick = timer.Tick();

        Assert.IsNotNull(tick);
        Assert.AreEqual(TimerType.Auction, tick.Value.type);
        Assert.AreEqual(2, tick.Value.playerId);
        Assert.AreEqual(20f, tick.Value.timeLeft);
        Assert.IsTrue(tick.Value.isActive);
    }

    [Test]
    public void Tick_ShouldUpdateIsActiveOnMultipleTicks()
    {
        timeProviderMock.Setup(tp => tp.Now).Returns(0);
        timer.Start(TimerType.Trade, 1, 0, 10);

        // Первый тик через 5 сек
        timeProviderMock.Setup(tp => tp.Now).Returns(5);
        var tick1 = timer.Tick();
        Assert.IsTrue(tick1.Value.isActive);
        Assert.IsFalse(tick1.Value.expired);
        Assert.AreEqual(5f, tick1.Value.timeLeft);

        // Второй тик через 10 сек
        timeProviderMock.Setup(tp => tp.Now).Returns(10);
        var tick2 = timer.Tick();
        Assert.IsFalse(tick2.Value.isActive);
        Assert.IsTrue(tick2.Value.expired);
        Assert.AreEqual(0f, tick2.Value.timeLeft);

        // Третий тик после истечения
        timeProviderMock.Setup(tp => tp.Now).Returns(12);
        var tick3 = timer.Tick();
        Assert.IsFalse(tick3.Value.isActive);
        Assert.IsTrue(tick3.Value.expired);
        Assert.AreEqual(0f, tick3.Value.timeLeft);
    }

    [Test]
    public void Start_ShouldCorrectlyHandleAllTimerTypes()
    {
        foreach (TimerType type in new[] { TimerType.Turn, TimerType.Auction, TimerType.Trade })
        {
            timer.Start(type, 1, 0, 5);
            timeProviderMock.Setup(tp => tp.Now).Returns(0);
            var tick = timer.Tick();
            Assert.IsNotNull(tick);
            Assert.AreEqual(type, tick.Value.type);
            timer = new TurnTimer(timeProviderMock.Object); // сброс таймера
        }
    }

    [Test]
    public void Tick_ShouldReturnDuration_WhenNowLessThanStartTime()
    {
        timer.Start(TimerType.Turn, 1, 10, 15); // старт в будущем
        timeProviderMock.Setup(tp => tp.Now).Returns(5);

        var tick = timer.Tick();
        Assert.IsNotNull(tick);
        Assert.AreEqual(20f, tick.Value.timeLeft);
        Assert.IsTrue(tick.Value.isActive);
        Assert.IsFalse(tick.Value.expired);
    }
}
