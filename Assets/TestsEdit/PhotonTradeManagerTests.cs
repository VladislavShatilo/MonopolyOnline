using NUnit.Framework;
using Moq;
using Photon.Pun;
using UnityEngine;
using System.Collections.Generic;

[TestFixture]
public class PhotonTradeManagerTests
{
    private PhotonTradeManager manager;
    private Mock<ITradeService> tradeServiceMock;
    private Mock<ICompanyRepository> companyRepoMock;
    private Mock<IPlayerRepository> playerRepoMock;
    private Mock<IPhotonViewWrapper> photonViewWrapperMock;
    private GameObject go;

    [SetUp]
    public void Setup()
    {
        go = new GameObject();
        manager = go.AddComponent<PhotonTradeManager>();

        tradeServiceMock = new Mock<ITradeService>();
        companyRepoMock = new Mock<ICompanyRepository>();
        playerRepoMock = new Mock<IPlayerRepository>();
        photonViewWrapperMock = new Mock<IPhotonViewWrapper>();

        manager.Construct(tradeServiceMock.Object, companyRepoMock.Object, playerRepoMock.Object, photonViewWrapperMock.Object);
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(go);
    }

    [Test]
    public void SendTradeRequest_ShouldCallRPC()
    {
        manager.SendTradeRequest(1, 2);

        photonViewWrapperMock.Verify(p => p.RPC(
            manager.photonView,
            "RPC_StartTradeRequest",
            RpcTarget.All,
            1, 2), Times.Once);
    }

    [Test]
    public void SendTradeOffer_ShouldSerializeCompaniesAndCallRPC()
    {
        var fromCompany = new Company(5, new CompanyData());

        var toCompany = new Company(10, new CompanyData());


        var fromPlayer = new PlayerData("p1",500,1,null);
        var toPlayer = new PlayerData("p2", 500, 2, null);

        playerRepoMock.Setup(r => r.GetPlayerById(1)).Returns(fromPlayer);
        playerRepoMock.Setup(r => r.GetPlayerById(2)).Returns(toPlayer);

        var offer = new TradeOffer(fromPlayer, toPlayer)
        {
            FromCompanies = new List<Company> { fromCompany },
            ToCompanies = new List<Company> { toCompany },
            FromMoney = 100,
            ToMoney = 200
        };

        manager.SendTradeOffer(offer);

        photonViewWrapperMock.Verify(p => p.RPC(
            manager.photonView,
            "RPC_SendTradeOffer",
            RpcTarget.All,
            1, 2,
            It.Is<string>(s => s.Contains("5")),
            It.Is<string>(s => s.Contains("10")),
            100, 200), Times.Once);
    }

    [Test]
    public void RPC_StartTradeRequest_ShouldCallTradeService()
    {
        var method = manager.GetType().GetMethod("RPC_StartTradeRequest", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        method.Invoke(manager, new object[] { 1, 2 });

        tradeServiceMock.Verify(t => t.StartTrade(1, 2), Times.Once);
    }

    [Test]
    public void RPC_CompleteTrade_ShouldCallTradeService()
    {
        var method = manager.GetType().GetMethod("RPC_CompleteTrade", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        method.Invoke(manager, new object[] { true });

        tradeServiceMock.Verify(t => t.OnTradeCompleted(true), Times.Once);
    }

    [Test]
    public void RPC_SendTradeOffer_ShouldDeserializeCompaniesAndCallTradeService()
    {

        var fromCompany = new Company(5,new CompanyData());

        var toCompany = new Company(10, new CompanyData());

        companyRepoMock.Setup(r => r.GetCompanyById(5)).Returns(fromCompany);
        companyRepoMock.Setup(r => r.GetCompanyById(10)).Returns(toCompany);

        var fromPlayer = new PlayerData("p1", 500, 1, null);
        var toPlayer = new PlayerData("p2", 500, 2, null);
        playerRepoMock.Setup(r => r.GetPlayerById(1)).Returns(fromPlayer);
        playerRepoMock.Setup(r => r.GetPlayerById(2)).Returns(toPlayer);

        string fromJson = "{\"Ids\":[5]}";
        string toJson = "{\"Ids\":[10]}";

        var method = manager.GetType().GetMethod("RPC_SendTradeOffer", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        method.Invoke(manager, new object[] { 1, 2, fromJson, toJson, 100, 200 });

        tradeServiceMock.Verify(t => t.OnTradeProposalReceived(It.Is<TradeOffer>(o =>
            o.FromPlayerData.Id == 1 &&
            o.ToPlayerData.Id == 2 &&
            o.FromCompanies[0].Id == 5 &&
            o.ToCompanies[0].Id == 10 &&
            o.FromMoney == 100 &&
            o.ToMoney == 200
        )), Times.Once);
    }
}
