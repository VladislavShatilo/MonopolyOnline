using TMPro;
using UnityEngine;

public class TestUIPayRentWindow : UIPayRentWindow
{

    public bool showCalled;
    public bool hideCalled;
    public bool hardHideCalled;

    public override void ShowWindow() => showCalled = true;
    public override void HideWindow() => hideCalled = true;
    public override void HardHideWindow() => hardHideCalled = true;
}