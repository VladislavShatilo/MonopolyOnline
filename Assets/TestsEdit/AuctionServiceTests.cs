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
            new PlayerData("A", 500, 1, null),
            new PlayerData("B", 500, 2, null),
            new PlayerData("C", 500, 3, null)
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
}
