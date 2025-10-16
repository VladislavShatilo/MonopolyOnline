using System;
using System.Collections.Generic;

public class TradeOffer
{
    public PlayerData FromPlayerData { get; }
    public PlayerData ToPlayerData { get; }

    public List<Company> FromCompanies { get; set; } = new();
    public List<Company> ToCompanies { get; set; } = new();

    public int FromMoney { get; set; }
    public int ToMoney { get; set; }

    #region CONSTRUCTOR

    public TradeOffer(PlayerData fromPlayerData, PlayerData toPlayerData)
    {
        FromPlayerData = fromPlayerData ?? throw new ArgumentNullException(nameof(fromPlayerData));
        ToPlayerData = toPlayerData ?? throw new ArgumentNullException(nameof(toPlayerData));
    }

    #endregion CONSTRUCTOR

    #region PUBLIC_METHODS

    public void SetFromCompanies(List<Company> companies)
    {
        if (companies == null)
        {
            FromCompanies.Clear();
            return;
        }

        if (companies.Contains(null))
            throw new ArgumentException("FromCompanies list contains null element.", nameof(companies));

        FromCompanies.Clear();
        FromCompanies.AddRange(companies);
    }

    public void SetToCompanies(List<Company> companies)
    {
        if (companies == null)
        {
            ToCompanies.Clear();
            return;
        }

        if (companies.Contains(null))
            throw new ArgumentException("ToCompanies list contains null element.", nameof(companies));

        ToCompanies.Clear();
        ToCompanies.AddRange(companies);
    }

    public int GetFromTotalValue()
    {
        int value = FromMoney;

        foreach (var c in FromCompanies)
        {
            if (c == null)
                throw new InvalidOperationException("FromCompanies contains null reference.");
            value += c.Price;
        }

        return value;
    }

    public int GetToTotalValue()
    {
        int value = ToMoney;

        foreach (var c in ToCompanies)
        {
            if (c == null)
                throw new InvalidOperationException("ToCompanies contains null reference.");
            value += c.Price;
        }

        return value;
    }

    public bool IsValid()
    {
        int left = GetFromTotalValue();
        int right = GetToTotalValue();

        if (left <= 0 || right <= 0)
            return false;

        float ratio = (float)left / right;
        return ratio >= 0.5f && ratio <= 2f;
    }

    #endregion PUBLIC_METHODS
}
