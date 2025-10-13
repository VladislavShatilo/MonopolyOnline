using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.TestTools;

public class UITradeWindowPlayModeTests
{
    private GameObject go;
    private UITradeWindow tradeWindow;

    private Button offerButton;
    private Button closeButton;
    private TMP_InputField leftInput;
    private TMP_InputField rightInput;
    private UIMoneyTrade leftMoneyTrade;
    private UIMoneyTrade rightMoneyTrade;

    private PlayerData fromPlayerData;
    private PlayerData toPlayerData;

    private TradeOffer testOffer;

    [UnitySetUp]
    public IEnumerator SetUp()
    {
        // Сначала создаём все UI элементы
        offerButton = new GameObject("OfferButton").AddComponent<Button>();
        closeButton = new GameObject("CloseButton").AddComponent<Button>();
        leftInput = new GameObject("LeftInput").AddComponent<TMP_InputField>();
        rightInput = new GameObject("RightInput").AddComponent<TMP_InputField>();
        leftMoneyTrade = new GameObject("LeftMoneyTrade").AddComponent<UIMoneyTrade>();
        rightMoneyTrade = new GameObject("RightMoneyTrade").AddComponent<UIMoneyTrade>();

        // Затем создаём сам компонент
        go = new GameObject("UITradeWindow");
        tradeWindow = go.AddComponent<UITradeWindow>();

        // Присваиваем поля через Reflection
        typeof(UITradeWindow).GetField("offerButton", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .SetValue(tradeWindow, offerButton);
        typeof(UITradeWindow).GetField("closeButton", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .SetValue(tradeWindow, closeButton);
        typeof(UITradeWindow).GetField("leftMoneyInputField", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .SetValue(tradeWindow, leftInput);
        typeof(UITradeWindow).GetField("rightMoneyInputField", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .SetValue(tradeWindow, rightInput);
        typeof(UITradeWindow).GetField("leftUIMoneyTrade", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .SetValue(tradeWindow, leftMoneyTrade);
        typeof(UITradeWindow).GetField("rightUIMoneyTrade", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .SetValue(tradeWindow, rightMoneyTrade);

        // Теперь вручную вызываем OnEnable, чтобы подписать события
        typeof(UITradeWindow).GetMethod("OnEnable", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .Invoke(tradeWindow, null);


        fromPlayerData = new PlayerData("p1", 5000, 1, null);
        toPlayerData = new PlayerData("p2", 5000, 2, null);
        // Создаем тестовое предложение
        testOffer = new TradeOffer(fromPlayerData, toPlayerData);

        yield return null;
    }

    [UnityTearDown]
    public IEnumerator TearDown()
    {
        Object.DestroyImmediate(go);
        Object.DestroyImmediate(offerButton.gameObject);
        Object.DestroyImmediate(closeButton.gameObject);
        Object.DestroyImmediate(leftInput.gameObject);
        Object.DestroyImmediate(rightInput.gameObject);
        Object.DestroyImmediate(leftMoneyTrade.gameObject);
        Object.DestroyImmediate(rightMoneyTrade.gameObject);

        yield return null;
    }

    [UnityTest]
    public IEnumerator Show_ShouldSetupUIAndValidateButton()
    {
        tradeWindow.Show(testOffer);
        yield return null;

        Assert.IsFalse(offerButton.interactable); // без введенных денег кнопка должна быть неактивна
    }

    [UnityTest]
    public IEnumerator MoneyInputFields_ShouldUpdateOfferAndValidateButton()
    {
        tradeWindow.Show(testOffer);
        yield return null;

        leftInput.onEndEdit.Invoke("100");
        rightInput.onEndEdit.Invoke("100");
        yield return null;

        Assert.AreEqual(100, testOffer.FromMoney);
        Assert.AreEqual(100, testOffer.ToMoney);
        Assert.IsTrue(offerButton.interactable);
    }

    [UnityTest]
    public IEnumerator OfferButtonClick_ShouldHideWindow()
    {
        bool windowHidden = false;

        tradeWindow.SetOfferAction(() => windowHidden = true);
        tradeWindow.Show(testOffer);
        yield return null;

        leftInput.onEndEdit.Invoke("100");
        rightInput.onEndEdit.Invoke("100");
        yield return null;

        offerButton.onClick.Invoke();
        yield return null;

        Assert.IsTrue(windowHidden);
    }

    [UnityTest]
    public IEnumerator CloseButtonClick_ShouldResetUI()
    {
        tradeWindow.Show(testOffer);
        yield return null;

        leftInput.onEndEdit.Invoke("50");
        rightInput.onEndEdit.Invoke("60");
        yield return null;

        closeButton.onClick.Invoke();
        yield return null;

        Assert.AreEqual("0", leftInput.text);
        Assert.AreEqual("0", rightInput.text);
    }
}
