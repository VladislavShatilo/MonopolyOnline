using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DiceResult
{
    public int First { get; }
    public int Second { get; }
    public bool IsDouble => First == Second;
    public int Sum => First + Second;

    public DiceResult(int first, int second)
    {
        if (first < 1 || first > 6) throw new ArgumentOutOfRangeException(nameof(first));
        if (second < 1 || second > 6) throw new ArgumentOutOfRangeException(nameof(second));

        First = first;
        Second = second;
    }
}
