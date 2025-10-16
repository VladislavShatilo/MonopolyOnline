using NUnit.Framework;
using Moq;
using Photon.Pun;
using UnityEngine;

public class PhotonChanceManagerTests
{
    private PhotonChanceManager chanceManager;
    private Mock<IChanceService> chanceServiceMock;
    private Mock<IPlayerRepository> playerRepositoryMock;
    private Mock<IBankService> bankServiceMock;
    private Mock<IPhotonTurnManager> turnManagerMock;
    private Mock<IPhotonPlayerMoveManager> playerMoveMock;
    private Mock<IPhotonJailManager> jailManagerMock;
    private Mock<IChatService> chatServiceMock;
    private Mock<IPhotonNetworkWrapper> networkMock;
    private Mock<IPhotonViewWrapper> viewWrapperMock;
    private PhotonView photonView;

    [SetUp]
    public void SetUp()
    {
        var go = new GameObject();
        chanceManager = go.AddComponent<PhotonChanceManager>();
        photonView = go.AddComponent<PhotonView>();

        chanceServiceMock = new Mock<IChanceService>();
        playerRepositoryMock = new Mock<IPlayerRepository>();
        bankServiceMock = new Mock<IBankService>();
        turnManagerMock = new Mock<IPhotonTurnManager>();
        playerMoveMock = new Mock<IPhotonPlayerMoveManager>();
        jailManagerMock = new Mock<IPhotonJailManager>();
        chatServiceMock = new Mock<IChatService>();
        networkMock = new Mock<IPhotonNetworkWrapper>();
        viewWrapperMock = new Mock<IPhotonViewWrapper>();

        chanceManager.Construct(
            chanceServiceMock.Object,
            playerRepositoryMock.Object,
            bankServiceMock.Object,
            turnManagerMock.Object,
            playerMoveMock.Object,
            jailManagerMock.Object,
            chatServiceMock.Object,
            networkMock.Object,
            viewWrapperMock.Object);
    }

    [Test]
    public void GiveRandomBuff_ShouldNotDoAnything_WhenNotMasterClient()
    {
        networkMock.Setup(n => n.IsMasterClient).Returns(false);

        chanceManager.GiveRandomBuff(1);

        viewWrapperMock.Verify(v => v.RPC(It.IsAny<PhotonView>(), It.IsAny<string>(), It.IsAny<RpcTarget>(), It.IsAny<object[]>()), Times.Never);
        turnManagerMock.Verify(t => t.RequestEndTurn(), Times.Never);
    }

    [Test]
    public void GiveRandomBuff_ShouldCallRPC_AndEndTurn_ForMoneyBuff()
    {
        networkMock.Setup(n => n.IsMasterClient).Returns(true);
        var buff = new ChanceBuff(BuffType.MoneyGainRandom1,100,200);

        chanceServiceMock.Setup(c => c.GetRandomBuff()).Returns(buff);

        chanceManager.GiveRandomBuff(1);

        viewWrapperMock.Verify(v => v.RPC(
            photonView,
            "RPC_ApplyBuff",
            RpcTarget.All,
            1,
            (int)buff.Type,
            buff.MinAmount,
            buff.MaxAmount), Times.Once);

        turnManagerMock.Verify(t => t.RequestEndTurn(), Times.Once);
    }

