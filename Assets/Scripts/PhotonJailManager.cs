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

    [Inject]
    public void Construct(IJailService jailService, IEventBus eventBus, IPhotonTurnManager photonTurnManager)
    {
        this.jailService = jailService;
        this.eventBus = eventBus;
        this.photonTurnManager = photonTurnManager;
    }
  
    public void SendToJail(int playerId)
    {
        if (!PhotonNetwork.IsMasterClient) return;
       // photonView.RPC(nameof(RPC_SendToJail), PhotonNetwork.CurrentRoom.GetPlayer(playerId), playerId);
        photonView.RPC(nameof(RPC_MoveToJail), RpcTarget.All, playerId);

        photonTurnManager.RequestEndTurn();

    }

    //[PunRPC]
    //private void RPC_SendToJail(int playerId)
    //{
    //    jailService.SendPlayerToJail(playerId);
    //}

    [PunRPC]
    private void RPC_MoveToJail(int playerId)
    {
        eventBus.Publish(new MoveToJailEvent(playerId)); // 10 = индекс клетки
        jailService.SendPlayerToJail(playerId);

    }
    public void ReleaseFromJail(int playerId, bool paid)
    {
        jailService.ReleasePlayer(playerId, paid);
        eventBus.Publish(new PlayerReleasedFromJailEvent(playerId, paid));
    }

    public void CheckDice(int playerId, int d1, int d2)
    {
        jailService.TryReleaseByDice(playerId, d1, d2);
      
    }
}
public class PlayerMovedToJailEvent
{
    public int PlayerId { get; }

    public PlayerMovedToJailEvent(int playerId)
    {
        PlayerId = playerId;
    }
}

public class PlayerReleasedFromJailEvent
{
    public int PlayerId { get; }
    public bool PaidFine { get; }

    public PlayerReleasedFromJailEvent(int playerId, bool paidFine)
    {
        PlayerId = playerId;
        PaidFine = paidFine;
    }
}

public class CheckDiceJailEvent
{
    public int PlayerId { get; }
    public int Dice1 { get; }
    public int Dice2 { get; }

    public CheckDiceJailEvent(int playerId, int dice1, int dice2)
    {
        PlayerId = playerId;
        Dice1 = dice1;
        Dice2 = dice2;
    }
}
