using System;
using UnityEngine;

public class RandomDiceService : IDiceService
{
    private readonly System.Random randomDice = new System.Random();
    private const int MAX_NUMBER_DICE = 6;
    public DiceResult Roll()
    {
        int first = randomDice.Next(1, MAX_NUMBER_DICE + 1);
        int second = randomDice.Next(1, MAX_NUMBER_DICE + 1);
        return new DiceResult(first, second);
    }
}
