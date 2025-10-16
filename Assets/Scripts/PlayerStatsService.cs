using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class PlayerStatsService : MonoBehaviour, IDisposable
{
    private IPhotonLoanManager photonLoanManager;
    private ILocalPlayerService localPlayerService;
    private IEventBus eventBus;
    private IPhotonTradeManager photonTradeManager;

    private UIPlayerStats playerStatsPrefab;
    private Transform playersStatsContainer;
    private List<PlayerStatsPresenter> presenters = new();

    #region LIFE_CYCLE

    [Inject]
    public void Construct(IPhotonLoanManager photonLoanManager, [Inject(Id = "PlayerStatsContainer")] Transform playersStatsContainer,
      [Inject(Id = "PlayerStatsPrefab")] UIPlayerStats playerStatsPrefab, IEventBus eventBus, ILocalPlayerService localPlayerService, IPhotonTradeManager photonTradeManager)
    {
        this.photonLoanManager = photonLoanManager ?? throw new ArgumentNullException(nameof(photonLoanManager));
        this.playerStatsPrefab = playerStatsPrefab ?? throw new ArgumentNullException(nameof(playerStatsPrefab));
        this.playersStatsContainer = playersStatsContainer ?? throw new ArgumentNullException(nameof(playersStatsContainer));
        this.eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
        this.localPlayerService = localPlayerService ?? throw new ArgumentNullException(nameof(localPlayerService));
        this.photonTradeManager = photonTradeManager ?? throw new ArgumentNullException(nameof(photonTradeManager));
    }

    public void Initialize()
    {
        eventBus.Subscribe<PlayerJoinedEvent>(OnPlayerJoined);
    }

    void IDisposable.Dispose()
    {
        eventBus.Unsubscribe<PlayerJoinedEvent>(OnPlayerJoined);
        foreach (var presenter in presenters)
        {
            presenter.Dispose();
        }
        presenters.Clear();
    }

    #endregion LIFE_CYCLE

    #region CALLBACKS

    private void OnPlayerJoined(PlayerJoinedEvent e)
    {
        if (playerStatsPrefab == null || playersStatsContainer == null) return;
        
        var uiStats = Instantiate(playerStatsPrefab, playersStatsContainer);

        var view = uiStats as IPlayerStatsView;

        var presenter = new PlayerStatsPresenter(view, photonLoanManager, eventBus, localPlayerService, photonTradeManager);
        if (presenter != null)
        {
            presenter.Init(e.Player);
            presenters.Add(presenter);
        }
    }

    #endregion CALLBACKS

}