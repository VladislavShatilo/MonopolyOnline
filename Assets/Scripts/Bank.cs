using Photon.Pun;
using Photon.Realtime;
using System;
using UnityEngine;
using Zenject;

public class Bank : MonoBehaviourPunCallbacks
{
    public static Bank Instance { get; private set; }

    public event Action<PlayerData, int> OnBalanceChanged;
    [Inject] private IPlayerRepository playerRepository;
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
    public void AddMoney(int playerId, int amount)
    {
        PlayerData player = GetPlayerDataById(playerId);
        if (amount <= 0) return;

        int newMoney = player.Money + amount;
        UpdatePlayerMoney(player, newMoney);
    }

    // Убираем деньги с проверкой
    public bool RemoveMoney(int playerId, int amount)
    {
        PlayerData player = GetPlayerDataById(playerId);

        if (amount <= 0) return false;
        if (!HasEnoughMoney(playerId, amount)) return false;

        int newMoney = player.Money - amount;
        UpdatePlayerMoney(player, newMoney);
        return true;
    }

    public bool HasEnoughMoney(int playerId, int amount)
    {
        PlayerData player = GetPlayerDataById(playerId);

        return player.Money >= amount;
    }

    public bool TransferMoney(int fromPlayerId, int toPlayerId, int amount)
    {
        PlayerData fromPlayer = GetPlayerDataById(fromPlayerId);
        PlayerData toPlayer = GetPlayerDataById(toPlayerId);

        if (!RemoveMoney(fromPlayerId, amount)) return false;
        AddMoney(toPlayerId, amount);
        return true;
    }
    private PlayerData GetPlayerDataById(int playerId)
    {
       return playerRepository.GetPlayerById(playerId);

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
