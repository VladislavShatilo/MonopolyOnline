using Photon.Pun;
using Photon.Realtime;
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
    #region LIFE_CYCLE

    [Inject]
    public void Construct(IPlayerMoveUseCase playerMoveUseCase, IEventBus eventBus, IPlayerRepository playerRepository, IBoardService boardService, IPhotonNetworkWrapper photonNetworkWrapper)
    {
        this.playerMoveUseCase = playerMoveUseCase;
        this.playerRepository = playerRepository;
        this.eventBus = eventBus;
        this.boardService = boardService;
        this.photonNetworkWrapper = photonNetworkWrapper;
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

        photonView.RPC(nameof(RPC_TeleportPlayer), RpcTarget.MasterClient, playerId);
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

        PlayerData player = playerRepository.GetPlayerById(playerId);
        int randomIndex;
        do
        {
            randomIndex = UnityEngine.Random.Range(0, boardService.CellsCount + 1);
        } while (randomIndex == player.CurrentCellId);

        photonView.RPC(nameof(RPC_TeleportPlayerBroadcast), RpcTarget.All, playerId, randomIndex, player.CurrentCellId);
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
        photonView.RPC(nameof(RPC_MovePlayer), RpcTarget.All, e.PlayerId, e.Steps, e.Forward);
    }

    #endregion CALLBACKS


}