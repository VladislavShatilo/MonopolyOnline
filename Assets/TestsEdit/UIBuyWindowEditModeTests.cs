using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class UIBuyWindowEditModeTests
{
    private UIBuyWindow window;
    private PlayerData testPlayer;
    private GameObject windowGO;
    bool buyEventReceived = false;
    bool auctionEventReceived = false;


    [SetUp]
    public void Setup()
    {
        windowGO = new GameObject("UIBuyWindow");
        window = windowGO.AddComponent<UIBuyWindow>();
        //  нопки и тексты через свойства
       // window.BuyButton = new GameObject("BuyBtn").AddComponent<Button>();
       // window.CantBuyButton = new GameObject("CantBuyBtn").AddComponent<Button>();
       // window.AuctionButton = new GameObject("AuctionBtn").AddComponent<Button>();
       // window.BuyButtonText = new GameObject("BuyText").AddComponent<TextMeshProUGUI>();
       // window.CantBuyButtonText = new GameObject("CantBuyText").AddComponent<TextMeshProUGUI>();
        //window.WindowAnimation = new GameObject("WindowAnimation").AddComponent<WindowAnimation>();

        var color = new PlayerColor(1, 0, 0);

        testPlayer = new PlayerData("TestPlayer", 0, 0, color, null);
       // EventBus.Subscribe<TryBuyCompanyEvent>(OnTryBuyCompany);
       // EventBus.Subscribe<StartAuctionEvent>(OnAuctionStart);
        windowGO.SetActive(false);
        windowGO.SetActive(true);
    }
    [TearDown]
    public void Teardown()
    {
        //EventBus.Unsubscribe<TryBuyCompanyEvent>(OnTryBuyCompany);
        //EventBus.Unsubscribe<StartAuctionEvent>(OnAuctionStart);
    }
    private void OnTryBuyCompany(TryBuyCompanyEvent e)
    {
        buyEventReceived = true;

    }
    private void OnAuctionStart(StartAuctionEvent e)
    {
        auctionEventReceived = true;
    }

}
