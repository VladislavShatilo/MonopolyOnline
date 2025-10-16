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

    public void Initialize()
    {
        eventBus.Subscribe<StartAuctionEvent>(StartAuction);
        eventBus.Subscribe<PlayerBidAuction>(PlayerBid);
        eventBus.Subscribe<PlayerPassAuction>(PlayerPass);
        eventBus.Subscribe<TimerExpiredEvent>(TimerExpiredEvent);
    }

    public void Dispose()
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
        if (e == null)
            throw new ArgumentNullException(nameof(e));
        auctionService.StartAuction(e.StarterActorNumber, e.CompanyId, e.CompanyBasePrice);
    }

    private void PlayerBid(PlayerBidAuction e)
    {
        if (e == null)
            throw new ArgumentNullException(nameof(e));
        auctionService.PlaceBid(e.PlayerId);
    }

    private void PlayerPass(PlayerPassAuction e)
    {
        if (e == null)
            throw new ArgumentNullException(nameof(e));
        auctionService.PassBid(e.PlayerId);
    }

    private void TimerExpiredEvent(TimerExpiredEvent e)
    {
        if (e == null)
            throw new ArgumentNullException(nameof(e));
        if (e.Type != TimerType.Auction) return;
        auctionService.PassBid(e.PlayerId);
    }

    #endregion CALLBACKS
}