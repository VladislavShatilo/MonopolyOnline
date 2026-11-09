using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DiceManager3D : MonoBehaviour, IDiceManager3D
{
    [SerializeField] private DiceRoll3D dice1;
    [SerializeField] private DiceRoll3D dice2;

    public void ShowDice(int first, int second)
    {
        if (dice1 == null || dice2 == null)
            throw new MissingReferenceException("DiceManager3D: Один или оба кубика (dice1, dice2) не заданы в инспекторе.");

        if (dice1.gameObject == null || dice2.gameObject == null)
            throw new MissingReferenceException("DiceManager3D: GameObject одного из кубиков уничтожен.");

        dice1.gameObject.SetActive(true);
        dice2.gameObject.SetActive(true);

        dice1.RollToResult(first);
        dice2.RollToResult(second);
    }
}
