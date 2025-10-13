using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TestUITurnWindow : UITurnWindow
{
    public bool showWindowCalled;
    public bool hideWindowCalled;
    public bool hardHideWindowCalled;

    public override void ShowWindow() => showWindowCalled = true;
    public override void HideWindow() => hideWindowCalled = true;
    public override void HardHideWindow() => hardHideWindowCalled = true;

  
}
