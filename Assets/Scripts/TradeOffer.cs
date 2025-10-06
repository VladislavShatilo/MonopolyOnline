using System.Collections.Generic;
using Zenject;

public class TradeOffer
{
    public PlayerData FromPlayerData { get; }
    public PlayerData ToPlayerData { get; }

    public List<Company> FromCompanies { get; } = new();
    public List<Company> ToCompanies { get; } = new();

    public int FromMoney { get; set; }
    public int ToMoney { get; set; }

    #region PUBLIC_METHODS

    public void SetFromCompanies(List<Company> companies)
    {
        FromCompanies.Clear();
        if (companies != null)
            FromCompanies.AddRange(companies);
    }

    public void SetToCompanies(List<Company> companies)
    {
        ToCompanies.Clear();
        if (companies != null)
            ToCompanies.AddRange(companies);
    }

    public TradeOffer(PlayerData fromPlayerData, PlayerData toPlayerData)
    {
        FromPlayerData = fromPlayerData;
        ToPlayerData = toPlayerData;
    }
    public int GetFromTotalValue()
    {
        int value = FromMoney;
        foreach (var c in FromCompanies) value += c.Price;
        return value;
    }

    public int GetToTotalValue()
    {
        int value = ToMoney;
        foreach (var c in ToCompanies) value += c.Price;
        return value;
    }

    public bool IsValid()
    {
        int left = GetFromTotalValue();
        int right = GetToTotalValue();

        if (left == 0 || right == 0) return false;
        float ratio = (float)left / right;
        return ratio >= 0.5f && ratio <= 2f;
    }

    #endregion PUBLIC_METHODS


}