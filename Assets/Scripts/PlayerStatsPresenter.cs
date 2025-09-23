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

    private PlayerData playerData;
    private bool _isSubscribed = false;

    public PlayerStatsPresenter(IPlayerStatsView view, IPhotonLoanManager photonLoanManager, IEventBus eventBus, ILocalPlayerService localPlayerService)
    {
        this.view = view;
        this.eventBus = eventBus;
        this.localPlayerService = localPlayerService;

        // Привязка кнопок
        view.BindTradeAction(OnTrade);
        view.BindTakeLoanAction(() => photonLoanManager.SendTakeLoan(playerData.Id));
        view.BindPayLoanAction(() => photonLoanManager.SendPayLoan(playerData.Id));

       
    }
    private void SubscribeEvents()
    {
        if (_isSubscribed) return;

        eventBus.Subscribe<OnUpdatePlayerMoneyEvent>(OnMoneyUpdate);
        eventBus.Subscribe<TurnStartEvent>(OnTurnStarted);
        eventBus.Subscribe<TimerUpdatedEvent>(OnTurnTimerUpdated);
        eventBus.Subscribe<TimerUpdatedEvent>(OnAuctionTimerUpdated);
        eventBus.Subscribe<OnTakeLoanEvent>(OnLoanUpdated);

        _isSubscribed = true;
    }

    private void UnsubscribeEvents()
    {
        if (!_isSubscribed) return;

        eventBus.Unsubscribe<OnUpdatePlayerMoneyEvent>(OnMoneyUpdate);
        eventBus.Unsubscribe<TurnStartEvent>(OnTurnStarted);
        eventBus.Unsubscribe<TimerUpdatedEvent>(OnTurnTimerUpdated);
        eventBus.Unsubscribe<TimerUpdatedEvent>(OnAuctionTimerUpdated);
        eventBus.Unsubscribe<OnTakeLoanEvent>(OnLoanUpdated);

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
        view.SetLoanButtonsVisible(false,false);
        view.SetTradeButtonVisible(false);
        view.SetAuctionHighlightVisible(false);
        view.SetTurnHighlightVisible(false);
        view.SetLoanContainerVisible(false);
        view.SetTimerGOVisible(false);
        SubscribeEvents();
    }

    private void OnMoneyUpdate(OnUpdatePlayerMoneyEvent e)
    {
        if (playerData.Id == e.Player.Id)
            view.SetMoney(e.Player.Money);
    }

    private void OnTurnTimerUpdated(TimerUpdatedEvent e)
    {
        if(e.Type != TimerType.Turn) return;
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
        Debug.Log("OnAuctionTimerUpdated");
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
        // 1. Если это НЕ мой ход  скрыть все кнопки
        if (currentTurnPlayerId != localPlayerId)
        {
           
            view.SetTradeButtonVisible(false);
            view.SetLoanButtonsVisible(false, false);
            return;
        }

        // 2. Если это МОЙ ход  показывать кнопки только на других игроков
        if (playerData.Id == localPlayerId)
        {
            view.SetTradeButtonVisible(false);

            if (!playerData.HasLoan)
            {
                view.SetLoanButtonsVisible(true, false);
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

    private void OnTrade()
    {
       // tradeService.SendTradeRequest(playerData.Id);
    }
}

public class AuctionTimerUpdatedEvent
{
    public int PlayerId { get; }
    public float TimeLeft { get; }
    public AuctionTimerUpdatedEvent(int playerId, float timeLeft)
    {
        PlayerId = playerId;
        TimeLeft = timeLeft;
    }
}
public class OnTakeLoanEvent
{
    public PlayerData PlayerData;

    public OnTakeLoanEvent(PlayerData playerData)
    {
        PlayerData = playerData;
    }
}
public class OnStartTurnLoanEvent
{
    public int PlayerId;

    public OnStartTurnLoanEvent(int playerId)
    {
        PlayerId = playerId;
    }
}