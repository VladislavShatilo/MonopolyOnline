using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

public class UIBuyWindowPlayModeTests : MonoBehaviour
{
    private UIBuyWindow window;
    private PlayerData testPlayer;
    private GameObject windowGO;
   

    [SetUp]
    public void Setup()
    {
        windowGO = new GameObject("UIBuyWindow");
        windowGO.SetActive(false); // объект неактивен

        window = windowGO.AddComponent<UIBuyWindow>();

        // Сначала создаём кнопки и тексты
        var buyBtn = new GameObject("BuyBtn", typeof(Button), typeof(RectTransform));
        var auctionBtn = new GameObject("AuctionBtn", typeof(Button), typeof(RectTransform));
        var cantBuyBtn = new GameObject("CantBuyBtn", typeof(Button), typeof(RectTransform));
        window.CantBuyButton = cantBuyBtn.GetComponent<Button>();
        // Присваиваем **до SetActive(true)**!
        window.BuyButton = buyBtn.GetComponent<Button>();
        window.AuctionButton = auctionBtn.GetComponent<Button>();
        window.BuyButtonText = new GameObject("BuyText").AddComponent<TextMeshProUGUI>();
        window.CantBuyButtonText = new GameObject("CantBuyText").AddComponent<TextMeshProUGUI>();
        var animGO = new GameObject("WindowAnimation");
        var anim = animGO.AddComponent<WindowAnimation>();
        animGO.transform.SetParent(windowGO.transform);

        var rectTransform = new GameObject("RectTrans", typeof(RectTransform));
        rectTransform.GetComponent<RectTransform>().position = Vector3.zero;
        anim.WindowRectTransform = rectTransform.GetComponent<RectTransform>();
        window.WindowAnimation = anim;
        var color = new PlayerColor(1, 0, 0);

        testPlayer = new PlayerData("TestPlayer", 0, 0, color, null);
        EventBus.ClearAll(); // вот это добавь

        windowGO.SetActive(true);
    }
    [TearDown]
    public void Teardown()
    {
        EventBus.ClearAll();
        Object.Destroy(windowGO);

    }
   
    [UnityTest]

    public IEnumerator OnEnable_AddsListeners_OnDisable_RemovesListeners()
    {
        bool internalBuyClicked = false;
        bool internalAuctionClicked = false;

        // подписываемся на EventBus, чтобы проверить внутренние слушатели
        EventBus.Subscribe<TryBuyCompanyEvent>((e) => internalBuyClicked = true);
        EventBus.Subscribe<StartAuctionEvent>((e) => internalAuctionClicked = true);

        yield return null;

        // Вызываем внутренние слушатели
        window.BuyButton.onClick.Invoke();
        window.AuctionButton.onClick.Invoke();

        Assert.IsTrue(internalBuyClicked);
        Assert.IsTrue(internalAuctionClicked);

        // Сбрасываем флаги
        internalBuyClicked = false;
        internalAuctionClicked = false;

        // Деактивируем окно — OnDisable должен убрать внутренние слушатели
        windowGO.SetActive(false);

        // Снова вызываем Invoke
        window.BuyButton.onClick.Invoke();
        window.AuctionButton.onClick.Invoke();

        // Теперь внутренние слушатели не должны сработать
        Assert.IsFalse(internalBuyClicked);
        Assert.IsFalse(internalAuctionClicked);

        EventBus.Unsubscribe<TryBuyCompanyEvent>((e) => internalBuyClicked = true);
        EventBus.Unsubscribe<StartAuctionEvent>((e) => internalAuctionClicked = true);
    }

    [UnityTest]
    public IEnumerator Hide_Windows_BuyButton()
    {
     
        yield return null;

        window.BuyButton.onClick.Invoke();
        yield return new WaitForSeconds(0.5f);

        Assert.AreEqual(new Vector3(0, 160, 0), window.WindowAnimation.WindowRectTransform.position);


    }

    [UnityTest]
    public IEnumerator Hide_Windows_AuctionButton()
    {

        yield return null;
        window.AuctionButton.onClick.Invoke();
        yield return new WaitForSeconds(0.5f);

        Assert.AreEqual(new Vector3(0, 160, 0), window.WindowAnimation.WindowRectTransform.position);


    }
    [UnityTest]
    public IEnumerator FormatPrice_ReturnsWithSeparators()
    {
        yield return null;

        var method = typeof(UIBuyWindow)
            .GetMethod("FormatPrice", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        string result = (string)method.Invoke(window, new object[] { 10000 });
        Assert.AreEqual("10,000", result);
    }

    [UnityTest]
    public IEnumerator ShowBuyWindow_PlayerCanAfford_ShowsBuyButton()
    {
        yield return null;
        var color = new PlayerColor(1, 0, 0);

        var testPlayer2 = new PlayerData("TestPlayer", 500, 0, color, null);

        window.ShowBuyWindow(testPlayer2, 1, 200);
        yield return new WaitForSeconds(0.5f);

        Assert.AreEqual(new Vector3(0, 0, 0), window.WindowAnimation.WindowRectTransform.position);

        Assert.IsTrue(window.BuyButton.gameObject.activeSelf);
        Assert.IsFalse(window.CantBuyButton.gameObject.activeSelf);
        Assert.AreEqual("Купить за 200", window.BuyButtonText.text);
    }

    [UnityTest]
    public IEnumerator ShowBuyWindow_PlayerCannotAfford_ShowsCantBuyButton()
    {
        yield return null;
        var color = new PlayerColor(1, 0, 0);

        var testPlayer1 = new PlayerData("TestPlayer", 100, 0, color, null);


        window.ShowBuyWindow(testPlayer1, 2, 3000);
        yield return new WaitForSeconds(0.5f);

        Assert.AreEqual(new Vector3(0, 0, 0), window.WindowAnimation.WindowRectTransform.position);

        Assert.IsFalse(window.BuyButton.gameObject.activeSelf);
        Assert.IsTrue(window.CantBuyButton.gameObject.activeSelf);
        Assert.AreEqual("Купить за 3,000", window.CantBuyButtonText.text);
    }


}
