using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class ProjectInstaller : MonoInstaller
{
    public override void InstallBindings()
    {

        // Здесь регистрируются сервисы, которые должны жить "вечно"
        Container.Bind<IAuthService>().To<AuthService>().AsSingle();
    }
}
