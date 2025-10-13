//using NUnit.Framework;
//using UnityEngine;
//using UnityEngine.TestTools;
//using TMPro;
//using UnityEngine.UI;
//using Zenject;
//using System.Collections;

//public class UITradeWindowPlayModeTests 
//{
//    private GameObject windowGO;
//    private UITradeWindow tradeWindow;

//    private Transform leftPanel;
//    private Transform rightPanel;
//    private GameObject companyPrefab;

//    private TMP_InputField leftMoneyInput;
//    private TMP_InputField rightMoneyInput;
//    private Button offerButton;
//    private Button closeButton;

//    private UIMoneyTrade leftMoneyTrade;
//    private UIMoneyTrade rightMoneyTrade;

//    private TextMeshProUGUI leftTotalText;
//    private TextMeshProUGUI rightTotalText;
//    private TextMeshProUGUI leftMoneyText;
//    private TextMeshProUGUI rightMoneyText;
//    private TextMeshProUGUI leftPlayerNameText;
//    private TextMeshProUGUI rightPlayerNameText;

//    private DiContainer container;

//    [UnitySetUp]
//    public IEnumerator SetUp()
//    {
//        // Создаем DI контейнер
//        container = new GameObject("DIContainer").AddComponent<DiContainer>();

//        // Создаем окно торговли
//        windowGO = new GameObject("TradeWindow");
//        tradeWindow = windowGO.AddComponent<UITradeWindow>();

//        // Панели и префаб
//        leftPanel = new GameObject("LeftPanel").transform;
//        leftPanel.SetParent(windowGO.transform);
//        rightPanel = new GameObject("RightPanel").transform;
//        rightPanel.SetParent(windowGO.transform);
//        companyPrefab = new GameObject("CompanyPrefab");
//        companyPrefab.AddComponent<UICompanyTrade>();

//        // Кнопки и InputFields
//        offerButton = new GameObject("OfferButton").AddComponent<Button>();
//        closeButton = new GameObject("CloseButton").AddComponent<Button>();
//        leftMoneyInput = new GameObject("LeftInput").AddComponent<TMP_InputField>();
//        rightMoneyInput = new GameObject("RightInput").AddComponent<TMP_InputField>();

//        // UIMoneyTrade
//        leftMoneyTrade = new GameObject("LeftMoneyTrade").AddComponent<UIMoneyTrade>();
//        rightMoneyTrade = new GameObject("RightMoneyTrade").AddComponent<UIMoneyTrade>();

//        // TextMeshPro элементы
//        leftTotalText = new GameObject("LeftTotalText").AddComponent<TextMeshProUGUI>();
//        rightTotalText = new GameObject("RightTotalText").AddComponent<TextMeshProUGUI>();
//        leftMoneyText = new GameObject("LeftMoneyText").AddComponent<TextMeshProUGUI>();
//        rightMoneyText = new GameObject("RightMoneyText").AddComponent<TextMeshProUGUI>();
//        leftPlayerNameText = new GameObject("LeftName").AddComponent<TextMeshProUGUI>();
//        rightPlayerNameText = new GameObject("RightName").AddComponent<TextMeshProUGUI>();

//        // Присваиваем SerializeField через Reflection
//        SetField(tradeWindow, "leftPanel", leftPanel);
//        SetField(tradeWindow, "rightPanel", rightPanel);
//        SetField(tradeWindow, "companyCardPrefab", companyPrefab);
//        SetField(tradeWindow, "offerButton", offerButton);
//        SetField(tradeWindow, "closeButton", closeButton);
//        SetField(tradeWindow, "leftMoneyInputField", leftMoneyInput);
//        SetField(tradeWindow, "rightMoneyInputField", rightMoneyInput);
//        SetField(tradeWindow, "leftUIMoneyTrade", leftMoneyTrade);
//        SetField(tradeWindow, "rightUIMoneyTrade", rightMoneyTrade);
//        SetField(tradeWindow, "leftTotalAmountText", leftTotalText);
//        SetField(tradeWindow, "rightTotalAmountText", rightTotalText);
//        SetField(tradeWindow, "leftMoneyText", leftMoneyText);
//        SetField(tradeWindow, "rightMoneyText", rightMoneyText);
//        SetField(tradeWindow, "leftPlayerNameText", leftPlayerNameText);
//        SetField(tradeWindow, "rightPlayerNameText", rightPlayerNameText);

//        // Внедрение DI
//        tradeWindow.GetType().GetMethod("Construct", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)
//            .Invoke(tradeWindow, new object[] { container });

//        yield return null;
//    }

//    private void SetField(object target, string fieldName, object value)
//    {
//        var field = target.GetType().GetField(fieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
//        field.SetValue(target, value);
//    }

//    [UnityTest]
//    public IEnumerator RefreshUI_ShouldUpdateTexts()
//    {
//        var offer = new TradeOffer
//        {
//            FromPlayerData = new PlayerData { Id = 1, Name = "Alice", PlayerColor = PlayerColor.Red },
//            ToPlayerData = new PlayerData { Id = 2, Name = "Bob", PlayerColor = PlayerColor.Blue },
//            FromMoney = 100,
//            ToMoney = 200
//        };

//        SetField(tradeWindow, "currentOffer", offer);

//        tradeWindow.RefreshUI();

//        Assert.IsTrue(leftTotalText.text.Contains("100"));
//        Assert.IsTrue(rightTotalText.text.Contains("200"));
//        Assert.IsTrue(leftPlayerNameText.text.Contains("Alice"));
//        Assert.IsTrue(rightPlayerNameText.text.Contains("Bob"));

//        yield return null;
//    }

//    [UnityTest]
//    public IEnumerator OfferButton_ShouldCallAction()
//    {
//        bool clicked = false;
//        tradeWindow.SetOfferAction(() => clicked = true);

//        offerButton.onClick.Invoke();

//        Assert.IsTrue(clicked);
//        yield return null;
//    }

//    [UnityTest]
//    public IEnumerator CloseButton_ShouldResetUI()
//    {
//        // Установим значения
//        leftMoneyInput.text = "500";
//        rightMoneyInput.text = "300";
//        leftMoneyTrade.RefreshUI();
//        rightMoneyTrade.RefreshUI();

//        tradeWindow.SetCloseAction(() => { });

//        closeButton.onClick.Invoke();

//        Assert.AreEqual("0", leftMoneyInput.text);
//        Assert.AreEqual("0", rightMoneyInput.text);
//        yield return null;
//    }

//    [UnityTest]
//    public IEnumerator UpdateTrade_ShouldUpdateOfferAndUI()
//    {
//        var offer = new TradeOffer
//        {
//            FromPlayerData = new PlayerData { Id = 1, Name = "Alice", PlayerColor = PlayerColor.Red },
//            ToPlayerData = new PlayerData { Id = 2, Name = "Bob", PlayerColor = PlayerColor.Blue },
//            FromMoney = 50,
//            ToMoney = 150
//        };

//        tradeWindow.UpdateTrade(offer);

//        var currentOffer = (TradeOffer)GetFieldValue(tradeWindow, "currentOffer");
//        Assert.AreEqual(offer, currentOffer);
//        Assert.IsTrue(leftTotalText.text.Contains("50"));
//        Assert.IsTrue(rightTotalText.text.Contains("150"));

//        yield return null;
//    }

//    private object GetFieldValue(object target, string fieldName)
//    {
//        return target.GetType().GetField(fieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(target);
//    }

//    [UnityTearDown]
//    public IEnumerator TearDown()
//    {
//        GameObject.Destroy(windowGO);
//        GameObject.Destroy(companyPrefab);
//        yield return null;
//    }
//}
