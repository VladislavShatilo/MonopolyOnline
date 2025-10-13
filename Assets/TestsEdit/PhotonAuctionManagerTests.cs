using Moq;
using NUnit.Framework;
using Photon.Pun;
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
}
