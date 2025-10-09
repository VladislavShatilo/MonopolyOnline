using System.Collections.Generic;
using Zenject;
public class GameManager
{
    private IPlayerRepository repository;
    private IPlayerSpawner spawner;
    private IPlayerColorService colorService;
    private IEventBus eventBus;
    private GameSettings gameSettings;

    #region LIFE_CYCLE

    [Inject]
    public void Construct(IPlayerRepository repository, IPlayerSpawner spawner, IPlayerColorService colorService,
       IEventBus eventBus, GameSettings gameSettings)
    {
        this.repository = repository;
        this.spawner = spawner;
        this.colorService = colorService;
        this.eventBus = eventBus;
        this.gameSettings = gameSettings;
    }

    #endregion LIFE_CYCLE

    #region PUBLIC_METHODS

    public void Initialize()
    {
        foreach (var p in Photon.Pun.PhotonNetwork.PlayerList)
        {
            if (repository.GetPlayerById(p.ActorNumber) != null) continue;

            var player = new PlayerData(
                p.NickName,
                gameSettings.startPlayerMoney,
                p.ActorNumber,
                colorService.GetColorForPlayer(p.ActorNumber),
                p);

            repository.AddPlayer(player);
            eventBus.Publish(new PlayerJoinedEvent(player));
        }

        spawner.SpawnLocalPlayer(Photon.Pun.PhotonNetwork.LocalPlayer.ActorNumber);

    }

    #endregion PUBLIC_METHODS

}
