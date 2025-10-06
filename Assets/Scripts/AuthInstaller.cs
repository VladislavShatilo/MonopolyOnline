using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class AuthInstaller : MonoInstaller
{
    [SerializeField] private AuthUI authUI;

    public override void InstallBindings()
    {
        Container.BindInterfacesTo<AuthUI>().FromInstance(authUI).AsSingle().NonLazy();
    }
}