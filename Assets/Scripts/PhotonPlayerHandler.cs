using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design;
using UnityEngine;
using Zenject;

public class PhotonPlayerHandler : MonoBehaviourPunCallbacks
{
    private IPlayerRepository repository;
    private IPlayerColorService colorService;
    private IEventBus eventBus;
    private GameSettings gameSettings;

    #region LIFE_CYCLE

    [Inject]
    public void Construct(IPlayerRepository repository, IPlayerColorService colorService, IEventBus eventBus, GameSettings gameSettings)
    {
        this.repository = repository ?? throw new ArgumentNullException(nameof(repository));
        this.colorService = colorService ?? throw new ArgumentNullException(nameof(colorService));
        this.eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
        this.gameSettings = gameSettings ?? throw new ArgumentNullException(nameof(gameSettings));
    }

    public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer)
    {
        var player = new PlayerData(
            newPlayer.NickName,
            gameSettings.startPlayerMoney,
            newPlayer.ActorNumber,
            colorService.GetColorForPlayer(newPlayer.ActorNumber),
            newPlayer);
        if(player != null)
        {
            repository.AddPlayer(player);
            eventBus.Publish(new PlayerJoinedEvent(player));
        }
       
    }

    public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer)
    {
        repository.RemovePlayer(otherPlayer.ActorNumber);
    }

    #endregion LIFE_CYCLE

}