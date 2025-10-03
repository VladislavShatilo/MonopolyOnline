public interface ICompanySyncService
{
    void SyncCompanyBought(int companyId, int playerId, int price);
    void SyncRentPaid(int companyId, int playerId, int ownerId, int rent);
}