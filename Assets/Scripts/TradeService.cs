using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class TradeService : ITradeService 
{
    private  IPhotonTradeManager photonTradeManager;
    private IBankService bankService;
    private IEventBus eventBus;
    private const float TRADE_DURATION = 15f;

    public TradeOffer CurrentOffer { get; private set; }
    public bool IsTradeActive { get; private set; }
    private int senderId;
    private int receiverId;
    private double tradeStartTime;

    [Inject]
    public void Construct(IPhotonTradeManager photonTradeManager, IBankService bankService, IEventBus eventBus)
    {
        this.photonTradeManager = photonTradeManager;
        this.bankService = bankService;
        this.eventBus = eventBus;
    }

    public void StartTrade(int fromPlayerId, int toPlayerId)
    {
        CancelTrade();
        senderId = fromPlayerId;
        receiverId = toPlayerId;
        IsTradeActive = true;
        CurrentOffer = new TradeOffer(senderId, receiverId);
        photonTradeManager.SendTradeRequest(fromPlayerId, toPlayerId);
    }

    public void OfferTrade()
    {
        if (CurrentOffer == null || !CurrentOffer.IsValid()) return;
        photonTradeManager.SendTradeProposal(CurrentOffer);
    }

    public void AcceptTrade() => photonTradeManager.SendTradeResult(true);
    public void DeclineTrade() => photonTradeManager.SendTradeResult(false);

    public void CancelTrade()
    {
        CurrentOffer = null;
        IsTradeActive = false;
        tradeStartTime = 0;
        senderId = -1;
        receiverId = -1;
    }

    public void AddCompanyToOffer(int playerId, Company company)
    {
        if (CurrentOffer == null) return;
        if (playerId == CurrentOffer.FromPlayerData.Id) CurrentOffer.FromCompanies.Add(company);
        else if (playerId == CurrentOffer.ToPlayerData.Id) CurrentOffer.ToCompanies.Add(company);
    }

    public void RemoveCompanyFromOffer(int playerId, Company company)
    {
        if (CurrentOffer == null) return;
        if (playerId == CurrentOffer.FromPlayerData.Id) CurrentOffer.FromCompanies.Remove(company);
        else if (playerId == CurrentOffer.ToPlayerData.Id) CurrentOffer.ToCompanies.Remove(company);
    }

    public void SetMoney(int playerId, int amount)
    {
        if (CurrentOffer == null) return;
        if (playerId == CurrentOffer.FromPlayerData.Id) CurrentOffer.FromMoney = amount;
        else if (playerId == CurrentOffer.ToPlayerData.Id) CurrentOffer.ToMoney = amount;
    }

    public void UpdateTimer()
    {
        if (!IsTradeActive || tradeStartTime <= 0) return;

        double elapsed = PhotonNetwork.Time - tradeStartTime;
        float timeLeft = Mathf.Max(0f, TRADE_DURATION - (float)elapsed);

        photonTradeManager.UpdateTradeTimer(receiverId, timeLeft);

        if (timeLeft <= 0f)
            DeclineTrade(); // Авто-отказ
    }

    // Вызывается из сетевого слоя
    public void OnTradeProposalReceived(TradeOffer offer)
    {
        CurrentOffer = offer;
        IsTradeActive = true;
        senderId = offer.FromPlayerData.Id;
        receiverId = offer.ToPlayerData.Id;
        tradeStartTime = PhotonNetwork.Time;
        eventBus.Publish(new TradeProposalReceivedEvent(offer, senderId, receiverId));
    }

    public void OnTradeCompleted(bool accepted)
    {
        if (accepted && CurrentOffer != null)
        {
            ApplyTrade(CurrentOffer);
        }
        eventBus.Publish(new TradeEndedEvent(accepted, senderId, receiverId));
        CancelTrade();
    }

    private void ApplyTrade(TradeOffer offer)
    {
        foreach (var c in offer.FromCompanies) c.TransferTo(receiverId);
        foreach (var c in offer.ToCompanies) c.TransferTo(senderId);

        bankService.RemoveMoney(senderId, offer.FromMoney);
        bankService.AddMoney(receiverId, offer.FromMoney);

        bankService.RemoveMoney(receiverId, offer.ToMoney);
        bankService.AddMoney(senderId, offer.ToMoney);

        
    }
}
