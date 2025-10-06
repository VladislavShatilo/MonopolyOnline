// Application/Interfaces/ICompanyService.cs
public interface ICompanyService
{
   
    void HandleCell(int cellIndex, int playerId);
    void TryBuyCompany(int cellIndex, int playerId, int price, BuyReason reason);
    void TryPayRent(int cellIndex, int playerId);
    void TransferCompany(int companyId, int newOwnerId);
    int CalculateRent(Company company, int diceSum);
}
