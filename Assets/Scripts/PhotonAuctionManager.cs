using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class PhotonAuctionManager : MonoBehaviourPun, IPhotonAuctionManager
{
    private IEventBus eventBus;
    private IPhotonNetworkWrapper photonNetworkWrapper;
    private IPhotonViewWrapper photonViewWrapper;
    #region LIFE_CYCLE

    [Inject]
    public void Consturct(IEventBus eventBus, IPhotonNetworkWrapper photonNetworkWrapper, IPhotonViewWrapper photonViewWrapper)
    {
        this.eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
        this.photonNetworkWrapper = photonNetworkWrapper ?? throw new ArgumentNullException(nameof(photonNetworkWrapper));
        this.photonViewWrapper = photonViewWrapper ?? throw new ArgumentNullException(nameof(photonViewWrapper));
        if (photonView == null) throw new NullReferenceException(nameof(photonView));

    }

    #endregion LIFE_CYCLE

    #region PUBLIC_METHODS

    public void PromptBidRequest(int playerId, int minAllowedBid, int companyId)
    {
        photonViewWrapper.RPC(photonView, nameof(RPC_PromptBid_Internal), RpcTarget.All, playerId, minAllowedBid, companyId);
    }

    public void StartAuctionRequest(int starterActorNumber, int companyId, int companyBasePrice)
    {
        photonViewWrapper.RPC(photonView, nameof(RPC_StartAuctionRequest), RpcTarget.MasterClient, starterActorNumber, companyId, companyBasePrice);
    }

    public void PlayerBidRequest(int playerId)
    {
        photonViewWrapper.RPC(photonView, nameof(RPC_UpdateBid), RpcTarget.MasterClient, playerId);
    }

    public void PlayerPassRequest(int playerId)
    {
        photonViewWrapper.RPC(photonView, nameof(RPC_UpdatePass), RpcTarget.MasterClient, playerId);
    }

    public void CloseAuctionWindowRequest(int playerId)
    {

        photonViewWrapper.RPC(photonView, nameof(RPC_CloseAuctionWindow),playerId, null);

    }

    #endregion PUBLIC_METHODS

    #region RPC

    [PunRPC]
    private void RPC_PromptBid_Internal(int playerId, int minAllowedBid, int companyId)
    {
        eventBus.Publish(new AuctionPromptBidEvent(playerId, minAllowedBid, companyId));
    }

    [PunRPC]
    private void RPC_StartAuctionRequest(int starterActorNumber, int companyId, int companyBasePrice)
    {
        if (!photonNetworkWrapper.IsMasterClient) return;

        eventBus.Publish(new StartAuctionEvent(starterActorNumber, companyId, companyBasePrice));
    }

    [PunRPC]
    private void RPC_UpdateBid(int playerId)
    {
        if (!photonNetworkWrapper.IsMasterClient) return;

        eventBus.Publish(new PlayerBidAuction(playerId));
    }

    [PunRPC]
    private void RPC_UpdatePass(int playerId)
    {
        if (!photonNetworkWrapper.IsMasterClient) return;

        eventBus.Publish(new PlayerPassAuction(playerId));
    }

    [PunRPC]
    private void RPC_CloseAuctionWindow()
    {
        eventBus.Publish(new AuctionEndEvent());
    }

    #endregion RPC
}