using System.Collections.Generic;
using UnityEngine;
using Zenject;
using Photon.Pun;

public class PhotonPlayerSpawner : IPlayerSpawner
{
    private PlayerSettings playerSettings;

    private readonly Dictionary<int, PlayerMove> playerMoves = new();


    [Inject]
    public void Construct([Inject(Id = "PlayerSettings")] PlayerSettings playerSettings)
    {
        this.playerSettings = playerSettings;

    }

    public void SpawnLocalPlayer(int localId)
    {
        if (playerMoves.ContainsKey(localId)) return;

        var go = PhotonNetwork.Instantiate(
            playerSettings.PlayerPrefab.name,
            playerSettings.StartPosition,
            Quaternion.identity,
            0,
            new object[] { localId - 1 }
        );

        var pm = go.GetComponent<PlayerMove>();
  
        playerMoves[localId] = pm;
   
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
