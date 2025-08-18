public interface ICellHandler
{
    void Handle(int cellIndex, int playerId);
    void ShowPurchaseUI(int cellIndex);
}

public class DefaultCompanyHandler : ICellHandler
{
    public void Handle(int cellIndex, int playerId)
    {
        var company = CompanyDatabase.Instance.GetCompanyById(cellIndex);
        if (!company.IsBought)
            CompanyManager.Instance.OfferPurchase(cellIndex, playerId);
        //else if (company.OwnerId == playerId)
            //Debug.Log("¬аш сектор Ч можно строить филиал");
        else
            CompanyManager.Instance.OfferRent(cellIndex, playerId);
    }

    public void ShowPurchaseUI(int cellIndex)
    {
        var cell = CellsManager.Instance.GetCellByIndex(cellIndex).GetComponent<CellData>();
        UIBuyWindow.Instance.ShowBuyWindow(cellIndex, cell.companyData);
    }
}

public class FieldCompanyHandler : ICellHandler
{
    public void Handle(int cellIndex, int playerId)
    {
        CompanyManager.Instance.OfferPurchase(cellIndex, playerId);
    }

    public void ShowPurchaseUI(int cellIndex)
    {
        var cell = CellsManager.Instance.GetCellByIndex(cellIndex).GetComponent<CellData>();
        UIBuyWindow.Instance.ShowBuyWindow(cellIndex, cell.fieldCompanyData);
    }
}

public class DiceCompanyHandler : ICellHandler
{
    public void Handle(int cellIndex, int playerId)
    {
       // int diceSum = DiceManager.Instance.LastRolledSum;
       // CompanyManager.Instance.OfferRent(cellIndex, playerId, diceSum * 100); // пример: аренда = кубики * 100
    }

    public void ShowPurchaseUI(int cellIndex)
    {
        var cell = CellsManager.Instance.GetCellByIndex(cellIndex).GetComponent<CellData>();
        UIBuyWindow.Instance.ShowBuyWindow(cellIndex, cell.diceCompanyData);
    }
}
