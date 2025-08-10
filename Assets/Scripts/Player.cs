using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Player
{
    public int id;
    public string Name;
    public int Money;
    public Color playerColor;

    public Player(string name, int startMoney, int id, Color color)
    {
        Name = name;
        Money = startMoney;
        this.id = id;
        playerColor = color;
    }

}
