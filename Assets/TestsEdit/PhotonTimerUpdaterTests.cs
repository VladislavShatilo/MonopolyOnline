using NUnit.Framework;
using Moq;
using UnityEngine;
using Photon.Pun;

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

        manager.Construct(timerManagerMock.Object, eventBusMock.Object, photonNetworkMock.Object, photonViewWrapperMock.Object);
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(go);
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

        // Проверяем публикацию события
        eventBusMock.Verify(e => e.Publish(It.Is<TimerUpdatedEvent>(ev =>
            ev.Type == TimerType.Turn && ev.PlayerId == 1 && ev.TimeLeft == 5.6f && ev.IsActive == true)), Times.Once);

        // Проверяем вызов RPC
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
}
