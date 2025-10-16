using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class BankService : IBankService
{
    private IPlayerRepository playerRepository;
    private IBankNotifier notifier;

    #region LIFE_CYCLE

    [Inject]
    public void Consturct(IPlayerRepository playerRepository, IBankNotifier notifier)
    {
        this.playerRepository = playerRepository ?? throw new ArgumentNullException(nameof(playerRepository));
        this.notifier = notifier ?? throw new ArgumentNullException(nameof(playerRepository));
    }

    #endregion LIFE_CYCLE

    #region PUBLIC_METHODS

    public void AddMoney(int playerId, int amount)
    {
        if (amount <= 0) return;
        var player = playerRepository.GetPlayerById(playerId) ?? throw new InvalidOperationException(nameof(AddMoney));

        player.Money += amount;
        notifier.NotifyBalanceChanged(player);
    }

    public bool RemoveMoney(int playerId, int amount)
    {
        if (amount <= 0) return false;
        var player = playerRepository.GetPlayerById(playerId) ?? throw new InvalidOperationException(nameof(RemoveMoney));
       
        if (player.Money < amount) return false;

        player.Money -= amount;
        notifier.NotifyBalanceChanged(player);
        return true;
    }

    public bool HasEnoughMoney(int playerId, int amount)
    {
        var player = playerRepository.GetPlayerById(playerId) ?? throw new InvalidOperationException(nameof(RemoveMoney));

        return player.Money >= amount;
    }

    public bool TransferMoney(int fromPlayerId, int toPlayerId, int amount)
    {
        if (!RemoveMoney(fromPlayerId, amount)) return false;
        AddMoney(toPlayerId, amount);
        return true;
    }

    #endregion PUBLIC_METHODS
}