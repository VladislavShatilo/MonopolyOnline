using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class PhotonBankNotifier : MonoBehaviourPun, IBankNotifier
{
    private IPlayerRepository playerRepository;
    private IEventBus eventBus;
    [Inject]
    public void Construct(IPlayerRepository playerRepository, IEventBus eventBus)
    {
        this.playerRepository = playerRepository;
        this.eventBus = eventBus;   
    }

    public void NotifyBalanceChanged(PlayerData player)
    {
        photonView.RPC(nameof(RPC_UpdateMoney), RpcTarget.All, player.Id,player.Money);
    }
    [PunRPC]
    private void RPC_UpdateMoney(int playerId,int money)
    {
        PlayerData player = playerRepository.GetPlayerById(playerId);
        player.Money = money; // обновляем локально
        eventBus.Publish(new OnUpdatePlayerMoneyEvent(player));
    }
}
