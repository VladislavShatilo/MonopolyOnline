using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TestUIRansomJailWindow : UIRansomJailWindow
{
    public bool showWindowCalled;
    public bool hideWindowCalled;
    public bool hardHideWindowCalled;

    public override void ShowWindow() => showWindowCalled = true;
    public override void HideWindow() => hideWindowCalled = true;
    public override void HardHideWindow() => hardHideWindowCalled = true;

    // Добавим публичные свойства для доступа к кнопкам и текстам
   
}
