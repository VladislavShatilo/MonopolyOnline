
public interface IPhotonLoanManager
{
    void TakeLoanRequest(int playerId);
    void PayLoanRequest(int playerId);
    void ShowLoanWindow(int playerId, int loanAmount);

}
