
public interface IPhotonTradeManager
{
    void SendTradeRequest(int fromPlayerId, int toPlayerId);
    void SendTradeProposal(TradeOffer offer);
    void SendTradeResult(bool accepted);
    void UpdateTradeTimer(int playerId, float timeLeft);

    void ReceiveTradeProposal(TradeOffer offer);
    void CompleteTrade(bool accepted);
}
