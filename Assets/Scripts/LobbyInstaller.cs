using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using Zenject;

public class LobbyInstaller : MonoInstaller
{
    [SerializeField] private LobbyUI lobbyUIPrefab;

    public override void InstallBindings()
    {
        Container.BindInterfacesAndSelfTo<LobbyUI>().FromInstance(lobbyUIPrefab).AsSingle().NonLazy();
    }
}