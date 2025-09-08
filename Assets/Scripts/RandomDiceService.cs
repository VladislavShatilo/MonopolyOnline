using System;
using UnityEngine;

public class RandomDiceService : IDiceService
{
    private readonly System.Random randomDice = new System.Random();

    public DiceResult Roll()
    {
        int first = randomDice.Next(1, 7);
        int second = randomDice.Next(1, 7);
        return new DiceResult(first, second);
    }
}
