using Moq;
using NUnit.Framework;
using Photon.Pun;
using System.Windows.Input;
using UnityEngine;

public class PhotonCompanySyncManagerTests
{
    private PhotonCompanySyncManager syncManager;
    private Mock<ICompanyRepository> companyRepoMock;
    private Mock<IPlayerRepository> playerRepoMock;
    private Mock<IEventBus> eventBusMock;
    private Mock<IPhotonViewWrapper> viewWrapperMock;
    private PhotonView photonView;

    [SetUp]
    public void SetUp()
    {
        var go = new GameObject();
        syncManager = go.AddComponent<PhotonCompanySyncManager>();
        photonView = go.AddComponent<PhotonView>();

        companyRepoMock = new Mock<ICompanyRepository>();
        playerRepoMock = new Mock<IPlayerRepository>();
        eventBusMock = new Mock<IEventBus>();
        viewWrapperMock = new Mock<IPhotonViewWrapper>();

        syncManager.Construct(companyRepoMock.Object, playerRepoMock.Object, eventBusMock.Object, viewWrapperMock.Object);
    }

    [Test]
    public void SyncCompanyBought_ShouldCallRPC_WithCorrectParameters()
    {
        syncManager.SyncCompanyBought(5, 1, 1000);

        viewWrapperMock.Verify(v => v.RPC(
            syncManager.photonView,
            "RPC_SyncCompanyBought",
            RpcTarget.Others,
            5, 1, 1000), Times.Once);
    }

    [Test]
    public void SyncRentPaid_ShouldCallRPC_WithCorrectParameters()
    {
        syncManager.SyncRentPaid(3, 2, 1, 500);

        viewWrapperMock.Verify(v => v.RPC(
            syncManager.photonView,
             "RPC_SyncRentPaid",
            RpcTarget.Others,
            3, 2, 1, 500), Times.Once);
    }

    [Test]
    public void RPC_SyncCompanyBought_ShouldUpdateCompanyAndPlayer()
    {
        var company = new Company(5, new CompanyData());
        var player = new PlayerData ("p1",2000,1,null);


        companyRepoMock.Setup(c => c.GetCompanyById(5)).Returns(company);
        playerRepoMock.Setup(p => p.GetPlayerById(1)).Returns(player);

        var method = typeof(PhotonCompanySyncManager)
            .GetMethod("RPC_SyncCompanyBought", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        method.Invoke(syncManager, new object[] { 5, 1, 1000 });

        Assert.AreEqual(1, company.OwnerId);
        Assert.IsTrue(company.IsBought);
        Assert.AreEqual(1000, player.Money);
        eventBusMock.Verify(e => e.Publish(It.IsAny<CompanyBoughtEvent>()), Times.Once);
    }

    [Test]
    public void RPC_SyncRentPaid_ShouldUpdatePlayersMoneyAndPublishEvent()
    {
        var company = new Company(5, new CompanyData());
        var owner = new PlayerData("p1", 1000, 1, null);
        var renter = new PlayerData("p2", 500, 2, null);
    

        companyRepoMock.Setup(c => c.GetCompanyById(5)).Returns(company);
        playerRepoMock.Setup(p => p.GetPlayerById(1)).Returns(owner);
        playerRepoMock.Setup(p => p.GetPlayerById(2)).Returns(renter);

        var method = typeof(PhotonCompanySyncManager)
            .GetMethod("RPC_SyncRentPaid", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        method.Invoke(syncManager, new object[] { 5, 2, 1, 200 });

        Assert.AreEqual(1200, owner.Money);
        Assert.AreEqual(300, renter.Money);
        eventBusMock.Verify(e => e.Publish(It.IsAny<RentPaidEvent>()), Times.Once);
    }
}
