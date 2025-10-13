using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class PhotonBranchManager : MonoBehaviourPun, IPhotonBranchManager
{
    private IBranchUseCase branchUseCase;
    private IPhotonNetworkWrapper photonNetworkWrapper;
    private IPhotonViewWrapper photonViewWrapper;

    #region LIFE_CYCLE

    [Inject]
    public void Construct(IBranchUseCase branchUseCase, IPhotonNetworkWrapper photonNetworkWrapper, IPhotonViewWrapper photonViewWrapper)
    {
        this.branchUseCase = branchUseCase;
        this.photonNetworkWrapper = photonNetworkWrapper;
        this.photonViewWrapper = photonViewWrapper;
    }

    #endregion LIFE_CYCLE

    #region PUBLIC_METHODS

    public void RequestBuyBranch(int companyId)
    {
        photonViewWrapper.RPC(photonView, nameof(RPC_BuyBranch), RpcTarget.MasterClient, companyId, PhotonNetwork.LocalPlayer.ActorNumber);
    }

    public void RequestSellBranch(int companyId)
    {
        photonViewWrapper.RPC(photonView, nameof(RPC_SellBranch), RpcTarget.MasterClient, companyId, PhotonNetwork.LocalPlayer.ActorNumber);
    }

    #endregion PUBLIC_METHODS

    #region RPC

    [PunRPC]
    private void RPC_BuyBranch(int companyId, int playerId)
    {
        if (!photonNetworkWrapper.IsMasterClient) return;

        int newRentLevel = branchUseCase.BuyBranch(companyId, playerId);
        photonViewWrapper.RPC(photonView, nameof(RPC_UpdateBranchUI), RpcTarget.All, companyId, playerId, newRentLevel);
    }

    [PunRPC]
    private void RPC_SellBranch(int companyId, int playerId)
    {
        if (!photonNetworkWrapper.IsMasterClient) return;

        int newRentLevel = branchUseCase.SellBranch(companyId, playerId);
        photonViewWrapper.RPC(photonView, nameof(RPC_UpdateBranchUI), RpcTarget.All, companyId, playerId, newRentLevel);
    }

    [PunRPC]
    private void RPC_UpdateBranchUI(int companyId, int playerId, int newRentLevel)
    {
        branchUseCase.UpdateBranchUI(companyId, playerId, newRentLevel);

    }

    #endregion RPC

}
