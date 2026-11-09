using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class PhotonTurnSynchronizer : MonoBehaviourPun,IPhotonTurnSynchronizer
{
    private IPlayerRepository playerRepository;
    private IEventBus eventBus;
    private IMortgageService mortgageService;
    private IPhotonNetworkWrapper photonNetworkWrapper;
    private IPhotonViewWrapper photonViewWrapper;

    #region LIFE_CYCLE

    [Inject]
    public void Construct(IPlayerRepository playerRepository, IEventBus eventBus, IMortgageService mortgageService, IPhotonNetworkWrapper photonNetworkWrapper, IPhotonViewWrapper photonViewWrapper)
    {
        this.playerRepository = playerRepository ?? throw new ArgumentNullException(nameof(playerRepository));
        this.eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
        this.mortgageService = mortgageService ?? throw new ArgumentNullException(nameof(mortgageService));
        this.photonNetworkWrapper = photonNetworkWrapper ?? throw new ArgumentNullException(nameof(photonNetworkWrapper));
        this.photonViewWrapper = photonViewWrapper ?? throw new ArgumentNullException(nameof(photonViewWrapper));
        if (photonView == null) throw new NullReferenceException(nameof(photonView));

    }

    #endregion LIFE_CYCLE

    #region PUBLIC_METHODS

    public void RequestStartTurn(int playerId, bool isNext)
    {
        photonViewWrapper.RPC(photonView, nameof(RPC_StartTurn), RpcTarget.All, playerId, isNext);
    }

    #endregion PUBLIC_METHODS

    #region RPC

    [PunRPC]
    private void RPC_StartTurn(int playerId, bool isNext)
    {
        var player = playerRepository.GetPlayerById(playerId) ?? throw new NullReferenceException(nameof(RPC_StartTurn)); ;

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
        if (photonNetworkWrapper.IsMasterClient)
        {
            mortgageService.TickTurn(playerId);
        }
    }

    #endregion RPC



}
