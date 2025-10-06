using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class PlayerStatsPresenter
{
    private IPlayerStatsView view;
    private IEventBus eventBus;
    private ILocalPlayerService localPlayerService;
    private IPhotonTradeManager photonTradeManager;
    private PlayerData playerData;
    private bool isSubscribed = false;

    #region LIFE_CYCLE

    public PlayerStatsPresenter(IPlayerStatsView view, IPhotonLoanManager photonLoanManager, IEventBus eventBus, ILocalPlayerService localPlayerService, IPhotonTradeManager photonTradeManager)
    {
        this.view = view;
        this.eventBus = eventBus;
        this.localPlayerService = localPlayerService;
        this.photonTradeManager = photonTradeManager;

        // Привязка кнопок
        view.BindTradeAction(OnTradeClick);
        view.BindTakeLoanAction(() => photonLoanManager.TakeLoanRequest(playerData.Id));
        view.BindPayLoanAction(() => photonLoanManager.PayLoanRequest(playerData.Id));
    }

    public void Dispose()
    {
        UnsubscribeEvents();
    }

    #endregion LIFE_CYCLE

    #region PUBLIC_METHODS

    public void Init(PlayerData data)
    {
        playerData = data;
        view.SetName(data.Name);
        view.SetMoney(data.Money);
        view.SetCapital(data.VisibleCapital, data.LiquidAssets);
        view.SetLeaveButtonVisible(data.photonPlayer.IsLocal);
        view.SetLoanButtonsVisible(false, false);
        view.SetTradeButtonVisible(false);
        view.SetAuctionHighlightVisible(false);
        view.SetTurnHighlightVisible(false);
        view.SetLoanContainerVisible(false);
        view.SetTimerGOVisible(false);
        SubscribeEvents();
    }

    #endregion PUBLIC_METHODS

    #region PRIVATE_METHODS

    private void SubscribeEvents()
    {
        if (isSubscribed) return;

        eventBus.Subscribe<OnUpdatePlayerMoneyEvent>(OnMoneyUpdate);
        eventBus.Subscribe<TurnStartEvent>(OnTurnStarted);
        eventBus.Subscribe<TimerUpdatedEvent>(OnTurnTimerUpdated);
        eventBus.Subscribe<TimerUpdatedEvent>(OnAuctionTimerUpdated);
        eventBus.Subscribe<TimerUpdatedEvent>(OnTradeTimerUpdated);

        eventBus.Subscribe<OnTakeLoanEvent>(OnLoanUpdated);

        isSubscribed = true;
    }

    private void UnsubscribeEvents()
    {
        if (!isSubscribed) return;

        eventBus.Unsubscribe<OnUpdatePlayerMoneyEvent>(OnMoneyUpdate);
        eventBus.Unsubscribe<TurnStartEvent>(OnTurnStarted);
        eventBus.Unsubscribe<TimerUpdatedEvent>(OnTurnTimerUpdated);
        eventBus.Unsubscribe<TimerUpdatedEvent>(OnAuctionTimerUpdated);
        eventBus.Unsubscribe<TimerUpdatedEvent>(OnTradeTimerUpdated);

        eventBus.Unsubscribe<OnTakeLoanEvent>(OnLoanUpdated);

        isSubscribed = false;
    }

    #endregion PRIVATE_METHODS

    #region CALLBACKS

    private void OnMoneyUpdate(OnUpdatePlayerMoneyEvent e)
    {
        if (playerData.Id == e.Player.Id)
            view.SetMoney(e.Player.Money);
    }

    private void OnTurnTimerUpdated(TimerUpdatedEvent e)
    {
        if (e.Type != TimerType.Turn) return;
        if (playerData.Id == e.PlayerId)
        {
            view.SetTimer(e.IsActive, e.TimeLeft, true, false);
        }
        else
        {
            view.SetTurnHighlightVisible(false);
            view.SetTimer(false, 0, false, false);
        }
    }

    private void OnAuctionTimerUpdated(TimerUpdatedEvent e)
    {
        if (e.Type != TimerType.Auction) return;
        if (playerData.Id == e.PlayerId)
        {
            view.SetTimer(e.IsActive, e.TimeLeft, false, true);
        }
        else
        {
            view.SetTurnHighlightVisible(false);
            view.SetTimer(false, 0, false, false);
        }
    }

    private void OnTradeTimerUpdated(TimerUpdatedEvent e)
    {
        if (e.Type != TimerType.Trade) return;
        if (playerData.Id == e.PlayerId)
        {
            view.SetTimer(e.IsActive, e.TimeLeft, false, true);
        }
        else
        {
            view.SetTurnHighlightVisible(false);
            view.SetTimer(false, 0, false, false);
        }
    }

    private void OnLoanUpdated(OnTakeLoanEvent e)
    {
        if (e.PlayerData.Id == playerData.Id)
            view.SetLoan(e.PlayerData.HasLoan, e.PlayerData.LoanTurnsLeft, e.PlayerData.photonPlayer.IsLocal);
    }

    private void OnTurnStarted(TurnStartEvent e)
    {
        if (playerData == null) return;

        int currentTurnPlayerId = e.PlayerId;
        int localPlayerId = localPlayerService.GetLocalPlayerId();

        if (currentTurnPlayerId != localPlayerId)
        {
            view.SetTradeButtonVisible(false);
            view.SetLoanButtonsVisible(false, false);
            return;
        }

        if (playerData.Id == localPlayerId)
        {
            view.SetTradeButtonVisible(false);

            if (!playerData.HasLoan)
            {
                view.SetLoanButtonsVisible(true, false);
            }
            else
            {
                view.SetLoanButtonsVisible(false, true);
            }
        }
        else
        {
            view.SetTradeButtonVisible(true);

            if (!playerData.HasLoan)
            {
                view.SetLoanButtonsVisible(false, false);
            }
        }
    }

    private void OnTradeClick()
    {
        photonTradeManager.SendTradeRequest(localPlayerService.GetLocalPlayerId(), playerData.Id);
    }

    #endregion CALLBACKS
}
