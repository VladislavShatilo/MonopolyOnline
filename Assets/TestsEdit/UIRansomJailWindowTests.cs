using NUnit.Framework;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UIRansomJailWindowTests
{
    private TestUIRansomJailWindow _window;

    [SetUp]
    public void SetUp()
    {
        var go = new GameObject("RansomJailWindow");
        _window = go.AddComponent<TestUIRansomJailWindow>();

        // Инициализация кнопок и текстов
        _window.RansomButtonPublic = CreateButton("RansomButton");
        _window.CantRansomButtonPublic = CreateButton("CantRansomButton");
        _window.RansomTextPublic = CreateTMP("RansomText");
        _window.CantRansomTextPublic = CreateTMP("CantRansomText");
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
    public void Show_SetsTextsAndButtonStates_WhenCanAfford()
    {
        _window.Show(1, 500, true);

        Assert.IsTrue(_window.RansomButtonPublic.gameObject.activeSelf);
        Assert.IsFalse(_window.CantRansomButtonPublic.gameObject.activeSelf);
        Assert.AreEqual("Заплатите 500", _window.RansomTextPublic.text);
        Assert.AreEqual("Заплатите 500", _window.CantRansomTextPublic.text);
        Assert.IsTrue(_window.showWindowCalled);
    }

    [Test]
    public void Show_SetsTextsAndButtonStates_WhenCantAfford()
    {
        _window.Show(1, 1000, false);

        Assert.IsFalse(_window.RansomButtonPublic.gameObject.activeSelf);
        Assert.IsTrue(_window.CantRansomButtonPublic.gameObject.activeSelf);
        Assert.AreEqual("Заплатите 1,000", _window.RansomTextPublic.text);
        Assert.AreEqual("Заплатите 1,000", _window.CantRansomTextPublic.text);
        Assert.IsTrue(_window.showWindowCalled);
    }
    [Test]
    public void SetRansomAction_InvokesDelegateWithPlayerId()
    {
        int calledPlayerId = -1;
        _window.Show(42, 100, true); // чтобы currentPlayerId установился
        _window.SetRansomAction(id => calledPlayerId = id);

        _window.RansomButtonPublic.onClick.Invoke();
        Assert.AreEqual(42, calledPlayerId);
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
}

