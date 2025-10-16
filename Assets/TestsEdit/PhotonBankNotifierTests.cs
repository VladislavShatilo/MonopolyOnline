using Moq;
using NUnit.Framework;
using Photon.Pun;
using System;
using System.Reflection;
using UnityEngine;

public class PhotonBankNotifierTests
{
    private PhotonBankNotifier bankNotifier;
    private Mock<IPlayerRepository> playerRepositoryMock;
    private Mock<IPhotonViewWrapper> photonViewWrapperMock;
    private Mock<IEventBus> eventBusMock;
    private PhotonView photonView;

    [SetUp]
    public void SetUp()
    {
        var go = new GameObject();
        bankNotifier = go.AddComponent<PhotonBankNotifier>();
        photonView = go.AddComponent<PhotonView>();

        playerRepositoryMock = new Mock<IPlayerRepository>();
        photonViewWrapperMock = new Mock<IPhotonViewWrapper>();
        eventBusMock = new Mock<IEventBus>();

        bankNotifier.Construct(playerRepositoryMock.Object, eventBusMock.Object, photonViewWrapperMock.Object);
    }

    [Test]
    public void NotifyBalanceChanged_ShouldCallRPC_WithCorrectArguments()
    {
        var player = new PlayerData("p1",500,1,null);



        bankNotifier.NotifyBalanceChanged(player);

        photonViewWrapperMock.Verify(p => p.RPC(
            photonView,
            "RPC_UpdateMoney",
            RpcTarget.All,
            player.Id,
            player.Money), Times.Once);
    }

    [Test]
    public void RPC_UpdateMoney_ShouldUpdatePlayerMoney_AndPublishEvent()
    {
        var player = new PlayerData("p1", 100, 1, null);

        playerRepositoryMock.Setup(r => r.GetPlayerById(player.Id)).Returns(player);

        // Вызываем приватный метод через Reflection
        var method = typeof(PhotonBankNotifier)
            .GetMethod("RPC_UpdateMoney", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        method.Invoke(bankNotifier, new object[] { player.Id, 500 });

        Assert.AreEqual(500, player.Money, "Player money should be updated");

        eventBusMock.Verify(e => e.Publish(It.Is<OnUpdatePlayerMoneyEvent>(evt => evt.Player == player)), Times.Once);
    }

    #region Construct Tests

    [Test]
    public void Construct_ShouldThrow_WhenPlayerRepositoryNull()
    {
        Assert.Throws<ArgumentNullException>(() =>
            bankNotifier.Construct(null, eventBusMock.Object, photonViewWrapperMock.Object));
    }

    [Test]
    public void Construct_ShouldThrow_WhenEventBusNull()
    {
        Assert.Throws<ArgumentNullException>(() =>
            bankNotifier.Construct(playerRepositoryMock.Object, null, photonViewWrapperMock.Object));
    }

    [Test]
    public void Construct_ShouldThrow_WhenPhotonViewWrapperNull()
    {
        Assert.Throws<ArgumentNullException>(() =>
            bankNotifier.Construct(playerRepositoryMock.Object, eventBusMock.Object, null));
    }

    [Test]
    public void Construct_ShouldThrow_WhenPhotonViewNull()
    {
        var go = new GameObject();
        var notifier = go.AddComponent<PhotonBankNotifier>();
        // удаляем PhotonView
        GameObject.DestroyImmediate(go.GetComponent<PhotonView>());
        Assert.Throws<NullReferenceException>(() =>
            notifier.Construct(playerRepositoryMock.Object, eventBusMock.Object, photonViewWrapperMock.Object));
    }

    #endregion

  
    #region RPC_UpdateMoney Tests


    [Test]
    public void RPC_UpdateMoney_ShouldThrow_WhenPlayerNotFound()
    {
        bankNotifier.Construct(playerRepositoryMock.Object, eventBusMock.Object, photonViewWrapperMock.Object);

        playerRepositoryMock.Setup(r => r.GetPlayerById(It.IsAny<int>())).Returns((PlayerData)null);

        var method = typeof(PhotonBankNotifier)
            .GetMethod("RPC_UpdateMoney", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        Assert.Throws<TargetInvocationException>(() => method.Invoke(bankNotifier, new object[] { 1, 500 }));
    }

    #endregion
}
