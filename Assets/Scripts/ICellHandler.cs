
public interface ICellHandler
{
    void Handle(int cellIndex, int playerId);
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
        else if (company.OwnerId == playerId)
        {
            //CompanyManager.Instance.OfferBranch(cellIndex, playerId);
        }
        else
        {
           // CompanyManager.Instance.OfferRent(cellIndex, playerId);
        }
    }
}

public class DiceCompanyHandler : ICellHandler
{
    public void Handle(int cellIndex, int playerId)
    {
        // например, аренда зависит от броска кубиков
       // int diceSum = DiceManager.Instance.LastRolledSum;
       // CompanyManager.Instance.OfferRent(cellIndex, playerId, diceSum);
    }
}

public class FieldCompanyHandler : ICellHandler
{
    public void Handle(int cellIndex, int playerId)
    {
        // специфическая логика поля
       // CompanyManager.Instance.OfferSpecialFieldPurchase(cellIndex, playerId);
    }
}