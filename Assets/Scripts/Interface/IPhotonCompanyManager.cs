
public interface IPhotonCompanyManager 
{
    void RequestBuyCompany(int cellIndex, int playerId, BuyReason reason);
    void RequestPayRent(int cellIndex, int playerId);
}
