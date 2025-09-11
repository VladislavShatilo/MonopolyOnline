using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class PhotonLoanManager : MonoBehaviourPun, IPhotonLoanManager
{
    private ILoanService loanService;

    [Inject]
    public void Construct(ILoanService loanService)
    {
        this.loanService = loanService;
    }

    public void SendTakeLoan(int playerId)
    {
        photonView.RPC(nameof(RPC_TakeLoan), RpcTarget.All, playerId);
    }

    public void SendPayLoan(int playerId)
    {
        photonView.RPC(nameof(RPC_PayLoan), RpcTarget.All, playerId);
    }

    public void ShowLoanWindow(int playerId)
    {
        photonView.RPC(nameof(RPC_ShowLoanWindow), PhotonNetwork.CurrentRoom.GetPlayer(playerId), playerId);
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
    private void RPC_ShowLoanWindow(int playerId)
    {
        // UILoanPayWindow.Instance.ShowLoanWindow(playerRepository.GetPlayerById(playerId));
    }
}
