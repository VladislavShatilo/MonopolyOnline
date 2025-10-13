using NUnit.Framework;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

public class UILoanPayWindowTests
{

    private TestUILoanPayWindow _window;

    [SetUp]
    public void SetUp()
    {
        var go = new GameObject("LoanPayWindow");
        _window = go.AddComponent<TestUILoanPayWindow>();

        // Инициализация кнопок и текстов
        _window.PayLoanButton = CreateButton("PayLoanButton");
        _window.CantPayLoanButton = CreateButton("CantPayLoanButton");
        _window.PayLoanText = CreateTMP("PayLoanText");
        _window.CantPayLoanText = CreateTMP("CantPayLoanText");
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
        _window.Show(1, 500, true);

        Assert.IsTrue(_window.PayLoanButton.gameObject.activeSelf);
        Assert.IsFalse(_window.CantPayLoanButton.gameObject.activeSelf);
        Assert.AreEqual("Заплатите банку 500", _window.PayLoanText.text);
        Assert.AreEqual("Заплатите банку 500", _window.CantPayLoanText.text);
        Assert.IsTrue(_window.showWindowCalled);
    }

    [Test]
    public void Show_SetsButtonsAndTexts_WhenCantAfford()
    {
        _window.Show(2, 1000, false);

        Assert.IsFalse(_window.PayLoanButton.gameObject.activeSelf);
        Assert.IsTrue(_window.CantPayLoanButton.gameObject.activeSelf);
        Assert.AreEqual("Заплатите банку 1,000", _window.PayLoanText.text);
        Assert.AreEqual("Заплатите банку 1,000", _window.CantPayLoanText.text);
        Assert.IsTrue(_window.showWindowCalled);
    }

    [Test]
    public void Hide_CallsHideWindow()
    {
        _window.Hide();
        Assert.IsTrue(_window.hideWindowCalled);
    }

    [Test]
    public void SetPayLoanAction_InvokesDelegate()
    {
        int receivedId = -1;
        _window.SetPayLoanAction(id => receivedId = id);

        _window.Show(3, 2000, true);
        _window.PayLoanButton.onClick.Invoke();

        Assert.AreEqual(3, receivedId);
    }
}
