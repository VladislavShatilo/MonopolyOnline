using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerColor
{
    public float R { get; }
    public float G { get; }
    public float B { get; }

    public PlayerColor(float r, float g, float b)
    {
        R = r; G = g; B = b;
    }
}
public interface IPlayerColorService    
{
    PlayerColor GetColorForPlayer(int actorNumber);
}
