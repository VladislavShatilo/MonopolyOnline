using System;
public interface ILoanPayWindow 
{
    void Show(int playerId, int loanAmount, bool canAfford);
    void Hide();
    void SetPayLoanAction(Action<int> onPayLoan);
}
