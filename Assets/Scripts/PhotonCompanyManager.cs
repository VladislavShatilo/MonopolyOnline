using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class PhotonCompanyManager : MonoBehaviourPun, IPhotonCompanyManager
{
    private ICompanyService companyService;
    private IPhotonViewWrapper photonViewWrapper;

    #region LIFE_CYCLE

    [Inject]
    public void Construct(ICompanyService companyService, IPhotonViewWrapper photonViewWrapper)
    {
        this.companyService = companyService ?? throw new ArgumentNullException(nameof(companyService));
        this.photonViewWrapper = photonViewWrapper ?? throw new ArgumentNullException(nameof(photonViewWrapper));
        if (photonView == null) throw new NullReferenceException(nameof(photonView));


    }

    #endregion LIFE_CYCLE

    #region PUBLIC_METHODS

    public void RequestBuyCompany(int cellIndex, int playerId, BuyReason reason)
    {
        photonViewWrapper.RPC(photonView, nameof(RPC_TryBuyCompany), RpcTarget.MasterClient, cellIndex, playerId, (int)reason);
    }

    public void RequestPayRent(int cellIndex, int playerId)
    {
        photonViewWrapper.RPC(photonView, nameof(RPC_TryPayRent), RpcTarget.MasterClient, cellIndex, playerId);
    }

    #endregion PUBLIC_METHODS

    #region RPC

    [PunRPC]
    private void RPC_TryBuyCompany(int cellIndex, int playerId, int reason)
    {
        companyService.TryBuyCompany(cellIndex, playerId, 0, (BuyReason)reason);
    }

    [PunRPC]
    private void RPC_TryPayRent(int cellIndex, int playerId)
    {
        companyService.TryPayRent(cellIndex, playerId);
    }

    #endregion RPC


}
