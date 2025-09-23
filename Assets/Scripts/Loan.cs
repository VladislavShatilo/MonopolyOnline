using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Loan
{
    public const int MaxTurns = 15;
    public int Amount { get; }
    public int TurnsLeft { get; private set; }

    public Loan(int amount)
    {
        Amount = amount;
        TurnsLeft = MaxTurns;
    }

    public void ReduceTurn()
    {
        TurnsLeft--;
        if (TurnsLeft < 0) TurnsLeft = 0;
    }

    public bool IsExpired() => TurnsLeft == 0;
}
