using System;

public interface IAuctionWindow
{
    void Show(int playerId, string companyName, int minAllowedBid, int money);
    void Hide();
    void SetPlayAction(Action<int> onPlay);
    void SetPassAction(Action<int> onPass);
}
