using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class PhotonBankNotifier : MonoBehaviourPun, IBankNotifier
{
    private IPlayerRepository playerRepository;
    [Inject]
    public void Construct(IPlayerRepository playerRepository)
    {
        this.playerRepository = playerRepository;
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
        EventBus.Publish(new OnUpdatePlayerMoneyEvent(player));
    }
}
