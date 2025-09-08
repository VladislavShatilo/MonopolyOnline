using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DiceManager3D : MonoBehaviour, IDiceManager3D
{
    [SerializeField] private DiceRoll3D dice1;
    [SerializeField] private DiceRoll3D dice2;

    public void ShowDice(int first, int second)
    {
        dice1.RollToResult(first);
        dice2.RollToResult(second);
    }
}
