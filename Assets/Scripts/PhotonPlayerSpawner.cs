using Photon.Pun;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using UnityEditor;
using UnityEngine;
using Zenject;

public class PhotonPlayerSpawner : MonoBehaviourPun, IPlayerSpawner
{
    private PlayerSettings playerSettings;
    private IPlayerColorService playerColorService;
    private readonly Dictionary<int, PlayerMove> playerMoves = new();
    private PlayerMove playerMove;

    #region LIFE_CYCLE

    [Inject]
    public void Construct([Inject(Id = "PlayerSettings")] PlayerSettings playerSettings, IPlayerColorService playerColorService)
    {
        this.playerSettings = playerSettings ?? throw new ArgumentNullException(nameof(playerSettings));
        this.playerColorService = playerColorService ?? throw new ArgumentNullException(nameof(playerColorService));
    }

    #endregion LIFE_CYCLE

    #region PUBLIC_METHODS

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
            new object[] { localId - 1, playerColor.R, playerColor.G, playerColor.B, startPostion }
        );
        playerMove = go.GetComponent<PlayerMove>();
        if (playerMove != null)
        {
            playerMoves[localId] = playerMove;
        }
    }



    #endregion PUBLIC_METHODS
}