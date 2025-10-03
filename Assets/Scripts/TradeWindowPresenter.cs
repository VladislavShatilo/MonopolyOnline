using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;

public class TradeWindowPresenter : ITradeWindowPresenter, IInitializable, IDisposable
{
    private ITradeWindow tradeWindow;
    private ITurnWindow turnWindow;
    private IEventBus eventBus;
    private TradeOffer currentOffer;
    private ILocalPlayerService localPlayerService;
    private IPhotonTradeManager photonTradeManager;
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
        //eventBus

    }
    private void OfferTrade()
    {
        photonTradeManager.SendTradeOffer(currentOffer);
        tradeWindow.Hide();
        turnWindow.Hide();
        //eventBus

    }
    public void OnMoneyChanged(string value, bool isLeft)
    {
        if (int.TryParse(value, out int amount) && currentOffer != null)
        {
            if (isLeft)
                currentOffer.FromMoney = amount;
            else
                currentOffer.ToMoney = amount;

            //EventBus.Publish(new TradeUpdatedEvent(currentOffer));
            //PresentOffer(currentOffer);
        }
    }

    
}

public class TradeStartedEvent
{
    public int FromPlayerId { get; }
    public int ToPlayerId { get; }
    public TradeOffer Offer { get; }

    public TradeStartedEvent(int from, int to, TradeOffer offer)
    { FromPlayerId = from; ToPlayerId = to; Offer = offer; }
}

public class TradeUpdatedEvent
{
    public TradeOffer Offer { get; }

    public TradeUpdatedEvent(TradeOffer offer)
    { Offer = offer; }
}

public class TradeCancelledEvent
{ }

public class TradeProposalReceivedEvent
{
    public TradeOffer Offer { get; }
  
    public TradeProposalReceivedEvent(TradeOffer offer, int from, int to)
    {
        Offer = offer;
    }
}

public class TradePromptEvent
{
    public int SenderId { get; }
    public int ReceiverId { get; }

    public TradePromptEvent(int senderId, int receiverId)
    { SenderId = senderId; ReceiverId = receiverId; }
}

public class TradeTimerUpdatedEvent
{
    public int PlayerId { get; }
    public float TimeLeft { get; }

    public TradeTimerUpdatedEvent(int playerId, float timeLeft)
    { PlayerId = playerId; TimeLeft = timeLeft; }
}

public class TradeEndedEvent
{
    public bool Accepted { get; }
    public int SenderId { get; }
    public int ReceiverId { get; }

    public TradeEndedEvent(bool accepted, int senderId, int receiverId)
    { Accepted = accepted; SenderId = senderId; ReceiverId = receiverId; }
}