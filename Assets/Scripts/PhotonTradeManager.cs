using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Zenject;

public class PhotonTradeManager : MonoBehaviourPun, IPhotonTradeManager
{
    private ITradeService tradeService;
    private ICompanyRepository companyRepository;
    private IEventBus eventBus;

    [Inject]
    public void Construct(ITradeService tradeService, ICompanyRepository companyRepository, IEventBus eventBus)
    {
        this.tradeService = tradeService;
        this.companyRepository = companyRepository;
        this.eventBus = eventBus;
    }

    public void SendTradeRequest(int fromPlayerId, int toPlayerId)
    {
        photonView.RPC(nameof(RPC_StartTradeRequest), RpcTarget.All, fromPlayerId, toPlayerId);
    }

    public void SendTradeProposal(TradeOffer offer)
    {
        photonView.RPC(nameof(RPC_SendTradeProposal), RpcTarget.MasterClient,
            offer.FromPlayerData.Id, offer.ToPlayerData.Id,
            JsonUtility.ToJson(new CompanyIdListWrapper { Ids = offer.FromCompanies.ConvertAll(c => c.Id) }),
            JsonUtility.ToJson(new CompanyIdListWrapper { Ids = offer.ToCompanies.ConvertAll(c => c.Id) }),
            offer.FromMoney, offer.ToMoney
        );
    }
    public void CompleteTrade(bool accepted)
    {
        photonView.RPC(nameof(RPC_CompleteTrade), RpcTarget.All, accepted);
    }
    public void SendTradeResult(bool accepted)
    {
        photonView.RPC(nameof(RPC_CompleteTrade), RpcTarget.All, accepted);
    }

    public void UpdateTradeTimer(int playerId, float timeLeft)
    {
        photonView.RPC(nameof(RPC_UpdateTradeTimer), RpcTarget.All, playerId, timeLeft);
    }

    [PunRPC]
    private void RPC_StartTradeRequest(int fromPlayerId, int toPlayerId)
    {
        tradeService.StartTrade(fromPlayerId, toPlayerId);
    }
    public void ReceiveTradeProposal(TradeOffer offer)
    {
        // Вызываем RPC на всех для получения трейда
        photonView.RPC(nameof(RPC_SendTradeProposal), RpcTarget.All,
            offer.FromPlayerData.Id,
            offer.ToPlayerData.Id,
            JsonUtility.ToJson(new CompanyIdListWrapper { Ids = offer.FromCompanies.ConvertAll(c => c.Id) }),
            JsonUtility.ToJson(new CompanyIdListWrapper { Ids = offer.ToCompanies.ConvertAll(c => c.Id) }),
            offer.FromMoney,
            offer.ToMoney
        );
    }
    [PunRPC]
    private void RPC_SendTradeProposal(int fromId, int toId, string fromJson, string toJson, int fromMoney, int toMoney)
    {
        var offer = new TradeOffer(fromId, toId)
        {
            FromMoney = fromMoney,
            ToMoney = toMoney
        };
        offer.SetFromCompanies(DeserializeCompanies(fromJson));
        offer.SetToCompanies(DeserializeCompanies(toJson));

        tradeService.OnTradeProposalReceived(offer);
    }

    [PunRPC]
    private void RPC_CompleteTrade(bool accepted)
    {
        tradeService.OnTradeCompleted(accepted);
    }

    [PunRPC]
    private void RPC_UpdateTradeTimer(int playerId, float timeLeft)
    {
        eventBus.Publish(new TradeTimerUpdatedEvent(playerId, timeLeft));
    }

    private List<Company> DeserializeCompanies(string json)
    {
        if (string.IsNullOrEmpty(json)) return new List<Company>();
        var wrapper = JsonUtility.FromJson<CompanyIdListWrapper>(json);
        var result = new List<Company>();
        foreach (var id in wrapper.Ids)
        {
            var company = companyRepository.GetCompanyById(id);
            if (company != null) result.Add(company);
        }
        return result;
    }

    [Serializable]
    private class CompanyIdListWrapper
    {
        public List<int> Ids = new List<int>();
    }
}
