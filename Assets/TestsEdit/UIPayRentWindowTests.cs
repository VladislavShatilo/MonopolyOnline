using NUnit.Framework;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;
using Object = UnityEngine.Object;

public class UIPayRentWindowTests
{
    private TestUIPayRentWindow _window;

    [SetUp]
    public void SetUp()
    {
        var go = new GameObject("PayRentWindow");
        _window = go.AddComponent<TestUIPayRentWindow>();

        // Инициализация кнопок и текстов
        _window.PayRentButton = CreateButton("PayRentButton");
        _window.CantPayRentButton = CreateButton("CantPayRentButton");
        _window.PayButtonText = CreateTMP("PayButtonText");
        _window.CantPayRentText = CreateTMP("CantPayRentText");
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
    public void Show_SetsTextsAndButtonStates_WhenCanPay()
    {
        _window.Show(1, 2, 500, true);

        Assert.IsTrue(_window.PayRentButton.gameObject.activeSelf);
        Assert.IsFalse(_window.CantPayRentButton.gameObject.activeSelf);
        Assert.AreEqual("Заплатите 500", _window.PayButtonText.text);
        Assert.AreEqual("Заплатите 500", _window.CantPayRentText.text);
        Assert.IsTrue(_window.showCalled);
    }

    [Test]
    public void Show_SetsTextsAndButtonStates_WhenCantPay()
    {
        _window.Show(1, 2, 1000, false);

        Assert.IsFalse(_window.PayRentButton.gameObject.activeSelf);
        Assert.IsTrue(_window.CantPayRentButton.gameObject.activeSelf);
        Assert.AreEqual("Заплатите 1,000", _window.PayButtonText.text);
        Assert.AreEqual("Заплатите 1,000", _window.CantPayRentText.text);
        Assert.IsTrue(_window.showCalled);
    }

    [Test]
    public void SetPayAction_InvokesDelegate()
    {
        bool payCalled = false;
        _window.SetPayAction(() => payCalled = true);

        _window.PayRentButton.onClick.Invoke();
        Assert.IsTrue(payCalled);
    }

    [Test]
    public void Hide_CallsHideWindow()
    {
        _window.Hide();
        Assert.IsTrue(_window.hideCalled);
    }

    [Test]
    public void HardHide_CallsHardHideWindow()
    {
        _window.HardHide();
        Assert.IsTrue(_window.hardHideCalled);
    }
  

    // 2. Проверка, что повторное присвоение SetPayAction удаляет предыдущие слушатели
    [Test]
    public void SetPayAction_ReplacesPreviousListeners()
    {
        int callCount1 = 0;
        int callCount2 = 0;

        _window.SetPayAction(() => callCount1++);
        _window.SetPayAction(() => callCount2++);

        _window.PayRentButton.onClick.Invoke();

        Assert.AreEqual(0, callCount1, "Старый слушатель не должен вызываться");
        Assert.AreEqual(1, callCount2, "Новый слушатель должен вызываться один раз");
    }

    // 3. Проверка публичных свойств get/set
    [Test]
    public void Properties_GetSet_WorkCorrectly()
    {
        var button = new GameObject().AddComponent<Button>();
        var text = new GameObject().AddComponent<TextMeshProUGUI>();

        _window.PayRentButton = button;
        _window.CantPayRentButton = button;
        _window.PayButtonText = text;
        _window.CantPayRentText = text;

        Assert.AreEqual(button, _window.PayRentButton);
        Assert.AreEqual(button, _window.CantPayRentButton);
        Assert.AreEqual(text, _window.PayButtonText);
        Assert.AreEqual(text, _window.CantPayRentText);

        GameObject.DestroyImmediate(text.gameObject);
    }

    // 4. Проверка правильного форматирования больших чисел
    [Test]
    public void Show_FormatsLargeNumbersCorrectly()
    {
        float rent = 1234567;
        _window.Show(1, 0, rent, true);

        string expectedText = $"Заплатите {rent.ToString("N0", System.Globalization.CultureInfo.InvariantCulture)}";
        Assert.AreEqual(expectedText, _window.PayButtonText.text);
        Assert.AreEqual(expectedText, _window.CantPayRentText.text);
    }
}