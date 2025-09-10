using Photon.Pun;
using System;
using System.Collections.Generic;
using UnityEngine;

public class TradeManager : MonoBehaviourPun
{
    public static TradeManager Instance { get; private set; }

    public TradeOffer CurrentOffer => currentOffer;
    public bool IsTradeActive => isTradeActive;

    private TradeOffer currentOffer;
    private bool isTradeActive = false;

    private int senderId;
    private int receiverId;
    private double tradeStartTime;
    private const float TRADE_DURATION = 15f;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }
    private void OnEnable()
    {
        EventBus.Subscribe<OfferTradeEvent>(_ => OfferTrade());
        EventBus.Subscribe<CancelTradeEvent>(_ => CancelTrade());
        EventBus.Subscribe<StartTradeRequestEvent>(StartTradeRequest);

    }
    private void OnDisable()
    {
        EventBus.Unsubscribe<OfferTradeEvent>(_ => OfferTrade());
        EventBus.Unsubscribe<CancelTradeEvent>(_ => CancelTrade());
        EventBus.Unsubscribe<StartTradeRequestEvent>(StartTradeRequest);

    }
    private void Update()
    {
        if (!PhotonNetwork.IsMasterClient || !isTradeActive) return;

        if (tradeStartTime <= 0) return; // Таймер ещё не запущен

        double elapsed = PhotonNetwork.Time - tradeStartTime;
        float timeLeft = Mathf.Max(0f, TRADE_DURATION - (float)elapsed);

        photonView.RPC(nameof(RPC_UpdateTradeTimer), RpcTarget.All, receiverId, timeLeft);

        if (timeLeft <= 0f)
            DeclineTrade(); // Авто-отказ
    }

    #region Start / Offer

    public void StartTradeRequest(StartTradeRequestEvent e)
    {
        EventBus.Publish(new CancelTradeEvent());
        photonView.RPC(nameof(RPC_StartTradeRequest), RpcTarget.All, e.FromId, e.ToId);
    }
    [PunRPC]
    private void RPC_StartTradeRequest(int fromPlayerId, int toPlayerId)
    {
        senderId = fromPlayerId;
        receiverId = toPlayerId;
        isTradeActive = true;
        currentOffer = new TradeOffer(senderId, receiverId);

        EventBus.Publish(new TradeStartedEvent(senderId, receiverId, currentOffer));
    }
    public void UpdateMoneyFromUI(int amount, UIMoneyTrade source)
    {
        if (currentOffer == null) return;

        // Определяем, чей это ввод: слева или справа
        if (source.CompareTag("LeftMoney")) // нужно проставить тег на UIMoneyTrade слева
        {
            currentOffer.FromMoney = amount;
        }
        else if (source.CompareTag("RightMoney")) // тег справа
        {
            currentOffer.ToMoney = amount;
        }

        // Обновляем окно трейда
        UITradeWindow.Instance.RefreshUI();
        EventBus.Publish(new TradeUpdatedEvent(currentOffer));
    }
    public void OfferTrade()
    {
        if (currentOffer == null || !currentOffer.IsValid()) return;

        // Отправка всем через MasterClient
        photonView.RPC(nameof(RPC_SendOfferToAll), RpcTarget.MasterClient,
            currentOffer.FromPlayerData.Id,
            currentOffer.ToPlayerData.Id,
            SerializeCompanies(currentOffer.FromCompanies),
            SerializeCompanies(currentOffer.ToCompanies),
            currentOffer.FromMoney,
            currentOffer.ToMoney);
    }

    [PunRPC]
    private void RPC_SendOfferToAll(int fromPlayerId, int toPlayerId, string fromCompaniesJson, string toCompaniesJson, int fromMoney, int toMoney)
    {
        photonView.RPC(nameof(RPC_ReceiveTradeProposal), RpcTarget.All, fromPlayerId, toPlayerId, fromCompaniesJson, toCompaniesJson, fromMoney, toMoney);
    }

    [PunRPC]
    private void RPC_ReceiveTradeProposal(int fromPlayerId, int toPlayerId,
        string fromCompaniesJson, string toCompaniesJson, int fromMoney, int toMoney)
    {
        Debug.Log("RPC_ReceiveTradeProposal");
        currentOffer = new TradeOffer(fromPlayerId, toPlayerId)
        {
            FromMoney = fromMoney,
            ToMoney = toMoney
        };
        currentOffer.SetFromCompanies(DeserializeCompanies(fromCompaniesJson));
        currentOffer.SetToCompanies(DeserializeCompanies(toCompaniesJson));
       // TurnManager.Instance.SetMode(TurnMode.Trade);

        EventBus.Publish(new TradeProposalReceivedEvent(currentOffer,fromPlayerId,toPlayerId));

        // Таймер стартует только у получателя
       // if (PhotonNetwork.LocalPlayer.ActorNumber == toPlayerId)
        {
            tradeStartTime = PhotonNetwork.Time;
        }
    }

    #endregion

    #region Accept / Decline

    public void AcceptTrade()
    {
        if (currentOffer == null) return;

        photonView.RPC(nameof(RPC_CompleteTrade), RpcTarget.All, true);
    }

    // Отклонить сделку
    public void DeclineTrade()
    {
        photonView.RPC(nameof(RPC_CompleteTrade), RpcTarget.All, false);
    }
    [PunRPC]
    private void RPC_CompleteTrade(bool accepted)
    {
        if (accepted)
        {
          

            Bank.Instance.RemoveMoney(senderId, currentOffer.FromMoney);
            Bank.Instance.AddMoney(receiverId, currentOffer.FromMoney);

            Bank.Instance.RemoveMoney(receiverId, currentOffer.ToMoney);
            Bank.Instance.AddMoney(senderId, currentOffer.ToMoney);

            ApplyTrade(currentOffer);
        }

        //TurnManager.Instance.SetMode(TurnMode.Normal);
        EventBus.Publish(new TradeTimerUpdatedEvent(-1, 0));
        EventBus.Publish(new TradeCancelledEvent());
        // Сброс всего состояния трейда
        currentOffer = null;
        isTradeActive = false;
        senderId = -1;
        receiverId = -1;
        tradeStartTime = 0;

        UITradeReviewWindow.Instance.HideWindow();
    }
    private void ApplyTrade(TradeOffer offer)
    {

        // Передать компании
        foreach (var c in offer.FromCompanies)
            c.TransferTo(receiverId);

        foreach (var c in offer.ToCompanies)
            c.TransferTo(senderId);
    }


    #endregion

    #region Offer Manipulation

    public void AddCompanyToOffer(int companyOwnerId, Company company)
    {
        if (currentOffer == null) return;

        if (companyOwnerId == currentOffer.FromPlayerData.Id) currentOffer.FromCompanies.Add(company);
        else if (companyOwnerId == currentOffer.ToPlayerData.Id) currentOffer.ToCompanies.Add(company);
        else return;

        EventBus.Publish(new TradeUpdatedEvent(currentOffer));
    }

    public void RemoveCompanyFromOffer(int companyOwnerId, Company company)
    {
        if (currentOffer == null) return;

        if (companyOwnerId == currentOffer.FromPlayerData.Id) currentOffer.FromCompanies.Remove(company);
        else if (companyOwnerId == currentOffer.ToPlayerData.Id) currentOffer.ToCompanies.Remove(company);
        else return;

        EventBus.Publish(new TradeUpdatedEvent(currentOffer));
    }

    public void SetMoney(int playerId, int amount)
    {
        if (currentOffer == null) return;

        if (playerId == currentOffer.FromPlayerData.Id) currentOffer.FromMoney = amount;
        else if (playerId == currentOffer.ToPlayerData.Id) currentOffer.ToMoney = amount;

        EventBus.Publish(new TradeUpdatedEvent(currentOffer));
    }

    public void CancelTrade()
    {
        currentOffer = null;
        isTradeActive = false;
        tradeStartTime = 0;
    }

    #endregion

    #region Timer RPC

    [PunRPC]
    private void RPC_UpdateTradeTimer(int currentReceiverId, float timeLeft)
    {
        EventBus.Publish(new TradeTimerUpdatedEvent(currentReceiverId, timeLeft));
    }

    #endregion

    #region Serialization Helpers

    public string SerializeCompanies(List<Company> companies)
    {
        var ids = new List<int>();
        foreach (var c in companies)
            ids.Add(c.Id);

        return JsonUtility.ToJson(new CompanyIdListWrapper { Ids = ids });
    }

    private List<Company> DeserializeCompanies(string json)
    {
        if (string.IsNullOrEmpty(json))
            return new List<Company>();

        var wrapper = JsonUtility.FromJson<CompanyIdListWrapper>(json);
        var result = new List<Company>();

        foreach (var id in wrapper.Ids)
        {
            //var company = CompanyDatabase.Instance.GetCompanyById(id);
            //if (company != null)
            //    result.Add(company);
        }

        return result;
    }

    [Serializable]
    private class CompanyIdListWrapper
    {
        public List<int> Ids = new List<int>();
    }

    #endregion
}

