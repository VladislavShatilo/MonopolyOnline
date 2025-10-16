using Moq;
using NUnit.Framework;
using Photon.Pun;
using System;
using UnityEngine;

[TestFixture]
public class PhotonTurnManagerTests
{
    private PhotonTurnManager manager;
    private Mock<ITurnService> turnServiceMock;
    private Mock<IPhotonNetworkWrapper> photonNetworkMock;
    private Mock<IPhotonViewWrapper> photonViewWrapperMock;
    private GameObject go;

    [SetUp]
    public void Setup()
    {
        go = new GameObject();
        manager = go.AddComponent<PhotonTurnManager>();

        turnServiceMock = new Mock<ITurnService>();
        photonNetworkMock = new Mock<IPhotonNetworkWrapper>();
        photonViewWrapperMock = new Mock<IPhotonViewWrapper>();
        var photonView = go.AddComponent<PhotonView>(); // <--- вот это

        manager.Construct(turnServiceMock.Object, photonNetworkMock.Object, photonViewWrapperMock.Object);
    }

    [TearDown]
    public void TearDown()
    {
        UnityEngine.Object.DestroyImmediate(go);
    }

    [Test]
    public void RequestStartRandomTurn_ShouldCallStartRandomTurn_WhenMasterClient()
    {
        photonNetworkMock.Setup(p => p.IsMasterClient).Returns(true);

        manager.RequestStartRandomTurn();

        turnServiceMock.Verify(t => t.StartRandomTurn(), Times.Once);
    }

    [Test]
    public void RequestStartRandomTurn_ShouldNotCallStartRandomTurn_WhenNotMasterClient()
    {
        photonNetworkMock.Setup(p => p.IsMasterClient).Returns(false);

        manager.RequestStartRandomTurn();

        turnServiceMock.Verify(t => t.StartRandomTurn(), Times.Never);
    }

    [Test]
    public void RequestEndTurn_ShouldCallRPC()
    {
        manager.RequestEndTurn();

        photonViewWrapperMock.Verify(p => p.RPC(manager.photonView, "RPC_RequestEndTurn", RpcTarget.MasterClient), Times.Once);
    }

    [Test]
    public void RegisterDouble_ShouldCallRPCWithPlayerId()
    {
        int playerId = 5;
        manager.RegisterDouble(playerId);

        photonViewWrapperMock.Verify(p => p.RPC(manager.photonView, "RPC_RegisterDouble", RpcTarget.MasterClient, playerId), Times.Once);
    }

    [Test]
    public void RPC_RequestEndTurn_ShouldCallEndTurn_WhenMasterClient()
    {
        photonNetworkMock.Setup(p => p.IsMasterClient).Returns(true);

        var method = manager.GetType().GetMethod("RPC_RequestEndTurn",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        method.Invoke(manager, null);

        turnServiceMock.Verify(t => t.EndTurn(), Times.Once);
    }

    [Test]
    public void RPC_RegisterDouble_ShouldCallRegisterDouble_WhenMasterClient()
    {
        photonNetworkMock.Setup(p => p.IsMasterClient).Returns(true);

        int playerId = 7;
        var method = manager.GetType().GetMethod("RPC_RegisterDouble",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        method.Invoke(manager, new object[] { playerId });

        turnServiceMock.Verify(t => t.RegisterDouble(playerId), Times.Once);
    }
    [Test]
    public void RPC_RequestEndTurn_ShouldNotCallEndTurn_WhenNotMasterClient()
    {
        photonNetworkMock.Setup(p => p.IsMasterClient).Returns(false);

        var method = manager.GetType().GetMethod("RPC_RequestEndTurn",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        method.Invoke(manager, null);

        turnServiceMock.Verify(t => t.EndTurn(), Times.Never);
    }

    [Test]
    public void RPC_RegisterDouble_ShouldNotCallRegisterDouble_WhenNotMasterClient()
    {
        photonNetworkMock.Setup(p => p.IsMasterClient).Returns(false);
        int playerId = 42;

        var method = manager.GetType().GetMethod("RPC_RegisterDouble",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        method.Invoke(manager, new object[] { playerId });

        turnServiceMock.Verify(t => t.RegisterDouble(It.IsAny<int>()), Times.Never);
    }

    [Test]
    public void Construct_ShouldThrow_WhenPhotonViewMissing()
    {
        UnityEngine.Object.DestroyImmediate(go.GetComponent<PhotonView>());

        var ex = Assert.Throws<NullReferenceException>(() =>
        {
            manager.Construct(turnServiceMock.Object, photonNetworkMock.Object, photonViewWrapperMock.Object);
        });

        Assert.That(ex.Message, Is.EqualTo("photonView"));
    }
}
