using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;

public class TradeReviewWindowPresenter :  IInitializable, IDisposable
{
    private ITradeReviewWindow tradeReviewWindow;
    private IEventBus eventBus;
    private ILocalPlayerService localPlayerService;
    private IPhotonTradeManager photonTradeManager;

    #region LIFE_CYCLE

    [Inject]
    public void Construct(ITradeReviewWindow tradeReviewWindow, IEventBus eventBus, ILocalPlayerService localPlayerService, IPhotonTradeManager photonTradeManager)
    {
        this.tradeReviewWindow = tradeReviewWindow;
        this.eventBus = eventBus;
        this.localPlayerService = localPlayerService;
        this.photonTradeManager = photonTradeManager;
    }

    public void Initialize()
    {
        eventBus.Subscribe<TradeProposalReceivedEvent>(ShowTradeReviewWindow);
        eventBus.Subscribe<TradeEndedEvent>(HideWindowsOnTradeEnded);

        tradeReviewWindow.SetAcceptAction(AcceptTrade);
        tradeReviewWindow.SetCancelAction(CancelTrade);
    }

    public void Dispose()
    {
        eventBus.Unsubscribe<TradeProposalReceivedEvent>(ShowTradeReviewWindow);
        eventBus.Unsubscribe<TradeEndedEvent>(HideWindowsOnTradeEnded);
    }

    #endregion LIFE_CYCLE

    #region CALLBACKS

    private void ShowTradeReviewWindow(TradeProposalReceivedEvent e)
    {
        int localId = localPlayerService.GetLocalPlayerId();

        if (e.Offer.ToPlayerData.Id == localId)
        {
            tradeReviewWindow.Show(true, e.Offer);
        }
        else if (e.Offer.FromPlayerData.Id == localId)
        {
            return;
        }
        else
        {
            tradeReviewWindow.Show(false, e.Offer);
        }
    }

    private void HideWindowsOnTradeEnded(TradeEndedEvent e)
    {
        tradeReviewWindow.Hide();
    }

    private void AcceptTrade()
    {
        photonTradeManager.CompleteTrade(true);
    }

    private void CancelTrade()
    {
        photonTradeManager.CompleteTrade(false);
    }

    #endregion CALLBACKS
}