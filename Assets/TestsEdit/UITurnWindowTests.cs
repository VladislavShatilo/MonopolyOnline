using NUnit.Framework;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UITurnWindowTests
{
    private TestUITurnWindow _window;

    [SetUp]
    public void SetUp()
    {
        var go = new GameObject("TurnWindow");
        _window = go.AddComponent<TestUITurnWindow>();

        // Создаём кнопки и поля ввода
        _window.ThrowDiceButtonPublic = CreateButton("ThrowDiceButton");
        _window.InputField1Public = CreateInputField("InputField1");
        _window.InputField2Public = CreateInputField("InputField2");
    }

    private Button CreateButton(string name)
    {
        var go = new GameObject(name);
        return go.AddComponent<Button>();
    }

    private TMP_InputField CreateInputField(string name)
    {
        var go = new GameObject(name);
        var input = go.AddComponent<TMP_InputField>();
        var textGO = new GameObject("Text");
        textGO.transform.SetParent(go.transform);
        input.textComponent = textGO.AddComponent<TextMeshProUGUI>();
        return input;
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(_window.gameObject);
    }
    [Test]
    public void Show_CallsShowWindow()
    {
        _window.Show();
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
        bool called = false;
        _window.SetThrowDiceAction(() => called = true);

        _window.ThrowDiceButtonPublic.onClick.Invoke();
        Assert.IsTrue(called);
    }
    [Test]
    public void GetSteps1_ReturnsParsedValue()
    {
        _window.InputField1Public.text = "5";
        Assert.AreEqual(5, _window.GetSteps1());
    }

    [Test]
    public void GetSteps1_ReturnsZero_WhenEmpty()
    {
        _window.InputField1Public.text = "";
        Assert.AreEqual(0, _window.GetSteps1());
    }

    [Test]
    public void GetSteps2_ReturnsParsedValue()
    {
        _window.InputField2Public.text = "10";
        Assert.AreEqual(10, _window.GetSteps2());
    }

    [Test]
    public void GetSteps2_ReturnsZero_WhenEmpty()
    {
        _window.InputField2Public.text = "";
        Assert.AreEqual(0, _window.GetSteps2());
    }
}
