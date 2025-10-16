using NUnit.Framework;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;
using Object = UnityEngine.Object;

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
    [Test]
    public void Start_Throws_WhenButtonsOrTextsAreNull()
    {
        var go = new GameObject("Window");
        var window = go.AddComponent<UILoanPayWindow>();

        // Не присваиваем кнопки и тексты
        var startMethod = typeof(UILoanPayWindow)
            .GetMethod("Start", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        var ex = Assert.Throws<System.Reflection.TargetInvocationException>(() =>
        {
            startMethod.Invoke(window, null);
        });

        Assert.IsInstanceOf<ArgumentNullException>(ex.InnerException);
        Assert.IsNotNull(((ArgumentNullException)ex.InnerException).ParamName);
    }

    // -----------------------------
    // Проверка null в SetPayLoanAction
    // -----------------------------
    [Test]
    public void SetPayLoanAction_AllowsNullDelegate()
    {
        Assert.DoesNotThrow(() => _window.SetPayLoanAction(null));
    }

    // -----------------------------
    // Проверка повторного вызова SetPayLoanAction
    // -----------------------------
    [Test]
    public void SetPayLoanAction_RemovesPreviousListeners()
    {
        int callCount = 0;
        _window.SetPayLoanAction(id => callCount++);
        _window.SetPayLoanAction(id => callCount += 10);

        _window.Show(1, 100, true);
        _window.PayLoanButton.onClick.Invoke();

        // Старый listener должен быть удалён
        Assert.AreEqual(10, callCount);
    }

    // -----------------------------
    // Проверка публичных свойств
    // -----------------------------
    [Test]
    public void PublicProperties_GetSetWorkCorrectly()
    {
        var newButton = CreateButton("NewButton");
        var newText = CreateTMP("NewText");

        _window.PayLoanButton = newButton;
        _window.CantPayLoanButton = newButton;
        _window.PayLoanText = newText;
        _window.CantPayLoanText = newText;

        Assert.AreEqual(newButton, _window.PayLoanButton);
        Assert.AreEqual(newButton, _window.CantPayLoanButton);
        Assert.AreEqual(newText, _window.PayLoanText);
        Assert.AreEqual(newText, _window.CantPayLoanText);
    }

    // -----------------------------
    // Проверка текста при Show
    // -----------------------------
    [Test]
    public void Show_FormatsTextWithThousandsSeparator()
    {
        _window.Show(5, 1234567, true);

        Assert.AreEqual("Заплатите банку 1,234,567", _window.PayLoanText.text);
        Assert.AreEqual("Заплатите банку 1,234,567", _window.CantPayLoanText.text);
    }
}
