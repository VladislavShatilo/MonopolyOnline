using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class PlayerStatsPresenter
{
    private IPlayerStatsView playerStatsView;
    private IEventBus eventBus;
    private ILocalPlayerService localPlayerService;
    private IPhotonTradeManager photonTradeManager;
    private PlayerData playerData;
    private bool isSubscribed = false;

    #region LIFE_CYCLE

    public PlayerStatsPresenter(IPlayerStatsView playerStatsView, IPhotonLoanManager photonLoanManager, IEventBus eventBus, ILocalPlayerService localPlayerService, IPhotonTradeManager photonTradeManager)
    {
        this.playerStatsView = playerStatsView ?? throw new ArgumentNullException(nameof(playerStatsView));
        this.eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
        this.localPlayerService = localPlayerService ?? throw new ArgumentNullException(nameof(localPlayerService));
        this.photonTradeManager = photonTradeManager ?? throw new ArgumentNullException(nameof(photonTradeManager));

        // ѕрив€зка кнопок
        playerStatsView.BindTradeAction(OnTradeClick);
        playerStatsView.BindTakeLoanAction(() =>
        {
            if (playerData != null)
                photonLoanManager.TakeLoanRequest(playerData.Id);
        });

        playerStatsView.BindPayLoanAction(() =>
        {
            if (playerData != null)
                photonLoanManager.PayLoanRequest(playerData.Id);
        });
    }

    public void Dispose()
    {
        UnsubscribeEvents();
    }

    #endregion LIFE_CYCLE

    #region PUBLIC_METHODS

    public void Init(PlayerData data)
    {
        if (data == null) return;

        playerData = data;  // присваиваем текущим игроком
        playerStatsView.SetName(data.Name);
        playerStatsView.SetMoney(data.Money);
        playerStatsView.SetCapital(data.VisibleCapital, data.LiquidAssets);
        playerStatsView.SetLeaveButtonVisible(data.photonPlayer.IsLocal);
        playerStatsView.SetLoanButtonsVisible(false, false);
        playerStatsView.SetTradeButtonVisible(false);
        playerStatsView.SetAuctionHighlightVisible(false);
        playerStatsView.SetTurnHighlightVisible(false);
        playerStatsView.SetLoanContainerVisible(false);
        playerStatsView.SetTimerGOVisible(false);
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
            playerStatsView.SetMoney(e.Player.Money);
    }

    private void OnTurnTimerUpdated(TimerUpdatedEvent e)
    {
        if (e.Type != TimerType.Turn) return;
        if (playerData.Id == e.PlayerId)
        {
            playerStatsView.SetTimer(e.IsActive, e.TimeLeft, true, false);
        }
        else
        {
            playerStatsView.SetTurnHighlightVisible(false);
            playerStatsView.SetTimer(false, 0, false, false);
        }
    }

    private void OnAuctionTimerUpdated(TimerUpdatedEvent e)
    {
        if (e.Type != TimerType.Auction) return;
        if (playerData.Id == e.PlayerId)
        {
            playerStatsView.SetTimer(e.IsActive, e.TimeLeft, false, true);
        }
        else
        {
            playerStatsView.SetTurnHighlightVisible(false);
            playerStatsView.SetTimer(false, 0, false, false);
        }
    }

    private void OnTradeTimerUpdated(TimerUpdatedEvent e)
    {
        if (e.Type != TimerType.Trade) return;
        if (playerData.Id == e.PlayerId)
        {
            playerStatsView.SetTimer(e.IsActive, e.TimeLeft, false, true);
        }
        else
        {
            playerStatsView.SetTurnHighlightVisible(false);
            playerStatsView.SetTimer(false, 0, false, false);
        }
    }

    private void OnLoanUpdated(OnTakeLoanEvent e)
    {
        if (e.PlayerData.Id == playerData.Id)
            playerStatsView.SetLoan(e.PlayerData.HasLoan, e.PlayerData.LoanTurnsLeft, e.PlayerData.photonPlayer.IsLocal);
    }

    private void OnTurnStarted(TurnStartEvent e)
    {
        if (playerData == null) return;

        int currentTurnPlayerId = e.PlayerId;
        int localPlayerId = localPlayerService.GetLocalPlayerId();

        if (currentTurnPlayerId != localPlayerId)
        {
            playerStatsView.SetTradeButtonVisible(false);
            playerStatsView.SetLoanButtonsVisible(false, false);
            return;
        }

        if (playerData.Id == localPlayerId)
        {
            playerStatsView.SetTradeButtonVisible(false);

            if (!playerData.HasLoan)
            {
                playerStatsView.SetLoanButtonsVisible(true, false);
            }
            else
            {
                playerStatsView.SetLoanButtonsVisible(false, true);
            }
        }
        else
        {
            playerStatsView.SetTradeButtonVisible(true);

            if (!playerData.HasLoan)
            {
                playerStatsView.SetLoanButtonsVisible(false, false);
            }
        }
    }

    private void OnTradeClick()
    {
        photonTradeManager.SendTradeRequest(localPlayerService.GetLocalPlayerId(), playerData.Id);
    }

    #endregion CALLBACKS
}
