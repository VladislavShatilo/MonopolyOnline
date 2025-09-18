using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class TurnPresenter : ITurnPresenter,IInitializable, IDisposable
{
    private ILocalPlayerService localPlayerService;
    private  IPhotonDiceManager photonDiceManager;
    private ITurnWindow uiTurnWindow;
    private IEventBus eventBus;
    [Inject]
    public void Construct(ILocalPlayerService localPlayerService, ITurnWindow uiTurnWindow, IPhotonDiceManager photonDiceManager, IEventBus eventBus)
    {
        this.localPlayerService = localPlayerService;
        this.uiTurnWindow = uiTurnWindow;
        this.photonDiceManager = photonDiceManager;
        this.eventBus = eventBus;

    }
    void IInitializable.Initialize()
    {
        uiTurnWindow.SetThrowDiceAction(OnThrowDiceClicked);

        eventBus.Subscribe<TurnStartEvent>(OnTurnStart);
    }
    void IDisposable.Dispose()
    {
        eventBus.Unsubscribe<TurnStartEvent>(OnTurnStart);
    }
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
        int result = uiTurnWindow.GetSteps();
        photonDiceManager.RequestDiceRoll(localPlayerService.GetLocalPlayerId(), false,result);
        uiTurnWindow.Hide();
    }
    public void ShowTurnFor(int playerId) => uiTurnWindow.Show();
    public void HideTurn() => uiTurnWindow.Hide();
}
