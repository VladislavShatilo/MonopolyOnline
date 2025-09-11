using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class PlayerViewService : IPlayerViewService, IDisposable
{
    private Transform playerRoot;
    private IPlayerColorService colorService;
    private PlayerSettings playerSettings;
    [Inject]
    public void Construct([Inject(Id = "PlayerRoot")] Transform playerRoot, IPlayerColorService colorService,[Inject(Id = "PlayerSettings")]PlayerSettings playerSettings)
    {
        this.playerRoot = playerRoot;
        this.colorService = colorService;
        this.playerSettings = playerSettings;  
    }
    public void InitializePlayer()
    {

        EventBus.Subscribe<PlayerSpawnedViewEvent>(InitializePlayerView);
    }
    void IDisposable.Dispose()
    {

        EventBus.Unsubscribe<PlayerSpawnedViewEvent>(InitializePlayerView);
    }
   

    private void InitializePlayerView(PlayerSpawnedViewEvent e)
    { 
        PlayerView view = e.PlayerView;
        var skin = view.GetComponent<PlayerSkin>();

        view.transform.SetParent(playerRoot, false);
        view.transform.GetComponent<RectTransform>().anchoredPosition = playerSettings.StartPosition;

        if (view.photonView != null)
        {
            var playerColor = colorService.GetColorForPlayer(view.photonView.Owner.ActorNumber);
            skin.SetColorDirect(playerColor.ToUnityColor());
        }
    }
   
}
