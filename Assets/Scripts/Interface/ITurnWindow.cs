public interface ITurnWindow
{
    void Show();
    void Hide();
    void HardHide();
    void SetThrowDiceAction(System.Action onClick);
    int GetSteps1();
    int GetSteps2();
}
