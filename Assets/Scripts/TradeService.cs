using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class TradeService : ITradeService,IInitializable,IDisposable
{
    private IPhotonTradeManager photonTradeManager;
    private IBankService bankService;
    private IEventBus eventBus;
    private IPlayerRepository playerRepository;
    private ITimerManager timerManager;
    private GameSettings gameSettings;
    private ITurnPresenter turnPresenter;
    private int turnTimeLeft;

    public TradeOffer CurrentOffer { get; private set; }
    public bool IsTradeActive { get; private set; }
    private int senderId;
    private int receiverId;

    #region LIFE_CYCLE

    [Inject]
    public void Construct(IPhotonTradeManager photonTradeManager, IBankService bankService, IEventBus eventBus, IPlayerRepository playerRepository
       , ITimerManager timerManager, GameSettings gameSettings, ITurnPresenter turnPresenter)
    {
        this.photonTradeManager = photonTradeManager;
        this.bankService = bankService;
        this.eventBus = eventBus;
        this.playerRepository = playerRepository;
        this.timerManager = timerManager;
        this.gameSettings = gameSettings;
        this.turnPresenter = turnPresenter;
    }
    void IInitializable.Initialize()
    {
        eventBus.Subscribe<TimerExpiredEvent>(TimerExpiredEvent);
    }
    void IDisposable.Dispose()
    {
        eventBus.Subscribe<TimerExpiredEvent>(TimerExpiredEvent);
    }

    #endregion LIFE_CYCLE

    #region PUBLIC_METHODS

    public void StartTrade(int fromPlayerId, int toPlayerId)
    {
        CancelTrade();
        senderId = fromPlayerId;
        receiverId = toPlayerId;
        IsTradeActive = true;
        PlayerData playerFrom = playerRepository.GetPlayerById(fromPlayerId);
        PlayerData playerTo = playerRepository.GetPlayerById(toPlayerId);
        CurrentOffer = new TradeOffer(playerFrom, playerTo);
        eventBus.Publish(new TradeStartedEvent(fromPlayerId, toPlayerId, CurrentOffer));
        //  photonTradeManager.SendTradeRequest(fromPlayerId, toPlayerId);
    }

    public void TimerExpiredEvent(TimerExpiredEvent e)
    {
        if (e.Type != TimerType.Trade) return;

        photonTradeManager.SendTradeResult(false);

    }

    public void CancelTrade()
    {
        CurrentOffer = null;
        IsTradeActive = false;
        senderId = -1;
        receiverId = -1;
    }

    public void AddCompanyToOffer(int playerId, Company company)
    {
        if (CurrentOffer == null) return;
        if (playerId == CurrentOffer.FromPlayerData.Id) CurrentOffer.FromCompanies.Add(company);
        else if (playerId == CurrentOffer.ToPlayerData.Id) CurrentOffer.ToCompanies.Add(company);

        eventBus.Publish(new TradeUpdatedEvent(CurrentOffer));

    }

    public void RemoveCompanyFromOffer(int playerId, Company company)
    {
        if (CurrentOffer == null) return;
        if (playerId == CurrentOffer.FromPlayerData.Id) CurrentOffer.FromCompanies.Remove(company);
        else if (playerId == CurrentOffer.ToPlayerData.Id) CurrentOffer.ToCompanies.Remove(company);

        eventBus.Publish(new TradeUpdatedEvent(CurrentOffer));

    }

    public void OnTradeProposalReceived(TradeOffer offer)
    {
        CurrentOffer = offer;
        IsTradeActive = true;
        senderId = offer.FromPlayerData.Id;
        receiverId = offer.ToPlayerData.Id;

        var tick = timerManager.Tick();
        if (tick.HasValue)
        {
            turnTimeLeft = (int)tick.Value.timeLeft;
        }
        else
        {
            // таймер неактивен → можно задать запасное значение
            turnTimeLeft = gameSettings.turnTime;
        }
        timerManager.StartTradeTimer(receiverId, gameSettings.tradeTime);
        eventBus.Publish(new TradeProposalReceivedEvent(offer, senderId, receiverId));
    }

    public void OnTradeCompleted(bool accepted)
    {
        if (accepted && CurrentOffer != null)
        {
            ApplyTrade(CurrentOffer);
        }
        timerManager.StartTurnTimer(senderId, turnTimeLeft);
        turnPresenter.ShowTurnFor(senderId);
        eventBus.Publish(new TradeEndedEvent(accepted, senderId, receiverId));
        CancelTrade();
    }


    #endregion PUBLIC_METHODS

    #region PRIVATE_METHODS

    private void ApplyTrade(TradeOffer offer)
    {

        foreach (var c in offer.FromCompanies)
        {
            c.TransferTo(receiverId);
            eventBus.Publish(new CompanyBoughtEvent(c.Id, receiverId));
            eventBus.Publish(new HideButtonsTradeEvent(c.Id));


        }
        foreach (var c in offer.ToCompanies)
        {
            c.TransferTo(senderId);
            eventBus.Publish(new CompanyBoughtEvent(c.Id, senderId));
            eventBus.Publish(new HideButtonsTradeEvent(c.Id));

        }


        if (PhotonNetwork.IsMasterClient)
        {

            bankService.RemoveMoney(senderId, offer.FromMoney);
            bankService.AddMoney(receiverId, offer.FromMoney);

            bankService.RemoveMoney(receiverId, offer.ToMoney);
            bankService.AddMoney(senderId, offer.ToMoney);
        }

    }

    #endregion PRIVATE_METHODS

}