    [Test]
    public void RPC_ApplyBuff_ShouldAddMoney_ForMoneyGainBuff()
    {
        networkMock.Setup(n => n.IsMasterClient).Returns(true); // <--- обязательно

        var player = new PlayerData("p1", 500, 1, null);

        playerRepositoryMock.Setup(r => r.GetPlayerById(1)).Returns(player);

        var method = typeof(PhotonChanceManager)
            .GetMethod("RPC_ApplyBuff", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        method.Invoke(chanceManager, new object[] { 1, (int)BuffType.MoneyGainRandom1, 100, 200 });

        bankServiceMock.Verify(b => b.AddMoney(1, It.IsInRange(100, 200, Moq.Range.Inclusive)), Times.Once);
        chatServiceMock.Verify(c => c.SendMessage(1, It.Is<string>(s => s.Contains("получил")), false), Times.Once);
    }

    [Test]
    public void RPC_ApplyBuff_ShouldRemoveMoney_ForMoneyLoseBuff()
    {
        networkMock.Setup(n => n.IsMasterClient).Returns(true); // <--- обязательно

        var player = new PlayerData("p1", 500, 1, null);

        playerRepositoryMock.Setup(r => r.GetPlayerById(1)).Returns(player);

        var method = typeof(PhotonChanceManager)
            .GetMethod("RPC_ApplyBuff", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        method.Invoke(chanceManager, new object[] { 1, (int)BuffType.MoneyLoseRandom1, 100, 200 });

        bankServiceMock.Verify(b => b.RemoveMoney(1, It.IsInRange(100, 200, Moq.Range.Inclusive)), Times.Once);
        chatServiceMock.Verify(c => c.SendMessage(1, It.Is<string>(s => s.Contains("потерял")), false), Times.Once);
    }

    [Test]
    public void RPC_ApplyBuff_ShouldCallTeleport_ForTeleportBuff()
    {
        var player = new PlayerData("p1", 500, 1, null);

        playerRepositoryMock.Setup(r => r.GetPlayerById(1)).Returns(player);

        var method = typeof(PhotonChanceManager)
            .GetMethod("RPC_ApplyBuff", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        method.Invoke(chanceManager, new object[] { 1, (int)BuffType.Teleport, 0, 0 });

        playerMoveMock.Verify(p => p.RequestTeleport(1), Times.Once);
        chatServiceMock.Verify(c => c.SendMessage(1, "телепортировался!", false), Times.Once);
    }

    [Test]
    public void RPC_ApplyBuff_ShouldSendToJail_ForJailBuff()
    {
        var player = new PlayerData("p1", 500, 1, null);

        playerRepositoryMock.Setup(r => r.GetPlayerById(1)).Returns(player);

        var method = typeof(PhotonChanceManager)
            .GetMethod("RPC_ApplyBuff", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        method.Invoke(chanceManager, new object[] { 1, (int)BuffType.Jail, 0, 0 });

        jailManagerMock.Verify(j => j.SendToJail(1), Times.Once);
        chatServiceMock.Verify(c => c.SendMessage(1, "попал в тюрьму!", false), Times.Once);
    }
    [Test]
    public void GiveRandomBuff_ShouldDoNothing_WhenBuffIsNull()
    {
        networkMock.Setup(n => n.IsMasterClient).Returns(true);
        chanceServiceMock.Setup(c => c.GetRandomBuff()).Returns((ChanceBuff)null);

        chanceManager.GiveRandomBuff(1);

        viewWrapperMock.Verify(v => v.RPC(It.IsAny<PhotonView>(), It.IsAny<string>(), It.IsAny<RpcTarget>(), It.IsAny<object[]>()), Times.Never);
        turnManagerMock.Verify(t => t.RequestEndTurn(), Times.Never);
    }

    [Test]
    public void RPC_ApplyBuff_ShouldHandleMoneyGainFixed()
    {
        networkMock.Setup(n => n.IsMasterClient).Returns(true);
        var player = new PlayerData("p1", 100, 1, null);
        playerRepositoryMock.Setup(r => r.GetPlayerById(1)).Returns(player);

        var method = typeof(PhotonChanceManager)
            .GetMethod("RPC_ApplyBuff", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        method.Invoke(chanceManager, new object[] { 1, (int)BuffType.MoneyGainFixed, 0, 500 });

        bankServiceMock.Verify(b => b.AddMoney(1, 500), Times.Once);
        chatServiceMock.Verify(c => c.SendMessage(1, It.Is<string>(s => s.Contains("получил")), false), Times.Once);
    }

    [Test]
    public void RPC_ApplyBuff_ShouldHandleMoneyLoseFixed()
    {
        networkMock.Setup(n => n.IsMasterClient).Returns(true);
        var player = new PlayerData("p1", 1000, 1, null);
        playerRepositoryMock.Setup(r => r.GetPlayerById(1)).Returns(player);

        var method = typeof(PhotonChanceManager)
            .GetMethod("RPC_ApplyBuff", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        method.Invoke(chanceManager, new object[] { 1, (int)BuffType.MoneyLoseFixed, 0, 500 });

        bankServiceMock.Verify(b => b.RemoveMoney(1, 500), Times.Once);
        chatServiceMock.Verify(c => c.SendMessage(1, It.Is<string>(s => s.Contains("потерял")), false), Times.Once);
    }

    [Test]
    public void RPC_ApplyBuff_ShouldSetSkipTurn()
    {
        var player = new PlayerData("p1", 500, 1, null);
        playerRepositoryMock.Setup(r => r.GetPlayerById(1)).Returns(player);

        var method = typeof(PhotonChanceManager)
            .GetMethod("RPC_ApplyBuff", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        method.Invoke(chanceManager, new object[] { 1, (int)BuffType.SkipTurn, 0, 0 });

        Assert.IsTrue(player.SkipNextTurn);
        chatServiceMock.Verify(c => c.SendMessage(1, "пропускает ход!", false), Times.Once);
    }

    [Test]
    public void RPC_ApplyBuff_ShouldSetReverseMove()
    {
        var player = new PlayerData("p1", 500, 1, null);
        playerRepositoryMock.Setup(r => r.GetPlayerById(1)).Returns(player);

        var method = typeof(PhotonChanceManager)
            .GetMethod("RPC_ApplyBuff", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        method.Invoke(chanceManager, new object[] { 1, (int)BuffType.ReverseMove, 0, 0 });

        Assert.IsTrue(player.NextMoveBackward);
        chatServiceMock.Verify(c => c.SendMessage(1, "идёт в обратную сторону!", false), Times.Once);
    }
}
