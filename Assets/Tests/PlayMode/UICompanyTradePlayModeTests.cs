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
        go.SetActive(false); // выключаем объект

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
        go.SetActive(true);

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

    [UnityEngine.TestTools.UnityTest]
    public System.Collections.IEnumerator RemoveButtonClicked_ShouldCallTradeServiceAndDestroyGameObject_WhenTradeActive()
    {
        uiTrade.SetCompanyTradeUI(testCompany, 42);
        uiTrade.gameObject.SetActive(true); // OnEnable вызовется

        removeButton.onClick.Invoke();

        yield return null; // ждем кадр, чтобы Destroy отработал

        tradeServiceMock.Verify(ts => ts.RemoveCompanyFromOffer(42, testCompany), Times.Once);
        Assert.IsTrue(uiTrade == null || uiTrade.Equals(null));
    }

    [UnityEngine.TestTools.UnityTest]
    public System.Collections.IEnumerator RemoveButtonClicked_ShouldDestroyGameObjectWithoutCallingService_WhenTradeInactive()
    {
        tradeServiceMock.Setup(ts => ts.IsTradeActive).Returns(false);
        uiTrade.SetCompanyTradeUI(testCompany, 42);
        uiTrade.gameObject.SetActive(true);

        removeButton.onClick.Invoke();

        yield return null;

        tradeServiceMock.Verify(ts => ts.RemoveCompanyFromOffer(It.IsAny<int>(), It.IsAny<Company>()), Times.Never);
        Assert.IsTrue(uiTrade == null || uiTrade.Equals(null));
    }

    [UnityTest]
    public System.Collections.IEnumerator RemoveButtonClicked_ShouldCallServiceAndDestroy()
    {
        uiTrade.SetCompanyTradeUI(testCompany, 42);
        uiTrade.gameObject.SetActive(true); // OnEnable будет вызван

        removeButton.onClick.Invoke();

        yield return null; // Ждём один кадр, чтобы Destroy отработал

        tradeServiceMock.Verify(ts => ts.RemoveCompanyFromOffer(42, testCompany), Times.Once);
        Assert.IsTrue(uiTrade == null || uiTrade.Equals(null));
    }
}
