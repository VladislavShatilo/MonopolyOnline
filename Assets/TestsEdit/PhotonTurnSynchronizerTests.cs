using NUnit.Framework;
using Moq;
using UnityEngine;
using Photon.Pun;

[TestFixture]
public class PhotonTurnSynchronizerTests
{
    private PhotonTurnSynchronizer manager;
    private Mock<IPlayerRepository> playerRepoMock;
    private Mock<IEventBus> eventBusMock;
    private Mock<IMortgageService> mortgageServiceMock;
    private Mock<IPhotonNetworkWrapper> photonNetworkMock;
    private Mock<IPhotonViewWrapper> photonViewWrapperMock;
    private GameObject go;

    [SetUp]
    public void Setup()
    {
        go = new GameObject();
        manager = go.AddComponent<PhotonTurnSynchronizer>();

        playerRepoMock = new Mock<IPlayerRepository>();
        eventBusMock = new Mock<IEventBus>();
        mortgageServiceMock = new Mock<IMortgageService>();
        photonNetworkMock = new Mock<IPhotonNetworkWrapper>();
        photonViewWrapperMock = new Mock<IPhotonViewWrapper>();

        manager.Construct(playerRepoMock.Object, eventBusMock.Object, mortgageServiceMock.Object,
            photonNetworkMock.Object, photonViewWrapperMock.Object);
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(go);
    }

    [Test]
    public void RequestStartTurn_ShouldCallRPC()
    {
        manager.RequestStartTurn(1, true);

        photonViewWrapperMock.Verify(p => p.RPC(manager.photonView, "RPC_StartTurn", RpcTarget.All, 1, true), Times.Once);
    }

    [Test]
    public void RPC_StartTurn_ShouldPublishJailEvent_WhenPlayerInJail()
    {
        var player = new PlayerData("p1", 500, 1, null);
        player.IsInJail = true;
        player.HasLoan = false;
       
        playerRepoMock.Setup(r => r.GetPlayerById(1)).Returns(player);

        var method = manager.GetType().GetMethod("RPC_StartTurn", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        method.Invoke(manager, new object[] { 1, false });

        eventBusMock.Verify(e => e.Publish(It.IsAny<StartTurnJailEvent>()), Times.Once);
        eventBusMock.Verify(e => e.Publish(It.IsAny<TurnStartEvent>()), Times.Never);
    }

    [Test]
    public void RPC_StartTurn_ShouldPublishTurnStartEvent_WhenPlayerNotInJail()
    {
        var player = new PlayerData("p1", 500, 2, null);
        player.IsInJail = false;
        player.HasLoan = false;

        playerRepoMock.Setup(r => r.GetPlayerById(2)).Returns(player);

        var method = manager.GetType().GetMethod("RPC_StartTurn", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        method.Invoke(manager, new object[] { 2, false });

        eventBusMock.Verify(e => e.Publish(It.IsAny<TurnStartEvent>()), Times.Once);
        eventBusMock.Verify(e => e.Publish(It.IsAny<StartTurnJailEvent>()), Times.Never);
    }

    [Test]
    public void RPC_StartTurn_ShouldPublishLoanEvent_WhenPlayerHasLoanAndIsNext()
    {
        var player = new PlayerData("p1", 500, 3, null);
        player.IsInJail = false;
        player.HasLoan = true;

        playerRepoMock.Setup(r => r.GetPlayerById(3)).Returns(player);

        var method = manager.GetType().GetMethod("RPC_StartTurn", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        method.Invoke(manager, new object[] { 3, true });

        eventBusMock.Verify(e => e.Publish(It.IsAny<OnStartTurnLoanEvent>()), Times.Once);
    }

    [Test]
    public void RPC_StartTurn_ShouldCallTickTurn_WhenMasterClient()
    {
        var player = new PlayerData("p1", 500, 4, null);
        player.IsInJail = false;
        player.HasLoan = false
            ;
        playerRepoMock.Setup(r => r.GetPlayerById(4)).Returns(player);
        photonNetworkMock.Setup(p => p.IsMasterClient).Returns(true);

        var method = manager.GetType().GetMethod("RPC_StartTurn", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        method.Invoke(manager, new object[] { 4, false });

        mortgageServiceMock.Verify(m => m.TickTurn(4), Times.Once);
    }
}
