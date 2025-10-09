using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class PhotonMortgageManager : MonoBehaviourPun, IPhotonMortgageManager
{
    private IMortgageService mortgageService;
    private ICompanyUIService companyUIService;
    private IPhotonNetworkWrapper photonNetworkWrapper;
    private GameSettings gameSettings;

    #region LIFE_CYCLE

    [Inject]
    public void Construct(IMortgageService mortgageService, ICompanyUIService companyUIService, GameSettings gameSettings)
    {
        this.mortgageService = mortgageService;
        this.companyUIService = companyUIService;
        this.gameSettings = gameSettings;
    }

    #endregion LIFE_CYCLE

    #region PUBLIC_METHODS

    public void RequestMortgageCompany(int companyId)
    {
        photonView.RPC(nameof(RPC_MortgageCompany), RpcTarget.MasterClient, companyId, PhotonNetwork.LocalPlayer.ActorNumber);
    }

    public void RequestBuyoutCompany(int companyId)
    {
        photonView.RPC(nameof(RPC_BuyBackCompany), RpcTarget.MasterClient, PhotonNetwork.LocalPlayer.ActorNumber, companyId);
    }

    #endregion PUBLIC_METHODS

    #region RPC

    [PunRPC]
    private void RPC_MortgageCompany(int companyId, int playerId)
    {
        if (!photonNetworkWrapper.IsMasterClient) return;
        mortgageService.MortgageCompany(companyId, playerId);
        photonView.RPC(nameof(RPC_SyncMortgage), RpcTarget.All, playerId, companyId, true);
    }

    [PunRPC]
    private void RPC_BuyBackCompany(int playerId, int companyId)
    {
        if (!photonNetworkWrapper.IsMasterClient) return;
        mortgageService.BuyoutCompany(companyId, playerId);
        photonView.RPC(nameof(RPC_SyncMortgage), RpcTarget.All, playerId, companyId, false);
    }

    [PunRPC]
    private void RPC_SyncMortgage(int playerId, int companyId, bool isMortgage)
    {
        // Здесь только UI-логика
        var ui = companyUIService.GetCompanyUI(companyId);
        if (ui == null) return;

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