using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class PlayerStatsPresenter
{
    private  IPlayerStatsView view;
    private  IPhotonLoanManager photonLoanManager;
    private  ITradeService tradeService;
    private PlayerData playerData;
    private bool _isSubscribed = false;

    public PlayerStatsPresenter(IPlayerStatsView view, IPhotonLoanManager photonLoanManager, ITradeService tradeService)
    {
        this.view = view;
        this.photonLoanManager = photonLoanManager;
        this.tradeService = tradeService;

        // Привязка кнопок
        view.BindTradeAction(OnTrade);
        view.BindTakeLoanAction(() => photonLoanManager.SendTakeLoan(playerData.Id));
        view.BindPayLoanAction(() => photonLoanManager.SendPayLoan(playerData.Id));

       
    }
    private void SubscribeEvents()
    {
        if (_isSubscribed) return;

        EventBus.Subscribe<OnUpdatePlayerMoneyEvent>(OnMoneyUpdate);
        EventBus.Subscribe<TurnStartEvent>(OnTurnStarted);
        EventBus.Subscribe<TurnTimerUpdatedEvent>(OnTurnTimerUpdated);
        EventBus.Subscribe<AuctionTimerUpdatedEvent>(OnAuctionTimerUpdated);
        EventBus.Subscribe<OnTakeLoanEvent>(OnLoanUpdated);

        _isSubscribed = true;
    }

    private void UnsubscribeEvents()
    {
        if (!_isSubscribed) return;

        EventBus.Unsubscribe<OnUpdatePlayerMoneyEvent>(OnMoneyUpdate);
        EventBus.Unsubscribe<TurnStartEvent>(OnTurnStarted);
        EventBus.Unsubscribe<TurnTimerUpdatedEvent>(OnTurnTimerUpdated);
        EventBus.Unsubscribe<AuctionTimerUpdatedEvent>(OnAuctionTimerUpdated);
        EventBus.Unsubscribe<OnTakeLoanEvent>(OnLoanUpdated);

        _isSubscribed = false;
    }
    public void Dispose()
    {
        UnsubscribeEvents();
    }
    public void Init(PlayerData data)
    {
        playerData = data;
        view.SetName(data.Name);
        view.SetMoney(data.Money);
        view.SetCapital(data.VisibleCapital, data.LiquidAssets);
        view.SetLeaveButtonVisible(data.photonPlayer.IsLocal);
        SubscribeEvents();
    }

    private void OnMoneyUpdate(OnUpdatePlayerMoneyEvent e)
    {
        Debug.Log("OnMoneyUpdate()");
        if (playerData.Id == e.Player.Id)
            view.SetMoney(e.Player.Money);
    }

    private void OnTurnTimerUpdated(TurnTimerUpdatedEvent e)
    {
        if (playerData.Id == e.PlayerId)
            view.SetTimer(e.IsCurrent, e.TimeLeft, true, false);
    }

    private void OnAuctionTimerUpdated(AuctionTimerUpdatedEvent e)
    {
        if (playerData.Id == e.PlayerId)
            view.SetTimer(true, e.TimeLeft, false, true);
    }

    private void OnLoanUpdated(OnTakeLoanEvent e)
    {
        if (e.PlayerData.Id == playerData.Id)
            view.SetLoan(e.PlayerData.HasLoan, e.PlayerData.LoanTurnsLeft, e.PlayerData.photonPlayer.IsLocal);
    }

    private void OnTurnStarted(TurnStartEvent e)
    {
        //if (playerData == null) return;

        //bool isLocalTurn = tradeService.IsLocalTurn(e.PlayerId);
        //bool isThisLocal = tradeService.IsLocalPlayer(playerData.Id);

        //view.SetTradeButtonVisible(isLocalTurn && !isThisLocal);

        //if (isThisLocal)
        //    view.SetLoanButtonsVisible(!playerData.HasLoan, playerData.HasLoan);
    }

    private void OnTrade()
    {
       // tradeService.SendTradeRequest(playerData.Id);
    }
}
