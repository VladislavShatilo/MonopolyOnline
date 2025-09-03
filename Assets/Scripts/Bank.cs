using Photon.Pun;
using Photon.Realtime;
using System;
using UnityEngine;

public class Bank : MonoBehaviourPunCallbacks
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

    // Добавляем деньги с синхронизацией
    public void AddMoney(PlayerData player, int amount)
    {
        if (amount <= 0) return;

        int newMoney = player.Money + amount;
        UpdatePlayerMoney(player, newMoney);
    }

    // Убираем деньги с проверкой
    public bool RemoveMoney(PlayerData player, int amount)
    {
        if (amount <= 0) return false;
        if (!HasEnoughMoney(player, amount)) return false;

        int newMoney = player.Money - amount;
        UpdatePlayerMoney(player, newMoney);
        return true;
    }

    public bool HasEnoughMoney(PlayerData player, int amount)
    {
        return player.Money >= amount;
    }

    public bool TransferMoney(PlayerData from, PlayerData to, int amount)
    {
        if (!RemoveMoney(from, amount)) return false;
        AddMoney(to, amount);
        return true;
    }

    private void UpdatePlayerMoney(PlayerData player, int newAmount)
    {
        player.Money = newAmount;
        OnBalanceChanged?.Invoke(player, newAmount);
        EventBus.Publish(new OnUpdatePlayerCapitalEvent(player));
        // Если игрок Photon
        if (player.photonPlayer != null)
        {
            var props = new ExitGames.Client.Photon.Hashtable { { "Money", newAmount } };
            player.photonPlayer.SetCustomProperties(props);
        }
    }
}
