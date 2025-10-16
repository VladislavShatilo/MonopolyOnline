using NUnit.Framework;
using System;
using System.Collections;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;
using Object = UnityEngine.Object;

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
    [Test]
    public void Start_Throws_WhenButtonsOrTextsAreNull()
    {
        var go = new GameObject("Window");
        var window = go.AddComponent<UIJailWindow>();

        var startMethod = typeof(UIJailWindow).GetMethod("Start",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        var ex = Assert.Throws<TargetInvocationException>(() =>
        {
            startMethod.Invoke(window, null); // вызываем private Start
        });

        // TargetInvocationException обернет реальное исключение
        Assert.IsInstanceOf<ArgumentNullException>(ex.InnerException);
        Assert.IsNotNull(((ArgumentNullException)ex.InnerException).ParamName);
    }

    // === Gраничные значения ransomMoney ===
    [TestCase(0)]
    [TestCase(-100)]
    [TestCase(int.MaxValue)]
    public void Show_SetsTexts_WithEdgeRansomMoney(int ransom)
    {
        _window.Show(1, ransom, true);
        string expected = $"Заплатите {ransom.ToString("N0", System.Globalization.CultureInfo.InvariantCulture)}";
        Assert.AreEqual(expected, _window.RansomText.text);
        Assert.AreEqual(expected, _window.CantRansomText.text);
    }

    // === Проверка повторного присвоения делегата ThrowDice ===
    [Test]
    public void SetThrowDiceAction_RemovesPreviousListeners()
    {
        int firstCall = -1;
        int secondCall = -1;

        _window.SetThrowDiceAction(id => firstCall = id);
        _window.SetThrowDiceAction(id => secondCall = id); // предыдущий должен быть удалён

        _window.Show(5, 0, true);
        _window.ThrowDiceButton.onClick.Invoke();

        Assert.AreEqual(-1, firstCall); // старый listener не должен сработать
        Assert.AreEqual(5, secondCall); // новый listener сработал
    }

    // === Проверка повторного присвоения делегата Ransom ===
    [Test]
    public void SetRansomAction_RemovesPreviousListeners()
    {
        int firstCall = -1;
        int secondCall = -1;

        _window.SetRansomAction(id => firstCall = id);
        _window.SetRansomAction(id => secondCall = id);

        _window.Show(7, 100, true);
        _window.RansomButton.onClick.Invoke();

        Assert.AreEqual(-1, firstCall);
        Assert.AreEqual(7, secondCall);
    }

    // === Делегаты null ===
    [Test]
    public void SetThrowDiceAction_WithNull_DoesNotThrow()
    {
        Assert.DoesNotThrow(() => _window.SetThrowDiceAction(null));
    }

    [Test]
    public void SetRansomAction_WithNull_DoesNotThrow()
    {
        Assert.DoesNotThrow(() => _window.SetRansomAction(null));
    }
}
