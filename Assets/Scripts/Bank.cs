using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bank : MonoBehaviour
{
    public static Bank Instance { get; private set; }

    public event Action<Player, int> OnBalanceChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

  
    public void AddMoney(Player player, int amount)
    {
        player.Money += amount;
        OnBalanceChanged?.Invoke(player, player.Money);
    }

    public bool RemoveMoney(Player player, int amount)
    {
        if (player.Money < amount)
            return false; // Недостаточно средств

        player.Money -= amount;
        OnBalanceChanged?.Invoke(player, player.Money);
        return true;
    }
    public bool BuyCompany(Player buyer, CompanyData company)
    {
        int price = company.price[0];

        if (buyer.Money < price)
        {
            Debug.Log("Недостаточно средств!");
            return false;
        }

        RemoveMoney(buyer, price);
        company.ownerID = buyer.id;
        company.isBought = true;

        Debug.Log($"{buyer.Name} купил компанию {company.name} за {price}$");
        return true;
    }

    public bool TransferMoney(Player from, Player to, int amount)
    {
        if (!RemoveMoney(from, amount))
            return false;

        AddMoney(to, amount);
        return true;
    }
}
