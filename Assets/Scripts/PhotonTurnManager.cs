using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class PhotonTurnManager : MonoBehaviourPun, IPhotonTurnManager
{
    private ITurnService turnService;
   
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
        photonView.RPC(nameof(RPC_RequestEndTurn), RpcTarget.MasterClient);

    }
    public void RegisterDouble(int playerId)
    {
        photonView.RPC(nameof(RPC_RegisterDouble), RpcTarget.MasterClient,playerId);

    }
    [PunRPC]
    private void RPC_RequestEndTurn()
    {
        if (PhotonNetwork.IsMasterClient)
            turnService.EndTurn(); 
    }

    [PunRPC]
    private void RPC_RegisterDouble(int playerId)
    {
        if (PhotonNetwork.IsMasterClient)
            turnService.RegisterDouble(playerId);
    }
}
