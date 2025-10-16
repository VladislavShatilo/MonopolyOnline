using NUnit.Framework;
using System;
using TMPro;
using UnityEditor.PackageManager.UI;
using UnityEngine;
using UnityEngine.UI;

public class UIAuctionWindowTests
{
    private TestUIAuctionWindow _window;

    [SetUp]
    public void SetUp()
    {
        var go = new GameObject("AuctionWindow");
        _window = go.AddComponent<TestUIAuctionWindow>();

        _window.PlayButton = CreateButton("PlayButton");
        _window.CantPlayButton = CreateButton("CantPlayButton");
        _window.CancelButton = CreateButton("CancelButton");
        _window.PlayPriceText = CreateTMP("PlayPriceText");
        _window.CantPriceText = CreateTMP("CantPriceText");
        _window.HeaderText = CreateTMP("HeaderText");


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
        UnityEngine.Object.DestroyImmediate(_window.gameObject);
    }

    [Test]
    public void Show_SetsCorrectTextAndButtons_WhenCanAfford()
    {
        _window.Show(1, "Apple Inc.", 500, 1000);

        Assert.IsTrue(_window.PlayButton.gameObject.activeSelf);
        Assert.IsFalse(_window.CantPlayButton.gameObject.activeSelf);
        Assert.AreEqual("На аукционе Apple Inc.", _window.HeaderText.text);
        Assert.AreEqual("Поднять до 500", _window.PlayPriceText.text);
        Assert.AreEqual("Поднять до 500", _window.CantPriceText.text);
        Assert.IsTrue(_window.showWindowCalled);
    }

    [Test]
    public void Show_SetsCorrectTextAndButtons_WhenCantAfford()
    {
        _window.Show(2, "Tesla", 1000, 500);

        Assert.IsFalse(_window.PlayButton.gameObject.activeSelf);
        Assert.IsTrue(_window.CantPlayButton.gameObject.activeSelf);
        Assert.AreEqual("На аукционе Tesla", _window.HeaderText.text);
        Assert.IsTrue(_window.showWindowCalled);
    }

    [Test]
    public void HandlePlayClicked_InvokesPlayAction()
    {
        int receivedId = -1;
        _window.SetPlayAction(id => receivedId = id);

        _window.Show(3, "Google", 300, 500);
        _window.InvokePlayClicked(); // вместо PlayButton.onClick.Invoke()

        Assert.AreEqual(3, receivedId);
        Assert.IsTrue(_window.hideWindowCalled);
    }

    [Test]
    public void HandlePassClicked_InvokesPassAction()
    {
        int receivedId = -1;
        _window.SetPassAction(id => receivedId = id);

        _window.Show(4, "Amazon", 300, 500);
        _window.InvokePassClicked(); // вместо CancelButton.onClick.Invoke()


        Assert.AreEqual(4, receivedId);
        Assert.IsTrue(_window.hideWindowCalled);
    }

    [Test]
    public void Hide_CallsHideWindow()
    {
        _window.Hide();
        Assert.IsTrue(_window.hideWindowCalled);
    }
    [Test]
    public void Start_ShouldThrowException_WhenPlayButtonNull()
    {
        _window.PlayButton = null;
        Assert.Throws<ArgumentNullException>(() => _window.ValidateUI());

    }

    [Test]
    public void Start_ShouldThrowException_WhenCantPlayButtonNull()
    {
        _window.CantPlayButton = null;

        Assert.Throws<ArgumentNullException>(() => _window.ValidateUI());
    }

    [Test]
    public void OnEnable_ShouldSubscribeToButtonClicks()
    {
        int playCalled = 0;
        int passCalled = 0;

        _window.SetPlayAction(id => playCalled++);
        _window.SetPassAction(id => passCalled++);

        _window.TestOnEnable();

        _window.PlayButton.onClick.Invoke();
        _window.CancelButton.onClick.Invoke();

        Assert.AreEqual(1, playCalled);
        Assert.AreEqual(1, passCalled);
    }

    [Test]
    public void OnDisable_ShouldUnsubscribeFromButtonClicks()
    {
        int playCalled = 0;
        int passCalled = 0;

        _window.SetPlayAction(id => playCalled++);
        _window.SetPassAction(id => passCalled++);

        _window.TestOnEnable();
        _window.TestOnDisable();

        _window.PlayButton.onClick.Invoke();
        _window.CancelButton.onClick.Invoke();

        Assert.AreEqual(0, playCalled);
        Assert.AreEqual(0, passCalled);
    }

    [Test]
    public void Show_ShouldFormatLargeNumbersCorrectly()
    {
        _window.Show(1, "MegaCorp", 1234567, 2000000);

        Assert.AreEqual("Поднять до 1,234,567", _window.PlayPriceText.text);
        Assert.AreEqual("Поднять до 1,234,567", _window.CantPriceText.text);
    }
}

