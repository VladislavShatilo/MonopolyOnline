using Photon.Pun;
using System.Collections.Generic;
using UnityEngine;

// Интерфейс обработчика клеток
public interface ICellHandler
{
    void Handle(int cellIndex, int playerId);
    void ShowPurchaseUI(PlayerData player,int cellIndex);
    void ShowRentUI(PlayerData player,int cellIndex);
    int GetPrice(int cellIndex);
    int GetRent(int cellIndex);
    int GetOwner(int cellIndex);
}

// Базовый класс с общей логикой
public abstract class BaseCompanyHandler : ICellHandler
{
    
    public virtual void Handle(int cellIndex, int playerId)
    {

        var company = CompanyDatabase.Instance.GetCompanyById(cellIndex);
        if (!company.IsBought)
        {
            CompanyManager.Instance.OfferPurchase(cellIndex, playerId);
        }
        else if (company.IsMortgaged)
        {
            CompanyManager.Instance.EndTurnIfMine();
        }
        else if (company.OwnerId != playerId)
        {
            CompanyManager.Instance.OfferRent(cellIndex, playerId);
        }
        else
        {
            CompanyManager.Instance.EndTurnIfMine();
        }


      
    }

    public abstract void ShowPurchaseUI(PlayerData player,int cellIndex);
    public abstract void ShowRentUI(PlayerData player,int cellIndex);
    public abstract int GetPrice(int cellIndex);
    public abstract int GetRent(int cellIndex);

    public int GetOwner(int cellIndex)
    {
        return CompanyDatabase.Instance.GetCompanyById(cellIndex).OwnerId;
    }

    // Универсальный метод подсчета количества клеток, купленных игроком в определенной группе
    protected int CountOwnedByPlayer(int playerId, CompanyType companyType)
    {
        int count = 0;

        List<Company> companiesList = new List<Company>(CompanyDatabase.Instance.Companies);
        for (int i = 0; i < companiesList.Count; i++)
        {
            if (companiesList[i].IsBought && companiesList[i].OwnerId == playerId && companiesList[i].Type == companyType)
                count++;
        }

        return count;
    }
}

// Обработчик обычной компании
public class DefaultCompanyHandler : BaseCompanyHandler
{
    public override void ShowPurchaseUI(PlayerData player,int cellIndex)
    {
        var cell = CellsManager.Instance.GetCellDataByIndex(cellIndex);
        UIBuyWindow.Instance.ShowBuyWindow(player,cellIndex, cell.companyData.price);
    }

    public override void ShowRentUI(PlayerData player, int cellIndex)
    {
        var company = CompanyDatabase.Instance.GetCompanyById(cellIndex);

        UIPayRentWindow.Instance.Show(player,cellIndex, company.CompanyData.rent[company.RentLevel]);
    }

    public override int GetPrice(int cellIndex)
    {
        return CellsManager.Instance.GetCellDataByIndex(cellIndex).companyData.price;
    }

    public override int GetRent(int cellIndex)
    {
        var company = CompanyDatabase.Instance.GetCompanyById(cellIndex);
        return company.CompanyData.rent[company.RentLevel];
    }
}

// Обработчик полевой компании
public class FieldCompanyHandler : BaseCompanyHandler
{
    public override void ShowPurchaseUI(PlayerData player,int cellIndex)
    {
        var cell = CellsManager.Instance.GetCellDataByIndex(cellIndex);
        UIBuyWindow.Instance.ShowBuyWindow(player,cellIndex, cell.fieldCompanyData.price);
    }

    public override void ShowRentUI(PlayerData player, int cellIndex)
    {
        var rent = GetRent(cellIndex);
        UIPayRentWindow.Instance.Show(player,cellIndex, rent);
    }

    public override int GetPrice(int cellIndex)
    {
        return CellsManager.Instance.GetCellDataByIndex(cellIndex).fieldCompanyData.price;
    }

    public override int GetRent(int cellIndex)
    {
        var company = CompanyDatabase.Instance.GetCompanyById(cellIndex);
        if (!company.IsBought) return 0;

        var cell = CellsManager.Instance.GetCellDataByIndex(cellIndex);
        int ownedCount = CountOwnedByPlayer(company.OwnerId, CompanyType.FieldCompany);
        return cell.fieldCompanyData.rentField[ownedCount - 1];
    }
}

// Обработчик компании с кубиком
public class DiceCompanyHandler : BaseCompanyHandler
{
    public override void ShowPurchaseUI(PlayerData player,int cellIndex)
    {
        var cell = CellsManager.Instance.GetCellDataByIndex(cellIndex);
        UIBuyWindow.Instance.ShowBuyWindow(player, cellIndex, cell.diceCompanyData.price);
    }

    public override void ShowRentUI(PlayerData player, int cellIndex)
    {
        var rent = GetRent(cellIndex);
        UIPayRentWindow.Instance.Show(player,cellIndex, rent);
    }

    public override int GetPrice(int cellIndex)
    {
        return CellsManager.Instance.GetCellDataByIndex(cellIndex).diceCompanyData.price;
    }

    public override int GetRent(int cellIndex)
    {
        var company = CompanyDatabase.Instance.GetCompanyById(cellIndex);
        if (!company.IsBought) return 0;

        var cell = CellsManager.Instance.GetCellDataByIndex(cellIndex);
        int ownedCount = CountOwnedByPlayer(company.OwnerId, CompanyType.DiceCompany);

        return cell.diceCompanyData.rentMultiplier[ownedCount - 1] /** TurnManager.Instance.DiceSum*/;
    }
}
