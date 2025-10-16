using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class PhotonPlayerMoveManager : MonoBehaviourPun, IPhotonPlayerMoveManager
{
    private IPlayerMoveUseCase playerMoveUseCase;
    private IPlayerRepository playerRepository;
    private IBoardService boardService;
    private IEventBus eventBus;
    private IPhotonNetworkWrapper photonNetworkWrapper;
    private IPhotonViewWrapper photonViewWrapper;

    #region LIFE_CYCLE

    [Inject]
    public void Construct(IPlayerMoveUseCase playerMoveUseCase, IEventBus eventBus, IPlayerRepository playerRepository, IBoardService boardService, IPhotonNetworkWrapper photonNetworkWrapper, IPhotonViewWrapper photonViewWrapper)
    {
        this.playerMoveUseCase = playerMoveUseCase ?? throw new ArgumentNullException(nameof(playerMoveUseCase));
        this.playerRepository = playerRepository ?? throw new ArgumentNullException(nameof(playerRepository));
        this.eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
        this.boardService = boardService ?? throw new ArgumentNullException(nameof(boardService));
        this.photonNetworkWrapper = photonNetworkWrapper ?? throw new ArgumentNullException(nameof(photonNetworkWrapper));
        this.photonViewWrapper = photonViewWrapper ?? throw new ArgumentNullException(nameof(photonViewWrapper));
    }

    private void OnEnable()
    {
        eventBus.Subscribe<OnPlayerMoveEvent>(RequestMove);
    }

    private void OnDisable()
    {
        eventBus.Unsubscribe<OnPlayerMoveEvent>(RequestMove);
    }

    #endregion LIFE_CYCLE

    #region PUBLIC_METHODS

    public void RequestTeleport(int playerId)
    {
        if (!photonNetworkWrapper.IsMasterClient) return;
        photonViewWrapper.RPC(photonView, nameof(RPC_TeleportPlayer), RpcTarget.MasterClient, playerId);
    }

    #endregion PUBLIC_METHODS

    #region RPC

    [PunRPC]
    private void RPC_MovePlayer(int playerId, int steps, bool forward)
    {
        playerMoveUseCase.MovePlayer(playerId, steps, forward);
    }



    [PunRPC]
    private void RPC_TeleportPlayer(int playerId)
    {
        if (!photonNetworkWrapper.IsMasterClient) return;

        PlayerData player = playerRepository.GetPlayerById(playerId) ?? throw new NullReferenceException(nameof(RPC_TeleportPlayer)); ;
        int randomIndex;
        do
        {
            randomIndex = UnityEngine.Random.Range(0, boardService.CellsCount + 1);
        } while (randomIndex == player.CurrentCellId);

        photonViewWrapper.RPC(photonView, nameof(RPC_TeleportPlayerBroadcast), RpcTarget.All, playerId, randomIndex, player.CurrentCellId);

    }

    [PunRPC]
    private void RPC_TeleportPlayerBroadcast(int playerId, int randomIndex, int currentCellId)
    {
        playerMoveUseCase.TeleportPlayer(playerId, randomIndex, currentCellId);
    }

    #endregion RPC

    #region CALLBACKS

    private void RequestMove(OnPlayerMoveEvent e)
    {
        photonViewWrapper.RPC(photonView, nameof(RPC_MovePlayer), RpcTarget.All, e.PlayerId, e.Steps, e.Forward);

    }

    #endregion CALLBACKS


}