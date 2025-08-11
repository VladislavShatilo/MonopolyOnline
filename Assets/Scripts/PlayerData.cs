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

    // Ссылка на Photon игрока (можно не хранить, если достаточно id)
    [System.NonSerialized]
    public Player photonPlayer;

    public PlayerData(string name, int startMoney, int id, Color color, Player photonPlayer = null)
    {
        Name = name;
        Money = startMoney;
        this.id = id;
        playerColor = color;
        this.photonPlayer = photonPlayer;
    }

    // Можно добавить методы для обновления данных из CustomProperties Photon
    public void UpdateFromPhotonPlayer(Player photonPlayer)
    {
        this.photonPlayer = photonPlayer;
        this.id = photonPlayer.ActorNumber;
        this.Name = photonPlayer.NickName;

        // Если используешь CustomProperties для денег и цвета, считывай тут
        if (photonPlayer.CustomProperties.TryGetValue("Money", out object money))
        {
            Money = (int)money;
        }
        if (photonPlayer.CustomProperties.TryGetValue("Color", out object colorValue))
        {
            // Пример: хранить цвет в виде int (ARGB), преобразовать обратно
            int argb = (int)colorValue;
            playerColor = IntToColor(argb);
        }
    }

    private Color IntToColor(int argb)
    {
        byte a = (byte)((argb >> 24) & 0xFF);
        byte r = (byte)((argb >> 16) & 0xFF);
        byte g = (byte)((argb >> 8) & 0xFF);
        byte b = (byte)(argb & 0xFF);
        return new Color32(r, g, b, a);
    }

}
