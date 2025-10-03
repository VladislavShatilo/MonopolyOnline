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
    private ITradeService tradeService;
    private ILocalPlayerService localPlayerService;
    private IEventBus eventBus;
    private IPhotonTradeManager photonTradeManager;

    private UIPlayerStats playerStatsPrefab;
    private Transform playersStatsContainer;
    private List<PlayerStatsPresenter> presenters = new List<PlayerStatsPresenter>();

    [Inject]
    public void Construct(IPhotonLoanManager photonLoanManager, ITradeService tradeService, [Inject(Id = "PlayerStatsContainer")] Transform playersStatsContainer,
        [Inject(Id = "PlayerStatsPrefab")] UIPlayerStats playerStatsPrefab, IEventBus eventBus, ILocalPlayerService localPlayerService, IPhotonTradeManager photonTradeManager)
    {

        this.photonLoanManager = photonLoanManager;
        this.tradeService = tradeService;
        this.playerStatsPrefab = playerStatsPrefab;
        this.playersStatsContainer = playersStatsContainer;
        this.eventBus = eventBus;
        this.localPlayerService = localPlayerService;
        this.photonTradeManager = photonTradeManager;
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

    private void OnPlayerJoined(PlayerJoinedEvent e)
    {
        // Создаём View
        var uiStats =  Instantiate(playerStatsPrefab, playersStatsContainer);

        // Зарегистрируем View как IPlayerStatsView
        var view = uiStats as IPlayerStatsView;

        var presenter = new PlayerStatsPresenter(view, photonLoanManager,eventBus, localPlayerService, photonTradeManager);
        presenter.Init(e.Player);
        presenters.Add(presenter);

    }
   
}