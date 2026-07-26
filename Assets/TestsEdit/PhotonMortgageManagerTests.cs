using NUnit.Framework;
using Moq;
using Photon.Pun;
using UnityEngine;
using Zenject;
using System;

[TestFixture]
public class PhotonMortgageManagerTests
{
    private PhotonMortgageManager manager;
    private Mock<IMortgageService> mortgageServiceMock;
    private Mock<ICompanyUIService> companyUIMock;
    private Mock<ILocalPlayerService> localPlayerMock;
    private Mock<IPhotonViewWrapper> viewWrapperMock;
    private Mock<IPhotonNetworkWrapper> networkWrapperMock;
    private GameSettings gameSettings;
    private PhotonView photonView;

    [SetUp]
    public void SetUp()
    {
        var go = new GameObject();
        manager = go.AddComponent<PhotonMortgageManager>();
        photonView = go.AddComponent<PhotonView>();

        mortgageServiceMock = new Mock<IMortgageService>();
        companyUIMock = new Mock<ICompanyUIService>();
        localPlayerMock = new Mock<ILocalPlayerService>();
        viewWrapperMock = new Mock<IPhotonViewWrapper>();
        networkWrapperMock = new Mock<IPhotonNetworkWrapper>();
        gameSettings = new GameSettings { mortgageTurns = 3 };

        // Внедрение моков через Reflection (или через отдельный метод Construct с сеттерами)
        manager.Construct(mortgageServiceMock.Object, companyUIMock.Object, gameSettings, localPlayerMock.Object, viewWrapperMock.Object);

        // Установка внутренних моков
        typeof(PhotonMortgageManager).GetField("photonViewWrapper", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(manager, viewWrapperMock.Object);
        typeof(PhotonMortgageManager).GetField("photonNetworkWrapper", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(manager, networkWrapperMock.Object);
    }

    #region Constructor Tests

    [Test]
    public void Construct_ShouldThrow_OnNullDependencies()
    {
        var go2 = new GameObject();
        var mgr = go2.AddComponent<PhotonMortgageManager>();
        Assert.Throws<ArgumentNullException>(() => mgr.Construct(null, companyUIMock.Object, gameSettings, localPlayerMock.Object, viewWrapperMock.Object));
        Assert.Throws<ArgumentNullException>(() => mgr.Construct(mortgageServiceMock.Object, null, gameSettings, localPlayerMock.Object, viewWrapperMock.Object));
        Assert.Throws<ArgumentNullException>(() => mgr.Construct(mortgageServiceMock.Object, companyUIMock.Object, null, localPlayerMock.Object, viewWrapperMock.Object));
        Assert.Throws<ArgumentNullException>(() => mgr.Construct(mortgageServiceMock.Object, companyUIMock.Object, gameSettings, null, viewWrapperMock.Object));
        Assert.Throws<ArgumentNullException>(() => mgr.Construct(mortgageServiceMock.Object, companyUIMock.Object, gameSettings, localPlayerMock.Object, null));

    }

    [Test]
    public void Construct_ShouldThrowNullReference_WhenPhotonViewMissing()
    {
        var go2 = new GameObject();
        var mgr = go2.AddComponent<PhotonMortgageManager>();
        Assert.Throws<NullReferenceException>(() => mgr.Construct(mortgageServiceMock.Object, companyUIMock.Object, gameSettings, localPlayerMock.Object, viewWrapperMock.Object));
    }

    #endregion

    #region Public Methods

    [Test]
    public void RequestMortgageCompany_ShouldCallRPC()
    {
        localPlayerMock.Setup(l => l.GetLocalPlayerId()).Returns(1);
        manager.RequestMortgageCompany(5);

        viewWrapperMock.Verify(v => v.RPC(manager.photonView,"RPC_MortgageCompany", RpcTarget.MasterClient, 5, 1), Times.Once);
    }

    [Test]
    public void RequestBuyoutCompany_ShouldCallRPC()
    {
        localPlayerMock.Setup(l => l.GetLocalPlayerId()).Returns(2);
        manager.RequestBuyoutCompany(10);

        viewWrapperMock.Verify(v => v.RPC(manager.photonView,"RPC_BuyBackCompany", RpcTarget.MasterClient, 2, 10), Times.Once);
    }

    #endregion

    #region RPC Tests

    [Test]
    public void RPC_MortgageCompany_ShouldCallService_AndSync_WhenMasterClient()
    {
        networkWrapperMock.Setup(n => n.IsMasterClient).Returns(true);
        var method = typeof(PhotonMortgageManager).GetMethod("RPC_MortgageCompany", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        method.Invoke(manager, new object[] { 5, 1 });

        mortgageServiceMock.Verify(m => m.MortgageCompany(5, 1), Times.Once);
        viewWrapperMock.Verify(v => v.RPC(manager.photonView,"RPC_SyncMortgage", RpcTarget.All, 1, 5, true), Times.Once);
    }

    [Test]
    public void RPC_MortgageCompany_ShouldNotCallService_WhenNotMasterClient()
    {
        networkWrapperMock.Setup(n => n.IsMasterClient).Returns(false);
        var method = typeof(PhotonMortgageManager).GetMethod("RPC_MortgageCompany", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        method.Invoke(manager, new object[] { 5, 1 });

        mortgageServiceMock.Verify(m => m.MortgageCompany(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
        viewWrapperMock.Verify(v => v.RPC(It.IsAny<PhotonView>(), It.IsAny<string>(), It.IsAny<RpcTarget>(), It.IsAny<object[]>()), Times.Never);
    }

    [Test]
    public void RPC_BuyBackCompany_ShouldCallService_AndSync_WhenMasterClient()
    {
        networkWrapperMock.Setup(n => n.IsMasterClient).Returns(true);
        var method = typeof(PhotonMortgageManager).GetMethod("RPC_BuyBackCompany", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        method.Invoke(manager, new object[] { 1, 5 });

        mortgageServiceMock.Verify(m => m.BuyoutCompany(5, 1), Times.Once);
        viewWrapperMock.Verify(v => v.RPC(manager.photonView, "RPC_SyncMortgage", RpcTarget.All, 1, 5, false), Times.Once);
    }

    [Test]
    public void RPC_BuyBackCompany_ShouldNotCallService_WhenNotMasterClient()
    {
        networkWrapperMock.Setup(n => n.IsMasterClient).Returns(false);
        var method = typeof(PhotonMortgageManager).GetMethod("RPC_BuyBackCompany", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        method.Invoke(manager, new object[] { 1, 5 });

        mortgageServiceMock.Verify(m => m.BuyoutCompany(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
        viewWrapperMock.Verify(v => v.RPC(It.IsAny<PhotonView>(), It.IsAny<string>(), It.IsAny<RpcTarget>(), It.IsAny<object[]>()), Times.Never);
    }

    [Test]
    public void RPC_SyncMortgage_ShouldUpdateUI_WhenMortgageTrue()
    {
        var uiMock = new Mock<IUICompanyCellView>();
        companyUIMock.Setup(c => c.GetCompanyUI(5)).Returns(uiMock.Object);

        var method = typeof(PhotonMortgageManager).GetMethod("RPC_SyncMortgage", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        method.Invoke(manager, new object[] { 1, 5, true });

        uiMock.Verify(u => u.SetMortgageTurnsText(gameSettings.mortgageTurns), Times.Once);
        uiMock.Verify(u => u.MortgageUI(), Times.Once);
    }

    [Test]
    public void RPC_SyncMortgage_ShouldUpdateUI_WhenMortgageFalse()
    {
        var uiMock = new Mock<IUICompanyCellView>();
        companyUIMock.Setup(c => c.GetCompanyUI(5)).Returns(uiMock.Object);

        var method = typeof(PhotonMortgageManager).GetMethod("RPC_SyncMortgage", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        method.Invoke(manager, new object[] { 1, 5, false });

        uiMock.Verify(u => u.BuyoutUI(), Times.Once);
    }

    [Test]
    public void RPC_SyncMortgage_ShouldThrow_WhenUIIsNull()
    {
        companyUIMock.Setup(c => c.GetCompanyUI(5)).Returns((IUICompanyCellView)null);

        var method = typeof(PhotonMortgageManager).GetMethod("RPC_SyncMortgage", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        Assert.Throws<System.Reflection.TargetInvocationException>(() => method.Invoke(manager, new object[] { 1, 5, true }));
    }

    #endregion
}
