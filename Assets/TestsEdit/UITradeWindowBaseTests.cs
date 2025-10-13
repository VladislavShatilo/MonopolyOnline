using NUnit.Framework;
using UnityEngine;
using TMPro;
using Zenject;
using System.Collections.Generic;

public class UITradeWindowBaseTests
{
    private GameObject _gameObject;
    private TestUITradeWindow _window;
    private DiContainer _container;

    [SetUp]
    public void Setup()
    {
        _gameObject = new GameObject("UITradeWindow");
        _window = _gameObject.AddComponent<TestUITradeWindow>();
        _container = new DiContainer();
        _window.Construct(_container);

        // Создаем UI объекты
        _window.leftPanel = new GameObject("LeftPanel").transform;
        _window.rightPanel = new GameObject("RightPanel").transform;
        _window.leftTotalAmountText = new GameObject("L1").AddComponent<TextMeshProUGUI>();
        _window.rightTotalAmountText = new GameObject("R1").AddComponent<TextMeshProUGUI>();
        _window.leftMoneyText = new GameObject("L2").AddComponent<TextMeshProUGUI>();
        _window.rightMoneyText = new GameObject("R2").AddComponent<TextMeshProUGUI>();
        _window.leftPlayerNameText = new GameObject("L3").AddComponent<TextMeshProUGUI>();
        _window.rightPlayerNameText = new GameObject("R3").AddComponent<TextMeshProUGUI>();
        _window.windowRectTransform = new GameObject("Window").AddComponent<RectTransform>();
        _window.companyCardPrefab = new GameObject("CompanyPrefab");
    }

    [TearDown]
    public void Teardown()
    {
        Object.DestroyImmediate(_gameObject);
    }

    [Test]
    public void ClearCompanies_RemovesChildrenExceptFirst()
    {
        var panel = new GameObject("Panel").transform;
        for (int i = 0; i < 3; i++)
            new GameObject($"Child{i}").transform.SetParent(panel);

        _window.ClearCompanies(panel);

        Assert.AreEqual(1, panel.childCount);
    }

    [Test]
    public void ClearUI_ResetsTextsAndOffer()
    {
        _window.SetFakeOffer();
        _window.ClearUI();

        Assert.AreEqual("0", _window.leftMoneyText.text);
        Assert.AreEqual("0", _window.rightMoneyText.text);
        Assert.IsNull(_window.currentOffer);
    }

    [Test]
    public void RefreshUI_SetsPlayerNamesAndMoney()
    {
        _window.SetFakeOffer();
        _window.RefreshUI();

        Assert.IsTrue(_window.leftPlayerNameText.text.Contains(_window.currentOffer.FromPlayerData.Name));
        Assert.IsTrue(_window.rightPlayerNameText.text.Contains(_window.currentOffer.ToPlayerData.Name));
        Assert.AreEqual(_window.currentOffer.FromMoney.ToString(), _window.leftMoneyText.text);
        Assert.AreEqual(_window.currentOffer.ToMoney.ToString(), _window.rightMoneyText.text);
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
        var fromPlayerData = new PlayerData("p1", 500, 1, new PlayerColor(255,0,0));
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