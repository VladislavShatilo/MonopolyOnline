using Moq;
using NUnit.Framework;
using Photon.Pun;
using System;
using UnityEngine;

[TestFixture]
public class PhotonTimerUpdaterTests
{
    private PhotonTimerUpdater manager;
    private Mock<ITimerManager> timerManagerMock;
    private Mock<IEventBus> eventBusMock;
    private Mock<IPhotonNetworkWrapper> photonNetworkMock;
    private Mock<IPhotonViewWrapper> photonViewWrapperMock;
    private GameObject go;

    [SetUp]
    public void Setup()
    {
        go = new GameObject();
        manager = go.AddComponent<PhotonTimerUpdater>();

        timerManagerMock = new Mock<ITimerManager>();
        eventBusMock = new Mock<IEventBus>();
        photonNetworkMock = new Mock<IPhotonNetworkWrapper>();
        photonViewWrapperMock = new Mock<IPhotonViewWrapper>();
        var photonView = go.AddComponent<PhotonView>(); // <--- вот это

        manager.Construct(timerManagerMock.Object, eventBusMock.Object, photonNetworkMock.Object, photonViewWrapperMock.Object);
    }

    [TearDown]
    public void TearDown()
    {
        UnityEngine.Object.DestroyImmediate(go);
    }

    [Test]
    public void Update_ShouldNotDoAnything_WhenNotMasterClient()
    {
        photonNetworkMock.Setup(p => p.IsMasterClient).Returns(false);

        manager.TickTimer();

        timerManagerMock.Verify(t => t.Tick(), Times.Never);
        eventBusMock.Verify(e => e.Publish(It.IsAny<object>()), Times.Never);
    }

    [Test]
    public void Update_ShouldPublishAndRPC_WhenTickReturnsValue()
    {
        photonNetworkMock.Setup(p => p.IsMasterClient).Returns(true);

        var tickResult = (TimerType.Turn, 1, 5.6f, true, false);
        timerManagerMock.Setup(t => t.Tick()).Returns(tickResult);

        manager.TickTimer();

        // ѕровер€ем публикацию событи€
        eventBusMock.Verify(e => e.Publish(It.Is<TimerUpdatedEvent>(ev =>
            ev.Type == TimerType.Turn && ev.PlayerId == 1 && ev.TimeLeft == 5.6f && ev.IsActive == true)), Times.Once);

        // ѕровер€ем вызов RPC
        photonViewWrapperMock.Verify(p => p.RPC(manager.photonView,
            "RPC_SyncTimer",
            RpcTarget.Others,
            tickResult.Item1, tickResult.Item2, tickResult.Item3, tickResult.Item4), Times.Once);
    }

    [Test]
    public void Update_ShouldPublishExpiredEvent_WhenExpiredTrue()
    {
        photonNetworkMock.Setup(p => p.IsMasterClient).Returns(true);

        var tickResult = (TimerType.Turn, 2, 3.5f, true, true);
        timerManagerMock.Setup(t => t.Tick()).Returns(tickResult);

        manager.TickTimer();

        eventBusMock.Verify(e => e.Publish(It.Is<TimerExpiredEvent>(ev =>
            ev.Type == TimerType.Turn && ev.PlayerId == 2)), Times.Once);
    }

    [Test]
    public void RPC_SyncTimer_ShouldPublishUpdatedEvent()
    {
        var method = manager.GetType().GetMethod("RPC_SyncTimer",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        method.Invoke(manager, new object[] { TimerType.Turn, 1, 4.2f, true });

        eventBusMock.Verify(e => e.Publish(It.Is<TimerUpdatedEvent>(ev =>
            ev.Type == TimerType.Turn && ev.PlayerId == 1 && ev.TimeLeft == 4.2f && ev.IsActive)), Times.Once);
    }
    [Test]
    public void TickTimer_ShouldDoNothing_WhenTickReturnsNull()
    {
        photonNetworkMock.Setup(p => p.IsMasterClient).Returns(true);
        timerManagerMock.Setup(t => t.Tick()).Returns((ValueTuple<TimerType, int, float, bool, bool>?)null);

        manager.TickTimer();

        eventBusMock.Verify(e => e.Publish(It.IsAny<object>()), Times.Never);
        photonViewWrapperMock.Verify(v => v.RPC(It.IsAny<PhotonView>(), It.IsAny<string>(), It.IsAny<RpcTarget>(), It.IsAny<object[]>()), Times.Never);
    }

    // --------------------------
    // 2. TickTimer не вызывает событи€, если секунда не изменилась и expired=false
    // --------------------------
    [Test]
    public void TickTimer_ShouldNotPublish_WhenSecondUnchangedAndNotExpired()
    {
        photonNetworkMock.Setup(p => p.IsMasterClient).Returns(true);

        // первый тик
        timerManagerMock.SetupSequence(t => t.Tick())
            .Returns((TimerType.Turn, 1, 5.2f, true, false))
            .Returns((TimerType.Turn, 1, 5.1f, true, false)); // ceil(5.1)=6, ceil(5.2)=6, секунда не мен€етс€

        manager.TickTimer(); // первый тик
        manager.TickTimer(); // второй тик, секунда не изменилась

        eventBusMock.Verify(e => e.Publish(It.IsAny<TimerUpdatedEvent>()), Times.Once);
        photonViewWrapperMock.Verify(v => v.RPC(It.IsAny<PhotonView>(), It.IsAny<string>(), It.IsAny<RpcTarget>(), It.IsAny<object[]>()), Times.Once);
    }

    // --------------------------
    // 3.  онструктор выбрасывает NullReferenceException при отсутствии photonView
    // --------------------------
    [Test]
    public void Construct_ShouldThrow_WhenPhotonViewNull()
    {
        var go2 = new GameObject();
        var manager2 = go2.AddComponent<PhotonTimerUpdater>();

        Assert.Throws<NullReferenceException>(() =>
            manager2.Construct(timerManagerMock.Object, eventBusMock.Object, photonNetworkMock.Object, photonViewWrapperMock.Object)
        );

        UnityEngine.Object.DestroyImmediate(go2);
    }
}
