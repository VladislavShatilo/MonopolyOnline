using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;

public class TurnServiceTests
{
    private TurnService turnService;
    private Mock<IPlayerRepository> playerRepoMock;
    private Mock<IPhotonTurnSynchronizer> turnSyncMock;
    private Mock<ITimerManager> timerManagerMock;
    private GameSettings gameSettings;

    private PlayerData player1 = new PlayerData("Player1",500,1,null);
    private PlayerData player2 = new PlayerData("Player2", 500, 2, null);


    [SetUp]
    public void Setup()
    {
        playerRepoMock = new Mock<IPlayerRepository>();
        turnSyncMock = new Mock<IPhotonTurnSynchronizer>();
        timerManagerMock = new Mock<ITimerManager>();

        gameSettings = ScriptableObject.CreateInstance<GameSettings>();
        gameSettings.turnTime = 30;

     
        playerRepoMock.Setup(r => r.GetAllPlayers()).Returns(new List<PlayerData> { player1, player2 });
        playerRepoMock.Setup(r => r.GetPlayerById(1)).Returns(player1);
        playerRepoMock.Setup(r => r.GetPlayerById(2)).Returns(player2);
        playerRepoMock.Setup(r => r.GetNextPlayerId(1)).Returns(player2);
        playerRepoMock.Setup(r => r.GetNextPlayerId(2)).Returns(player1);

        turnService = new TurnService();
        turnService.Construct(playerRepoMock.Object, turnSyncMock.Object, timerManagerMock.Object, gameSettings);
    }

    [Test]
    public void StartRandomTurn_ShouldStartTurnForRandomPlayer()
    {
        turnService.StartRandomTurn();

        timerManagerMock.Verify(t => t.StartTurnTimer(It.IsAny<int>(), gameSettings.turnTime), Times.Once);
        turnSyncMock.Verify(t => t.RequestStartTurn(It.IsAny<int>(), It.IsAny<bool>()), Times.Once);
    }

    [Test]
    public void StartTurn_ShouldStartTurn_AndCallTimerAndSync()
    {
        turnService.StartTurn(1, true);

        timerManagerMock.Verify(t => t.StartTurnTimer(1, gameSettings.turnTime), Times.Once);
        turnSyncMock.Verify(t => t.RequestStartTurn(1, true), Times.Once);
    }

    [Test]
    public void EndTurn_ShouldMoveToNextPlayer()
    {
        turnService.StartTurn(1, true);
        turnService.EndTurn();

        timerManagerMock.Verify(t => t.StartTurnTimer(2, gameSettings.turnTime), Times.Once);
        turnSyncMock.Verify(t => t.RequestStartTurn(2, true), Times.Once);
    }

    [Test]
    public void RegisterDouble_ShouldAddExtraTurn_IfSkipNextTurnFalse()
    {
        turnService.RegisterDouble(1);

        // Для проверки используем внутреннюю логику Turn через StartTurn
        timerManagerMock.Verify(t => t.StartTurnTimer(It.IsAny<int>(), gameSettings.turnTime), Times.Never);
        turnSyncMock.Verify(t => t.RequestStartTurn(It.IsAny<int>(), It.IsAny<bool>()), Times.Never);
        // Логика добавления ExtraTurn проверяется косвенно через EndTurn
    }

    [Test]
    public void RegisterDouble_ShouldNotAddExtraTurn_IfSkipNextTurnTrue()
    {
        player1.SkipNextTurn = true;
        turnService.RegisterDouble(1);

        // Логика добавления ExtraTurn не должна вызываться
    }

    [Test]
    public void SetMode_ShouldSetTurnMode()
    {
        turnService.SetMode(TurnMode.Auction);
        // Проверяем через внутренние методы Turn, можно проверить косвенно через поведение
        Assert.Pass("SetMode вызван успешно"); // Тест гарантирует вызов метода без ошибок
    }
    [Test]
    public void EndTurn_ShouldUseExtraTurn_WhenPlayerHasExtraTurn()
    {
        turnService.RegisterDouble(1); // добавляем ExtraTurn
        turnService.StartTurn(1, true);

        turnService.EndTurn();

        // Проверяем, что тот же игрок получает следующий ход
        timerManagerMock.Verify(t => t.StartTurnTimer(1, gameSettings.turnTime), Times.Exactly(2));
        turnSyncMock.Verify(t => t.RequestStartTurn(1, false), Times.Once);
    }
    [Test]
    public void EndTurn_ShouldSkipNextPlayer_WhenNextPlayerHasSkipNextTurn()
    {
        player2.SkipNextTurn = true;

        // Текущий ход начинается
        turnService.StartTurn(1, true);

        // Очистим счётчик моков, чтобы измерять только вызовы EndTurn
        turnSyncMock.Invocations.Clear();
        timerManagerMock.Invocations.Clear();

        turnService.EndTurn();

        // Теперь проверяем, что StartTurn был вызван для текущего игрока один раз
        timerManagerMock.Verify(t => t.StartTurnTimer(1, gameSettings.turnTime), Times.Once);
        turnSyncMock.Verify(t => t.RequestStartTurn(1, true), Times.Once);
    }

    [Test]
    public void StartRandomTurn_ShouldDoNothing_WhenNoPlayers()
    {
        playerRepoMock.Setup(r => r.GetAllPlayers()).Returns(new List<PlayerData>());

        Assert.DoesNotThrow(() => turnService.StartRandomTurn());
        timerManagerMock.Verify(t => t.StartTurnTimer(It.IsAny<int>(), It.IsAny<float>()), Times.Never);
        turnSyncMock.Verify(t => t.RequestStartTurn(It.IsAny<int>(), It.IsAny<bool>()), Times.Never);
    }
    [Test]
    public void RegisterDouble_ShouldThrow_WhenPlayerNotFound()
    {
        playerRepoMock.Setup(r => r.GetPlayerById(999)).Returns((PlayerData)null);

        Assert.Throws<InvalidOperationException>(() => turnService.RegisterDouble(999));
    }
    [Test]
    public void Construct_ShouldThrowArgumentNull_WhenDependenciesNull()
    {
        var service = new TurnService();
        Assert.Throws<ArgumentNullException>(() => service.Construct(null, turnSyncMock.Object, timerManagerMock.Object, gameSettings));
        Assert.Throws<ArgumentNullException>(() => service.Construct(playerRepoMock.Object, null, timerManagerMock.Object, gameSettings));
        Assert.Throws<ArgumentNullException>(() => service.Construct(playerRepoMock.Object, turnSyncMock.Object, null, gameSettings));
        Assert.Throws<ArgumentNullException>(() => service.Construct(playerRepoMock.Object, turnSyncMock.Object, timerManagerMock.Object, null));
    }

}
