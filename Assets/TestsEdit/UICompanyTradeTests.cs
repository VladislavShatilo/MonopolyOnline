using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Moq;
using System.Globalization;

[TestFixture]
public class UICompanyTradeTests
{
    private GameObject go;
    private UICompanyTrade uiTrade;
    private TextMeshProUGUI companyNameText;
    private TextMeshProUGUI companyPriceText;
    private Button removeButton;

    private Mock<ITradeService> tradeServiceMock;
    private Company testCompany;

    [SetUp]
    public void SetUp()
    {
        // Создаем объект и компонент
        go = new GameObject();
        uiTrade = go.AddComponent<UICompanyTrade>();

        // Создаем UI элементы
        companyNameText = new GameObject().AddComponent<TextMeshProUGUI>();
        companyPriceText = new GameObject().AddComponent<TextMeshProUGUI>();
        removeButton = new GameObject().AddComponent<Button>();

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

        // Конструктор Zenject
        uiTrade.Construct(tradeServiceMock.Object);

        // Тестовая компания
        testCompany = new Company(1,new CompanyData())
        { Name = "MyCompany", Price = 1500 };
    }

    [TearDown]
    public void TearDown()
    {
        GameObject.DestroyImmediate(go);
        GameObject.DestroyImmediate(companyNameText.gameObject);
        GameObject.DestroyImmediate(companyPriceText.gameObject);
        GameObject.DestroyImmediate(removeButton.gameObject);
    }

    [Test]
    public void SetCompanyTradeUI_ShouldUpdateUIFields()
    {
        uiTrade.SetCompanyTradeUI(testCompany, 42);

        Assert.AreEqual("MyCompany", companyNameText.text);
        Assert.AreEqual(testCompany.Price.ToString("N0", CultureInfo.InvariantCulture), companyPriceText.text);
    }

    [Test]
    public void RemoveButtonClicked_ShouldCallTradeServiceAndDestroyGameObject_EditModeSafe()
    {
        uiTrade.SetCompanyTradeUI(testCompany, 42);

        // Подписка на кнопку
        typeof(UICompanyTrade)
            .GetMethod("OnEnable", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .Invoke(uiTrade, null);

        // Подменяем Destroy на DestroyImmediate для теста
        typeof(UICompanyTrade)
            .GetMethod("OnRemoveClicked", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .Invoke(uiTrade, null);

        tradeServiceMock.Verify(ts => ts.RemoveCompanyFromOffer(42, testCompany), Times.Once);

        // Проверяем, что объект уничтожен
        Assert.IsTrue(uiTrade == null || uiTrade.Equals(null));
    }

    [Test]
    public void RemoveButtonClicked_WhenTradeInactive_ShouldDestroyGameObjectWithoutCallingService()
    {
        tradeServiceMock.Setup(ts => ts.IsTradeActive).Returns(false);
        uiTrade.SetCompanyTradeUI(testCompany, 42);

        // Имитируем OnEnable и клик
        removeButton.onClick.Invoke();

        // Метод не должен вызываться
        tradeServiceMock.Verify(ts => ts.RemoveCompanyFromOffer(It.IsAny<int>(), It.IsAny<Company>()), Times.Never);

        // Проверяем уничтожение объекта
        Assert.IsTrue(go == null || go.Equals(null));
    }
}
