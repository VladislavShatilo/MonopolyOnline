public class TestUIBuyWindow : UIBuyWindow
{
    public bool showWindowCalled;
    public bool hideWindowCalled;

    public override void ShowWindow() => showWindowCalled = true;
    public override void HideWindow() => hideWindowCalled = true;
}