using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class TurnPresenter : ITurnPresenter, IInitializable, IDisposable
{
    private ILocalPlayerService localPlayerService;
    private IPhotonDiceManager photonDiceManager;
    private ITurnWindow uiTurnWindow;
    private IEventBus eventBus;

    #region LIFE_CYCLE

    [Inject]
    public void Construct(ILocalPlayerService localPlayerService, ITurnWindow uiTurnWindow, IPhotonDiceManager photonDiceManager, IEventBus eventBus)
    {
        this.localPlayerService = localPlayerService ?? throw new ArgumentNullException(nameof(localPlayerService));
        this.uiTurnWindow = uiTurnWindow ?? throw new ArgumentNullException(nameof(uiTurnWindow));
        this.photonDiceManager = photonDiceManager ?? throw new ArgumentNullException(nameof(photonDiceManager));
        this.eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
    }

    public void Initialize()
    {
        uiTurnWindow.SetThrowDiceAction(OnThrowDiceClicked);

        eventBus.Subscribe<TurnStartEvent>(OnTurnStart);
    }

    public void Dispose()
    {
        eventBus.Unsubscribe<TurnStartEvent>(OnTurnStart);
    }

    #endregion LIFE_CYCLE

    #region PUBLIC_METHODS

    public void ShowTurnFor(int playerId)
    {
        int localId = localPlayerService.GetLocalPlayerId();

        if (playerId == localId)
        {
            uiTurnWindow.Show();
        }
        else
        {
            uiTurnWindow.Hide();
        }
    }

    public void HideTurn() => uiTurnWindow.Hide();

    #endregion PUBLIC_METHODS

    #region CALLBACKS

    private void OnTurnStart(TurnStartEvent e)
    {
        int localId = localPlayerService.GetLocalPlayerId();

        if (e.PlayerId == localId)
            uiTurnWindow.Show();
        else
            uiTurnWindow.Hide();
    }

    private void OnThrowDiceClicked()
    {
        int result1 = uiTurnWindow.GetSteps1();
        int result2 = uiTurnWindow.GetSteps2();
        photonDiceManager.RequestDiceRoll(localPlayerService.GetLocalPlayerId(), false, result1, result2);
        uiTurnWindow.Hide();
    }

    #endregion CALLBACKS
}