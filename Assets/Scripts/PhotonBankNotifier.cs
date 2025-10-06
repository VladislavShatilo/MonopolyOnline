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

    #region LIFE_CYCLE

    [Inject]
    public void Construct(IPlayerRepository playerRepository, IEventBus eventBus)
    {
        this.playerRepository = playerRepository;
        this.eventBus = eventBus;
    }

    #endregion LIFE_CYCLE

    #region PUBLIC_METHODS

    public void NotifyBalanceChanged(PlayerData player)
    {
        photonView.RPC(nameof(RPC_UpdateMoney), RpcTarget.All, player.Id, player.Money);
    }

    #endregion PUBLIC_METHODS

    #region RPC

    [PunRPC]
    private void RPC_UpdateMoney(int playerId, int money)
    {
        PlayerData player = playerRepository.GetPlayerById(playerId);
        player.Money = money; // обновляем локально
        eventBus.Publish(new OnUpdatePlayerMoneyEvent(player));
    }

    #endregion RPC
}