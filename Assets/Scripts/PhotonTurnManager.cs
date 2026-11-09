using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class PhotonTurnManager : MonoBehaviourPun, IPhotonTurnManager
{
    private ITurnService turnService;
    private IPhotonNetworkWrapper photonNetworkWrapper;
    private IPhotonViewWrapper photonViewWrapper;

    #region LIFE_CYCLE

    [Inject]
    public void Construct(ITurnService turnService, IPhotonNetworkWrapper photonNetworkWrapper, IPhotonViewWrapper photonViewWrapper)
    {
        this.turnService = turnService ?? throw new ArgumentNullException(nameof(turnService));
        this.photonNetworkWrapper = photonNetworkWrapper ?? throw new ArgumentNullException(nameof(photonNetworkWrapper));
        this.photonViewWrapper = photonViewWrapper ?? throw new ArgumentNullException(nameof(photonViewWrapper));
        if (photonView == null) throw new NullReferenceException(nameof(photonView));

    }

    #endregion LIFE_CYCLE

    #region PUBLIC_METHODS

    public void RequestStartRandomTurn()
    {
        if (!photonNetworkWrapper.IsMasterClient) return;
        turnService.StartRandomTurn();
    }

    public void RequestEndTurn()
    {
        photonViewWrapper.RPC(photonView,nameof(RPC_RequestEndTurn), RpcTarget.MasterClient);
    }

    public void RegisterDouble(int playerId)
    {
        photonViewWrapper.RPC(photonView, nameof(RPC_RegisterDouble), RpcTarget.MasterClient, playerId);

    }

    #endregion PUBLIC_METHODS

    #region RPC

    [PunRPC]
    private void RPC_RequestEndTurn()
    {
        if (photonNetworkWrapper.IsMasterClient)
            turnService.EndTurn();
    }

    [PunRPC]
    private void RPC_RegisterDouble(int playerId)
    {
        if (photonNetworkWrapper.IsMasterClient)
            turnService.RegisterDouble(playerId);
    }

    #endregion RPC
}