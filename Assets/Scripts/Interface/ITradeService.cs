using System.Security.Cryptography;

public interface ITradeService
{
    bool IsTradeActive { get; }
    TradeOffer CurrentOffer { get; }

    void StartTrade(int fromPlayerId, int toPlayerId);
    void OfferTrade();
    void AcceptTrade();
    void DeclineTrade();
    void CancelTrade();

    void AddCompanyToOffer(int playerId, Company company);
    void RemoveCompanyFromOffer(int playerId, Company company);
    void SetMoney(int playerId, int amount);
    void OnTradeProposalReceived(TradeOffer offer);
    void OnTradeCompleted(bool accepted);
    void UpdateTimer();
}
