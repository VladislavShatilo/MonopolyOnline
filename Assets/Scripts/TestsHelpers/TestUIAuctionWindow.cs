using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class TestUIAuctionWindow : UIAuctionWindow
{
    public bool showWindowCalled;
    public bool hideWindowCalled;

    public override void ShowWindow() => showWindowCalled = true;
    public override void HideWindow() => hideWindowCalled = true;

    public void InvokePlayClicked() => HandlePlayClicked();
    public void InvokePassClicked() => HandlePassClicked();
    public void TestOnEnable() => base.OnEnable();
    public void TestOnDisable() => base.OnDisable();


}