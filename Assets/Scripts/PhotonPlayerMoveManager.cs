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
    private IEventBus eventBus;
    [Inject]
    public void Construct(IPlayerMoveUseCase playerMoveUseCase, IEventBus eventBus, IPlayerRepository playerRepository)
    {
        this.playerMoveUseCase = playerMoveUseCase;
        this.playerRepository = playerRepository;
        this.eventBus = eventBus;
    }

    private void OnEnable()
    {
        eventBus.Subscribe<OnPlayerMoveEvent>(RequestMove);
    }
    private void OnDisable()
    {
        eventBus.Unsubscribe<OnPlayerMoveEvent>(RequestMove);

    }
    private void RequestMove(OnPlayerMoveEvent e)
    {
        photonView.RPC(nameof(RPC_MovePlayer), RpcTarget.All, e.PlayerId, e.Steps, e.Forward);
    }

    [PunRPC]
    private void RPC_MovePlayer(int playerId, int steps, bool forward)
    {
        playerMoveUseCase.MovePlayer(playerId, steps, forward);
    }

    public void RequestTeleport(int playerId)
    {
        Debug.Log("RequestTeleport");
        if (!PhotonNetwork.IsMasterClient) return;

        photonView.RPC(nameof(RPC_TeleportPlayer), RpcTarget.MasterClient, playerId);
    }

    [PunRPC]
    private void RPC_TeleportPlayer(int playerId)
    {
        Debug.Log("RPC_TeleportPlayer");

        if (!PhotonNetwork.IsMasterClient) return;

        PlayerData player = playerRepository.GetPlayerById(playerId);
        int randomIndex;
        do
        {
            randomIndex = UnityEngine.Random.Range(0, 41); // можно заменить на boardService.CellsCount
        } while (randomIndex == player.CurrentCellId);

        Debug.Log($"[MASTER] Teleport target for player {playerId}: {randomIndex}");
    
        photonView.RPC(nameof(RPC_TeleportPlayerBroadcast), RpcTarget.All, playerId, randomIndex, player.CurrentCellId);
    }
    [PunRPC]
    private void RPC_TeleportPlayerBroadcast(int playerId, int randomIndex, int currentCellId)
    {
        playerMoveUseCase.TeleportPlayer(playerId, randomIndex, currentCellId);
    }
}
