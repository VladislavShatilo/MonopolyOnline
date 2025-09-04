
using Zenject;

public class GameplayInstaller : MonoInstaller
{
    public override void InstallBindings()
    {
        Container.Bind<IPlayerRepository>()
                 .To<GameManagerPlayerRepository>()
                 .AsSingle();
    }
}
