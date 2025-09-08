using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;

public class GameManager
{
    private IPlayerRepository repository;
    private IPlayerSpawner spawner;
    private IPlayerColorService colorService;

  
    [Inject]
    public void Construct(IPlayerRepository repository, IPlayerSpawner spawner, IPlayerColorService colorService)
    {
        this.repository = repository;
        this.spawner = spawner;
        this.colorService = colorService;
    }

    public void Initialize()
    {

        foreach (var p in Photon.Pun.PhotonNetwork.PlayerList)
        {
            if (repository.GetPlayerById(p.ActorNumber) != null) continue;

            var player = new PlayerData(
                p.NickName,
                100_000,
                p.ActorNumber,
                colorService.GetColorForPlayer(p.ActorNumber),
                p);

            repository.AddPlayer(player);
            EventBus.Publish(new PlayerJoinedEvent(player));

        }

        spawner.SpawnLocalPlayer(Photon.Pun.PhotonNetwork.LocalPlayer.ActorNumber);
    }

}
public class PlayerJoinedEvent
{
    public PlayerData Player { get; }

    public PlayerJoinedEvent(PlayerData player)
    {
        Player = player;
    }
}

public class PlayerLeftEvent
{
    public int PlayerId { get; }

    public PlayerLeftEvent(int playerId)
    {
        PlayerId = playerId;
    }
}

public class AllPlayersInitializedEvent
{
    public List<PlayerData> Players { get; }

    public AllPlayersInitializedEvent(List<PlayerData> players)
    {
        Players = players;
    }
}


public class TryStartGameEvent { }