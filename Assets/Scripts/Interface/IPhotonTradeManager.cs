
public interface IPhotonTradeManager
{
    void SendTradeRequest(int fromPlayerId, int toPlayerId);
    void SendTradeOffer(TradeOffer offer);
    void SendTradeResult(bool accepted);
    void CompleteTrade(bool accepted);
}
