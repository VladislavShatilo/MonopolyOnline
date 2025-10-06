using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design;
using UnityEngine;
using Zenject;

public class AuctionUseCase : IInitializable, IDisposable
{
    private IAuctionService auctionService;
    private IEventBus eventBus;

    #region LIFE_CYCLE

    [Inject]
    public void Construct(IAuctionService auctionService, IEventBus eventBus)
    {
        this.auctionService = auctionService ?? throw new ArgumentNullException(nameof(auctionService));
        this.eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
    }

    void IInitializable.Initialize()
    {
        eventBus.Subscribe<StartAuctionEvent>(StartAuction);
        eventBus.Subscribe<PlayerBidAuction>(PlayerBid);
        eventBus.Subscribe<PlayerPassAuction>(PlayerPass);
        eventBus.Subscribe<TimerExpiredEvent>(TimerExpiredEvent);
    }

    void IDisposable.Dispose()
    {
        eventBus.Unsubscribe<StartAuctionEvent>(StartAuction);
        eventBus.Unsubscribe<PlayerBidAuction>(PlayerBid);
        eventBus.Unsubscribe<PlayerPassAuction>(PlayerPass);
        eventBus.Unsubscribe<TimerExpiredEvent>(TimerExpiredEvent);
    }

    #endregion LIFE_CYCLE

    #region CALLBACKS

    private void StartAuction(StartAuctionEvent e)
    {
        auctionService.StartAuction(e.StarterActorNumber, e.CompanyId, e.CompanyBasePrice);
    }

    private void PlayerBid(PlayerBidAuction e)
    {
        auctionService.PlaceBid(e.PlayerId);
    }

    private void PlayerPass(PlayerPassAuction e)
    {
        auctionService.PassBid(e.PlayerId);
    }

    private void TimerExpiredEvent(TimerExpiredEvent e)
    {
        if (e.Type != TimerType.Auction) return;
        auctionService.PassBid(e.PlayerId);
    }

    #endregion CALLBACKS
}