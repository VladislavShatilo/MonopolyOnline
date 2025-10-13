using NUnit.Framework;
using Moq;
using Photon.Pun;
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
}
