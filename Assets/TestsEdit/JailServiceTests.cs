using Moq;
using NUnit.Framework;
using System;
using System.Drawing.Printing;
using UnityEngine;

public class JailServiceTests
{
    private JailService service;
    private Mock<IPlayerRepository> playerRepo;
    private Mock<IEventBus> eventBus;
    private Mock<IPhotonTurnManager> turnManager;
    private Mock<IPhotonNetworkWrapper> networkWrapper;
    private GameSettings settings;

    [SetUp]
    public void SetUp()
    {
        playerRepo = new Mock<IPlayerRepository>();
        eventBus = new Mock<IEventBus>();
        turnManager = new Mock<IPhotonTurnManager>();
        networkWrapper = new Mock<IPhotonNetworkWrapper>();
        settings =  ScriptableObject.CreateInstance<GameSettings>();

        service = new JailService();
        service.Construct(playerRepo.Object, eventBus.Object, turnManager.Object, settings, networkWrapper.Object);
    }

    // SendPlayerToJail
    [Test]
    public void SendPlayerToJail_PlayerExists_SetsCellAndPublishesEvent()
    {
        var player = new PlayerData("p", 0, 1, null);
        playerRepo.Setup(r => r.GetPlayerById(1)).Returns(player);

        service.SendPlayerToJail(1);

        Assert.AreEqual(player.CurrentCellId, settings.jailCellId);

        Assert.AreEqual(player.IsInJail,true);
        Assert.AreEqual(player.JailTurnsLeft, settings.jailTurns);

        eventBus.Verify(e => e.Publish(It.IsAny<SetTurnsJailEvent>()), Times.Once);
    }

    [Test]
    public void SendPlayerToJail_PlayerNotFound_Throws()
    {
        playerRepo.Setup(r => r.GetPlayerById(1)).Returns((PlayerData)null);
        Assert.Throws<NullReferenceException>(() => service.SendPlayerToJail(1));
    }

    // ReleasePlayer
    [Test]
    public void ReleasePlayer_PlayerExists_CallsReleaseAndPublishesEvent()
    {
        var player = new PlayerData("p", 0, 1, null);
        playerRepo.Setup(r => r.GetPlayerById(1)).Returns(player);

        service.ReleasePlayer(1, true);


        Assert.AreEqual(player.IsInJail, false);
        Assert.AreEqual(player.JailTurnsLeft, 0);
        eventBus.Verify(e => e.Publish(It.Is<SetTurnsJailEvent>(x => x.PlayerID == 1 && x.Turns == 0)), Times.Once);
    }

    [Test]
    public void ReleasePlayer_PlayerNotFound_Throws()
    {
        playerRepo.Setup(r => r.GetPlayerById(1)).Returns((PlayerData)null);
        Assert.Throws<NullReferenceException>(() => service.ReleasePlayer(1, true));
    }

    // TryReleaseByDice
    [Test]
    public void TryReleaseByDice_PlayerNotInJail_DoesNothing()
    {
        var player = new PlayerData("p", 0, 1, null);
        player.IsInJail = false;
        playerRepo.Setup(r => r.GetPlayerById(1)).Returns(player);

        service.TryReleaseByDice(1, 1, 2);

        Assert.AreEqual(player.IsInJail, false);
        Assert.AreEqual(player.JailTurnsLeft, 0);

        eventBus.Verify(e => e.Publish(It.IsAny<SetTurnsJailEvent>()), Times.Never);
        turnManager.Verify(t => t.RequestEndTurn(), Times.Never);
    }

    [Test]
    public void TryReleaseByDice_Doubles_ReleasesPlayer()
    {
        var player = new PlayerData("p", 0, 1, null);
        player.IsInJail = true;

        playerRepo.Setup(r => r.GetPlayerById(1)).Returns(player);

        service.TryReleaseByDice(1, 2, 2);

        Assert.AreEqual(player.IsInJail, false);
        Assert.AreEqual(player.JailTurnsLeft, 0);

        eventBus.Verify(e => e.Publish(It.IsAny<SetTurnsJailEvent>()), Times.Once);
    }

    [Test]
    public void TryReleaseByDice_NotDoubles_DecreasesTurnsAndEndsTurnIfMaster()
    {
        var player = new PlayerData("p", 0, 1, null);
        player.JailTurnsLeft = 2;
        player.IsInJail = true;
      
        playerRepo.Setup(r => r.GetPlayerById(1)).Returns(player);
        networkWrapper.Setup(n => n.IsMasterClient).Returns(true);

        service.TryReleaseByDice(1, 1, 2);

        Assert.AreEqual(1, player.JailTurnsLeft);
        eventBus.Verify(e => e.Publish(It.Is<SetTurnsJailEvent>(x => x.Turns == 1)), Times.Once);
        turnManager.Verify(t => t.RequestEndTurn(), Times.Once);
    }

    [Test]
    public void TryReleaseByDice_PlayerNotFound_Throws()
    {
        playerRepo.Setup(r => r.GetPlayerById(1)).Returns((PlayerData)null);
        Assert.Throws<NullReferenceException>(() => service.TryReleaseByDice(1, 1, 2));
    }

    // GetTurnsLeft
    [Test]
    public void GetTurnsLeft_PlayerExists_ReturnsJailTurns()
    {
        var player = new PlayerData("p", 0, 1, null);
        player.JailTurnsLeft = 3;
        playerRepo.Setup(r => r.GetPlayerById(1)).Returns(player);

        int turns = service.GetTurnsLeft(1);

        Assert.AreEqual(3, turns);
    }

    [Test]
    public void GetTurnsLeft_PlayerNotFound_Throws()
    {
        playerRepo.Setup(r => r.GetPlayerById(1)).Returns((PlayerData)null);
        Assert.Throws<NullReferenceException>(() => service.GetTurnsLeft(1));
    }
}
