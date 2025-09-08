using NUnit.Framework;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

public class UILoanPayWindowTests
{
    private UILoanPayWindow window;
    private GameObject windowGO;
    private WindowAnimation mockAnimation;
    private bool payEventReceived;
    private int payEventPlayerId;

    private PlayerData testPlayer;

    [SetUp]
    public void Setup()
    {
        // Создаём объект окна
        windowGO = new GameObject("UILoanPayWindow");
        windowGO.SetActive(false);
        window = windowGO.AddComponent<UILoanPayWindow>();

        // Кнопки
        var payGO = new GameObject("PayLoanButton", typeof(Button));
        var cantPayGO = new GameObject("CantPayLoanButton", typeof(Button));
        window.PayLoanButton = payGO.GetComponent<Button>();
        window.CantPayLoanButton = cantPayGO.GetComponent<Button>();

        // Тексты
        var payTextGO = new GameObject("PayLoanText", typeof(TextMeshProUGUI));
        var cantPayTextGO = new GameObject("CantPayLoanText", typeof(TextMeshProUGUI));
        window.PayLoanText = payTextGO.GetComponent<TextMeshProUGUI>();
        window.CantPayLoanText = cantPayTextGO.GetComponent<TextMeshProUGUI>();

        // Заглушка WindowAnimation
        var animGO = new GameObject("WindowAnimation");
        mockAnimation = animGO.AddComponent<WindowAnimation>();
        window.WindowAnimation = mockAnimation;

        var rectGO = new GameObject("Rect", typeof(RectTransform));
        rectGO.transform.SetParent(animGO.transform);
        mockAnimation.WindowRectTransform = rectGO.GetComponent<RectTransform>();

        EventBus.ClearAll();

        windowGO.SetActive(true);

        // Тестовый игрок
        var color = new PlayerColor(1, 0, 0);

        testPlayer = new PlayerData("Test", 6000, 15, color, null);

        // Настройка PhotonNetwork.LocalPlayer для теста
        if (PhotonNetwork.LocalPlayer == null)
        {
            var go = new GameObject("PhotonPlayer");
            go.AddComponent<PhotonView>();
        }
    }

    [TearDown]
    public void Teardown()
    {
        EventBus.ClearAll();
        Object.Destroy(windowGO);
        Object.Destroy(mockAnimation.gameObject);
    }

    [UnityTest]
    public IEnumerator ShowLoanWindow_UpdatesUIAndButtons()
    {
        window.ShowLoanWindow(testPlayer);
        yield return null;
        yield return new WaitForSeconds(0.5f);

        Assert.AreEqual(new Vector3(0, 0, 0), window.WindowAnimation.WindowRectTransform.position);

        Assert.IsTrue(window.PayLoanButton.gameObject.activeSelf, "PayLoanButton should be active");
        Assert.IsFalse(window.CantPayLoanButton.gameObject.activeSelf, "CantPayLoanButton should be inactive");
        Assert.AreEqual("Заплатите банку 5,500", window.PayLoanText.text);
        Assert.AreEqual("Заплатите банку 5,500", window.CantPayLoanText.text);
    }
    [UnityTest]
    public IEnumerator ShowLoanWindow_UpdatesUIAndButtonsNoMoney()
    {
        testPlayer.Money = 0;
        window.ShowLoanWindow(testPlayer);
        yield return null;
        yield return new WaitForSeconds(0.5f);

        Assert.AreEqual(new Vector3(0, 0, 0), window.WindowAnimation.WindowRectTransform.position);

        Assert.IsFalse(window.PayLoanButton.gameObject.activeSelf, "PayLoanButton should be active");
        Assert.IsTrue(window.CantPayLoanButton.gameObject.activeSelf, "CantPayLoanButton should be inactive");
        Assert.AreEqual("Заплатите банку 5,500", window.PayLoanText.text);
        Assert.AreEqual("Заплатите банку 5,500", window.CantPayLoanText.text);
    }
    [UnityTest]
    public IEnumerator PayLoanButton_PublishesEventAndHidesWindow()
    {
        yield return null;


        EventBus.Subscribe<PayLoanEvent>(e =>
        {
            payEventReceived = true;
            payEventPlayerId = e.PlayerId;
        });

        typeof(UILoanPayWindow).GetField("player",
       System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
       .SetValue(window, testPlayer);

        window.PayLoanButton.onClick.Invoke();

        yield return null;

        Assert.AreEqual(15, payEventPlayerId);

        Assert.IsTrue(payEventReceived);
        yield return new WaitForSeconds(0.5f);

        Assert.AreEqual(new Vector3(0,160,0), window.WindowAnimation.WindowRectTransform.position);
    }

    [UnityTest]
    public IEnumerator OnEnable_AddsButtonListener()
    {
        bool clicked = false;
        window.PayLoanButton.onClick.AddListener(() => clicked = true);
        window.enabled = true;
        yield return null;

        window.PayLoanButton.onClick.Invoke();
        Assert.IsTrue(clicked, "PayLoanButton listener not added on OnEnable");
    }
}
