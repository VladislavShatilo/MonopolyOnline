using Moq;
using NUnit.Framework;
using Photon.Pun;
using System;
using UnityEngine;

public class PhotonJailManagerTests
{
    private PhotonJailManager jailManager;
    private Mock<IJailService> jailServiceMock;
    private Mock<IPhotonTurnManager> turnManagerMock;
    private Mock<IEventBus> eventBusMock;
    private Mock<IPhotonNetworkWrapper> networkMock;
    private Mock<IPhotonViewWrapper> viewWrapperMock;
    private PhotonView photonView;

    [SetUp]
    public void SetUp()
    {
        var go = new GameObject();
        jailManager = go.AddComponent<PhotonJailManager>();
        photonView = go.AddComponent<PhotonView>();

        jailServiceMock = new Mock<IJailService>();
        turnManagerMock = new Mock<IPhotonTurnManager>();
        eventBusMock = new Mock<IEventBus>();
        networkMock = new Mock<IPhotonNetworkWrapper>();
        viewWrapperMock = new Mock<IPhotonViewWrapper>();

        jailManager.Construct(jailServiceMock.Object, eventBusMock.Object, turnManagerMock.Object, networkMock.Object, viewWrapperMock.Object);
    }

    [Test]
    public void SendToJail_ShouldCallRPC_AndEndTurn_WhenMasterClient()
    {
        networkMock.Setup(n => n.IsMasterClient).Returns(true);

        jailManager.SendToJail(1);

        viewWrapperMock.Verify(v => v.RPC(jailManager.photonView,"RPC_MoveToJail",
            RpcTarget.All, 1), Times.Once);
        turnManagerMock.Verify(t => t.RequestEndTurn(), Times.Once);
    }

    [Test]
    public void SendToJail_ShouldNotCallRPC_WhenNotMasterClient()
    {
        networkMock.Setup(n => n.IsMasterClient).Returns(false);

        jailManager.SendToJail(1);

        viewWrapperMock.Verify(v => v.RPC(It.IsAny<PhotonView>(), It.IsAny<string>(), It.IsAny<RpcTarget>(), It.IsAny<object[]>()), Times.Never);
        turnManagerMock.Verify(t => t.RequestEndTurn(), Times.Never);
    }

    [Test]
    public void CheckDice_ShouldCallTryReleaseByDice()
    {
        jailManager.CheckDice(1, 4, 3);

        jailServiceMock.Verify(j => j.TryReleaseByDice(1, 4, 3), Times.Once);
    }

    [Test]
    public void RPC_MoveToJail_ShouldPublishEvent_AndSendPlayerToJail()
    {
        var method = typeof(PhotonJailManager)
            .GetMethod("RPC_MoveToJail", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        method.Invoke(jailManager, new object[] { 1 });

        eventBusMock.Verify(e => e.Publish(It.Is<MoveToJailEvent>(ev => ev.PlayerID == 1)), Times.Once);
        jailServiceMock.Verify(j => j.SendPlayerToJail(1), Times.Once);
    }
    [Test]
    public void Construct_ShouldThrowArgumentNullException_WhenJailServiceIsNull()
    {
        Assert.Throws<ArgumentNullException>(() =>
            jailManager.Construct(null, eventBusMock.Object, turnManagerMock.Object, networkMock.Object, viewWrapperMock.Object));
    }

    [Test]
    public void Construct_ShouldThrowArgumentNullException_WhenEventBusIsNull()
    {
        Assert.Throws<ArgumentNullException>(() =>
            jailManager.Construct(jailServiceMock.Object, null, turnManagerMock.Object, networkMock.Object, viewWrapperMock.Object));
    }

    [Test]
    public void Construct_ShouldThrowArgumentNullException_WhenTurnManagerIsNull()
    {
        Assert.Throws<ArgumentNullException>(() =>
            jailManager.Construct(jailServiceMock.Object, eventBusMock.Object, null, networkMock.Object, viewWrapperMock.Object));
    }

    [Test]
    public void Construct_ShouldThrowArgumentNullException_WhenNetworkWrapperIsNull()
    {
        Assert.Throws<ArgumentNullException>(() =>
            jailManager.Construct(jailServiceMock.Object, eventBusMock.Object, turnManagerMock.Object, null, viewWrapperMock.Object));
    }

    [Test]
    public void Construct_ShouldThrowArgumentNullException_WhenViewWrapperIsNull()
    {
        Assert.Throws<ArgumentNullException>(() =>
            jailManager.Construct(jailServiceMock.Object, eventBusMock.Object, turnManagerMock.Object, networkMock.Object, null));
    }

    [Test]
    public void Construct_ShouldThrowNullReferenceException_WhenPhotonViewIsMissing()
    {
        var goWithoutView = new GameObject();
        var managerWithoutView = goWithoutView.AddComponent<PhotonJailManager>();

        Assert.Throws<NullReferenceException>(() =>
            managerWithoutView.Construct(jailServiceMock.Object, eventBusMock.Object, turnManagerMock.Object, networkMock.Object, viewWrapperMock.Object));

        GameObject.DestroyImmediate(goWithoutView);
    }

}
