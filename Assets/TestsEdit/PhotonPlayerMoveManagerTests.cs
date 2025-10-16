using Moq;
using NUnit.Framework;
using Photon.Pun;
using System;
using UnityEngine;

[TestFixture]
public class PhotonPlayerMoveManagerTests
{
    private PhotonPlayerMoveManager manager;
    private Mock<IPlayerMoveUseCase> playerMoveUseCaseMock;
    private Mock<IPlayerRepository> playerRepositoryMock;
    private Mock<IBoardService> boardServiceMock;
    private Mock<IEventBus> eventBusMock;
    private Mock<IPhotonNetworkWrapper> photonNetworkMock;
    private Mock<IPhotonViewWrapper> photonViewWrapperMock;
    private GameObject go;

    [SetUp]
    public void Setup()
    {
        go = new GameObject();
        manager = go.AddComponent<PhotonPlayerMoveManager>();

        playerMoveUseCaseMock = new Mock<IPlayerMoveUseCase>();
        playerRepositoryMock = new Mock<IPlayerRepository>();
        boardServiceMock = new Mock<IBoardService>();
        eventBusMock = new Mock<IEventBus>();
        photonNetworkMock = new Mock<IPhotonNetworkWrapper>();
        photonViewWrapperMock = new Mock<IPhotonViewWrapper>();

        manager.Construct(playerMoveUseCaseMock.Object, eventBusMock.Object,
                          playerRepositoryMock.Object, boardServiceMock.Object,
                          photonNetworkMock.Object, photonViewWrapperMock.Object);
    }

    [TearDown]
    public void TearDown()
    {
       UnityEngine.Object.DestroyImmediate(go);
    }

    [Test]
    public void RequestTeleport_ShouldCallRPC_WhenMasterClient()
    {
        photonNetworkMock.Setup(p => p.IsMasterClient).Returns(true);

        manager.RequestTeleport(1);

        photonViewWrapperMock.Verify(p => p.RPC(
            manager.photonView,
            "RPC_TeleportPlayer",
            RpcTarget.MasterClient,
            1), Times.Once);
    }

    [Test]
    public void RequestTeleport_ShouldNotCallRPC_WhenNotMasterClient()
    {
        photonNetworkMock.Setup(p => p.IsMasterClient).Returns(false);

        manager.RequestTeleport(1);

        photonViewWrapperMock.Verify(p => p.RPC(
            It.IsAny<PhotonView>(),
            It.IsAny<string>(),
            It.IsAny<RpcTarget>(),
            It.IsAny<object[]>()), Times.Never);
    }

    [Test]
    public void RequestMove_ShouldCallRPC_MovePlayer()
    {
        
        var e = new OnPlayerMoveEvent (2,3,true);

        // вызываем приватный метод через делегат
        var requestMove = manager.GetType()
            .GetMethod("RequestMove", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        requestMove.Invoke(manager, new object[] { e });

        photonViewWrapperMock.Verify(p => p.RPC(
            manager.photonView,
            "RPC_MovePlayer",
            RpcTarget.All,
            e.PlayerId,
            e.Steps,
            e.Forward), Times.Once);
    }

    [Test]
    public void RPC_TeleportPlayerBroadcast_ShouldCallUseCase()
    {
        var method = manager.GetType().GetMethod("RPC_TeleportPlayerBroadcast",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        method.Invoke(manager, new object[] { 1, 5, 2 });

        playerMoveUseCaseMock.Verify(p => p.TeleportPlayer(1, 5, 2), Times.Once);
    }

    [Test]
    public void RPC_MovePlayer_ShouldCallUseCase()
    {
        var method = manager.GetType().GetMethod("RPC_MovePlayer",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        method.Invoke(manager, new object[] { 1, 3, true });

        playerMoveUseCaseMock.Verify(p => p.MovePlayer(1, 3, true), Times.Once);
    }
    [Test]
    public void OnEnable_ShouldSubscribeToEvent()
    {
        var method = typeof(PhotonPlayerMoveManager).GetMethod("OnEnable", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        method.Invoke(manager, null);

        eventBusMock.Verify(e => e.Subscribe<OnPlayerMoveEvent>(It.IsAny<Action<OnPlayerMoveEvent>>()), Times.Once);
    }

    [Test]
    public void OnDisable_ShouldUnsubscribeFromEvent()
    {
        var method = typeof(PhotonPlayerMoveManager).GetMethod("OnDisable", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        method.Invoke(manager, null);

        eventBusMock.Verify(e => e.Unsubscribe<OnPlayerMoveEvent>(It.IsAny<Action<OnPlayerMoveEvent>>()), Times.Once);
    }

}
