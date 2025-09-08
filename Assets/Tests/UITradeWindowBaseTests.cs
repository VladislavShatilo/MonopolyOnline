using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.TestTools;

public class UITradeWindowBaseTests : MonoBehaviour
{
    private GameObject windowGO;
    private UITradeWindowBase window;
    private GameObject leftPanelGO;
    private GameObject rightPanelGO;
    private GameObject companyPrefab;

    private PlayerData leftPlayer;
    private PlayerData rightPlayer;

    [SetUp]
    public void Setup()
    {
        // Создаем GameObject с компонентом окна
        windowGO = new GameObject("UITradeWindowBase");
        window = windowGO.AddComponent<UITradeWindowBase>();

        // Панели
        leftPanelGO = new GameObject("LeftPanel");
        rightPanelGO = new GameObject("RightPanel");
        window.GetType().GetField("leftPanel", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
              .SetValue(window, leftPanelGO.transform);
        window.GetType().GetField("rightPanel", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
              .SetValue(window, rightPanelGO.transform);

        // Префаб компании
        companyPrefab = new GameObject("CompanyPrefab");
        companyPrefab.AddComponent<UICompanyTrade>();
        window.GetType().GetField("companyCardPrefab", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
              .SetValue(window, companyPrefab);

        // RectTransform окна
        var rectGO = new GameObject("Rect");
        var rect = rectGO.AddComponent<RectTransform>();
        window.GetType().GetField("windowRectTransform", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
              .SetValue(window, rect);

        // Тексты
        window.GetType().GetField("leftTotalAmountText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
              .SetValue(window, CreateTMPText("LeftTotal"));
        window.GetType().GetField("rightTotalAmountText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
              .SetValue(window, CreateTMPText("RightTotal"));
        window.GetType().GetField("leftMoneyText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
              .SetValue(window, CreateTMPText("LeftMoney"));
        window.GetType().GetField("rightMoneyText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
              .SetValue(window, CreateTMPText("RightMoney"));
        window.GetType().GetField("leftPlayerNameText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
              .SetValue(window, CreateTMPText("LeftName"));
        window.GetType().GetField("rightPlayerNameText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
              .SetValue(window, CreateTMPText("RightName"));
        var color1 = new PlayerColor(1, 0, 0);
        var color2 = new PlayerColor(0, 1, 0);

        // Игроки (мок GameManager)
        leftPlayer = new PlayerData("Left", 1000, 0, color1, null);
        rightPlayer = new PlayerData("Right", 2000, 0, color2, null);
        //GameManager.Instance = new MockGameManager(leftPlayer, rightPlayer);
    }

    [TearDown]
    public void TearDown()
    {
        Object.Destroy(windowGO);
        Object.Destroy(leftPanelGO);
        Object.Destroy(rightPanelGO);
        Object.Destroy(companyPrefab);
        //GameManager.Instance = null;
    }

    private TextMeshProUGUI CreateTMPText(string name)
    {
        var go = new GameObject(name);
        return go.AddComponent<TextMeshProUGUI>();
    }

    [UnityTest]
    public IEnumerator ShowWindow_HidesAndMovesRect()
    {
        window.ShowWindow();
        yield return new WaitForSeconds(0.6f); // дождаться DOTween
        var rect = windowGO.GetComponent<UITradeWindowBase>().GetType()
            .GetField("windowRectTransform", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .GetValue(window) as RectTransform;

        Assert.AreEqual(Vector2.zero, rect.anchoredPosition);
    }

    [UnityTest]
    public IEnumerator HideWindow_MovesRectDown()
    {
        window.HideWindow();
        yield return new WaitForSeconds(0.6f);
        var rect = windowGO.GetComponent<UITradeWindowBase>().GetType()
            .GetField("windowRectTransform", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .GetValue(window) as RectTransform;

        Assert.AreEqual(new Vector2(0, 450), rect.anchoredPosition);
    }

    [UnityTest]
    public IEnumerator RefreshUI_UpdatesTextsAndCompanies()
    {
        //var offer = new TradeOffer
        //{
        //    FromPlayerId = leftPlayer.Id,
        //    ToPlayerId = rightPlayer.Id,
        //    FromMoney = 500,
        //    ToMoney = 300,
        //    FromCompanies = new List<Company> { new Company("C1", 100), new Company("C2", 200) },
        //    ToCompanies = new List<Company> { new Company("C3", 150) }
        //};
      //  window.GetType().GetField("currentOffer", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            //  .SetValue(window, offer);

        window.RefreshUI();
        yield return null;

        var leftTotal = (window.GetType().GetField("leftTotalAmountText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                        .GetValue(window) as TextMeshProUGUI).text;
        var rightTotal = (window.GetType().GetField("rightTotalAmountText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                         .GetValue(window) as TextMeshProUGUI).text;

        Assert.AreEqual("800", leftTotal.Replace(",", ""));
        Assert.AreEqual("450", rightTotal.Replace(",", ""));
        Assert.AreEqual(2, leftPanelGO.transform.childCount);
        Assert.AreEqual(1, rightPanelGO.transform.childCount);
    }

    [UnityTest]
    public IEnumerator ClearUI_ResetsTextsAndRemovesCompanies()
    {
        // Добавим детей в панели
        var child = new GameObject("Child");
        child.transform.SetParent(leftPanelGO.transform);
        child = new GameObject("Child");
        child.transform.SetParent(rightPanelGO.transform);

       // window.ClearUI();
        yield return null;

        var leftTotal = (window.GetType().GetField("leftTotalAmountText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                        .GetValue(window) as TextMeshProUGUI).text;
        var rightTotal = (window.GetType().GetField("rightTotalAmountText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                         .GetValue(window) as TextMeshProUGUI).text;

        Assert.AreEqual("0", leftTotal);
        Assert.AreEqual("0", rightTotal);
        Assert.AreEqual(0, leftPanelGO.transform.childCount);
        Assert.AreEqual(0, rightPanelGO.transform.childCount);
    }

    // Mock GameManager
    //private class MockGameManager : GameManager
    //{
    //    private PlayerData left;
    //    private PlayerData right;
    //    public MockGameManager(PlayerData left, PlayerData right)
    //    {
    //        this.left = left;
    //        this.right = right;
    //    }
       
    //}
}
