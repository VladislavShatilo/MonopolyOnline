using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class PhotonTurnManager : MonoBehaviourPun, IPhotonTurnManager
{
    private ITurnService turnService;
    private void Awake()
    {
        Debug.Log($"PhotonTurnManager created on {gameObject.name}, ViewID={photonView.ViewID}");
    }
    [Inject]
    public void Construct(ITurnService turnService)
    {
        this.turnService = turnService;
    }
    public void RequestStartRandomTurn()
    {
        if (!PhotonNetwork.IsMasterClient) return;
        turnService.StartRandomTurn();
    }

    public void RequestEndTurn()
    {
        Debug.Log("RequestEndTurn CALLED\n" + System.Environment.StackTrace);
        photonView.RPC(nameof(RPC_RequestEndTurn), RpcTarget.MasterClient);

    }

    [PunRPC]
    private void RPC_RequestEndTurn()
    {
        if (PhotonNetwork.IsMasterClient)
            turnService.EndTurn(); 
    }
}
