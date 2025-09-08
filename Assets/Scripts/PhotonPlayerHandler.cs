using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class PhotonPlayerHandler : Photon.Pun.MonoBehaviourPunCallbacks
{
    private IPlayerRepository repository;
    private IPlayerColorService colorService;

    [Inject]
    public void Construct(IPlayerRepository repository, IPlayerColorService colorService)
    {
        this.repository = repository;
        this.colorService = colorService;
    }

    public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer)
    {

        var player = new PlayerData(
            newPlayer.NickName,
            100_000,
            newPlayer.ActorNumber,
            colorService.GetColorForPlayer(newPlayer.ActorNumber),
            newPlayer);

        repository.AddPlayer(player);
        EventBus.Publish(new PlayerJoinedEvent(player));
    }

    public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer)
    {
        repository.RemovePlayer(otherPlayer.ActorNumber);
        EventBus.Publish(new PlayerLeftEvent(otherPlayer.ActorNumber));
    }
}
