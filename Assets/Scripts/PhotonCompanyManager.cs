using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class PhotonCompanyManager : MonoBehaviourPun, IPhotonCompanyManager
{
    private ICompanyService companyService;
   
    [Inject]
    public void Construct(ICompanyService companyService)
    {
        this.companyService = companyService;
      
    }

    [PunRPC]
    private void RPC_TryBuyCompany(int cellIndex, int playerId, int reason)
    {
        companyService.TryBuyCompany(cellIndex, playerId, (BuyReason)reason);
    }

    [PunRPC]
    private void RPC_TryPayRent(int cellIndex, int playerId)
    {
        companyService.TryPayRent(cellIndex, playerId);
    }
  

    public void RequestBuyCompany(int cellIndex, int playerId, BuyReason reason)
    {
        photonView.RPC(nameof(RPC_TryBuyCompany), RpcTarget.MasterClient, cellIndex, playerId, (int)reason);
    }

    public void RequestPayRent(int cellIndex, int playerId)
    {
        photonView.RPC(nameof(RPC_TryPayRent), RpcTarget.MasterClient, cellIndex, playerId);
    }
}
