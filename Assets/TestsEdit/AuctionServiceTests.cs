using Moq;
using NUnit.Framework;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[TestFixture]
public class AuctionServiceTests
{
    private AuctionService auctionService;
    private Mock<IPhotonAuctionManager> photonAuctionManager;
    private Mock<IPhotonTurnManager> photonTurnManager;
    private Mock<IEventBus> eventBus;
    private Mock<ITimerManager> timerManager;
    private Mock<IPlayerRepository> playerRepository;
    private GameSettings gameSettings;

    [SetUp]
    public void Setup()
    {
        photonAuctionManager = new Mock<IPhotonAuctionManager>();
        photonTurnManager = new Mock<IPhotonTurnManager>();
        eventBus = new Mock<IEventBus>();
        timerManager = new Mock<ITimerManager>();
        playerRepository = new Mock<IPlayerRepository>();
        gameSettings = new GameSettings { auctionTime = 10 };

        auctionService = new AuctionService();
        auctionService.Construct(
            photonAuctionManager.Object,
            photonTurnManager.Object,
            eventBus.Object,
            timerManager.Object,
            gameSettings,
            playerRepository.Object
        );
    }

    [Test]
    public void StartAuction_WithNoEligiblePlayers_ShouldEndAuctionNoWinner()
    {
        playerRepository.Setup(r => r.GetAllPlayers()).Returns(new List<PlayerData>());

        auctionService.StartAuction(1, 100, 500);

        photonTurnManager.Verify(t => t.RequestEndTurn(), Times.Once);
    }

    [Test]
    public void StartAuction_WithEligiblePlayers_ShouldPromptCurrentBidder()
    {
        var players = new List<PlayerData>
        {
            new("A", 500, 1, null),
            new("B", 500, 2, null),
            new("C", 500, 3, null)
        };
        playerRepository.Setup(r => r.GetAllPlayers()).Returns(players);

        auctionService.StartAuction(1, 100, 500);

        photonAuctionManager.Verify(p => p.PromptBidRequest(2,600, 100), Times.Once);
        timerManager.Verify(t => t.StartAuctionTimer(2, gameSettings.auctionTime), Times.Once);
    }

    [Test]
    public void PlaceBid_FirstBid_ShouldIncreasePriceByIncrement()
    {
        var players = new List<PlayerData>
        {
            new PlayerData("A", 500, 1, null),
            new PlayerData("B", 500, 2, null)
        };
        playerRepository.Setup(r => r.GetAllPlayers()).Returns(players);

        auctionService.StartAuction(1, 100, 500);
        auctionService.PlaceBid(2);

        photonAuctionManager.Verify(p => p.PromptBidRequest(2, 600, 100), Times.Once);
    }

    [Test]
    public void PlaceBid_SubsequentBid_ShouldIncreasePriceByIncrement()
    {
        var players = new List<PlayerData>
        {
            new PlayerData("A", 1000, 1, null),
            new PlayerData("B", 1000, 2, null),
            new PlayerData("B", 1000, 3, null)
        };
        playerRepository.Setup(r => r.GetAllPlayers()).Returns(players);

        auctionService.StartAuction(1, 100, 500);
        auctionService.PlaceBid(2);
        auctionService.PlaceBid(3);

        photonAuctionManager.Verify(p => p.PromptBidRequest(2, 800, 100), Times.Once);
    }

    [Test]
    public void PassBid_ShouldAddPlayerToPassedAndCloseWindow()
    {
        var players = new List<PlayerData>
        {
            new PlayerData("A", 500, 1, null),
            new PlayerData("B", 500, 2, null)
        };
        playerRepository.Setup(r => r.GetAllPlayers()).Returns(players);

        auctionService.StartAuction(1, 100, 500);
        auctionService.PassBid(2);

        photonAuctionManager.Verify(p => p.CloseAuctionWindowRequest(2), Times.Once);
    }

    [Test]
    public void AuctionEnds_WithWinner_ShouldPublishEvent()
    {
        var players = new List<PlayerData>
        {
            new PlayerData("A", 500, 1, null),
            new PlayerData("B", 500, 2, null)
        };
        playerRepository.Setup(r => r.GetAllPlayers()).Returns(players);

        auctionService.StartAuction(1, 100, 500);
        auctionService.PlaceBid(2);
        auctionService.PassBid(1); // оставляем 1 игрока, победитель 2

        eventBus.Verify(e => e.Publish(It.Is<EndAuctionWithWinnerEvent>(ev => ev.WinnerId == 2 && ev.FinalPrice == 600)), Times.Once);
    }
    [Test]
    public void AuctionEnds_NoEligibleBidders_ShouldEndAuctionNoWinner()
    {
        var players = new List<PlayerData>
        {
            new("A", 500, 1, null)
        };
        playerRepository.Setup(r => r.GetAllPlayers()).Returns(players);

        // Все игроки сразу пасуют
        auctionService.StartAuction(1, 100, 500);
        auctionService.PassBid(1);

        photonTurnManager.Verify(t => t.RequestEndTurn(), Times.Once);
    }

   

