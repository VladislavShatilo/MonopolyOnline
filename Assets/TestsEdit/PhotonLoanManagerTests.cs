using Moq;
using NUnit.Framework;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections.Generic;
using UnityEngine;

public class PhotonLoanManagerTests
{
    private PhotonLoanManager loanManager;
    private Mock<ILoanService> loanServiceMock;
    private Mock<IEventBus> eventBusMock;
    private Mock<IPhotonViewWrapper> viewWrapperMock;

    private PhotonView photonView;
    private Mock<Room> roomMock;

    [SetUp]
    public void SetUp()
    {
        var go = new GameObject();
        loanManager = go.AddComponent<PhotonLoanManager>();
        photonView = go.AddComponent<PhotonView>();

        loanServiceMock = new Mock<ILoanService>();
        eventBusMock = new Mock<IEventBus>();
        viewWrapperMock = new Mock<IPhotonViewWrapper>();

        loanManager.Construct(loanServiceMock.Object, eventBusMock.Object, viewWrapperMock.Object);

   
    }

    [Test]
    public void TakeLoanRequest_ShouldCallRPC()
    {
        loanManager.TakeLoanRequest(1);

        viewWrapperMock.Verify(v => v.RPC(loanManager.photonView,
            "RPC_TakeLoan",
            RpcTarget.All,
            1), Times.Once);
    }

    [Test]
    public void PayLoanRequest_ShouldCallRPC()
    {
        loanManager.PayLoanRequest(2);

        viewWrapperMock.Verify(v => v.RPC(loanManager.photonView,
            "RPC_PayLoan",
            RpcTarget.All,
            2), Times.Once);
    }

    [Test]
    public void ShowLoanWindow_ShouldCallRPC()
    {
        var players = new List<PlayerData>
        {
            new PlayerData("A", 500, 1, null),
            new PlayerData("B", 500, 2, null),
            new PlayerData("C", 500, 3, null)

        };

        loanManager.ShowLoanWindow(3, 500);

        viewWrapperMock.Verify(v => v.RPC(loanManager.photonView,
            "RPC_ShowLoanWindow",
            3,
            3, 500), Times.Once);
       
    }

    [Test]
    public void RPC_TakeLoan_ShouldCallLoanService()
    {
        var method = typeof(PhotonLoanManager).GetMethod("RPC_TakeLoan",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        method.Invoke(loanManager, new object[] { 1 });

        loanServiceMock.Verify(s => s.TakeLoanConfirmed(1), Times.Once);
    }

    [Test]
    public void RPC_PayLoan_ShouldCallLoanService()
    {
        var method = typeof(PhotonLoanManager).GetMethod("RPC_PayLoan",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        method.Invoke(loanManager, new object[] { 2 });

        loanServiceMock.Verify(s => s.PayLoanConfirmed(2), Times.Once);
    }

    [Test]
    public void RPC_ShowLoanWindow_ShouldPublishEvent()
    {
        var method = typeof(PhotonLoanManager).GetMethod("RPC_ShowLoanWindow",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        method.Invoke(loanManager, new object[] { 3, 500 });

        eventBusMock.Verify(e => e.Publish(It.Is<OfferLoanPayEvent>(ev => ev.PlayerId == 3 && ev.LoanAmount == 500)), Times.Once);
    }
    [Test]
    public void Construct_ShouldThrowArgumentNullException_WhenLoanServiceIsNull()
    {
        Assert.Throws<ArgumentNullException>(() =>
            loanManager.Construct(null, eventBusMock.Object, viewWrapperMock.Object));
    }

    [Test]
    public void Construct_ShouldThrowArgumentNullException_WhenEventBusIsNull()
    {
        Assert.Throws<ArgumentNullException>(() =>
            loanManager.Construct(loanServiceMock.Object, null, viewWrapperMock.Object));
    }

    [Test]
    public void Construct_ShouldThrowArgumentNullException_WhenViewWrapperIsNull()
    {
        Assert.Throws<ArgumentNullException>(() =>
            loanManager.Construct(loanServiceMock.Object, eventBusMock.Object, null));
    }

    [Test]
    public void Construct_ShouldThrowNullReferenceException_WhenPhotonViewIsMissing()
    {
        var goWithoutView = new GameObject();
        var managerWithoutView = goWithoutView.AddComponent<PhotonLoanManager>();

        Assert.Throws<NullReferenceException>(() =>
            managerWithoutView.Construct(loanServiceMock.Object, eventBusMock.Object, viewWrapperMock.Object));

        GameObject.DestroyImmediate(goWithoutView);
    }
}
