using Moq;
using NUnit.Framework;
using Photon.Pun;
using System;
using System.Collections;
using System.Reflection;
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
        var photonView = go.AddComponent<PhotonView>(); // <--- вот это

        manager.Construct(eventBusMock.Object, companyRepoMock.Object, companyUIMock.Object, viewWrapperMock.Object);
    }

    #region LIFE_CYCLE TESTS

    [Test]
    public void OnEnable_ShouldSubscribeToEvents()
    {
        var method = typeof(PhotonMortgageSync).GetMethod("OnEnable", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        method.Invoke(manager, null);

        eventBusMock.Verify(e => e.Subscribe<CompanyMortgagedEvent>(It.IsAny<Action<CompanyMortgagedEvent>>()), Times.Once);
        eventBusMock.Verify(e => e.Subscribe<CompanyBoughtBackEvent>(It.IsAny<Action<CompanyBoughtBackEvent>>()), Times.Once);
        eventBusMock.Verify(e => e.Subscribe<CompanyFreedFromMortgageEvent>(It.IsAny<Action<CompanyFreedFromMortgageEvent>>()), Times.Once);
        eventBusMock.Verify(e => e.Subscribe<CompanyTickUIEvent>(It.IsAny<Action<CompanyTickUIEvent>>()), Times.Once);
    }

    [Test]
    public void OnDisable_ShouldUnsubscribeFromEvents()
    {
        var method = typeof(PhotonMortgageSync).GetMethod("OnDisable", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        method.Invoke(manager, null);

        eventBusMock.Verify(e => e.Unsubscribe<CompanyMortgagedEvent>(It.IsAny<Action<CompanyMortgagedEvent>>()), Times.Once);
        eventBusMock.Verify(e => e.Unsubscribe<CompanyBoughtBackEvent>(It.IsAny<Action<CompanyBoughtBackEvent>>()), Times.Once);
        eventBusMock.Verify(e => e.Unsubscribe<CompanyFreedFromMortgageEvent>(It.IsAny<Action<CompanyFreedFromMortgageEvent>>()), Times.Once);
        eventBusMock.Verify(e => e.Unsubscribe<CompanyTickUIEvent>(It.IsAny<Action<CompanyTickUIEvent>>()), Times.Once);
    }

    #endregion

    #region CALLBACKS TESTS

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

    #endregion

    #region RPC TESTS

    [Test]
    public void RPC_SetMortgage_ShouldUpdateCompany()
    {
        var company = new Company(1, new CompanyData());
        companyRepoMock.Setup(r => r.GetCompanyById(1)).Returns(company);

        var method = typeof(PhotonMortgageSync).GetMethod("RPC_SetMortgage", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        method.Invoke(manager, new object[] { 1, true, 3 });

        Assert.IsTrue(company.IsMortgaged);
        Assert.AreEqual(3, company.MortgageTurnsLeft);
    }

    [Test]
    public void RPC_SetMortgage_ShouldThrow_WhenCompanyNull()
    {
        companyRepoMock.Setup(r => r.GetCompanyById(1)).Returns((Company)null);
        var method = typeof(PhotonMortgageSync).GetMethod("RPC_SetMortgage", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        Assert.Throws<TargetInvocationException>(() => method.Invoke(manager, new object[] { 1, true, 3 }));
    }

    [Test]
    public void RPC_CompanyFreed_ShouldUpdateCompanyAndCallUI()
    {
        var company = new Company(1, new CompanyData()) { IsMortgaged = true, IsBought = true, OwnerId = 5 };
        var uiMock = new Mock<IUICompanyCellView>();
        companyRepoMock.Setup(r => r.GetCompanyById(1)).Returns(company);
        companyUIMock.Setup(u => u.GetCompanyUI(1)).Returns(uiMock.Object);

        var method = typeof(PhotonMortgageSync).GetMethod("RPC_CompanyFreed", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        method.Invoke(manager, new object[] { 1, false, 0 });

        Assert.IsFalse(company.IsMortgaged);
        Assert.IsFalse(company.IsBought);
        Assert.AreEqual(-1, company.OwnerId);
        uiMock.Verify(u => u.LoseCompanyUI(company), Times.Once);
    }

    [Test]
    public void RPC_CompanyFreed_ShouldThrow_WhenCompanyNull()
    {
        companyRepoMock.Setup(r => r.GetCompanyById(1)).Returns((Company)null);
        var method = typeof(PhotonMortgageSync).GetMethod("RPC_CompanyFreed", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        Assert.Throws<TargetInvocationException>(() => method.Invoke(manager, new object[] { 1, false, 0 }));
    }

    [Test]
    public void RPC_CompanyFreed_ShouldThrow_WhenUINull()
    {
        var company = new Company(1, new CompanyData());
        companyRepoMock.Setup(r => r.GetCompanyById(1)).Returns(company);
        companyUIMock.Setup(u => u.GetCompanyUI(1)).Returns((IUICompanyCellView)null);

        var method = typeof(PhotonMortgageSync).GetMethod("RPC_CompanyFreed", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        Assert.Throws<TargetInvocationException>(() => method.Invoke(manager, new object[] { 1, false, 0 }));
    }

    [Test]
    public void RPC_CompanyTickUI_ShouldCallUI()
    {
        var uiMock = new Mock<IUICompanyCellView>();
        companyUIMock.Setup(u => u.GetCompanyUI(4)).Returns(uiMock.Object);

        var method = typeof(PhotonMortgageSync).GetMethod("RPC_CompanyTickUI", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        method.Invoke(manager, new object[] { 4, 5 });

        uiMock.Verify(u => u.SetMortgageTurnsText(5), Times.Once);
    }

    [Test]
    public void RPC_CompanyTickUI_ShouldThrow_WhenUINull()
    {
        companyUIMock.Setup(u => u.GetCompanyUI(4)).Returns((IUICompanyCellView)null);

        var method = typeof(PhotonMortgageSync).GetMethod("RPC_CompanyTickUI", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        Assert.Throws<TargetInvocationException>(() => method.Invoke(manager, new object[] { 4, 5 }));
    }

    #endregion
}
