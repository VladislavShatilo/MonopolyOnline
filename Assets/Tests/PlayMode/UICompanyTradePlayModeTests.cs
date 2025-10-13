using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Moq;
using UnityEngine.TestTools;

public class UICompanyTradePlayModeTests
{
    private GameObject go;
    private UICompanyTrade uiTrade;
    private TextMeshProUGUI companyNameText;
    private TextMeshProUGUI companyPriceText;
    private Button removeButton;

    private Mock<ITradeService> tradeServiceMock;
    private Company testCompany;

    [UnitySetUp]
    public IEnumerator SetUp()
    {
        // Создаем объект и компонент
        go = new GameObject("UICompanyTrade");
        uiTrade = go.AddComponent<UICompanyTrade>();

        // Создаём UI элементы заранее
        companyNameText = new GameObject("CompanyName").AddComponent<TextMeshProUGUI>();
        companyPriceText = new GameObject("CompanyPrice").AddComponent<TextMeshProUGUI>();
        removeButton = new GameObject("RemoveButton").AddComponent<Button>();

        // Присвоение полей через Reflection
        typeof(UICompanyTrade).GetField("companyName", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .SetValue(uiTrade, companyNameText);
        typeof(UICompanyTrade).GetField("companyPrice", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .SetValue(uiTrade, companyPriceText);
        typeof(UICompanyTrade).GetField("removeCompanyButton", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .SetValue(uiTrade, removeButton);

        // Mock ITradeService
        tradeServiceMock = new Mock<ITradeService>();
        tradeServiceMock.Setup(ts => ts.IsTradeActive).Returns(true);
        uiTrade.Construct(tradeServiceMock.Object);

        // Тестовая компания
        testCompany = new Company(1, new CompanyData())
        {
            Name = "MyCompany",
            Price = 1500
        };

        // Явный вызов OnEnable после присвоения кнопки
        uiTrade.GetType().GetMethod("OnEnable", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .Invoke(uiTrade, null);

        yield return null;
    }

    [UnityTearDown]
    public IEnumerator TearDown()
    {
        Object.DestroyImmediate(go);
        Object.DestroyImmediate(companyNameText.gameObject);
        Object.DestroyImmediate(companyPriceText.gameObject);
        Object.DestroyImmediate(removeButton.gameObject);
        yield return null;
    }

    [UnityTest]
    public IEnumerator SetCompanyTradeUI_ShouldUpdateUIFields()
    {
        uiTrade.SetCompanyTradeUI(testCompany, 42);

        Assert.AreEqual("MyCompany", companyNameText.text);
        Assert.AreEqual(testCompany.Price.ToString("N0", System.Globalization.CultureInfo.InvariantCulture), companyPriceText.text);

        yield return null;
    }

    [UnityTest]
    public IEnumerator RemoveButtonClicked_ShouldCallTradeServiceAndDestroyGameObject()
    {
        uiTrade.SetCompanyTradeUI(testCompany, 42);

        // Эмулируем клик
        removeButton.onClick.Invoke();

        // Проверяем вызов метода RemoveCompanyFromOffer
        tradeServiceMock.Verify(ts => ts.RemoveCompanyFromOffer(42, testCompany), Times.Once);

        // Проверяем, что объект уничтожен (Destroy в PlayMode срабатывает через один кадр)
        yield return null;
        Assert.IsTrue(go == null || go.Equals(null));
    }

    [UnityTest]
    public IEnumerator RemoveButtonClicked_WhenTradeInactive_ShouldDestroyGameObjectWithoutCallingService()
    {
        tradeServiceMock.Setup(ts => ts.IsTradeActive).Returns(false);
        uiTrade.SetCompanyTradeUI(testCompany, 42);

        // Эмулируем клик
        removeButton.onClick.Invoke();

        // Метод не должен вызываться
        tradeServiceMock.Verify(ts => ts.RemoveCompanyFromOffer(It.IsAny<int>(), It.IsAny<Company>()), Times.Never);

        yield return null;
        Assert.IsTrue(go == null || go.Equals(null));
    }
}
