using Photon.Pun;
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
    #region LIFE_CYCLE

    [Inject]
    public void Construct(IJailService jailService, IEventBus eventBus, IPhotonTurnManager photonTurnManager, IPhotonNetworkWrapper photonNetworkWrapper)
    {
        this.jailService = jailService;
        this.eventBus = eventBus;
        this.photonTurnManager = photonTurnManager;
        this.photonNetworkWrapper = photonNetworkWrapper;
    }

    #endregion LIFE_CYCLE

    #region PUBLIC_METHODS

    public void SendToJail(int playerId)
    {
        if (!photonNetworkWrapper.IsMasterClient) return;
        photonView.RPC(nameof(RPC_MoveToJail), RpcTarget.All, playerId);

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