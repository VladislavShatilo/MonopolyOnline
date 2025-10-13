using NUnit.Framework;
using Moq;
using Photon.Pun;
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
}
