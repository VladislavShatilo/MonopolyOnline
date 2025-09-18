using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class DicePresenter : IInitializable,IDisposable
{
    private  IDiceManager3D diceManager3D;
    private IEventBus eventBus;

    [Inject]   
    public void Construct(IDiceManager3D diceManager3D, IEventBus eventBus)
    {
        this.diceManager3D = diceManager3D;
        this.eventBus = eventBus;

    }
    void IInitializable.Initialize()
    {
        eventBus.Subscribe<DiceRolledEvent>(OnDiceRolled);
    }
    void IDisposable.Dispose()
    {
        eventBus.Unsubscribe<DiceRolledEvent>(OnDiceRolled);
    }
    private void OnDiceRolled(DiceRolledEvent e)
    {
        diceManager3D.ShowDice(e.DiceResult.First, e.DiceResult.Second);
    }
}
