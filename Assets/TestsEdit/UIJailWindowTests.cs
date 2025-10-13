using NUnit.Framework;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

public class UIJailWindowTests
{
    private TestUIJailWindow _window;

    [SetUp]
    public void SetUp()
    {
        var go = new GameObject("JailWindow");
        _window = go.AddComponent<TestUIJailWindow>();

        _window.RansomButton = CreateButton("RansomButton");
        _window.CantRansomButton = CreateButton("CantRansomButton");
        _window.ThrowDiceButton = CreateButton("ThrowDiceButton");

        _window.RansomText = CreateTMP("RansomText");
        _window.CantRansomText = CreateTMP("CantRansomText");

    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(_window.gameObject);
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

    [Test]
    public void Show_SetsButtonsAndTexts_WhenCanAfford()
    {
        _window.Show(1, 500, true);

        Assert.IsTrue(_window.RansomButton.gameObject.activeSelf);
        Assert.IsFalse(_window.CantRansomButton.gameObject.activeSelf);
        Assert.AreEqual("Заплатите 500", _window.RansomText.text);
        Assert.AreEqual("Заплатите 500", _window.CantRansomText.text);
        Assert.IsTrue(_window.showWindowCalled);
    }

    [Test]
    public void Show_SetsButtonsAndTexts_WhenCantAfford()
    {
        _window.Show(2, 1000, false);

        Assert.IsFalse(_window.RansomButton.gameObject.activeSelf);
        Assert.IsTrue(_window.CantRansomButton.gameObject.activeSelf);
        Assert.AreEqual("Заплатите 1,000", _window.RansomText.text);
        Assert.AreEqual("Заплатите 1,000", _window.CantRansomText.text);
        Assert.IsTrue(_window.showWindowCalled);
    }

    [Test]
    public void Hide_CallsHideWindow()
    {
        _window.Hide();
        Assert.IsTrue(_window.hideWindowCalled);
    }

    [Test]
    public void HardHide_CallsHardHideWindow()
    {
        _window.HardHide();
        Assert.IsTrue(_window.hardHideWindowCalled);
    }

    [Test]
    public void SetThrowDiceAction_InvokesDelegate()
    {
        int receivedId = -1;
        _window.SetThrowDiceAction(id => receivedId = id);

        _window.Show(3, 0, true);
        _window.ThrowDiceButton.onClick.Invoke();

        Assert.AreEqual(3, receivedId);
    }

    [Test]
    public void SetRansomAction_InvokesDelegate()
    {
        int receivedId = -1;
        _window.SetRansomAction(id => receivedId = id);

        _window.Show(4, 200, true);
        _window.RansomButton.onClick.Invoke();

        Assert.AreEqual(4, receivedId);
    }
}
