using NUnit.Framework;
using Moq;
using Photon.Pun;
using UnityEngine;

public class PhotonCompanyManagerTests
{
    private PhotonCompanyManager companyManager;
    private Mock<ICompanyService> companyServiceMock;
    private Mock<IPhotonViewWrapper> viewWrapperMock;
    private PhotonView photonView;

    [SetUp]
    public void SetUp()
    {
        var go = new GameObject();
        companyManager = go.AddComponent<PhotonCompanyManager>();
        photonView = go.AddComponent<PhotonView>();

        companyServiceMock = new Mock<ICompanyService>();
        viewWrapperMock = new Mock<IPhotonViewWrapper>();

        companyManager.Construct(companyServiceMock.Object, viewWrapperMock.Object);
    }

    [Test]
    public void RequestBuyCompany_ShouldCallRPC_WithCorrectParameters()
    {
        companyManager.RequestBuyCompany(5, 1, BuyReason.Buy);

        viewWrapperMock.Verify(v => v.RPC(
            companyManager.photonView,
            "RPC_TryBuyCompany",
            RpcTarget.MasterClient,
            5, 1, (int)BuyReason.Buy), Times.Once);
    }

    [Test]
    public void RequestPayRent_ShouldCallRPC_WithCorrectParameters()
    {
        companyManager.RequestPayRent(3, 2);

        viewWrapperMock.Verify(v => v.RPC(
            companyManager.photonView,
            "RPC_TryPayRent",
            RpcTarget.MasterClient,
            3, 2), Times.Once);
    }

    [Test]
    public void RPC_TryBuyCompany_ShouldCallCompanyService()
    {
        var method = typeof(PhotonCompanyManager)
            .GetMethod("RPC_TryBuyCompany", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        method.Invoke(companyManager, new object[] { 7, 1, (int)BuyReason.Auction });

        companyServiceMock.Verify(c => c.TryBuyCompany(7, 1, 0, BuyReason.Auction), Times.Once);
    }

    [Test]
    public void RPC_TryPayRent_ShouldCallCompanyService()
    {
        var method = typeof(PhotonCompanyManager)
            .GetMethod("RPC_TryPayRent", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        method.Invoke(companyManager, new object[] { 4, 2 });

        companyServiceMock.Verify(c => c.TryPayRent(4, 2), Times.Once);
    }
}
