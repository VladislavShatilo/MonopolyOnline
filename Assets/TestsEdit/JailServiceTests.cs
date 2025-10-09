using Moq;
using NUnit.Framework;
using Photon.Realtime;
using UnityEngine;

public class JailServiceTests
{
    private JailService jailService;
    private Mock<IPlayerRepository> playerRepoMock;
    private Mock<IEventBus> eventBusMock;
    private Mock<IPhotonTurnManager> turnManagerMock;
    private Mock<IPhotonNetworkWrapper> photonWrapperMock;
    private GameSettings gameSettings;
    private Mock<PlayerData> playerMock;

    [SetUp]
    public void Setup()
    {
        playerRepoMock = new Mock<IPlayerRepository>();
        eventBusMock = new Mock<IEventBus>();
        turnManagerMock = new Mock<IPhotonTurnManager>();
        photonWrapperMock = new Mock<IPhotonNetworkWrapper>();


        // Вместо мокирования PlayerData (MonoBehaviour/ScriptableObject), создаём обычный объект
        var player = new PlayerData("P1",500,1,null)
        {
            CurrentCellId = 0,
            JailTurnsLeft = 2,
            IsInJail = true
        };
        // Для методов SendToJail и Release можно сделать виртуальные методы и наследовать, либо сделать TestPlayerData
        var playerMock = new Mock<PlayerData>();
        playerMock.SetupProperty(p => p.CurrentCellId, 0);
        playerMock.SetupProperty(p => p.JailTurnsLeft, 2);
        playerMock.Setup(p => p.IsInJail).Returns(true);
        playerMock.Setup(p => p.SendToJail(It.IsAny<GameSettings>()));
        playerMock.Setup(p => p.Release());

        playerRepoMock.Setup(r => r.GetPlayerById(It.IsAny<int>())).Returns(playerMock.Object);

        jailService = new JailService();
        jailService.Construct(playerRepoMock.Object, eventBusMock.Object, turnManagerMock.Object, gameSettings, photonWrapperMock.Object);
    }
    [Test]
    public void SendPlayerToJail_ShouldSetCell_CallSendToJail_AndPublishEvent()
    {
        jailService.SendPlayerToJail(1);

        Assert.AreEqual(10, playerMock.Object.CurrentCellId);
        playerMock.Verify(p => p.SendToJail(gameSettings), Times.Once);
        eventBusMock.Verify(e => e.Publish(It.Is<SetTurnsJailEvent>(ev => ev.PlayerID == 1 && ev.Turns == playerMock.Object.JailTurnsLeft)), Times.Once);
    }

    [Test]
    public void ReleasePlayer_ShouldCallRelease_AndPublishEventWithZeroTurns()
    {
        jailService.ReleasePlayer(1, false);

        playerMock.Verify(p => p.Release(), Times.Once);
        eventBusMock.Verify(e => e.Publish(It.Is<SetTurnsJailEvent>(ev => ev.PlayerID == 1 && ev.Turns == 0)), Times.Once);
    }

    [Test]
    public void TryReleaseByDice_ShouldReleasePlayer_WhenDoublesRolled()
    {
        jailService.TryReleaseByDice(1, 3, 3);

        playerMock.Verify(p => p.Release(), Times.Once);
        eventBusMock.Verify(e => e.Publish(It.IsAny<SetTurnsJailEvent>()), Times.Once);
        turnManagerMock.Verify(tm => tm.RequestEndTurn(), Times.Never);
    }

    [Test]
    public void TryReleaseByDice_ShouldDecreaseTurnsAndEndTurn_WhenNotDoubles_AndMasterClient()
    {
        photonWrapperMock.Setup(p => p.IsMasterClient).Returns(true);

        jailService.TryReleaseByDice(1, 2, 3);

        Assert.AreEqual(1, playerMock.Object.JailTurnsLeft);
        eventBusMock.Verify(e => e.Publish(It.Is<SetTurnsJailEvent>(ev => ev.PlayerID == 1 && ev.Turns == 1)), Times.Once);
        turnManagerMock.Verify(tm => tm.RequestEndTurn(), Times.Once);
    }

    [Test]
    public void TryReleaseByDice_ShouldDoNothing_WhenPlayerNotInJail()
    {
        playerMock.Setup(p => p.IsInJail).Returns(false);

        jailService.TryReleaseByDice(1, 2, 3);

        playerMock.Verify(p => p.Release(), Times.Never);
        eventBusMock.Verify(e => e.Publish(It.IsAny<SetTurnsJailEvent>()), Times.Never);
        turnManagerMock.Verify(tm => tm.RequestEndTurn(), Times.Never);
    }

    [Test]
    public void GetTurnsLeft_ShouldReturnPlayerJailTurns()
    {
        var turns = jailService.GetTurnsLeft(1);
        Assert.AreEqual(2, turns);
    }
}
