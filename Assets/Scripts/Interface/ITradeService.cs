using System.Security.Cryptography;

public interface ITradeService
{
    bool IsTradeActive { get; }
    TradeOffer CurrentOffer { get; }

    void StartTrade(int fromPlayerId, int toPlayerId);
    void CancelTrade();

    void AddCompanyToOffer(int playerId, Company company);
    void RemoveCompanyFromOffer(int playerId, Company company);
    void OnTradeProposalReceived(TradeOffer offer);
    void OnTradeCompleted(bool accepted);
}
