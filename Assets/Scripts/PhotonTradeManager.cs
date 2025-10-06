using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Zenject;

public class PhotonTradeManager : MonoBehaviourPun, IPhotonTradeManager
{
    [Serializable]
    private class CompanyIdListWrapper
    {
        public List<int> Ids = new();
    }

    private ITradeService tradeService;
    private ICompanyRepository companyRepository;
    private IPlayerRepository playerRepository;

    #region LIFE_CYCLE

    [Inject]
    public void Construct(ITradeService tradeService, ICompanyRepository companyRepository, IPlayerRepository playerRepository)
    {
        this.tradeService = tradeService;
        this.companyRepository = companyRepository;
        this.playerRepository = playerRepository;
    }

    #endregion LIFE_CYCLE

    #region PUBLIC_METHODS

    public void SendTradeRequest(int fromPlayerId, int toPlayerId)
    {
        photonView.RPC(nameof(RPC_StartTradeRequest), RpcTarget.All, fromPlayerId, toPlayerId);
    }

    public void SendTradeOffer(TradeOffer offer)
    {
        photonView.RPC(nameof(RPC_SendTradeOffer), RpcTarget.All,
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

    #endregion PUBLIC_METHODS

    #region PRIVATE_METHODS

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

    #endregion PRIVATE_METHODS

    #region RPC

    [PunRPC]
    private void RPC_StartTradeRequest(int fromPlayerId, int toPlayerId)
    {
        tradeService.StartTrade(fromPlayerId, toPlayerId);
    }

    [PunRPC]
    private void RPC_SendTradeOffer(int fromId, int toId, string fromJson, string toJson, int fromMoney, int toMoney)
    {
        PlayerData playerFrom = playerRepository.GetPlayerById(fromId);
        PlayerData playerTo = playerRepository.GetPlayerById(toId);
        var offer = new TradeOffer(playerFrom, playerTo)
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

    #endregion RPC
}