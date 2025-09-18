using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;

public class AuctionService : IAuctionService
{
    private int starterActorNumber;
    private int companyId;
    private int basePrice;
    private int currentPrice;
    private int lastBidder = -1;

    private List<int> bidders = new List<int>(); // очередь игроков
    private HashSet<int> passed = new HashSet<int>();

    private int currentBidderIndex = 0;

    private const int FIRST_BID_INCREMENT = 100;

    private IPhotonAuctionManager photonAuctionManager;
    private IPhotonTurnManager photonTurnManager;
    private IEventBus eventBus;

    [Inject]
    public void Construct(IPhotonAuctionManager photonAuctionManager, IPhotonTurnManager photonTurnManager, IEventBus eventBus)
    {
        this.photonAuctionManager = photonAuctionManager;
        this.photonTurnManager = photonTurnManager;
        this.eventBus = eventBus;
    }

    public void StartAuction(int starterActorNumber, int companyId, int basePrice)
    {
        this.starterActorNumber = starterActorNumber;
        this.companyId = companyId;
        this.basePrice = basePrice;
        this.currentPrice = basePrice;
        this.lastBidder = -1;

        passed.Clear();
        passed.Add(starterActorNumber); // отказался первый

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

        currentPrice = (lastBidder == -1) ? basePrice + FIRST_BID_INCREMENT : currentPrice + FIRST_BID_INCREMENT;
        lastBidder = playerId;

        MoveToNextBidderOrEnd();
    }

    public void PassBid(int playerId)
    {
        if (!IsPlayerTurn(playerId)) return;

        passed.Add(playerId);

        MoveToNextBidderOrEnd();
    }

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
                // Уже кто-то делал ставку → он победитель
                int winner = lastBidder;
                int price = currentPrice;
                EndAuction_WithWinner(winner, price);
            }
            else
            {
                // Остался один, но ставок ещё не было → даём ему шанс поставить
                currentBidderIndex = bidders.IndexOf(lastActive);
                PromptCurrentBidder();
            }

            return;
        }

        // крутим индекс по кругу
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

        int minAllowedBid = (lastBidder == -1) ? basePrice + FIRST_BID_INCREMENT : currentPrice + FIRST_BID_INCREMENT;

        photonAuctionManager.PromptBidRequest(currentBidder, minAllowedBid, companyId);
    }

    private void EndAuction_NoWinner()
    {
        Debug.Log("EndAuction_NoWinner()");
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
}
public class EndAuctionWithWinnerEvent
{
    public int WinnerId;
    public int CompanyId;
    public int FinalPrice;
    public EndAuctionWithWinnerEvent(int winnerId, int finalPrice,int companyId)
    {
        WinnerId = winnerId;
        FinalPrice = finalPrice;
        CompanyId = companyId;
    }
}