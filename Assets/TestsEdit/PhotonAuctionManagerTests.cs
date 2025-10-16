using Moq;
using NUnit.Framework;
using Photon.Pun;
using System;
using UnityEngine;

public class PhotonAuctionManagerTests
{
    private PhotonAuctionManager auctionManager;
    private Mock<IEventBus> eventBusMock;
    private Mock<IPhotonNetworkWrapper> photonNetworkMock;
    private Mock<IPhotonViewWrapper> photonViewWrapperMock;
    private PhotonView photonView;

    [SetUp]
    public void SetUp()
    {
        var go = new GameObject();
        auctionManager = go.AddComponent<PhotonAuctionManager>();
        photonView = go.AddComponent<PhotonView>();

        eventBusMock = new Mock<IEventBus>();
        photonNetworkMock = new Mock<IPhotonNetworkWrapper>();
        photonViewWrapperMock = new Mock<IPhotonViewWrapper>();

        auctionManager.Consturct(eventBusMock.Object, photonNetworkMock.Object, photonViewWrapperMock.Object);
    }

    [Test]
    public void PromptBidRequest_ShouldCallRPC_WithCorrectArguments()
    {
        auctionManager.PromptBidRequest(1, 100, 5);

        photonViewWrapperMock.Verify(p => p.RPC(
            photonView,
            "RPC_PromptBid_Internal",
            RpcTarget.All,
            1, 100, 5), Times.Once);
    }

    [Test]
    public void StartAuctionRequest_ShouldCallRPC_MasterClient()
    {
        auctionManager.StartAuctionRequest(2, 10, 500);

        photonViewWrapperMock.Verify(p => p.RPC(
            photonView,
            "RPC_StartAuctionRequest",
            RpcTarget.MasterClient,
            2, 10, 500), Times.Once);
    }

    [Test]
    public void RPC_PromptBid_Internal_ShouldPublishEvent()
    {
        // вызываем приватный метод через reflection
        var method = typeof(PhotonAuctionManager)
            .GetMethod("RPC_PromptBid_Internal", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        method.Invoke(auctionManager, new object[] { 1, 100, 5 });

        eventBusMock.Verify(e => e.Publish(It.Is<AuctionPromptBidEvent>(
            evt => evt.PlayerId == 1 && evt.Bid == 100 && evt.CompanyId == 5)), Times.Once);
    }

    [Test]
    public void RPC_StartAuctionRequest_ShouldPublishEvent_OnlyIfMasterClient()
    {
        photonNetworkMock.Setup(p => p.IsMasterClient).Returns(true);

        var method = typeof(PhotonAuctionManager)
            .GetMethod("RPC_StartAuctionRequest", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        method.Invoke(auctionManager, new object[] { 2, 10, 500 });

        eventBusMock.Verify(e => e.Publish(It.Is<StartAuctionEvent>(
            evt => evt.StarterActorNumber == 2 && evt.CompanyId == 10 && evt.CompanyBasePrice == 500)), Times.Once);

        // если не мастер, событие не публикуется
        eventBusMock.Reset();
        photonNetworkMock.Setup(p => p.IsMasterClient).Returns(false);

        method.Invoke(auctionManager, new object[] { 2, 10, 500 });

        eventBusMock.Verify(e => e.Publish(It.IsAny<StartAuctionEvent>()), Times.Never);
    }
    [Test]
    public void PlayerBidRequest_ShouldCallRPC_MasterClient()
    {
        auctionManager.PlayerBidRequest(42);
        photonViewWrapperMock.Verify(p => p.RPC(
            auctionManager.photonView, "RPC_UpdateBid", RpcTarget.MasterClient, 42), Times.Once);
    }

    [Test]
    public void PlayerPassRequest_ShouldCallRPC_MasterClient()
    {
        auctionManager.PlayerPassRequest(42);
        photonViewWrapperMock.Verify(p => p.RPC(
            auctionManager.photonView, "RPC_UpdatePass", RpcTarget.MasterClient, 42), Times.Once);
    }

    [Test]
    public void CloseAuctionWindowRequest_ShouldCallRPC_WithPlayerId()
    {
        // Arrange
        int playerId = 5;

        // Act
        auctionManager.CloseAuctionWindowRequest(playerId);

        // Assert
        photonViewWrapperMock.Verify(p => p.RPC(
            auctionManager.photonView,
            "RPC_CloseAuctionWindow",
            playerId,
            null),
            Times.Once);
    }

    [Test]
    public void RPC_UpdateBid_ShouldPublishEvent_OnlyIfMasterClient()
    {
        photonNetworkMock.Setup(p => p.IsMasterClient).Returns(true);
        var method = typeof(PhotonAuctionManager).GetMethod("RPC_UpdateBid", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        method.Invoke(auctionManager, new object[] { 1 });
        eventBusMock.Verify(e => e.Publish(It.Is<PlayerBidAuction>(ev => ev.PlayerId == 1)), Times.Once);

        photonNetworkMock.Setup(p => p.IsMasterClient).Returns(false);
        eventBusMock.Reset();
        method.Invoke(auctionManager, new object[] { 1 });
        eventBusMock.Verify(e => e.Publish(It.IsAny<PlayerBidAuction>()), Times.Never);
    }

    [Test]
    public void RPC_UpdatePass_ShouldPublishEvent_OnlyIfMasterClient()
    {
        photonNetworkMock.Setup(p => p.IsMasterClient).Returns(true);
        var method = typeof(PhotonAuctionManager).GetMethod("RPC_UpdatePass", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        method.Invoke(auctionManager, new object[] { 1 });
        eventBusMock.Verify(e => e.Publish(It.Is<PlayerPassAuction>(ev => ev.PlayerId == 1)), Times.Once);

        photonNetworkMock.Setup(p => p.IsMasterClient).Returns(false);
        eventBusMock.Reset();
        method.Invoke(auctionManager, new object[] { 1 });
        eventBusMock.Verify(e => e.Publish(It.IsAny<PlayerPassAuction>()), Times.Never);
    }

    [Test]
    public void RPC_CloseAuctionWindow_ShouldPublishAuctionEndEvent()
    {
        var method = typeof(PhotonAuctionManager).GetMethod("RPC_CloseAuctionWindow", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        method.Invoke(auctionManager, null);
        eventBusMock.Verify(e => e.Publish(It.IsAny<AuctionEndEvent>()), Times.Once);
    }

    [Test]
    public void Construct_NullParameters_ShouldThrow()
    {
        var go = new GameObject();
        var manager = go.AddComponent<PhotonAuctionManager>();
        var ex = Assert.Throws<ArgumentNullException>(() => manager.Consturct(null, photonNetworkMock.Object, photonViewWrapperMock.Object));
        Assert.IsNotNull(ex);

        ex = Assert.Throws<ArgumentNullException>(() => manager.Consturct(eventBusMock.Object, null, photonViewWrapperMock.Object));
        Assert.IsNotNull(ex);

        ex = Assert.Throws<ArgumentNullException>(() => manager.Consturct(eventBusMock.Object, photonNetworkMock.Object, null));
        Assert.IsNotNull(ex);
    }

}
