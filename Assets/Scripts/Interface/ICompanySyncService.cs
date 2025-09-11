public interface ICompanySyncService
{
    void SyncCompanyBought(int companyId, int playerId, int price, BuyReason reason);
    void SyncRentPaid(int companyId, int playerId, int ownerId, int rent);
}