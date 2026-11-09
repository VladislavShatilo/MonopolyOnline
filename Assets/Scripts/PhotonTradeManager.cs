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
    private IPhotonViewWrapper photonViewWrapper;
    #region LIFE_CYCLE

    [Inject]
    public void Construct(ITradeService tradeService, ICompanyRepository companyRepository, IPlayerRepository playerRepository, IPhotonViewWrapper photonViewWrapper)
    {
        this.tradeService = tradeService ?? throw new ArgumentNullException(nameof(tradeService));
        this.companyRepository = companyRepository ?? throw new ArgumentNullException(nameof(companyRepository));
        this.playerRepository = playerRepository ?? throw new ArgumentNullException(nameof(playerRepository));
        this.photonViewWrapper = photonViewWrapper ?? throw new ArgumentNullException(nameof(photonViewWrapper));
        if (photonView == null) throw new NullReferenceException(nameof(photonView));

    }

    #endregion LIFE_CYCLE

    #region PUBLIC_METHODS

    public void SendTradeRequest(int fromPlayerId, int toPlayerId)
    {
        photonViewWrapper.RPC(photonView,nameof(RPC_StartTradeRequest), RpcTarget.All, fromPlayerId, toPlayerId);
        
    }

    public void SendTradeOffer(TradeOffer offer)
    {
        photonViewWrapper.RPC(photonView, nameof(RPC_SendTradeOffer), RpcTarget.All,
            offer.FromPlayerData.Id, offer.ToPlayerData.Id,
            JsonUtility.ToJson(new CompanyIdListWrapper { Ids = offer.FromCompanies.ConvertAll(c => c.Id) }),
            JsonUtility.ToJson(new CompanyIdListWrapper { Ids = offer.ToCompanies.ConvertAll(c => c.Id) }),
            offer.FromMoney, offer.ToMoney);

       
    }

    public void CompleteTrade(bool accepted)
    {
        photonViewWrapper.RPC(photonView, nameof(RPC_CompleteTrade), RpcTarget.All, accepted);
    }

    public void SendTradeResult(bool accepted)
    {
        photonViewWrapper.RPC(photonView, nameof(RPC_CompleteTrade), RpcTarget.All, accepted);

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
            var company = companyRepository.GetCompanyById(id) ?? throw new NullReferenceException(nameof(DeserializeCompanies));
            result.Add(company);
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
        PlayerData playerFrom = playerRepository.GetPlayerById(fromId) ?? throw new NullReferenceException(nameof(RPC_SendTradeOffer)); 
        PlayerData playerTo = playerRepository.GetPlayerById(toId) ?? throw new NullReferenceException(nameof(RPC_SendTradeOffer));
        var offer = new TradeOffer(playerFrom, playerTo)
        {
            FromMoney = fromMoney,
            ToMoney = toMoney
        };
        if(offer != null)
        {
            offer.SetFromCompanies(DeserializeCompanies(fromJson));
            offer.SetToCompanies(DeserializeCompanies(toJson));
        }
      

        tradeService.OnTradeProposalReceived(offer);
    }

    [PunRPC]
    private void RPC_CompleteTrade(bool accepted)
    {
        tradeService.OnTradeCompleted(accepted);
    }

    #endregion RPC
}