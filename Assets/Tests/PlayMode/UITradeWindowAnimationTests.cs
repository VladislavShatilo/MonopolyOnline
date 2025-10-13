using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.TestTools;

public class UITradeWindowAnimationTests
{

    private TestUITradeWindow window;

    [UnitySetUp]
    public IEnumerator Setup()
    {
        var go = new GameObject("Window");
        window = go.AddComponent<TestUITradeWindow>();
        window.windowRectTransform = go.AddComponent<RectTransform>();
        yield return null;
    }

    [UnityTest]
    public IEnumerator ShowWindow_MovesToZero()
    {
        window.windowRectTransform.anchoredPosition = new Vector2(0, 450);
        window.ShowWindow();

        yield return new WaitForSeconds(2f);

        Assert.AreEqual(Vector2.zero, window.windowRectTransform.anchoredPosition);
    }
}
public class TestUITradeWindow : UITradeWindowBase
{
    // Делаем protected поля доступными для тестов
    public new Transform leftPanel { get => base.leftPanel; set => base.leftPanel = value; }
    public new Transform rightPanel { get => base.rightPanel; set => base.rightPanel = value; }

    public new TextMeshProUGUI leftTotalAmountText { get => base.leftTotalAmountText; set => base.leftTotalAmountText = value; }
    public new TextMeshProUGUI rightTotalAmountText { get => base.rightTotalAmountText; set => base.rightTotalAmountText = value; }
    public new TextMeshProUGUI leftMoneyText { get => base.leftMoneyText; set => base.leftMoneyText = value; }
    public new TextMeshProUGUI rightMoneyText { get => base.rightMoneyText; set => base.rightMoneyText = value; }
    public new TextMeshProUGUI leftPlayerNameText { get => base.leftPlayerNameText; set => base.leftPlayerNameText = value; }
    public new TextMeshProUGUI rightPlayerNameText { get => base.rightPlayerNameText; set => base.rightPlayerNameText = value; }
    public new RectTransform windowRectTransform { get => base.windowRectTransform; set => base.windowRectTransform = value; }
    public new GameObject companyCardPrefab { get => base.companyCardPrefab; set => base.companyCardPrefab = value; }

    public new TradeOffer currentOffer { get => base.currentOffer; set => base.currentOffer = value; }


    // Открываем protected методы
    public new void ClearUI() => base.ClearUI();
    public new void ClearCompanies(Transform panel)
    {
        int childCount = panel.childCount;
        for (int i = childCount - 1; i >= 0; i--)
        {
            if (i > 0)
            {
                Object.DestroyImmediate(panel.GetChild(i).gameObject);
            }
        }
    }
    public new void PopulateCompanies(Transform panel, List<Company> companies, int playerId, ref int sum) =>
        base.PopulateCompanies(panel, companies, playerId, ref sum);

    // Удобный метод для тестов
    public void SetFakeOffer()
    {
        var fromPlayerData = new PlayerData("p1", 500, 1, new PlayerColor(255, 0, 0));
        var toPlayerData = new PlayerData("p2", 500, 2, new PlayerColor(0, 255, 0));

        currentOffer = new TradeOffer(fromPlayerData, toPlayerData)
        {
            FromMoney = 100,
            ToMoney = 200,

            FromCompanies = new List<Company>(),
            ToCompanies = new List<Company>()
        };

    }
}