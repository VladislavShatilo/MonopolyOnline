using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class PhotonJailManager : MonoBehaviourPun, IPhotonJailManager
{
    private IJailService jailService;
    private IPhotonTurnManager photonTurnManager;
    private IEventBus eventBus;
    private IPhotonNetworkWrapper photonNetworkWrapper;
    private IPhotonViewWrapper photonViewWrapper;

    #region LIFE_CYCLE

    [Inject]
    public void Construct(IJailService jailService, IEventBus eventBus, IPhotonTurnManager photonTurnManager, IPhotonNetworkWrapper photonNetworkWrapper, IPhotonViewWrapper photonViewWrapper)
    {
        this.jailService = jailService ?? throw new ArgumentNullException(nameof(jailService));
        this.eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
        this.photonTurnManager = photonTurnManager ?? throw new ArgumentNullException(nameof(photonTurnManager));
        this.photonNetworkWrapper = photonNetworkWrapper ?? throw new ArgumentNullException(nameof(photonNetworkWrapper));
        this.photonViewWrapper = photonViewWrapper ?? throw new ArgumentNullException(nameof(photonViewWrapper));
        if (photonView == null) throw new NullReferenceException(nameof(photonView));

    }

    #endregion LIFE_CYCLE

    #region PUBLIC_METHODS

    public void SendToJail(int playerId)
    {
        if (!photonNetworkWrapper.IsMasterClient) return;
        photonViewWrapper.RPC(photonView, nameof(RPC_MoveToJail), RpcTarget.All, playerId); 

        photonTurnManager.RequestEndTurn();
    }

    public void CheckDice(int playerId, int d1, int d2)
    {
        jailService.TryReleaseByDice(playerId, d1, d2);
    }

    #endregion PUBLIC_METHODS

    #region RPC

    [PunRPC]
    private void RPC_MoveToJail(int playerId)
    {
        eventBus.Publish(new MoveToJailEvent(playerId)); // 10 = индекс клетки
        jailService.SendPlayerToJail(playerId);
    }

    #endregion RPC
}