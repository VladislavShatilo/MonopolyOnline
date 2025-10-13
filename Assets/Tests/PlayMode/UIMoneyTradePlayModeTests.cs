using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.TestTools;
using UnityEngine.EventSystems;

public class UIMoneyTradePlayModeTests
{
    private GameObject go;
    private UIMoneyTrade moneyTrade;
    private Button openButton;
    private TMP_InputField inputField;
    private TextMeshProUGUI moneyValueText;
    private TextMeshProUGUI moneyText;
    private Image penImage;

    [UnitySetUp]
    public IEnumerator SetUp()
    {
        // Создаем объект и компонент
        go = new GameObject("UIMoneyTrade");
        go.AddComponent<RectTransform>();
        moneyTrade = go.AddComponent<UIMoneyTrade>();

        // EventSystem необходим для проверки IsPointerOverUI
        if (Object.FindObjectOfType<EventSystem>() == null)
        {
            var esGO = new GameObject("EventSystem");
            esGO.AddComponent<EventSystem>();
            esGO.AddComponent<StandaloneInputModule>();
        }

        // Создаем UI элементы
        openButton = new GameObject("OpenButton").AddComponent<Button>();
        inputField = new GameObject("InputField").AddComponent<TMP_InputField>();
        moneyValueText = new GameObject("MoneyValue").AddComponent<TextMeshProUGUI>();
        moneyText = new GameObject("MoneyText").AddComponent<TextMeshProUGUI>();
        penImage = new GameObject("PenImage").AddComponent<Image>();

        // Присвоение через Reflection
        typeof(UIMoneyTrade).GetField("openMoneyButton", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .SetValue(moneyTrade, openButton);
        typeof(UIMoneyTrade).GetField("moneyInputField", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .SetValue(moneyTrade, inputField);
        typeof(UIMoneyTrade).GetField("moneyTextValue", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .SetValue(moneyTrade, moneyValueText);
        typeof(UIMoneyTrade).GetField("moneyText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .SetValue(moneyTrade, moneyText);
        typeof(UIMoneyTrade).GetField("penImage", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .SetValue(moneyTrade, penImage);

        // Вызываем Start вручную, чтобы подписаться на кнопку и инициализировать UI
        typeof(UIMoneyTrade).GetMethod("Start", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .Invoke(moneyTrade, null);

        yield return null;
    }

    [UnityTearDown]
    public IEnumerator TearDown()
    {
        Object.DestroyImmediate(go);
        Object.DestroyImmediate(openButton.gameObject);
        Object.DestroyImmediate(inputField.gameObject);
        Object.DestroyImmediate(moneyValueText.gameObject);
        Object.DestroyImmediate(moneyText.gameObject);
        Object.DestroyImmediate(penImage.gameObject);
        yield return null;
    }

    [UnityTest]
    public IEnumerator RefreshUI_ShouldResetMoneyAndToggleFields()
    {
        moneyTrade.RefreshUI();

        Assert.AreEqual("0", moneyValueText.text);
        Assert.AreEqual("0", inputField.text);
        Assert.IsTrue(moneyValueText.gameObject.activeSelf);
        Assert.IsTrue(penImage.gameObject.activeSelf);
        Assert.IsFalse(inputField.gameObject.activeSelf);
        yield return null;
    }

    [UnityTest]
    public IEnumerator OpenMoneyButton_ShouldShowInputField()
    {
        // Эмулируем клик кнопки
        openButton.onClick.Invoke();
        yield return null;

        Assert.IsTrue(inputField.gameObject.activeSelf);
        Assert.IsFalse(moneyValueText.gameObject.activeSelf);
        Assert.IsFalse(penImage.gameObject.activeSelf);
        Assert.IsFalse(moneyText.gameObject.activeSelf);
        Assert.AreEqual("0", inputField.text);
    }

    [UnityTest]
    public IEnumerator OnMoneyChanged_ShouldUpdateValueAndCloseInput()
    {
        openButton.onClick.Invoke();
        yield return null;

        // Симуляция ввода нового значения
        typeof(UIMoneyTrade).GetMethod("OnMoneyChanged", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .Invoke(moneyTrade, new object[] { "12345" });
        yield return null;

        Assert.AreEqual("12,345", moneyValueText.text);
        Assert.IsTrue(moneyValueText.gameObject.activeSelf);
        Assert.IsTrue(penImage.gameObject.activeSelf);
        Assert.IsTrue(moneyText.gameObject.activeSelf);
        Assert.IsFalse(inputField.gameObject.activeSelf);
    }

    [UnityTest]
    public IEnumerator OnMoneyChanged_WhenInvalidInput_ShouldSetZero()
    {
        openButton.onClick.Invoke();
        yield return null;

        typeof(UIMoneyTrade).GetMethod("OnMoneyChanged", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .Invoke(moneyTrade, new object[] { "invalid" });
        yield return null;

        Assert.AreEqual("0", moneyValueText.text);
    }
}
