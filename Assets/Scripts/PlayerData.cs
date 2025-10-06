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
    public bool SkipNextTurn { get; set; } = false;
    public bool NextMoveBackward { get; set; } = false;
    public bool HasLoan = false;
    public int LoanTurnsLeft;
    public int LastDiceSum;

    public List<Company> OwnedCompanies = new();

    [System.NonSerialized]
    public Player photonPlayer;

    private int loanAmount;

    #region LIFE_CYCLE

    public PlayerData(string name, int startMoney, int id, PlayerColor color, int loanAmount, Player photonPlayer = null)
    {
        IsInJail = false;
        Name = name;
        Money = startMoney;
        this.Id = id;
        PlayerColor = color;
        this.photonPlayer = photonPlayer;
        CurrentCellId = 0;
        this.loanAmount = loanAmount;
    }

    #endregion LIFE_CYCLE

    #region PUBLIC_METHODS

    public int VisibleCapital
    {
        get
        {
            int capital = Money;
            for (int i = 0; i < OwnedCompanies.Count; i++)
            {
                capital += OwnedCompanies[i].Price;
                if (OwnedCompanies[i].Type == CompanyType.Company)
                {
                    //capital += OwnedCompanies[i].CompanyData.branchPrice * OwnedCompanies[i].RentLevel;
                }
            }
            return capital;
        }
    }

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
                    // liquid += OwnedCompanies[i].branchPrice * OwnedCompanies[i].RentLevel;
                }
            }
            if (!HasLoan)
            {
                liquid += loanAmount; // кредиты
            }
            return liquid;
        }
    }

    public void SendToJail(GameSettings gameSettings)
    {
        IsInJail = true;
        CurrentCellId = gameSettings.jailCellId;
        JailTurnsLeft = gameSettings.jailTurns;
    }

    public void Release()
    {
        IsInJail = false;
        JailTurnsLeft = 0;
    }

    #endregion PUBLIC_METHODS
}