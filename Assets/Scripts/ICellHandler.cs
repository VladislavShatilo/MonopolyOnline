public interface ICellHandler
{
    void Handle(int cellIndex, int playerId);
    void ShowPurchaseUI(int cellIndex);
    void ShowRentUI(int cellIndex);
    int GetPrice(int cellIndex);
    int GetRent(int cellIndex);

    int GetOwner(int cellIndex);
}

public class DefaultCompanyHandler : ICellHandler
{
    public void Handle(int cellIndex, int playerId)
    {
        var company = CompanyDatabase.Instance.GetCompanyById(cellIndex);
        if (!company.IsBought)
        {
            CompanyManager.Instance.OfferPurchase(cellIndex, playerId);
        }
        else if (company.IsBought && company.OwnerId != playerId)
        {
            CompanyManager.Instance.OfferRent(cellIndex, playerId);
        }

    }

    public void ShowPurchaseUI(int cellIndex)
    {
        var cell = CellsManager.Instance.GetCellDataByIndex(cellIndex);
        UIBuyWindow.Instance.ShowBuyWindow(cellIndex, cell.companyData);
    }
    public void ShowRentUI(int cellIndex)
    {
        var company = CompanyDatabase.Instance.GetCompanyById(cellIndex);

        UIPayRent.Instance.ShowRentWindow(cellIndex, company.CompanyBranchData.rent[company.RentLevel]);
    }
    public int GetPrice(int cellIndex)
    {
        return CellsManager.Instance.GetCellDataByIndex(cellIndex).companyData.price;
    }
    public int GetRent(int cellIndex)
    {
        var company = CompanyDatabase.Instance.GetCompanyById(cellIndex);
        return company.CompanyBranchData.rent[company.RentLevel];
    }
    public int GetOwner(int cellIndex)
    {
        var company = CompanyDatabase.Instance.GetCompanyById(cellIndex);
        return company.OwnerId;
    }
}

public class FieldCompanyHandler : ICellHandler
{
    public void Handle(int cellIndex, int playerId)
    {
        var company = CompanyDatabase.Instance.GetCompanyById(cellIndex);
        if (!company.IsBought)
        {
            CompanyManager.Instance.OfferPurchase(cellIndex, playerId);
        }
        else if (company.IsBought && company.OwnerId != playerId)
        {
            CompanyManager.Instance.OfferRent(cellIndex, playerId);
        }
       
    }

    public void ShowPurchaseUI(int cellIndex)
    {
        var cell = CellsManager.Instance.GetCellDataByIndex(cellIndex);
        UIBuyWindow.Instance.ShowBuyWindow(cellIndex, cell.fieldCompanyData);
    }
    public void ShowRentUI(int cellIndex)
    {
        var cell = CellsManager.Instance.GetCellDataByIndex(cellIndex);
        var company = CompanyDatabase.Instance.GetCompanyById(cellIndex);

        UIPayRent.Instance.ShowRentWindow(cellIndex, cell.fieldCompanyData.rentField[company.RentLevel]);
    }
    public int GetPrice(int cellIndex)
    {
        return CellsManager.Instance.GetCellDataByIndex(cellIndex).fieldCompanyData.price;
    }
    public int GetRent(int cellIndex)
    {
        var company = CompanyDatabase.Instance.GetCompanyById(cellIndex);
        return CellsManager.Instance.GetCellDataByIndex(cellIndex).fieldCompanyData.rentField[company.RentLevel];
    }
    public int GetOwner(int cellIndex)
    {
        var company = CompanyDatabase.Instance.GetCompanyById(cellIndex);
        return company.OwnerId;
    }
}

public class DiceCompanyHandler : ICellHandler
{
    public void Handle(int cellIndex, int playerId)
    {
        var company = CompanyDatabase.Instance.GetCompanyById(cellIndex);
        if (!company.IsBought)
        {
            CompanyManager.Instance.OfferPurchase(cellIndex, playerId);
        }
        else if (company.IsBought && company.OwnerId != playerId)
        {
            CompanyManager.Instance.OfferRent(cellIndex, playerId);
        }
       
    }
    public void ShowPurchaseUI(int cellIndex)
    {
        var cell = CellsManager.Instance.GetCellDataByIndex(cellIndex);
        UIBuyWindow.Instance.ShowBuyWindow(cellIndex, cell.diceCompanyData);
    }
    public void ShowRentUI(int cellIndex)
    {
        var cell = CellsManager.Instance.GetCellDataByIndex(cellIndex);
        var company = CompanyDatabase.Instance.GetCompanyById(cellIndex);

        UIPayRent.Instance.ShowRentWindow(cellIndex, cell.diceCompanyData.rentMultiplier[company.RentLevel]*RandomNumbers.Instance.SumOfDices());
    }
    public int GetPrice(int cellIndex)
    {
        return CellsManager.Instance.GetCellDataByIndex(cellIndex).diceCompanyData.price;
    }
    public int GetRent(int cellIndex)
    {
        var company = CompanyDatabase.Instance.GetCompanyById(cellIndex);
        return CellsManager.Instance.GetCellDataByIndex(cellIndex).diceCompanyData.rentMultiplier[company.RentLevel];
    }
    public int GetOwner(int cellIndex)
    {
        var company = CompanyDatabase.Instance.GetCompanyById(cellIndex);
        return company.OwnerId;
    }
}
