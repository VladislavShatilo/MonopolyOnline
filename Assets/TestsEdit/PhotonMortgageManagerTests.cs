using NUnit.Framework;
using Moq;
using Photon.Pun;
using UnityEngine;

public class PhotonMortgageManagerTests
{
    private PhotonMortgageManager manager;
    private Mock<IMortgageService> mortgageServiceMock;
    private Mock<ICompanyUIService> companyUIServiceMock;
    private Mock<IPhotonNetworkWrapper> photonNetworkMock;
    private Mock<IPhotonViewWrapper> viewWrapperMock;
    private GameSettings gameSettings;

    private GameObject go;

    [SetUp]
    public void SetUp()
    {
        go = new GameObject();
        manager = go.AddComponent<PhotonMortgageManager>();

        mortgageServiceMock = new Mock<IMortgageService>();
        companyUIServiceMock = new Mock<ICompanyUIService>();
        photonNetworkMock = new Mock<IPhotonNetworkWrapper>();
        viewWrapperMock = new Mock<IPhotonViewWrapper>();

        gameSettings = ScriptableObject.CreateInstance<GameSettings>();
        gameSettings.mortgageTurns = 3;

        // Вызов конструктора вручную
        manager.Construct(mortgageServiceMock.Object, companyUIServiceMock.Object, gameSettings);

        // Прокидываем мок viewWrapper и photonNetwork
        typeof(PhotonMortgageManager)
            .GetField("photonViewWrapper", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .SetValue(manager, viewWrapperMock.Object);

        typeof(PhotonMortgageManager)
            .GetField("photonNetworkWrapper", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .SetValue(manager, photonNetworkMock.Object);
    }

    [Test]
    public void RequestMortgageCompany_ShouldCallRPC()
    {
        photonNetworkMock.Setup(p => p.IsMasterClient).Returns(true);

        manager.RequestMortgageCompany(5);

        viewWrapperMock.Verify(v => v.RPC(manager.photonView, "RPC_MortgageCompany", RpcTarget.MasterClient, 5, -1), Times.Once);
    }

    [Test]
    public void RPC_MortgageCompany_ShouldCallServiceAndSync()
    {
        photonNetworkMock.Setup(p => p.IsMasterClient).Returns(true);

        var method = typeof(PhotonMortgageManager).GetMethod("RPC_MortgageCompany", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        method.Invoke(manager, new object[] { 5, 1 });

        mortgageServiceMock.Verify(m => m.MortgageCompany(5, 1), Times.Once);
        viewWrapperMock.Verify(v => v.RPC(manager.photonView, "RPC_SyncMortgage", RpcTarget.All, 1, 5, true), Times.Once);
    }

    [Test]
    public void RPC_SyncMortgage_ShouldCallUI_ForMortgage()
    {
        var uiMock = new Mock<IUICompanyCellView>();
        companyUIServiceMock.Setup(s => s.GetCompanyUI(5)).Returns(uiMock.Object);

        var method = typeof(PhotonMortgageManager).GetMethod("RPC_SyncMortgage", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        method.Invoke(manager, new object[] { 1, 5, true });

        uiMock.Verify(ui => ui.SetMortgageTurnsText(3), Times.Once);
        uiMock.Verify(ui => ui.MortgageUI(), Times.Once);
    }

    [Test]
    public void RPC_SyncMortgage_ShouldCallUI_ForBuyout()
    {
        var uiMock = new Mock<IUICompanyCellView>();
        companyUIServiceMock.Setup(s => s.GetCompanyUI(5)).Returns(uiMock.Object);

        var method = typeof(PhotonMortgageManager).GetMethod("RPC_SyncMortgage", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        method.Invoke(manager, new object[] { 1, 5, false });

        uiMock.Verify(ui => ui.BuyoutUI(), Times.Once);
    }

    [Test]
    public void RPC_SyncMortgage_ShouldDoNothing_WhenUINull()
    {
        companyUIServiceMock.Setup(s => s.GetCompanyUI(5)).Returns((IUICompanyCellView)null);

        var method = typeof(PhotonMortgageManager).GetMethod("RPC_SyncMortgage", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        method.Invoke(manager, new object[] { 1, 5, true });

        // Проверяем, что ничего не вызвалось
        mortgageServiceMock.VerifyNoOtherCalls();
    }
}
