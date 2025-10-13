using Moq;
using NUnit.Framework;
using Photon.Pun;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;

public class PhotonMortgageSyncTests
{
    private PhotonMortgageSync manager;
    private Mock<IEventBus> eventBusMock;
    private Mock<ICompanyRepository> companyRepoMock;
    private Mock<ICompanyUIService> companyUIMock;
    private Mock<IPhotonViewWrapper> viewWrapperMock;

    [SetUp]
    public void Setup()
    {
        eventBusMock = new Mock<IEventBus>();
        companyRepoMock = new Mock<ICompanyRepository>();
        companyUIMock = new Mock<ICompanyUIService>();
        viewWrapperMock = new Mock<IPhotonViewWrapper>();

        var go = new GameObject();
        manager = go.AddComponent<PhotonMortgageSync>();
        manager.Construct(eventBusMock.Object, companyRepoMock.Object, companyUIMock.Object, viewWrapperMock.Object);
    }
    [Test]
    public void OnEnable_ShouldSubscribeToEvents()
    {
        var onEnable = typeof(PhotonMortgageSync).GetMethod("OnEnable", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        onEnable.Invoke(manager, null);

        eventBusMock.Verify(e => e.Subscribe<CompanyMortgagedEvent>(It.IsAny<System.Action<CompanyMortgagedEvent>>()), Times.Once);
        eventBusMock.Verify(e => e.Subscribe<CompanyBoughtBackEvent>(It.IsAny<System.Action<CompanyBoughtBackEvent>>()), Times.Once);
        eventBusMock.Verify(e => e.Subscribe<CompanyFreedFromMortgageEvent>(It.IsAny<System.Action<CompanyFreedFromMortgageEvent>>()), Times.Once);
        eventBusMock.Verify(e => e.Subscribe<CompanyTickUIEvent>(It.IsAny<System.Action<CompanyTickUIEvent>>()), Times.Once);
    }

    [Test]
    public void OnDisable_ShouldUnsubscribeFromEvents()
    {
        var onEnable = typeof(PhotonMortgageSync).GetMethod("OnDisable", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        onEnable.Invoke(manager, null);

        eventBusMock.Verify(e => e.Unsubscribe<CompanyMortgagedEvent>(It.IsAny<System.Action<CompanyMortgagedEvent>>()), Times.Once);
        eventBusMock.Verify(e => e.Unsubscribe<CompanyBoughtBackEvent>(It.IsAny<System.Action<CompanyBoughtBackEvent>>()), Times.Once);
        eventBusMock.Verify(e => e.Unsubscribe<CompanyFreedFromMortgageEvent>(It.IsAny<System.Action<CompanyFreedFromMortgageEvent>>()), Times.Once);
        eventBusMock.Verify(e => e.Unsubscribe<CompanyTickUIEvent>(It.IsAny<System.Action<CompanyTickUIEvent>>()), Times.Once);
    }
    [Test]
    public void OnCompanyMortgaged_ShouldCallRPC_SetMortgage()
    {
        var method = typeof(PhotonMortgageSync).GetMethod("OnCompanyMortgaged", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        method.Invoke(manager, new object[] { new CompanyMortgagedEvent(1, 1, 3) });

      
        viewWrapperMock.Verify(v => v.RPC(manager.photonView, "RPC_SetMortgage", RpcTarget.All, 1, true, 3), Times.Once);
    }

    [Test]
    public void OnCompanyBoughtBack_ShouldCallRPC_SetMortgage()
    {
        var method = typeof(PhotonMortgageSync).GetMethod("OnCompanyBoughtBack", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        method.Invoke(manager, new object[] { new CompanyBoughtBackEvent(1, 2) });

      

        viewWrapperMock.Verify(v => v.RPC(manager.photonView, "RPC_SetMortgage", RpcTarget.All, 2, false, 0), Times.Once);
    }

    [Test]
    public void OnCompanyFreedFromMortgage_ShouldCallRPC_CompanyFreed()
    {
        var method = typeof(PhotonMortgageSync).GetMethod("OnCompanyFreedFromMortgage", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        method.Invoke(manager, new object[] { new CompanyFreedFromMortgageEvent(3) });


        viewWrapperMock.Verify(v => v.RPC(manager.photonView, "RPC_CompanyFreed", RpcTarget.All, 3, false, 0), Times.Once);
    }

    [Test]
    public void OnCompanyTickUI_ShouldCallRPC_CompanyTickUI()
    {
        var method = typeof(PhotonMortgageSync).GetMethod("OnCompanyTickUI", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        method.Invoke(manager, new object[] { new CompanyTickUIEvent(4, 5) });


        viewWrapperMock.Verify(v => v.RPC(manager.photonView, "RPC_CompanyTickUI", RpcTarget.All, 4, 5), Times.Once);
    }
    [Test]
    public void RPC_SetMortgage_ShouldUpdateCompany()
    {
        // Arrange
        var company = new Company(1,new CompanyData()); // если есть публичный конструктор
        companyRepoMock.Setup(r => r.GetCompanyById(1)).Returns(company);

        // Act
        var method = typeof(PhotonMortgageSync).GetMethod(
            "RPC_SetMortgage",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance
        );

        method.Invoke(manager, new object[] { 1, true, 3 });

        // Assert
        Assert.IsTrue(company.IsMortgaged);
        Assert.AreEqual(3, company.MortgageTurnsLeft);
    }
}
