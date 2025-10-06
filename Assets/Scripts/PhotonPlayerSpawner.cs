using Photon.Pun;
using System.Collections.Generic;
using System.ComponentModel.Design;
using UnityEditor;
using UnityEngine;
using Zenject;

public class PhotonPlayerSpawner : MonoBehaviourPun, IPlayerSpawner
{
    private PlayerSettings playerSettings;
    private IPlayerColorService playerColorService;
    private IEventBus eventBus;
    private readonly Dictionary<int, PlayerMove> playerMoves = new();
    private PlayerMove playerMove;
    [Inject]
    public void Construct([Inject(Id = "PlayerSettings")] PlayerSettings playerSettings, IPlayerColorService playerColorService, IEventBus eventBus)
    {
        this.playerSettings = playerSettings;
        this.playerColorService = playerColorService;
        this.eventBus = eventBus;
    }

    public void SpawnLocalPlayer(int localId)
    {
        if (playerMoves.ContainsKey(localId)) return;
        PlayerColor playerColor = playerColorService.GetColorForPlayer(localId);
        Vector3 startPostion = playerSettings.StartPosition;
        var go = PhotonNetwork.Instantiate(
            playerSettings.PlayerPrefab.name,
            playerSettings.StartPosition,
            Quaternion.identity,
            0,
            new object[] { localId - 1,playerColor.R, playerColor.G, playerColor.B, startPostion }
        );
        playerMove = go.GetComponent<PlayerMove>();

        //  pm.Initialize(boardService, eventBus,localId);

        playerMoves[localId] = playerMove;

    }
  
    public void RemovePlayer(int playerId)
    {
        //if (!playerMoves.TryGetValue(playerId, out var move)) return;

        //if (move != null && move.photonView != null && move.photonView.IsMine)
        //    PhotonNetwork.Destroy(move.gameObject);
        //else if (move != null)
        //    Object.Destroy(move.gameObject);

        //playerMoves.Remove(playerId);
    }
}

public class PlayerSpawnedViewEvent
{
    public PlayerView PlayerView;

    public PlayerSpawnedViewEvent(PlayerView playerView)
    {
        PlayerView = playerView;
    }
}