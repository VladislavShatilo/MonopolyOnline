// Application/Interfaces/ICompanyService.cs
public interface ICompanyService
{
   
        void HandleCell(int cellIndex, int playerId);
        void TryBuyCompany(int cellIndex, int playerId, BuyReason reason);
        void TryPayRent(int cellIndex, int playerId);
    
}
