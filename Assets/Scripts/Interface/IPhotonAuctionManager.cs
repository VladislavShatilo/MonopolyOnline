
public interface IPhotonAuctionManager
{
    void PromptBidRequest(int playerId, int minAllowedBid, int companyId);
   // void EndAuctionRequest(int winnerId, int price, int companyId);
    void PlayerBidRequest(int playerId);
    ///void BroadcastAuctionEnd(int winnerId, int finalPrice);
    void StartAuctionRequest(int starterActorNumber, int companyId, int companyBasePrice);
    void PlayerPassRequest(int playerId);
    void CloseAuctionWindowRequest(int playerId);
}
