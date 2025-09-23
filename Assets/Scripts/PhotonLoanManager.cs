using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class PhotonLoanManager : MonoBehaviourPun, IPhotonLoanManager
{
    private ILoanService loanService;
    private IEventBus eventBus;

    [Inject]
    public void Construct(ILoanService loanService, IEventBus eventBus)
    {
        this.loanService = loanService;
        this.eventBus = eventBus;
    }

    public void TakeLoanRequest(int playerId)
    {
        photonView.RPC(nameof(RPC_TakeLoan), RpcTarget.All, playerId);
    }

    public void PayLoanRequest(int playerId)
    {
        photonView.RPC(nameof(RPC_PayLoan), RpcTarget.All, playerId);
    }

    public void ShowLoanWindow(int playerId, int loanAmount)
    {
        photonView.RPC(nameof(RPC_ShowLoanWindow), PhotonNetwork.CurrentRoom.GetPlayer(playerId), playerId, loanAmount);
    }

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
    private void RPC_ShowLoanWindow(int playerId,int loanAmount)
    {
        eventBus.Publish(new OfferLoanPayEvent(playerId,loanAmount));
            // UILoanPayWindow.Instance.ShowLoanWindow(playerRepository.GetPlayerById(playerId));
    }
}
