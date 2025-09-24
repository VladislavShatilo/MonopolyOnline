using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design;
using UnityEngine;
using Zenject;

public class PhotonPlayerHandler : Photon.Pun.MonoBehaviourPunCallbacks
{
    private IPlayerRepository repository;
    private IPlayerColorService colorService;
    private IEventBus eventBus;
    private GameSettings gameSettings;
    [Inject]
    public void Construct(IPlayerRepository repository, IPlayerColorService colorService, IEventBus eventBus, GameSettings gameSettings)
    {
        this.repository = repository;
        this.colorService = colorService;
        this.eventBus = eventBus;
        this.gameSettings = gameSettings;   
    }

    public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer)
    {
        var player = new PlayerData(
            newPlayer.NickName,
            gameSettings.startPlayerMoney,
            newPlayer.ActorNumber,
            colorService.GetColorForPlayer(newPlayer.ActorNumber),
            gameSettings.loanAmount,
            newPlayer);

        repository.AddPlayer(player);
        eventBus.Publish(new PlayerJoinedEvent(player));
    }
  
    public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer)
    {
        repository.RemovePlayer(otherPlayer.ActorNumber);
        eventBus.Publish(new PlayerLeftEvent(otherPlayer.ActorNumber));
    }
}