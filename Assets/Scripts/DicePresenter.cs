using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class DicePresenter : IInitializable,IDisposable
{
    private  IDiceManager3D diceManager3D;

    [Inject]   
    public void Construct(IDiceManager3D diceManager3D)
    {
        this.diceManager3D = diceManager3D;

    }
    void IInitializable.Initialize()
    {
        EventBus.Subscribe<DiceRolledEvent>(OnDiceRolled);
    }
    void IDisposable.Dispose()
    {
        EventBus.Unsubscribe<DiceRolledEvent>(OnDiceRolled);
    }
    private void OnDiceRolled(DiceRolledEvent e)
    {
        diceManager3D.ShowDice(e.DiceResult.First, e.DiceResult.Second);
    }
}
