using Moq;
using NUnit.Framework;
using System;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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
        go = new GameObject();
        uiTrade = go.AddComponent<UICompanyTrade>();

        companyNameText = new GameObject().AddComponent<TextMeshProUGUI>();
        companyPriceText = new GameObject().AddComponent<TextMeshProUGUI>();
        removeButton = new GameObject().AddComponent<Button>();

        // Присвоение полей через Reflection
        var flags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
        typeof(UICompanyTrade).GetField("companyName", flags).SetValue(uiTrade, companyNameText);
        typeof(UICompanyTrade).GetField("companyPrice", flags).SetValue(uiTrade, companyPriceText);
        typeof(UICompanyTrade).GetField("removeCompanyButton", flags).SetValue(uiTrade, removeButton);

        tradeServiceMock = new Mock<ITradeService>();
        tradeServiceMock.Setup(ts => ts.IsTradeActive).Returns(true);

        uiTrade.Construct(tradeServiceMock.Object);

        testCompany = new Company(1, new CompanyData())
        {
            Name = "MyCompany",
            Price = 1500
        };
    }

    [TearDown]
    public void TearDown()
    {
        GameObject.DestroyImmediate(go);
        GameObject.DestroyImmediate(companyNameText.gameObject);
        GameObject.DestroyImmediate(companyPriceText.gameObject);
        GameObject.DestroyImmediate(removeButton.gameObject);
    }

    // -----------------------------
    // Конструктор и исключения
    // -----------------------------
    [Test]
    public void Construct_ShouldThrow_WhenTradeServiceNull()
    {
        Assert.Throws<ArgumentNullException>(() => uiTrade.Construct(null));
    }

    [Test]
    public void Construct_ShouldThrow_WhenCompanyNameNull()
    {
        typeof(UICompanyTrade).GetField("companyName", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .SetValue(uiTrade, null);

        Assert.Throws<ArgumentNullException>(() => uiTrade.Construct(tradeServiceMock.Object));
    }

    [Test]
    public void Construct_ShouldThrow_WhenCompanyPriceNull()
    {
        typeof(UICompanyTrade).GetField("companyPrice", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .SetValue(uiTrade, null);

        Assert.Throws<ArgumentNullException>(() => uiTrade.Construct(tradeServiceMock.Object));
    }

    [Test]
    public void Construct_ShouldThrow_WhenRemoveButtonNull()
    {
        typeof(UICompanyTrade).GetField("removeCompanyButton", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .SetValue(uiTrade, null);

        Assert.Throws<ArgumentNullException>(() => uiTrade.Construct(tradeServiceMock.Object));
    }

    // -----------------------------
    // UI
    // -----------------------------
    [Test]
    public void SetCompanyTradeUI_ShouldUpdateUIFields()
    {
        uiTrade.SetCompanyTradeUI(testCompany, 42);

        Assert.AreEqual("MyCompany", companyNameText.text);
        Assert.AreEqual(testCompany.Price.ToString("N0", CultureInfo.InvariantCulture), companyPriceText.text);
    }


   

    [Test]
    public void OnDisable_ShouldRemoveListener()
    {
        // Сначала добавляем слушатель
        var onEnable = typeof(UICompanyTrade).GetMethod("OnEnable", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        onEnable.Invoke(uiTrade, null);

        // Потом вызываем OnDisable
        var onDisable = typeof(UICompanyTrade).GetMethod("OnDisable", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        onDisable.Invoke(uiTrade, null);

        Assert.AreEqual(0, removeButton.onClick.GetPersistentEventCount());
    }
}
