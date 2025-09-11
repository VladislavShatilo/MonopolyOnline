
public interface IPhotonLoanManager
{
    void SendTakeLoan(int playerId);
    void SendPayLoan(int playerId);
    void ShowLoanWindow(int playerId);

}