#region Events

public class TradeStartedEvent
{
    public int FromPlayerId { get; }
    public int ToPlayerId { get; }
    public TradeOffer Offer { get; }
    public TradeStartedEvent(int from, int to, TradeOffer offer) { FromPlayerId = from; ToPlayerId = to; Offer = offer; }
}

public class TradeUpdatedEvent
{
    public TradeOffer Offer { get; }
    public TradeUpdatedEvent(TradeOffer offer) { Offer = offer; }
}

public class TradeCancelledEvent { }

public class TradeProposalReceivedEvent
{
    public TradeOffer Offer { get; }
    public int FromPlayerId { get; }
    public int ToPlayerId { get; }
    public TradeProposalReceivedEvent(TradeOffer offer, int from, int to) 
    { 
        Offer = offer;
        FromPlayerId = from; 
        ToPlayerId = to;
    }
}

public class TradePromptEvent
{
    public int SenderId { get; }
    public int ReceiverId { get; }
    public TradePromptEvent(int senderId, int receiverId) { SenderId = senderId; ReceiverId = receiverId; }
}

public class TradeTimerUpdatedEvent
{
    public int PlayerId { get; }
    public float TimeLeft { get; }
    public TradeTimerUpdatedEvent(int playerId, float timeLeft) { PlayerId = playerId; TimeLeft = timeLeft; }
}

public class TradeEndedEvent
{
    public bool Accepted { get; }
    public int SenderId { get; }
    public int ReceiverId { get; }
    public TradeEndedEvent(bool accepted, int senderId, int receiverId) { Accepted = accepted; SenderId = senderId; ReceiverId = receiverId; }
}

#endregion
