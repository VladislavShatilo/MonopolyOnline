using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;

public class TradeWindowPresenter : IInitializable, IDisposable
{
    private ITradeWindow tradeWindow;
    private ITurnWindow turnWindow;
    private IEventBus eventBus;
    private TradeOffer currentOffer;
    private ILocalPlayerService localPlayerService;
    private IPhotonTradeManager photonTradeManager;

    #region LIFE_CYCLE

    [Inject]
    public void Construct(ITradeWindow view, ILocalPlayerService localPlayerService, IEventBus eventBus, IPhotonTradeManager photonTradeManager, ITurnWindow turnWindow)
    {
        this.tradeWindow = view;
        this.localPlayerService = localPlayerService;
        this.eventBus = eventBus;
        this.photonTradeManager = photonTradeManager;
        this.turnWindow = turnWindow;
    }

    void IInitializable.Initialize()
    {
        eventBus.Subscribe<TradeStartedEvent>(ShowTradeWindow);
        eventBus.Subscribe<TradeUpdatedEvent>(UpdateTrade);

        tradeWindow.SetCloseAction(CloseTrade);
        tradeWindow.SetOfferAction(OfferTrade);
    }

    void IDisposable.Dispose()
    {
        eventBus.Unsubscribe<TradeStartedEvent>(ShowTradeWindow);
        eventBus.Unsubscribe<TradeUpdatedEvent>(UpdateTrade);
    }

    #endregion LIFE_CYCLE

    #region CALLBACKS

    private void ShowTradeWindow(TradeStartedEvent e)
    {
        int localId = localPlayerService.GetLocalPlayerId();

        if (e.FromPlayerId == localId)
        {
            tradeWindow.Show(e.Offer);
            currentOffer = e.Offer;
        }
        else
        {
            tradeWindow.Hide();
        }
    }

    private void UpdateTrade(TradeUpdatedEvent e)
    {
        tradeWindow.UpdateTrade(e.Offer);
    }

    private void CloseTrade()
    {
        tradeWindow.Hide();
    }

    private void OfferTrade()
    {
        photonTradeManager.SendTradeOffer(currentOffer);
        tradeWindow.Hide();
        turnWindow.Hide();
    }

    #endregion CALLBACKS


}