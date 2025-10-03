
public interface IPhotonTradeManager
{
    void SendTradeRequest(int fromPlayerId, int toPlayerId);
    void SendTradeOffer(TradeOffer offer);
    void SendTradeResult(bool accepted);
    void UpdateTradeTimer(int playerId, float timeLeft);
    void CompleteTrade(bool accepted);
}
