using System;

public interface IAuctionService
{
    void StartAuction(int starterId, int companyId, int basePrice);
    void PlaceBid(int playerId);
    void PassBid(int playerId);
    
}
