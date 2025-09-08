using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class PlayerData
{
    public int Id;               // ActorNumber в Photon
    public string Name;
    public int Money;
    public PlayerColor PlayerColor;
    public int CurrentCellId;

    public bool IsInJail = false;
    public int JailTurnsLeft;
    public bool SkipNextTurn = false;
    public bool NextMoveBackward { get; set; } = false;
    public bool HasLoan = false;
    public int LoanTurnsLeft;

    public List<Company> OwnedCompanies = new List<Company>();
    [System.NonSerialized]
    public Player photonPlayer;

    public PlayerData(string name, int startMoney, int id, PlayerColor color, Player photonPlayer = null)
    {
        IsInJail = false;
        Name = name;
        Money = startMoney;
        this.Id = id;
        PlayerColor = color;
        this.photonPlayer = photonPlayer;
        CurrentCellId = 0;
    }

    // Видимая капитализация
    public int VisibleCapital
    {
        get
        {
            int capital = Money;
            for (int i = 0; i < OwnedCompanies.Count; i++)
            {
                capital += OwnedCompanies[i].Price;
                if(OwnedCompanies[i].Type == CompanyType.Company)
                {
                    capital += OwnedCompanies[i].CompanyData.branchPrice * OwnedCompanies[i].RentLevel;
                }

            }
            return capital;
        }
    }

    // Скрытые ресурсы, которые можно быстро мобилизовать
    public int LiquidAssets
    {
        get
        {
            int liquid = Money;
            for (int i = 0; i < OwnedCompanies.Count; i++)
            {
                if (!OwnedCompanies[i].IsMortgaged)
                {
                    liquid += OwnedCompanies[i].MortgagePrice;

                }
                if (OwnedCompanies[i].Type == CompanyType.Company)
                {
                    liquid += OwnedCompanies[i].CompanyData.branchPrice * OwnedCompanies[i].RentLevel;
                }

            }      
            if (!HasLoan)
            {
                liquid += 5000; // кредиты

            }
            return liquid;
        }
    }

    bool CanPay(int amount)
    {
        return LiquidAssets >= amount;
    }

    // Ссылка на Photon игрока (можно не хранить, если достаточно id)
  

   
}
