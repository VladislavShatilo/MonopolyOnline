using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class PhotonLoanManager : MonoBehaviourPun, IPhotonLoanManager
{
    private ILoanService loanService;
    private IEventBus eventBus;
    private IPhotonViewWrapper photonViewWrapper;

    #region LIFE_CYCLE

    [Inject]
    public void Construct(ILoanService loanService, IEventBus eventBus, IPhotonViewWrapper photonViewWrapper)
    {
        this.loanService = loanService;
        this.eventBus = eventBus;
        this.photonViewWrapper = photonViewWrapper;
    }

    #endregion LIFE_CYCLE

    #region PUBLIC_METHODS

    public void TakeLoanRequest(int playerId)
    {
        photonViewWrapper.RPC(photonView, nameof(RPC_TakeLoan), RpcTarget.All, playerId);
    }

    public void PayLoanRequest(int playerId)
    {
        photonViewWrapper.RPC(photonView, nameof(RPC_PayLoan), RpcTarget.All, playerId);
    }

    public void ShowLoanWindow(int playerId, int loanAmount)
    {
        photonViewWrapper.RPC(photonView, nameof(RPC_ShowLoanWindow), PhotonNetwork.CurrentRoom.GetPlayer(playerId), playerId, loanAmount);
    }

    #endregion PUBLIC_METHODS

    #region RPC

    [PunRPC]
    private void RPC_TakeLoan(int playerId)
    {
        loanService.TakeLoanConfirmed(playerId);
    }

    [PunRPC]
    private void RPC_PayLoan(int playerId)
    {
        loanService.PayLoanConfirmed(playerId);
    }

    [PunRPC]
    private void RPC_ShowLoanWindow(int playerId, int loanAmount)
    {
        eventBus.Publish(new OfferLoanPayEvent(playerId, loanAmount));
    }

    #endregion RPC
}