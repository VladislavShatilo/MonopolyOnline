using NUnit.Framework;
using Moq;
using UnityEngine;
using System;
using System.Reflection;

[TestFixture]
public class CompanyUIManagerTests
{
    private GameObject gameObject;
    private CompanyUIManager manager;
    private Mock<IEventBus> mockEventBus;
    private RectTransform companyWindow;
    private RectTransform fieldWindow;
    private RectTransform diceWindow;
    private Mock<UICompanyStats> mockCompanyStats;
    private Mock<UIFieldCompanyStats> mockFieldStats;
    private Mock<UIDiceStats> mockDiceStats;

    private MethodInfo onEnableMethod;
    private MethodInfo onDisableMethod;
    private MethodInfo onShowCompanyWindowMethod;
    private MethodInfo configureWindowPositionMethod;
    private MethodInfo handleClickMethod;

    [SetUp]
    public void SetUp()
    {
        gameObject = new GameObject();
        manager = gameObject.AddComponent<CompanyUIManager>();

        // Создаём RectTransform для окон
        companyWindow = new GameObject("CompanyWindow").AddComponent<RectTransform>();
        fieldWindow = new GameObject("FieldWindow").AddComponent<RectTransform>();
        diceWindow = new GameObject("DiceWindow").AddComponent<RectTransform>();

        // Присваиваем через Reflection
        SetPrivateField("companyInfoWindow", companyWindow);
        SetPrivateField("fieldCompanyInfoWindow", fieldWindow);
        SetPrivateField("diceCompanyInfoWindow", diceWindow);

        mockCompanyStats = new Mock<UICompanyStats>();
        mockFieldStats = new Mock<UIFieldCompanyStats>();
        mockDiceStats = new Mock<UIDiceStats>();

        SetPrivateField("statsCompanyPanel", mockCompanyStats.Object);
        SetPrivateField("statsFieldCompanyPanel", mockFieldStats.Object);
        SetPrivateField("statsDiceCompanyPanel", mockDiceStats.Object);

        mockEventBus = new Mock<IEventBus>();
        manager.Construct(mockEventBus.Object);

        // Сохраняем ссылки на приватные методы
        onEnableMethod = typeof(CompanyUIManager).GetMethod("OnEnable", BindingFlags.NonPublic | BindingFlags.Instance);
        onDisableMethod = typeof(CompanyUIManager).GetMethod("OnDisable", BindingFlags.NonPublic | BindingFlags.Instance);
        onShowCompanyWindowMethod = typeof(CompanyUIManager).GetMethod("OnShowCompanyWindow", BindingFlags.NonPublic | BindingFlags.Instance);
        configureWindowPositionMethod = typeof(CompanyUIManager).GetMethod("ConfigureWindowPosition", BindingFlags.NonPublic | BindingFlags.Instance);
        handleClickMethod = typeof(CompanyUIManager).GetMethod("HandleClick", BindingFlags.NonPublic | BindingFlags.Instance);
    }

    private void SetPrivateField(string name, object value)
    {
        typeof(CompanyUIManager).GetField(name, BindingFlags.NonPublic | BindingFlags.Instance)
            ?.SetValue(manager, value);
    }

    [TearDown]
    public void TearDown()
    {
        UnityEngine.Object.DestroyImmediate(gameObject);
        UnityEngine.Object.DestroyImmediate(companyWindow.gameObject);
        UnityEngine.Object.DestroyImmediate(fieldWindow.gameObject);
        UnityEngine.Object.DestroyImmediate(diceWindow.gameObject);
    }

    [Test]
    public void HideAllWindows_DeactivatesAllWindows()
    {
        companyWindow.gameObject.SetActive(true);
        fieldWindow.gameObject.SetActive(true);
        diceWindow.gameObject.SetActive(true);

        manager.HideAllWindows();

        Assert.IsFalse(companyWindow.gameObject.activeSelf);
        Assert.IsFalse(fieldWindow.gameObject.activeSelf);
        Assert.IsFalse(diceWindow.gameObject.activeSelf);
    }

    [Test]
    public void OnEnable_SubscribesToEvents()
    {
        onEnableMethod.Invoke(manager, null);

        mockEventBus.Verify(e => e.Subscribe<ShowCompanyWindowEvent>(It.IsAny<Action<ShowCompanyWindowEvent>>()), Times.Once);
        mockEventBus.Verify(e => e.Subscribe<ShowFieldCompanyWindowEvent>(It.IsAny<Action<ShowFieldCompanyWindowEvent>>()), Times.Once);
        mockEventBus.Verify(e => e.Subscribe<ShowDiceCompanyWindowEvent>(It.IsAny<Action<ShowDiceCompanyWindowEvent>>()), Times.Once);
    }

    [Test]
    public void OnDisable_UnsubscribesFromEvents()
    {
        onDisableMethod.Invoke(manager, null);

        mockEventBus.Verify(e => e.Unsubscribe<ShowCompanyWindowEvent>(It.IsAny<Action<ShowCompanyWindowEvent>>()), Times.Once);
        mockEventBus.Verify(e => e.Unsubscribe<ShowFieldCompanyWindowEvent>(It.IsAny<Action<ShowFieldCompanyWindowEvent>>()), Times.Once);
        mockEventBus.Verify(e => e.Unsubscribe<ShowDiceCompanyWindowEvent>(It.IsAny<Action<ShowDiceCompanyWindowEvent>>()), Times.Once);
    }

    [Test]
    public void OnShowCompanyWindow_ShowsWindow_AndSetsData()
    {
        var cell = new GameObject().AddComponent<RectTransform>();
        var data = new CompanyData();
        var e = new ShowCompanyWindowEvent(cell, StatsWindowPosition.Up, data);

        onShowCompanyWindowMethod.Invoke(manager, new object[] { e });

        mockCompanyStats.Verify(s => s.SetData(data), Times.Once);
        Assert.IsTrue(companyWindow.gameObject.activeSelf);
    }

    [Test]
    public void HandleClick_HidesWindows_WhenClickOutside()
    {
        companyWindow.gameObject.SetActive(true);
        fieldWindow.gameObject.SetActive(true);
        diceWindow.gameObject.SetActive(true);

        handleClickMethod.Invoke(manager, new object[] { new Vector2(999, 999) });

        Assert.IsFalse(companyWindow.gameObject.activeSelf);
        Assert.IsFalse(fieldWindow.gameObject.activeSelf);
        Assert.IsFalse(diceWindow.gameObject.activeSelf);
    }

    [Test]
    public void ConfigureWindowPosition_UpdatesPivotAndPosition()
    {
        var cell = new GameObject().AddComponent<RectTransform>();
        cell.anchoredPosition = new Vector2(50, 50);

        configureWindowPositionMethod.Invoke(manager, new object[] { companyWindow, cell, StatsWindowPosition.LeftDown });

        Assert.AreNotEqual(Vector2.zero, companyWindow.anchoredPosition);
        Assert.IsTrue(companyWindow.gameObject.activeSelf);
    }
}
