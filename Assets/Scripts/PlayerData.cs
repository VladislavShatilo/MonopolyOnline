using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlayerData
{
    public int id;               // ActorNumber в Photon
    public string Name;
    public int Money;
    public Color playerColor;

    public bool IsInJail = false;
    public int JailTurnsLeft;
    public bool SkipNextTurn = false;
    public bool NextMoveBackward { get; set; } = false;
    public bool HasLoan = false;
    public int LoanTurnsLeft;



    // Ссылка на Photon игрока (можно не хранить, если достаточно id)
    [System.NonSerialized]
    public Player photonPlayer;

    public PlayerData(string name, int startMoney, int id, Color color, Player photonPlayer = null)
    {
        IsInJail = false;
        Name = name;
        Money = startMoney;
        this.id = id;
        playerColor = color;
        this.photonPlayer = photonPlayer;
    }

   
}
