using Moq;
using NUnit.Framework;
using System.Collections;
using System.Reflection;
using UnityEngine;
using UnityEngine.TestTools;

public class CompanyUIManagerPlayModeTests
{
    private GameObject canvasGO;
    private CompanyUIManager manager;
    private Mock<IEventBus> mockEventBus;
    private RectTransform cell;

    private Mock<ICompanyStatsUI<string>> mockStatsPanel;

    [UnitySetUp]
    public IEnumerator Setup()
    {
        canvasGO = new GameObject("Canvas", typeof(Canvas));
        var canvas = canvasGO.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        var managerGO = new GameObject("CompanyUIManager");
        managerGO.transform.SetParent(canvasGO.transform);
        manager = managerGO.AddComponent<CompanyUIManager>();

        // создаём окна
        var companyWindow = new GameObject("CompanyWindow", typeof(RectTransform));
        companyWindow.transform.SetParent(canvasGO.transform);
        var fieldWindow = new GameObject("FieldWindow", typeof(RectTransform));
        fieldWindow.transform.SetParent(canvasGO.transform);
        var diceWindow = new GameObject("DiceWindow", typeof(RectTransform));
        diceWindow.transform.SetParent(canvasGO.transform);

        // создаём панели
        var companyPanelGO = new GameObject("CompanyPanel", typeof(TestStatsPanel));
        var fieldPanelGO = new GameObject("FieldPanel", typeof(TestStatsPanel));
        var dicePanelGO = new GameObject("DicePanel", typeof(TestStatsPanel));

        // задаём приватные поля через Reflection
        typeof(CompanyUIManager).GetField("companyInfoWindow", BindingFlags.NonPublic | BindingFlags.Instance)
            .SetValue(manager, companyWindow.GetComponent<RectTransform>());
        typeof(CompanyUIManager).GetField("fieldCompanyInfoWindow", BindingFlags.NonPublic | BindingFlags.Instance)
            .SetValue(manager, fieldWindow.GetComponent<RectTransform>());
        typeof(CompanyUIManager).GetField("diceCompanyInfoWindow", BindingFlags.NonPublic | BindingFlags.Instance)
            .SetValue(manager, diceWindow.GetComponent<RectTransform>());

        typeof(CompanyUIManager).GetField("statsCompanyPanel", BindingFlags.NonPublic | BindingFlags.Instance)
            .SetValue(manager, companyPanelGO.GetComponent<TestStatsPanel>());
        typeof(CompanyUIManager).GetField("statsFieldCompanyPanel", BindingFlags.NonPublic | BindingFlags.Instance)
            .SetValue(manager, fieldPanelGO.GetComponent<TestStatsPanel>());
        typeof(CompanyUIManager).GetField("statsDiceCompanyPanel", BindingFlags.NonPublic | BindingFlags.Instance)
            .SetValue(manager, dicePanelGO.GetComponent<TestStatsPanel>());

        // Мокаем EventBus
        mockEventBus = new Mock<IEventBus>();
        manager.Construct(mockEventBus.Object);

        var cellGO = new GameObject("Cell", typeof(RectTransform));
        cell = cellGO.GetComponent<RectTransform>();
        cell.anchoredPosition = Vector2.zero;

        yield return null;
    }

    [UnityTearDown]
    public IEnumerator TearDown()
    {
        Object.DestroyImmediate(canvasGO);
        yield return null;
    }

    [UnityTest]
    public IEnumerator ShowCompanyWindow_ActivatesWindowAndSetsData()
    {
        var showEvent = new ShowCompanyWindowEvent(cell, StatsWindowPosition.Up, new CompanyData());
       

        // Вызываем приватный метод через Reflection
        var method = typeof(CompanyUIManager).GetMethod("OnShowCompanyWindow", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        method.Invoke(manager, new object[] { showEvent });

        yield return null;

        var window = (RectTransform)typeof(CompanyUIManager)
            .GetField("companyInfoWindow", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .GetValue(manager);

        Assert.IsTrue(window.gameObject.activeSelf, "Company window should be active");
        mockStatsPanel.Verify(p => p.SetData("TestData"), Times.Once);
    }

    [UnityTest]
    public IEnumerator HideAllWindows_DeactivatesAllWindows()
    {
        var hideMethod = typeof(CompanyUIManager).GetMethod("HideAllWindows", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
        hideMethod.Invoke(manager, null);

        yield return null;

        var companyWindow = (RectTransform)typeof(CompanyUIManager)
            .GetField("companyInfoWindow", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .GetValue(manager);
        var fieldWindow = (RectTransform)typeof(CompanyUIManager)
            .GetField("fieldCompanyInfoWindow", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .GetValue(manager);
        var diceWindow = (RectTransform)typeof(CompanyUIManager)
            .GetField("diceCompanyInfoWindow", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .GetValue(manager);

        Assert.IsFalse(companyWindow.gameObject.activeSelf);
        Assert.IsFalse(fieldWindow.gameObject.activeSelf);
        Assert.IsFalse(diceWindow.gameObject.activeSelf);
    }
}
public class TestStatsPanel : MonoBehaviour, ICompanyStatsUI<string>
{
    public string receivedData;
    public void SetData(string data)
    {
        receivedData = data;
    }
}