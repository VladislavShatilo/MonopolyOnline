using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class BankService : IBankService
{
    private  IPlayerRepository playerRepository;
    private  IBankNotifier notifier;

    [Inject]
    public void Consturct(IPlayerRepository playerRepository, IBankNotifier notifier)
    {
        this.playerRepository = playerRepository;
        this.notifier = notifier;
    }

    public void AddMoney(int playerId, int amount)
    {
        if (amount <= 0) return;
        var player = playerRepository.GetPlayerById(playerId);
        player.Money += amount;
        notifier.NotifyBalanceChanged(player);
    }

    public bool RemoveMoney(int playerId, int amount)
    {
        Debug.Log("500");
        if (amount <= 0) return false;
        var player = playerRepository.GetPlayerById(playerId);

        if (player.Money < amount) return false;

        player.Money -= amount;
        notifier.NotifyBalanceChanged(player);
        return true;
    }

    public bool HasEnoughMoney(int playerId, int amount)
    {
        var player = playerRepository.GetPlayerById(playerId);
        return player.Money >= amount;
    }

    public bool TransferMoney(int fromPlayerId, int toPlayerId, int amount)
    {
        if (!RemoveMoney(fromPlayerId, amount)) return false;
        AddMoney(toPlayerId, amount);
        return true;
    }
}
