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

        var players = PhotonNetwork.PlayerList;
        if (players.Length == 0) return;

        var randomPlayer = players[UnityEngine.Random.Range(0, players.Length)];
        photonView.RPC(nameof(RPC_StartTurn), RpcTarget.All, randomPlayer.ActorNumber, true);
    }

    public void RequestEndTurn()
    {
        if (PhotonNetwork.IsMasterClient)
            turnService.EndTurn();
        else
            photonView.RPC(nameof(RPC_RequestEndTurn), RpcTarget.MasterClient);
    }

    [PunRPC]
    private void RPC_StartTurn(int playerId, bool isNext)
    {
        turnService.StartTurn(playerId, isNext);
    }

    [PunRPC]
    private void RPC_RequestEndTurn()
    {
        if (PhotonNetwork.IsMasterClient)
            turnService.EndTurn();
    }
}
