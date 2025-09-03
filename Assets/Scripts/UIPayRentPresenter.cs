using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

public class UIPayRentPresenter 
{
    private readonly PlayerData player;
    private readonly int cellIndex;
    private readonly float rent;

    public UIPayRentPresenter(PlayerData player, int cellIndex, float rent)
    {
        this.player = player;
        this.cellIndex = cellIndex;
        this.rent = rent;
    }

    public PresenterState GetState()
    {
        string rentText = $"Заплатите {FormatMoney(rent)}";
        bool canPay = player != null && player.Money >= rent;

        return new PresenterState(rentText, canPay);
    }

    public void OnPayRent()
    {
        if (player == null)
        {
            throw new NullReferenceException();
         
        }

        if (player.Money < rent)
        {
            Debug.Log("Недостаточно средств для аренды");
            return;
        }
        EventBus.Publish(new TryPayRentEvent(cellIndex));
    }

    private static string FormatMoney(float value) =>
        value.ToString("N0", CultureInfo.InvariantCulture);
}
public readonly struct PresenterState
{
    public readonly string RentText;
    public readonly bool CanPay;

    public PresenterState(string rentText, bool canPay)
    {
        RentText = rentText;
        CanPay = canPay;
    }
}
public class TryPayRentEvent
{
    public int CellIndex;
    public TryPayRentEvent(int cellIndex)
    {
        CellIndex = cellIndex;
    }
}