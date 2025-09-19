using System;

public interface IRansomJailWindow
{
    void Show(int playerId, int fine,bool canAfford);
    void Hide();
    void HardHide();   
    void SetRansomAction(Action<int> onRansom);
}