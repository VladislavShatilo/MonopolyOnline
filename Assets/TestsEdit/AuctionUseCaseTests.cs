using Moq;
using NUnit.Framework;
using System;
using System.Reflection;

[TestFixture]
public class AuctionUseCaseTests
{
    private AuctionUseCase useCase;
    private Mock<IAuctionService> auctionService;
    private Mock<IEventBus> eventBus;

    [SetUp]
    public void Setup()
    {
        auctionService = new Mock<IAuctionService>();
        eventBus = new Mock<IEventBus>();

        useCase = new AuctionUseCase();
        useCase.Construct(auctionService.Object, eventBus.Object);
    }

    [Test]
    public void Initialize_ShouldSubscribeToAllEvents()
    {
        useCase.Initialize();

        eventBus.Verify(e => e.Subscribe<StartAuctionEvent>(It.IsAny<Action<StartAuctionEvent>>()), Times.Once);
        eventBus.Verify(e => e.Subscribe<PlayerBidAuction>(It.IsAny<Action<PlayerBidAuction>>()), Times.Once);
        eventBus.Verify(e => e.Subscribe<PlayerPassAuction>(It.IsAny<Action<PlayerPassAuction>>()), Times.Once);
        eventBus.Verify(e => e.Subscribe<TimerExpiredEvent>(It.IsAny<Action<TimerExpiredEvent>>()), Times.Once);
    }

    [Test]
    public void Dispose_ShouldUnsubscribeFromAllEvents()
    {
        useCase.Dispose();

        eventBus.Verify(e => e.Unsubscribe<StartAuctionEvent>(It.IsAny<Action<StartAuctionEvent>>()), Times.Once);
        eventBus.Verify(e => e.Unsubscribe<PlayerBidAuction>(It.IsAny<Action<PlayerBidAuction>>()), Times.Once);
        eventBus.Verify(e => e.Unsubscribe<PlayerPassAuction>(It.IsAny<Action<PlayerPassAuction>>()), Times.Once);
        eventBus.Verify(e => e.Unsubscribe<TimerExpiredEvent>(It.IsAny<Action<TimerExpiredEvent>>()), Times.Once);
    }

    [Test]
    public void StartAuctionEvent_ShouldCallAuctionServiceStartAuction()
    {
        var evt = new StartAuctionEvent(1, 5, 500);

        // напрямую вызываем приватный callback через рефлексию
        var method = typeof(AuctionUseCase).GetMethod("StartAuction", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        method.Invoke(useCase, new object[] { evt });

        auctionService.Verify(a => a.StartAuction(1, 5, 500), Times.Once);
    }

    [Test]
    public void PlayerBidEvent_ShouldCallAuctionServicePlaceBid()
    {
        var evt = new PlayerBidAuction(2);
        var method = typeof(AuctionUseCase).GetMethod("PlayerBid", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        method.Invoke(useCase, new object[] { evt });

        auctionService.Verify(a => a.PlaceBid(2), Times.Once);
    }

    [Test]
    public void PlayerPassEvent_ShouldCallAuctionServicePassBid()
    {
        var evt = new PlayerPassAuction(3);
        var method = typeof(AuctionUseCase).GetMethod("PlayerPass", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        method.Invoke(useCase, new object[] { evt });

        auctionService.Verify(a => a.PassBid(3), Times.Once);
    }

    [Test]
    public void TimerExpiredEvent_WithAuctionType_ShouldCallPassBid()
    {
        var evt = new TimerExpiredEvent(TimerType.Auction, 4);
        var method = typeof(AuctionUseCase).GetMethod("TimerExpiredEvent", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        method.Invoke(useCase, new object[] { evt });

        auctionService.Verify(a => a.PassBid(4), Times.Once);
    }

    [Test]
    public void TimerExpiredEvent_WithNonAuctionType_ShouldNotCallPassBid()
    {
        var evt = new TimerExpiredEvent(TimerType.Turn, 4);
        var method = typeof(AuctionUseCase).GetMethod("TimerExpiredEvent", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        method.Invoke(useCase, new object[] { evt });

        auctionService.Verify(a => a.PassBid(It.IsAny<int>()), Times.Never);
    }
    [Test]
    public void Construct_ShouldThrowArgumentNullException_WhenDependenciesAreNull()
    {
        var useCase = new AuctionUseCase();

        Assert.Throws<ArgumentNullException>(() => useCase.Construct(null, new Mock<IEventBus>().Object));
        Assert.Throws<ArgumentNullException>(() => useCase.Construct(new Mock<IAuctionService>().Object, null));
    }
    [Test]
    public void StartAuction_ShouldThrow_WhenEventIsNull()
    {
        var method = typeof(AuctionUseCase)
            .GetMethod("StartAuction", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        var ex = Assert.Throws<TargetInvocationException>(() => method.Invoke(useCase, new object[] { null }));

        Assert.IsInstanceOf<ArgumentNullException>(ex.InnerException);
        Assert.That(((ArgumentNullException)ex.InnerException).ParamName, Is.EqualTo("e"));
    }


    [Test]
    public void PlayerBid_ShouldThrow_WhenEventIsNull()
    {
        var method = typeof(AuctionUseCase).GetMethod("PlayerBid", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        Assert.Throws<TargetInvocationException>(() => method.Invoke(useCase, new object[] { null }));
    }

    [Test]
    public void PlayerPass_ShouldThrow_WhenEventIsNull()
    {
        var method = typeof(AuctionUseCase).GetMethod("PlayerPass", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        Assert.Throws<TargetInvocationException>(() => method.Invoke(useCase, new object[] { null }));
    }

    [Test]
    public void TimerExpiredEvent_ShouldThrow_WhenEventIsNull()
    {
        var method = typeof(AuctionUseCase).GetMethod("TimerExpiredEvent", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        Assert.Throws<TargetInvocationException>(() => method.Invoke(useCase, new object[] { null }));
    }

}