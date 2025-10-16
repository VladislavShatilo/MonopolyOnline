using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class PhotonBankNotifier : MonoBehaviourPun, IBankNotifier
{
    private IPlayerRepository playerRepository;
    private IPhotonViewWrapper photonViewWrapper;

    private IEventBus eventBus;

    #region LIFE_CYCLE

    [Inject]
    public void Construct(IPlayerRepository playerRepository, IEventBus eventBus, IPhotonViewWrapper photonViewWrapper)
    {
        this.playerRepository = playerRepository ?? throw new ArgumentNullException(nameof(playerRepository));
        this.eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
        this.photonViewWrapper = photonViewWrapper ?? throw new ArgumentNullException(nameof(photonViewWrapper));
        if (photonView == null) throw new NullReferenceException(nameof(photonView));

    }

    #endregion LIFE_CYCLE

    #region PUBLIC_METHODS

    public void NotifyBalanceChanged(PlayerData player)
    {
        photonViewWrapper.RPC(photonView, nameof(RPC_UpdateMoney), RpcTarget.All, player.Id, player.Money);
    }

    #endregion PUBLIC_METHODS

    #region RPC

    [PunRPC]
    private void RPC_UpdateMoney(int playerId, int money)
    {
        PlayerData player = playerRepository.GetPlayerById(playerId) ?? throw new InvalidOperationException(nameof(RPC_UpdateMoney));
        player.Money = money;
        eventBus.Publish(new OnUpdatePlayerMoneyEvent(player));
    }

    #endregion RPC
}