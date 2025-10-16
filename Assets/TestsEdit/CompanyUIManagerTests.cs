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

    private MethodInfo onShowCompanyWindowMethod;
    private MethodInfo onShowFieldCompanyWindowMethod;
    private MethodInfo onShowDiceCompanyWindowMethod;
    private MethodInfo configureWindowPositionMethod;
    private MethodInfo handleClickMethod;

    [SetUp]
    public void SetUp()
    {
        gameObject = new GameObject();
        manager = gameObject.AddComponent<CompanyUIManager>();

        companyWindow = new GameObject("CompanyWindow").AddComponent<RectTransform>();
        fieldWindow = new GameObject("FieldWindow").AddComponent<RectTransform>();
        diceWindow = new GameObject("DiceWindow").AddComponent<RectTransform>();

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

        onShowCompanyWindowMethod = typeof(CompanyUIManager).GetMethod("OnShowCompanyWindow", BindingFlags.NonPublic | BindingFlags.Instance);
        onShowFieldCompanyWindowMethod = typeof(CompanyUIManager).GetMethod("OnShowFieldCompanyWindow", BindingFlags.NonPublic | BindingFlags.Instance);
        onShowDiceCompanyWindowMethod = typeof(CompanyUIManager).GetMethod("OnShowDiceCompanyWindow", BindingFlags.NonPublic | BindingFlags.Instance);
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
        typeof(CompanyUIManager).GetMethod("OnEnable", BindingFlags.NonPublic | BindingFlags.Instance)
            ?.Invoke(manager, null);

        mockEventBus.Verify(e => e.Subscribe<ShowCompanyWindowEvent>(It.IsAny<Action<ShowCompanyWindowEvent>>()), Times.Once);
        mockEventBus.Verify(e => e.Subscribe<ShowFieldCompanyWindowEvent>(It.IsAny<Action<ShowFieldCompanyWindowEvent>>()), Times.Once);
        mockEventBus.Verify(e => e.Subscribe<ShowDiceCompanyWindowEvent>(It.IsAny<Action<ShowDiceCompanyWindowEvent>>()), Times.Once);
    }

    [Test]
    public void OnDisable_UnsubscribesFromEvents()
    {
        typeof(CompanyUIManager).GetMethod("OnDisable", BindingFlags.NonPublic | BindingFlags.Instance)
            ?.Invoke(manager, null);

        mockEventBus.Verify(e => e.Unsubscribe<ShowCompanyWindowEvent>(It.IsAny<Action<ShowCompanyWindowEvent>>()), Times.Once);
        mockEventBus.Verify(e => e.Unsubscribe<ShowFieldCompanyWindowEvent>(It.IsAny<Action<ShowFieldCompanyWindowEvent>>()), Times.Once);
        mockEventBus.Verify(e => e.Unsubscribe<ShowDiceCompanyWindowEvent>(It.IsAny<Action<ShowDiceCompanyWindowEvent>>()), Times.Once);
    }

    [TestCase(StatsWindowPosition.Up)]
    [TestCase(StatsWindowPosition.Down)]
    [TestCase(StatsWindowPosition.LeftUp)]
    [TestCase(StatsWindowPosition.LeftDown)]
    [TestCase(StatsWindowPosition.RightUp)]
    [TestCase(StatsWindowPosition.RightDown)]
    public void ConfigureWindowPosition_AllPositions_UpdatesWindow(StatsWindowPosition position)
    {
        var cell = new GameObject().AddComponent<RectTransform>();
        cell.anchoredPosition = new Vector2(50, 50);

        configureWindowPositionMethod.Invoke(manager, new object[] { companyWindow, cell, position });

        Assert.AreNotEqual(Vector2.zero, companyWindow.anchoredPosition);
        Assert.IsTrue(companyWindow.gameObject.activeSelf);
    }

    [Test]
    public void HandleClick_DoesNotHide_WhenClickInsideWindow()
    {
        companyWindow.gameObject.SetActive(true);
        // Симулируем клик по центру окна
        var pos = companyWindow.position;
        Vector2 pos1=  new Vector2((int)pos.x, (int)pos.y);
        handleClickMethod.Invoke(manager, new object[] { pos1 });

        Assert.IsTrue(companyWindow.gameObject.activeSelf);
    }
}
