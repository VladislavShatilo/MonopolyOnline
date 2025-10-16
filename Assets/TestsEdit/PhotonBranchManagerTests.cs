using Moq;
using NUnit.Framework;
using Photon.Pun;
using System;
using UnityEngine;

public class PhotonBranchManagerTests
{
    private PhotonBranchManager branchManager;
    private Mock<IBranchUseCase> branchUseCaseMock;
    private Mock<IPhotonViewWrapper> photonViewWrapperMock;
    private Mock<IPhotonNetworkWrapper> photonNetworkMock;
    private PhotonView photonView;

    [SetUp]
    public void SetUp()
    {
        var go = new GameObject();
        branchManager = go.AddComponent<PhotonBranchManager>();
        photonView = go.AddComponent<PhotonView>();

        branchUseCaseMock = new Mock<IBranchUseCase>();
        photonViewWrapperMock = new Mock<IPhotonViewWrapper>();
        photonNetworkMock = new Mock<IPhotonNetworkWrapper>();

        branchManager.Construct(branchUseCaseMock.Object, photonNetworkMock.Object, photonViewWrapperMock.Object);
    }

    [Test]
    public void RequestBuyBranch_ShouldCallRPC_WithCorrectArguments()
    {
        branchManager.RequestBuyBranch(10);

        photonViewWrapperMock.Verify(p => p.RPC(
            branchManager.photonView,
            "RPC_BuyBranch",
            RpcTarget.MasterClient,
            10,
            PhotonNetwork.LocalPlayer.ActorNumber), Times.Once);
    }

    [Test]
    public void RPC_BuyBranch_ShouldCallBranchUseCase_AndUpdateUI_WhenMasterClient()
    {
        photonNetworkMock.Setup(p => p.IsMasterClient).Returns(true);
        branchUseCaseMock.Setup(b => b.BuyBranch(5, 1)).Returns(3); // newRentLevel = 3

        var method = typeof(PhotonBranchManager)
            .GetMethod("RPC_BuyBranch", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        method.Invoke(branchManager, new object[] { 5, 1 });

        branchUseCaseMock.Verify(b => b.BuyBranch(5, 1), Times.Once);
        photonViewWrapperMock.Verify(p => p.RPC(branchManager.photonView, "RPC_UpdateBranchUI",
            RpcTarget.All, 5, 1, 3), Times.Once);
    }

    [Test]
    public void RPC_BuyBranch_ShouldNotCallBranchUseCase_WhenNotMasterClient()
    {
        photonNetworkMock.Setup(p => p.IsMasterClient).Returns(false);

        var method = typeof(PhotonBranchManager)
            .GetMethod("RPC_BuyBranch", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        method.Invoke(branchManager, new object[] { 5, 1 });

        branchUseCaseMock.Verify(b => b.BuyBranch(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
        photonViewWrapperMock.Verify(p => p.RPC(It.IsAny<PhotonView>(), It.IsAny<string>(), It.IsAny<RpcTarget>(), It.IsAny<object[]>()), Times.Never);
    }

    [Test]
    public void RPC_UpdateBranchUI_ShouldCallBranchUseCase_UpdateBranchUI()
    {
        var method = typeof(PhotonBranchManager)
            .GetMethod("RPC_UpdateBranchUI", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        method.Invoke(branchManager, new object[] { 5, 1, 3 });

        branchUseCaseMock.Verify(b => b.UpdateBranchUI(5, 1, 3), Times.Once);
    }
    [Test]
    public void RequestSellBranch_ShouldCallRPC_WithCorrectArguments()
    {
        branchManager.RequestSellBranch(10);

        photonViewWrapperMock.Verify(p => p.RPC(
            branchManager.photonView,
            "RPC_SellBranch",
            RpcTarget.MasterClient,
            10,
            PhotonNetwork.LocalPlayer.ActorNumber), Times.Once);
    }

    // ------------------ RPC_SellBranch ------------------
    [Test]
    public void RPC_SellBranch_ShouldCallBranchUseCase_AndUpdateUI_WhenMasterClient()
    {
        photonNetworkMock.Setup(p => p.IsMasterClient).Returns(true);
        branchUseCaseMock.Setup(b => b.SellBranch(5, 1)).Returns(2); // newRentLevel = 2

        var method = typeof(PhotonBranchManager)
            .GetMethod("RPC_SellBranch", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        method.Invoke(branchManager, new object[] { 5, 1 });

        branchUseCaseMock.Verify(b => b.SellBranch(5, 1), Times.Once);
        photonViewWrapperMock.Verify(p => p.RPC(branchManager.photonView, "RPC_UpdateBranchUI",
            RpcTarget.All, 5, 1, 2), Times.Once);
    }

    [Test]
    public void RPC_SellBranch_ShouldNotCallBranchUseCase_WhenNotMasterClient()
    {
        photonNetworkMock.Setup(p => p.IsMasterClient).Returns(false);

        var method = typeof(PhotonBranchManager)
            .GetMethod("RPC_SellBranch", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        method.Invoke(branchManager, new object[] { 5, 1 });

        branchUseCaseMock.Verify(b => b.SellBranch(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
        photonViewWrapperMock.Verify(p => p.RPC(It.IsAny<PhotonView>(), It.IsAny<string>(), It.IsAny<RpcTarget>(), It.IsAny<object[]>()), Times.Never);
    }

    // ------------------ Construct null arguments ------------------
    [Test]
    public void Construct_ShouldThrowArgumentNull_WhenBranchUseCaseIsNull()
    {
        var go = new GameObject();
        var manager = go.AddComponent<PhotonBranchManager>();

        Assert.Throws<ArgumentNullException>(() => manager.Construct(null, photonNetworkMock.Object, photonViewWrapperMock.Object));
    }

    [Test]
    public void Construct_ShouldThrowArgumentNull_WhenPhotonNetworkWrapperIsNull()
    {
        var go = new GameObject();
        var manager = go.AddComponent<PhotonBranchManager>();

        Assert.Throws<ArgumentNullException>(() => manager.Construct(branchUseCaseMock.Object, null, photonViewWrapperMock.Object));
    }

    [Test]
    public void Construct_ShouldThrowArgumentNull_WhenPhotonViewWrapperIsNull()
    {
        var go = new GameObject();
        var manager = go.AddComponent<PhotonBranchManager>();

        Assert.Throws<ArgumentNullException>(() => manager.Construct(branchUseCaseMock.Object, photonNetworkMock.Object, null));
    }

    [Test]
    public void Construct_ShouldThrowNullReference_WhenPhotonViewIsNull()
    {
        var go = new GameObject();
        var manager = go.AddComponent<PhotonBranchManager>();
        GameObject.DestroyImmediate(manager.GetComponent<PhotonView>()); // удаляем PhotonView

        Assert.Throws<NullReferenceException>(() => manager.Construct(branchUseCaseMock.Object, photonNetworkMock.Object, photonViewWrapperMock.Object));
    }
}
