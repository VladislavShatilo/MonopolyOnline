using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class ChanceBuff
{
    public BuffType Type { get; }
  
    public int MinAmount { get; }
    public int MaxAmount { get; }

    public ChanceBuff(BuffType type, int minAmount = 0, int maxAmount = 0)
    {
        Type = type;
        MinAmount = minAmount;
        MaxAmount = maxAmount;
    }


}
