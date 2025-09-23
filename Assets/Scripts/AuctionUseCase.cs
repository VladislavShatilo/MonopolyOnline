using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design;
using UnityEngine;
using Zenject;

public class AuctionUseCase : IInitializable,IDisposable
{
    private  IAuctionService auctionService;
    private  IPhotonAuctionManager photonAuctionManager;
    private IEventBus eventBus;
    [Inject]
    public  void Construct(IAuctionService auctionService, IPhotonAuctionManager photonAuctionManager, IEventBus eventBus)
    {
        this.auctionService = auctionService;
        this.photonAuctionManager = photonAuctionManager;
        this.eventBus = eventBus;

       
    }
    void IInitializable.Initialize()
    {
        eventBus.Subscribe<StartAuctionEvent>(StartAuction);
        eventBus.Subscribe<PlayerBidAuction>(PlayerBid);
        eventBus.Subscribe<PlayerPassAuction>(PlayerPass);
        eventBus.Subscribe<TimerExpiredEvent>(TimerExpiredEvent);

        //auctionService.OnBidUpdated += (playerId, newPrice) =>
        //{
        //    photonAuctionManager.BroadcastBidUpdate(playerId, newPrice);
        //};

        //auctionService.OnAuctionEnded += (winnerId, finalPrice) =>
        //{
        //    photonAuctionManager.BroadcastAuctionEnd(winnerId, finalPrice);
        //};
    }
    void IDisposable.Dispose()
    {

        eventBus.Unsubscribe<StartAuctionEvent>(StartAuction);
        eventBus.Unsubscribe<PlayerBidAuction>(PlayerBid);
        eventBus.Unsubscribe<PlayerPassAuction>(PlayerPass);
        eventBus.Unsubscribe<TimerExpiredEvent>(TimerExpiredEvent);


        //auctionService.OnBidUpdated -= (playerId, newPrice) =>
        //{
        //    photonAuctionManager.BroadcastBidUpdate(playerId, newPrice);
        //};

        //auctionService.OnAuctionEnded -= (winnerId, finalPrice) =>
        //{
        //    photonAuctionManager.BroadcastAuctionEnd(winnerId, finalPrice);
        //};
    }
    private void StartAuction(StartAuctionEvent e)
    {

        auctionService.StartAuction(e.StarterActorNumber, e.CompanyId, e.CompanyBasePrice);
    }

    public void PlayerBid(PlayerBidAuction e)
    {
        auctionService.PlaceBid(e.PlayerId);
    }

    public void PlayerPass(PlayerPassAuction e)
    {
        auctionService.PassBid(e.PlayerId);
    }
    public void TimerExpiredEvent(TimerExpiredEvent e)
    {
        if (e.Type != TimerType.Auction) return;
        auctionService.PassBid(e.PlayerId);
    }
}
public class PlayerBidAuction
{
    public int PlayerId;
    public PlayerBidAuction(int playerId)
    {
        PlayerId = playerId;
    }
}