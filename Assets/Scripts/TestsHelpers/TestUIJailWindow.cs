using TMPro;
using UnityEngine;

public class TestUIJailWindow : UIJailWindow
{
    public bool showWindowCalled;
    public bool hideWindowCalled;
    public bool hardHideWindowCalled;

    public override void ShowWindow() { showWindowCalled = true; }
    public override void HideWindow() { hideWindowCalled = true; }
    public override void HardHideWindow() { hardHideWindowCalled = true; }
}
