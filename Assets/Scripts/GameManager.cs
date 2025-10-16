using System;
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
        this.repository = repository ?? throw new ArgumentNullException(nameof(repository));
        this.spawner = spawner ?? throw new ArgumentNullException(nameof(spawner));
        this.colorService = colorService ?? throw new ArgumentNullException(nameof(colorService));
        this.eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
        this.gameSettings = gameSettings ?? throw new ArgumentNullException(nameof(gameSettings));
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

            if (player != null)
            {
                repository.AddPlayer(player);
                eventBus.Publish(new PlayerJoinedEvent(player));
            }
        }

        spawner.SpawnLocalPlayer(Photon.Pun.PhotonNetwork.LocalPlayer.ActorNumber);

    }

    #endregion PUBLIC_METHODS

}
