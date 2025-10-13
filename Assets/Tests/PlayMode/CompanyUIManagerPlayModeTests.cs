using Moq;
using NUnit.Framework;
using System;
using System.Collections;
using System.Reflection;
using UnityEngine;
using UnityEngine.TestTools;

public class CompanyUIManagerPlayModeTests
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

 
    [UnityTest]
    public IEnumerator HandleClick_HidesWindows_WhenClickOutside()
    {
        companyWindow.gameObject.SetActive(true);
        fieldWindow.gameObject.SetActive(true);
        diceWindow.gameObject.SetActive(true);

        handleClickMethod.Invoke(manager, new object[] { new Vector2(999, 999) });
        yield return new WaitForEndOfFrame();

        Assert.IsFalse(companyWindow.gameObject.activeSelf);
        Assert.IsFalse(fieldWindow.gameObject.activeSelf);
        Assert.IsFalse(diceWindow.gameObject.activeSelf);
    }

}
