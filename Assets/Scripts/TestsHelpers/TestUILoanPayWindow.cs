using TMPro;
using UnityEngine.UI;

public class TestUILoanPayWindow : UILoanPayWindow
{
    public bool showWindowCalled;
    public bool hideWindowCalled;

    public override void ShowWindow() => showWindowCalled = true;
    public override void HideWindow() => hideWindowCalled = true;

  
}