    // ===== Очередность ходов =====
    [Test]
    public void PlaceBid_ShouldMoveToNextBidderCorrectly()
    {
        var players = new List<PlayerData>
        {
            new("A", 500, 1, null),
            new("B", 500, 2, null),
            new("C", 500, 3, null)
        };
        playerRepository.Setup(r => r.GetAllPlayers()).Returns(players);

        auctionService.StartAuction(1, 100, 500); // Стартуем с 1

        auctionService.PlaceBid(2); // B ставит
        auctionService.PlaceBid(3); // C ставит

        // После ставок очередь вернулась к A
        photonAuctionManager.Verify(p => p.PromptBidRequest(2, 600, 100), Times.Once);
        photonAuctionManager.Verify(p => p.PromptBidRequest(3, 700, 100), Times.Once);
    }

   

    // ===== Граничные значения и старт с последнего игрока =====
    [Test]
    public void StartAuction_WithLastPlayerAsStarter_ShouldBuildCorrectOrder()
    {
        var players = new List<PlayerData>
        {
            new("A", 500, 1, null),
            new("B", 500, 2, null),
            new("C", 500, 3, null)
        };
        playerRepository.Setup(r => r.GetAllPlayers()).Returns(players);

        auctionService.StartAuction(3, 100, 500); // стартуем с последнего игрока

        // Первый вызов PromptBidRequest должен быть для игрока с ID > 3, т.е. A (1)
        photonAuctionManager.Verify(p => p.PromptBidRequest(1, 600, 100), Times.Once);
    }
    [Test]
    public void Construct_ShouldThrowArgumentNullException_WhenAnyDependencyIsNull()
    {
        var auctionService = new AuctionService();

        var photonAuctionManager = new Mock<IPhotonAuctionManager>().Object;
        var photonTurnManager = new Mock<IPhotonTurnManager>().Object;
        var eventBus = new Mock<IEventBus>().Object;
        var timerManager = new Mock<ITimerManager>().Object;
        var playerRepository = new Mock<IPlayerRepository>().Object;
        var gameSettings = ScriptableObject.CreateInstance<GameSettings>(); // Правильный способ

        // Проверим все зависимости по очереди
        Assert.Throws<ArgumentNullException>(() =>
            auctionService.Construct(null, photonTurnManager, eventBus, timerManager, gameSettings, playerRepository));

        Assert.Throws<ArgumentNullException>(() =>
            auctionService.Construct(photonAuctionManager, null, eventBus, timerManager, gameSettings, playerRepository));

        Assert.Throws<ArgumentNullException>(() =>
            auctionService.Construct(photonAuctionManager, photonTurnManager, null, timerManager, gameSettings, playerRepository));

        Assert.Throws<ArgumentNullException>(() =>
            auctionService.Construct(photonAuctionManager, photonTurnManager, eventBus, null, gameSettings, playerRepository));

        Assert.Throws<ArgumentNullException>(() =>
            auctionService.Construct(photonAuctionManager, photonTurnManager, eventBus, timerManager, null, playerRepository));

        Assert.Throws<ArgumentNullException>(() =>
            auctionService.Construct(photonAuctionManager, photonTurnManager, eventBus, timerManager, gameSettings, null));
    }
    [Test]
    public void IsPlayerTurn_ShouldReturnTrue_WhenItIsPlayersTurn()
    {
        playerRepository.Setup(r => r.GetAllPlayers()).Returns(new List<PlayerData>
        {
            new("PlayerA", 500, 1, null),
            new("PlayerB", 500, 2, null)
        });

        auctionService.StartAuction(1, 100, 500);

        bool result = InvokePrivate<bool>("IsPlayerTurn", 2);
        Assert.IsTrue(result);
    }

    [Test]
    public void IsPlayerTurn_ShouldReturnFalse_WhenPlayerAlreadyPassed()
    {
        playerRepository.Setup(r => r.GetAllPlayers()).Returns(new List<PlayerData>
        {
            new("PlayerA", 500, 1, null),
            new("PlayerB", 500, 2, null)
        });

        auctionService.StartAuction(1, 100, 500);
        auctionService.PassBid(1); // игрок 1 сдался

        bool result = InvokePrivate<bool>("IsPlayerTurn", 1);
        Assert.IsFalse(result);
    }

    [Test]
    public void IsPlayerTurn_ShouldReturnFalse_WhenNoBidders()
    {
        playerRepository.Setup(r => r.GetAllPlayers()).Returns(new List<PlayerData>());
        auctionService.StartAuction(1, 100, 500);

        bool result = InvokePrivate<bool>("IsPlayerTurn", 1);
        Assert.IsFalse(result);
    }

    // ---------- Вспомогательный метод для вызова приватных ----------

    private T InvokePrivate<T>(string methodName, params object[] args)
    {
        var method = typeof(AuctionService).GetMethod(methodName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        return (T)method.Invoke(auctionService, args);
    }
}
