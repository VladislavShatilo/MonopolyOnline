using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class PhotonTurnSynchronizer : MonoBehaviourPun,IPhotonTurnSynchronizer
{
    private IPlayerRepository playerRepository;
    private IEventBus eventBus;
    [Inject]
    public void Construct(IPlayerRepository playerRepository, IEventBus eventBus)
    {
        this.playerRepository = playerRepository;
        this.eventBus = eventBus;
    }
    public void RequestStartTurn(int playerId, bool isNext)
    {

        photonView.RPC(nameof(RPC_StartTurn), RpcTarget.All, playerId, isNext);
    }
    [PunRPC]
    private void RPC_StartTurn(int playerId, bool isNext)
    {
        var player = playerRepository.GetPlayerById(playerId);
      
        if (player.IsInJail)
        {
            eventBus.Publish(new StartTurnJailEvent(playerId));
        }
        else
        {
            eventBus.Publish(new TurnStartEvent(playerId));
        }

        if (isNext && player.HasLoan)
        {
            eventBus.Publish(new OnStartTurnLoanEvent(playerId));
        }
    }

}
