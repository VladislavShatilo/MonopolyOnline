using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class PhotonAuctionManager : MonoBehaviourPun, IPhotonAuctionManager
{
    private IEventBus eventBus;

    [Inject]
    public void Consturct(IEventBus eventBus)
    {
        this.eventBus = eventBus;
    }
    public void PromptBidRequest(int playerId, int minAllowedBid, int companyId)
    {
        photonView.RPC(nameof(RPC_PromptBid_Internal), RpcTarget.All, playerId, minAllowedBid, companyId);
    }
    [PunRPC]
    private void RPC_PromptBid_Internal(int playerId, int minAllowedBid, int companyId)
    {
        // Тут уже локально у каждого клиента поднимем событие
        eventBus.Publish(new AuctionPromptBidEvent(playerId, minAllowedBid, companyId));
    }
    //public void EndAuctionRequest(int winnerId, int price, int companyId)
    //{
    //    photonView.RPC(nameof(RPC_EndAuction_Internal), RpcTarget.All, winnerId, price, companyId);
    //}

    //[PunRPC]
    //private void RPC_EndAuction_Internal(int winnerId, int price, int companyId)
    //{
    //    EventBus.Publish(new AuctionEndedEvent(winnerId, price, companyId));
    //}
    public void StartAuctionRequest(int starterActorNumber, int companyId, int companyBasePrice)
    {

        photonView.RPC(nameof(RPC_StartAuctionRequest), RpcTarget.MasterClient, starterActorNumber, companyId, companyBasePrice);

    }
    [PunRPC]
    private void RPC_StartAuctionRequest(int starterActorNumber, int companyId, int companyBasePrice)
    {
        if (!PhotonNetwork.IsMasterClient) return;

        eventBus.Publish(new StartAuctionEvent(starterActorNumber, companyId, companyBasePrice));


    }
    public void PlayerBidRequest(int playerId)
    {
        photonView.RPC(nameof(RPC_UpdateBid), RpcTarget.MasterClient, playerId);
    }

    [PunRPC]
    private void RPC_UpdateBid(int playerId)
    {
        if (!PhotonNetwork.IsMasterClient) return;


        eventBus.Publish(new PlayerBidAuction(playerId));
    }
    public void PlayerPassRequest(int playerId)
    {
        photonView.RPC(nameof(RPC_UpdatePass), RpcTarget.MasterClient, playerId);
    }

    [PunRPC]
    private void RPC_UpdatePass(int playerId)
    {
        if (!PhotonNetwork.IsMasterClient) return;


        eventBus.Publish(new PlayerPassAuction(playerId));
    }

    public void CloseAuctionWindowRequest(int playerId)
    {
        photonView.RPC(nameof(RPC_CloseAuctionWindow), PhotonNetwork.CurrentRoom.GetPlayer(playerId));
    }

    [PunRPC]
    private void RPC_CloseAuctionWindow()
    {
        eventBus.Publish(new AuctionEndEvent());

    }
    //public void BroadcastAuctionEnd(int winnerId, int finalPrice)
    //{
    //    photonView.RPC(nameof(RPC_AuctionEnd), RpcTarget.All, winnerId, finalPrice);
    //}

    //[PunRPC]
    //private void RPC_AuctionEnd(int winnerId, int finalPrice)
    //{
    //    EventBus.Publish(new AuctionEndedEventWin(winnerId, finalPrice, 0, AuctionEndReason.Winner));
    //}
}
public class StartAuctionEvent
{
    public int StarterActorNumber;
    public int CompanyId;
    public int CompanyBasePrice;
    public StartAuctionEvent(int starterActorNumber, int companyId, int companyBasePrice)
    {
        StarterActorNumber = starterActorNumber;
        CompanyId = companyId;
        CompanyBasePrice= companyBasePrice;
    }
}
public class AuctionPromptBidEvent
{
    public int PlayerId { get; }
    public int Bid { get; }
    public int CompanyId { get; }
    public AuctionPromptBidEvent(int playerId, int bid, int companyId)
    {
        PlayerId = playerId;
        Bid = bid;
        CompanyId = companyId;
    }
}
public class AuctionEndEvent
{
  
}
public class PlayerPassAuction
{
    public int PlayerId;
    public PlayerPassAuction(int playerId)
    {
        PlayerId = playerId;
    }
}