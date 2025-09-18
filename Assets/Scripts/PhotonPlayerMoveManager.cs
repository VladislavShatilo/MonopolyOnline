using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class PhotonPlayerMoveManager : MonoBehaviourPun, IPhotonPlayerMoveManager
{
    private IPlayerMoveUseCase playerMoveUseCase;
    private IEventBus eventBus;
    [Inject]
    public void Construct(IPlayerMoveUseCase playerMoveUseCase, IEventBus eventBus)
    {
        this.playerMoveUseCase = playerMoveUseCase;
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

    public void RequestTeleport(int playerId, int cellIndex)
    {
        photonView.RPC(nameof(RPC_TeleportPlayer), RpcTarget.AllBuffered, playerId, cellIndex);
    }

    [PunRPC]
    private void RPC_TeleportPlayer(int playerId, int cellIndex)
    {
        playerMoveUseCase.TeleportPlayer(playerId, cellIndex);
    }
}
