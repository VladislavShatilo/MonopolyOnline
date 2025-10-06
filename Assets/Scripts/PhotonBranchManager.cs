using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class PhotonBranchManager : MonoBehaviourPun, IPhotonBranchManager
{
    private IBranchUseCase branchUseCase;

    #region LIFE_CYCLE

    [Inject]
    public void Construct(IBranchUseCase branchUseCase)
    {
        this.branchUseCase = branchUseCase;
    }

    #endregion LIFE_CYCLE

    #region PUBLIC_METHODS

    public void RequestBuyBranch(int companyId)
    {
        photonView.RPC(nameof(RPC_BuyBranch), RpcTarget.MasterClient, companyId, PhotonNetwork.LocalPlayer.ActorNumber);
    }

    public void RequestSellBranch(int companyId)
    {
        photonView.RPC(nameof(RPC_SellBranch), RpcTarget.MasterClient, companyId, PhotonNetwork.LocalPlayer.ActorNumber);
    }

    #endregion PUBLIC_METHODS

    #region RPC

    [PunRPC]
    private void RPC_BuyBranch(int companyId, int playerId)
    {
        if (!PhotonNetwork.IsMasterClient) return;

        int newRentLevel = branchUseCase.BuyBranch(companyId, playerId);

        // Рассылаем всем клиентам (включая мастера) обновление UI
        photonView.RPC(nameof(RPC_UpdateBranchUI), RpcTarget.All, companyId, playerId, newRentLevel);
    }

    [PunRPC]
    private void RPC_SellBranch(int companyId, int playerId)
    {
        if (!PhotonNetwork.IsMasterClient) return;

        int newRentLevel = branchUseCase.SellBranch(companyId, playerId);
        photonView.RPC(nameof(RPC_UpdateBranchUI), RpcTarget.All, companyId, playerId, newRentLevel);
    }

    [PunRPC]
    private void RPC_UpdateBranchUI(int companyId, int playerId, int newRentLevel)
    {
        branchUseCase.UpdateBranchUI(companyId, playerId, newRentLevel);

    }

    #endregion RPC

}
