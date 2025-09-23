public interface IPlayerStatsView
{
    void SetName(string name);
    void SetMoney(int money);
    void SetCapital(int visibleCapital, int liquidAssets);
    void SetTimer(bool active, float timeLeft, bool highlightTurn, bool highlightAuction);
    void SetLoan(bool hasLoan, int turnsLeft, bool isLocal);
    void SetTradeButtonVisible(bool visible);
    void SetLoanButtonsVisible(bool canTakeLoan, bool canPayLoan);
    void SetLeaveButtonVisible(bool visible);
    void SetTurnHighlightVisible(bool visible);
    void SetAuctionHighlightVisible(bool visible);
    void SetLoanContainerVisible(bool visible);
    void SetTimerGOVisible(bool visible);
    void BindTradeAction(System.Action onTrade);
    void BindTakeLoanAction(System.Action onTakeLoan);
    void BindPayLoanAction(System.Action onPayLoan);
}
