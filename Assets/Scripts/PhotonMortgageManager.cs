using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class PhotonMortgageManager : MonoBehaviourPun, IPhotonMortgageManager
{
    private IMortgageService mortgageService;
    private ICompanyUIService companyUIService;
    private IPhotonNetworkWrapper photonNetworkWrapper;
    private IPhotonViewWrapper photonViewWrapper;
    private ILocalPlayerService localPlayerService;
    private GameSettings gameSettings;

    #region LIFE_CYCLE

    [Inject]
    public void Construct(IMortgageService mortgageService, ICompanyUIService companyUIService, GameSettings gameSettings, ILocalPlayerService localPlayerService)
    {
        this.mortgageService = mortgageService ?? throw new ArgumentNullException(nameof(mortgageService));
        this.companyUIService = companyUIService ?? throw new ArgumentNullException(nameof(companyUIService));
        this.gameSettings = gameSettings ?? throw new ArgumentNullException(nameof(gameSettings));
        this.localPlayerService = localPlayerService ?? throw new ArgumentNullException(nameof(localPlayerService));
        if (photonView == null) throw new NullReferenceException(nameof(photonView));


    }

    #endregion LIFE_CYCLE

    #region PUBLIC_METHODS

    public void RequestMortgageCompany(int companyId)
    {
        photonViewWrapper.RPC(photonView, nameof(RPC_MortgageCompany), RpcTarget.MasterClient, companyId, localPlayerService.GetLocalPlayerId());
    }

    public void RequestBuyoutCompany(int companyId)
    {
        photonViewWrapper.RPC(photonView, nameof(RPC_BuyBackCompany), RpcTarget.MasterClient, localPlayerService.GetLocalPlayerId(), companyId);

    }

    #endregion PUBLIC_METHODS

    #region RPC

    [PunRPC]
    private void RPC_MortgageCompany(int companyId, int playerId)
    {
        if (!photonNetworkWrapper.IsMasterClient) return;
        mortgageService.MortgageCompany(companyId, playerId);
        photonViewWrapper.RPC(photonView, nameof(RPC_SyncMortgage), RpcTarget.All, playerId, companyId, true);
    }

    [PunRPC]
    private void RPC_BuyBackCompany(int playerId, int companyId)
    {
        if (!photonNetworkWrapper.IsMasterClient) return;
        mortgageService.BuyoutCompany(companyId, playerId);
        photonViewWrapper.RPC(photonView, nameof(RPC_SyncMortgage), RpcTarget.All, playerId, companyId, false);

    }

    [PunRPC]
    private void RPC_SyncMortgage(int playerId, int companyId, bool isMortgage)
    {
        var ui = companyUIService.GetCompanyUI(companyId) ?? throw new NullReferenceException(nameof(RPC_SyncMortgage)); 

        if (isMortgage)
        {
            ui.SetMortgageTurnsText(gameSettings.mortgageTurns);
            ui.MortgageUI();
        }
        else
        {
            ui.BuyoutUI();
        }
    }

    #endregion RPC
}