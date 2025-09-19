public interface IJailWindow
{
    void Show(int playerId, int ransomMoney, bool canAfford);
    void Hide();
    void HardHide();
    void SetThrowDiceAction(System.Action<int> onThrowDice);
    void SetRansomAction(System.Action<int> onRansom);
}