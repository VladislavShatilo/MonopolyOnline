using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class PlayerView : MonoBehaviourPun
{
    public void Start()
    {
        EventBus.Publish(new PlayerSpawnedViewEvent(this));

    }
}
public class PlayerSpawnedViewEvent
{
    public PlayerView PlayerView;
    public PlayerSpawnedViewEvent(PlayerView playerView)
    {
        PlayerView= playerView;
    }
}