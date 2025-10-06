using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;

public class AuctionService : IAuctionService
{
    #region CONSTANTS

    private const int FIRST_BID_INCREMENT = 100;

    #endregion CONSTANTS

    private IPhotonAuctionManager photonAuctionManager;
    private IPhotonTurnManager photonTurnManager;
    private IEventBus eventBus;
    private ITimerManager timerManager;
    private GameSettings gameSettings;

    private int companyId;
    private int basePrice;
    private int currentPrice;
    private int lastBidder = -1;

    private List<int> bidders = new();
    private readonly HashSet<int> passed = new();

    private int currentBidderIndex = 0;

    #region LIFE_CYCLE

    [Inject]
    public void Construct(IPhotonAuctionManager photonAuctionManager, IPhotonTurnManager photonTurnManager, IEventBus eventBus,
        ITimerManager timerManager, GameSettings gameSettings)
    {
        this.photonAuctionManager = photonAuctionManager ?? throw new ArgumentNullException(nameof(photonAuctionManager));
        this.photonTurnManager = photonTurnManager ?? throw new ArgumentNullException(nameof(photonTurnManager));
        this.eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
        this.timerManager = timerManager ?? throw new ArgumentNullException(nameof(timerManager));
        this.gameSettings = gameSettings ?? throw new ArgumentNullException(nameof(gameSettings));
    }

    #endregion LIFE_CYCLE

    #region PUBLIC_METHODS

    public void StartAuction(int starterActorNumber, int companyId, int basePrice)
    {
        this.companyId = companyId;
        this.basePrice = basePrice;
        currentPrice = basePrice;
        lastBidder = -1;

        passed.Clear();
        passed.Add(starterActorNumber);

        bidders = BuildTurnOrderStartingAfter(starterActorNumber, passed);
        currentBidderIndex = 0;

        if (bidders.Count == 0)
        {
            EndAuction_NoWinner();
            return;
        }
        PromptCurrentBidder();
    }

    public void PlaceBid(int playerId)
    {
        if (!IsPlayerTurn(playerId)) return;

        if (lastBidder == -1)
        {
            currentPrice = basePrice + FIRST_BID_INCREMENT;
        }
        else
        {
            currentPrice = currentPrice + FIRST_BID_INCREMENT;
        }

        lastBidder = playerId;

        MoveToNextBidderOrEnd();
    }

    public void PassBid(int playerId)
    {
        if (!IsPlayerTurn(playerId)) return;

        passed.Add(playerId);
        photonAuctionManager.CloseAuctionWindowRequest(playerId);
        MoveToNextBidderOrEnd();
    }

    #endregion PUBLIC_METHODS

    #region PRIVATE_METHODS

    private bool IsPlayerTurn(int playerId)
    {
        if (bidders.Count == 0) return false;
        return bidders[currentBidderIndex] == playerId && !passed.Contains(playerId);
    }

    private void MoveToNextBidderOrEnd()
    {
        var activeBidders = bidders.Where(b => !passed.Contains(b)).ToList();

        if (activeBidders.Count == 0)
        {
            EndAuction_NoWinner();
            return;
        }

        if (activeBidders.Count == 1)
        {
            int lastActive = activeBidders[0];

            if (lastBidder != -1)
            {
                int winner = lastBidder;
                int price = currentPrice;
                EndAuction_WithWinner(winner, price);
            }
            else
            {
                currentBidderIndex = bidders.IndexOf(lastActive);
                PromptCurrentBidder();
            }

            return;
        }

        do
        {
            currentBidderIndex = (currentBidderIndex + 1) % bidders.Count;
        }
        while (passed.Contains(bidders[currentBidderIndex]));

        PromptCurrentBidder();
    }

    private void PromptCurrentBidder()
    {
        if (bidders.Count == 0) return;

        int currentBidder = bidders[currentBidderIndex];
        if (passed.Contains(currentBidder)) return;
        timerManager.StartAuctionTimer(currentBidder, gameSettings.auctionTime);

        int minAllowedBid = (lastBidder == -1) ? basePrice + FIRST_BID_INCREMENT : currentPrice + FIRST_BID_INCREMENT;

        photonAuctionManager.PromptBidRequest(currentBidder, minAllowedBid, companyId);
    }

    private void EndAuction_NoWinner()
    {
        photonTurnManager.RequestEndTurn();
    }

    private void EndAuction_WithWinner(int winnerId, int finalPrice)
    {
        eventBus.Publish(new EndAuctionWithWinnerEvent(winnerId, finalPrice, companyId));
    }

    private List<int> BuildTurnOrderStartingAfter(int starterActorNumber, HashSet<int> excludedPlayers)
    {
        var list = PhotonNetwork.PlayerList
            .OrderBy(p => p.ActorNumber)
            .Select(p => p.ActorNumber)
            .Where(p => !excludedPlayers.Contains(p))
            .ToList();

        if (list.Count == 0) return new List<int>();

        int idx = list.FindIndex(x => x > starterActorNumber);
        if (idx < 0) idx = 0;

        var ordered = new List<int>();
        for (int i = 0; i < list.Count; i++)
        {
            ordered.Add(list[(idx + i) % list.Count]);
        }

        return ordered;
    }

    #endregion PRIVATE_METHODS
}