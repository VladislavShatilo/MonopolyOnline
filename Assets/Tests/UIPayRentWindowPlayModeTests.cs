using NUnit.Framework;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

public class UIPayRentWindowPlayModeTests
{
    private GameObject windowGO;
    private UIPayRentWindow window;
    private bool eventReceived;
    private int receivedCellIndex;
    [SetUp]
    public void Setup()
    {
        // Создаём объект окна
        windowGO = new GameObject("UIPayRentWindow");
        window = windowGO.AddComponent<UIPayRentWindow>();
        
        // Кнопки и тексты
        var payBtnGO = new GameObject("PayButton", typeof(Button), typeof(RectTransform));
        var payText = new GameObject("PayText", typeof(TextMeshProUGUI));
        payText.transform.SetParent(payBtnGO.transform);

        var cantBtnGO = new GameObject("CantPayButton", typeof(Button), typeof(RectTransform));
        var cantText = new GameObject("CantPayText", typeof(TextMeshProUGUI));
        cantText.transform.SetParent(cantBtnGO.transform);

        // Присваиваем через сеттеры
       //window.PayRentButtonSetter = payBtnGO.GetComponent<Button>();
        //window.PayButtonTextSetter = payText.GetComponent<TextMeshProUGUI>();
        //window.CantPayRentButtonSetter = cantBtnGO.GetComponent<Button>();
       //window.CantPayRentTextSetter = cantText.GetComponent<TextMeshProUGUI>();

        var animGO = new GameObject("WindowAnimation");
        var anim = animGO.AddComponent<WindowAnimation>();
        animGO.transform.SetParent(windowGO.transform);

        var rectTransform = new GameObject("RectTrans", typeof(RectTransform));
        rectTransform.GetComponent<RectTransform>().position = Vector3.zero;
        anim.WindowRectTransform = rectTransform.GetComponent<RectTransform>();
        window.WindowAnimation = anim;

       // EventBus.Subscribe<TryPayRentEvent>(OnTryPayRentTest);
        eventReceived = false;
       // window.WindowAnimationSetter = anim;
        windowGO.SetActive(false);
        windowGO.SetActive(true);
    }
    private void OnTryPayRentTest( )
    {
        
    }
    [TearDown]
    public void Teardown()
    {
        Object.Destroy(windowGO);
      //  EventBus.Unsubscribe<TryPayRentEvent>(OnTryPayRentTest);

    }

    [UnityTest]
    public IEnumerator Show_WithEnoughMoney_ShowsPayButton()
    {
        var color = new PlayerColor(1, 0, 0);

        var player = new PlayerData("Vlad", 500, 0, color, photonPlayer: null);

        //window.Show(player, 0, 200);
        yield return null;
        yield return new WaitForSeconds(0.5f);

        Assert.AreEqual(new Vector3(0, 0, 0), window.WindowAnimation.WindowRectTransform.position);

       // Assert.IsTrue(window.PayRentButton.gameObject.activeSelf);
       // Assert.IsFalse(window.CantPayRentButton.gameObject.activeSelf);
       // Assert.AreEqual("Заплатите 200", window.PayButtonText.text);
    }

    [UnityTest]
    public IEnumerator Show_WithNotEnoughMoney_ShowsCantPayButton()
    {
        var color = new PlayerColor(1, 0, 0);

        var player = new PlayerData("Vlad", 100, 0, color, photonPlayer: null);

        //window.Show(player, 0, 200);
        yield return null;
        yield return new WaitForSeconds(0.5f);

        Assert.AreEqual(new Vector3(0, 0, 0), window.WindowAnimation.WindowRectTransform.position);

      //  Assert.IsFalse(window.PayRentButton.gameObject.activeSelf);
       // Assert.IsTrue(window.CantPayRentButton.gameObject.activeSelf);
       // Assert.AreEqual("Заплатите 200", window.CantPayRentText.text);
    }
    [UnityTest]
    public IEnumerator ClickPayButton_InvokesPresenterLogic()
    {
        var color = new PlayerColor(1, 0, 0);

        var player = new PlayerData("Vlad", 500, 0, color, null);

        // показываем окно
       // window.Show(player, 7, 200); // cellIndex = 7
        yield return null;

        // имитация клика
       // window.PayRentButton.onClick.Invoke();
        yield return null;

        Assert.IsTrue(eventReceived);
        Assert.AreEqual(7, receivedCellIndex);
        yield return new WaitForSeconds(0.5f);

        Assert.AreEqual(new Vector3(0, 160, 0), window.WindowAnimation.WindowRectTransform.position);

    }

}
