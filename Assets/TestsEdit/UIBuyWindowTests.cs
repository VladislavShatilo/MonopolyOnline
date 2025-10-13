using NUnit.Framework;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

public class UIBuyWindowTests
{
    private TestUIBuyWindow _window;

    [SetUp]
    public void SetUp()
    {
        var go = new GameObject("BuyWindow");
        _window = go.AddComponent<TestUIBuyWindow>();

        _window.BuyButton = CreateButton("BuyButton");
        _window.CantBuyButton = CreateButton("CantBuyButton");
        _window.AuctionButton = CreateButton("AuctionButton");
        _window.BuyButtonText = CreateTMP("BuyButtonText");
        _window.CantBuyButtonText = CreateTMP("CantBuyButtonText");
    }
   
    private Button CreateButton(string name)
    {
        var go = new GameObject(name);
        return go.AddComponent<Button>();
    }

    private TextMeshProUGUI CreateTMP(string name)
    {
        var go = new GameObject(name);
        return go.AddComponent<TextMeshProUGUI>();
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(_window.gameObject);
    }
    [Test]
    public void Show_SetsButtonsAndTexts_WhenCanAfford()
    {
        _window.Show(1, 5, 1000, true);



        Assert.IsTrue(_window.BuyButton.gameObject.activeSelf);
        Assert.IsFalse(_window.CantBuyButton.gameObject.activeSelf);
        Assert.AreEqual("Купить за 1,000", _window.BuyButtonText.text);
        Assert.AreEqual("Купить за 1,000", _window.CantBuyButtonText.text);
        Assert.IsTrue(_window.showWindowCalled);
    }

    [Test]
    public void Hide_CallsHideWindow()
    {
        _window.Hide();
        Assert.IsTrue(_window.hideWindowCalled);
    }
    [Test]
    public void SetBuyAction_InvokesDelegate()
    {
        int receivedCellIndex = -1;
        _window.SetBuyAction(idx => receivedCellIndex = idx);


        // Установим cellIndex через Show
        _window.Show(1, 42, 500, true);

        _window.BuyButton.onClick.Invoke();
        Assert.AreEqual(42, receivedCellIndex);
    }

    [Test]
    public void SetAuctionAction_InvokesDelegate()
    {
        int receivedCellIndex = -1;
        _window.SetAuctionAction(idx => receivedCellIndex = idx);

        _window.Show(1, 99, 500, true);

        _window.AuctionButton.onClick.Invoke();
        Assert.AreEqual(99, receivedCellIndex);
    }
}
