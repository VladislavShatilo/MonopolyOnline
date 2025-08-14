using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bank : MonoBehaviour
{
    public static Bank Instance { get; private set; }

    public event Action<PlayerData, int> OnBalanceChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

  
    public void AddMoney(PlayerData player, int amount)
    {
        player.Money += amount;
        OnBalanceChanged?.Invoke(player, player.Money);
    }

    public void RemoveMoney(PlayerData player, int amount)
    {
        player.Money -= amount;
        OnBalanceChanged?.Invoke(player, player.Money);
    }
    public bool hasEnoughMoney(PlayerData player, int amount)
    {
        if (player.Money < amount)
        {
            return false;
        }
        else
        {
            return true;


        }

    }

    public void TransferMoney(PlayerData from, PlayerData to, int amount)
    {
        

        AddMoney(to, amount);
    }
}
