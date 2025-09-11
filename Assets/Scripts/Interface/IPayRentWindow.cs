using System;

public interface IPayRentWindow
{
    void Show(int playerId, int cellIndex, float rent, bool canPay);
    void Hide();
    void HardHide();
    void SetPayAction(Action payAction);
}