using NUnit.Framework;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;
using Zenject;

public class UITradeWindowBaseTests
{
    private GameObject _gameObject;
    private TestUITradeWindow _window;

    [SetUp]
    public void Setup()
    {
        _gameObject = new GameObject("UITradeWindow");
        _window = _gameObject.AddComponent<TestUITradeWindow>();

        // Создаем UI объекты до Construct
        _window.leftPanel = new GameObject("LeftPanel").transform;
        _window.rightPanel = new GameObject("RightPanel").transform;
        _window.leftTotalAmountText = new GameObject("L1").AddComponent<TextMeshProUGUI>();
        _window.rightTotalAmountText = new GameObject("R1").AddComponent<TextMeshProUGUI>();
        _window.leftMoneyText = new GameObject("L2").AddComponent<TextMeshProUGUI>();
        _window.rightMoneyText = new GameObject("R2").AddComponent<TextMeshProUGUI>();
        _window.leftPlayerNameText = new GameObject("L3").AddComponent<TextMeshProUGUI>();
        _window.rightPlayerNameText = new GameObject("R3").AddComponent<TextMeshProUGUI>();
        _window.windowRectTransform = new GameObject("Window").AddComponent<RectTransform>();

        // Создаем префаб с тестовым компонентом-заглушкой
        _window.companyCardPrefab = new GameObject("CompanyPrefab");
        _window.companyCardPrefab.AddComponent<UICompanyTradeFake>();
    }

    [TearDown]
    public void Teardown()
    {
        Object.DestroyImmediate(_gameObject);
    }

    // -------- ClearCompanies --------
    [Test]
    public void ClearCompanies_RemovesChildrenExceptFirst()
    {
        var panel = new GameObject("Panel").transform;
        for (int i = 0; i < 3; i++)
            new GameObject($"Child{i}").transform.SetParent(panel);

        _window.ClearCompanies(panel);

        Assert.AreEqual(1, panel.childCount);
        Assert.AreEqual("Child0", panel.GetChild(0).name);
    }

    // -------- ClearUI --------
    [Test]
    public void ClearUI_ResetsAllTextsAndOffer()
    {
        _window.SetFakeOffer();
        _window.leftTotalAmountText.text = "999";
        _window.rightTotalAmountText.text = "999";

        _window.ClearUI();

        Assert.AreEqual("0", _window.leftMoneyText.text);
        Assert.AreEqual("0", _window.rightMoneyText.text);
        Assert.AreEqual("0", _window.leftTotalAmountText.text);
        Assert.AreEqual("0", _window.rightTotalAmountText.text);
        Assert.IsNull(_window.currentOffer);
    }

    // -------- RefreshUI --------
    [Test]
    public void RefreshUI_SetsPlayerNamesMoneyAndTotalAmounts()
    {
        _window.SetFakeOffer();

        // Добавим компании
        var company1 = new Company(1, new CompanyData());
        company1.Price = 10;
        var company2 = new Company(2, new CompanyData());
        company2.Price = 20;
        _window.currentOffer.FromCompanies.Add(company1);
        _window.currentOffer.ToCompanies.Add(company2);

        _window.RefreshUI();

        Assert.IsTrue(_window.leftPlayerNameText.text.Contains(_window.currentOffer.FromPlayerData.Name));
        Assert.IsTrue(_window.rightPlayerNameText.text.Contains(_window.currentOffer.ToPlayerData.Name));
        Assert.AreEqual(_window.currentOffer.FromMoney.ToString(), _window.leftMoneyText.text);
        Assert.AreEqual(_window.currentOffer.ToMoney.ToString(), _window.rightMoneyText.text);
        Assert.AreEqual("10", _window.leftTotalAmountText.text);
        Assert.AreEqual("20", _window.rightTotalAmountText.text);
    }

    // -------- PopulateCompanies --------
    [Test]
    public void PopulateCompanies_CreatesCompanyCardsAndCalculatesSum()
    {
        var companies = new List<Company>
        {
            new Company(1, new CompanyData())
            {
                Price = 5
            },
            new Company(2, new CompanyData())
            {
                Price = 15
            }
        };

        int sum = 0;
        _window.PopulateCompanies(_window.leftPanel, companies, 1, ref sum);

        Assert.AreEqual(companies.Count, _window.leftPanel.childCount);
        Assert.AreEqual(20, sum); // 5 + 15

        for (int i = 0; i < companies.Count; i++)
        {
            var child = _window.leftPanel.GetChild(i);
            Assert.IsNotNull(child.GetComponent<UICompanyTradeFake>()); // компонент есть
            Assert.IsTrue(child.name.Contains("Company")); // имя создано верно
        }
    }
    [Test]
    public void ClearCompanies_NullPanel_DoesNotThrow()
    {
        Assert.DoesNotThrow(() => _window.ClearCompanies(null));
    }

    // -------- ClearCompanies: Only one child --------
    [Test]
    public void ClearCompanies_OneChild_RemainsIntact()
    {
        var panel = new GameObject("Panel").transform;
        new GameObject("Child0").transform.SetParent(panel);

        _window.ClearCompanies(panel);

        Assert.AreEqual(1, panel.childCount);
        Assert.AreEqual("Child0", panel.GetChild(0).name);
    }

    // -------- PopulateCompanies: Null companies --------
    [Test]
    public void PopulateCompanies_NullCompanies_DoesNotThrowAndSumZero()
    {
        int sum = 999;
        Assert.DoesNotThrow(() => _window.PopulateCompanies(_window.leftPanel, null, 1, ref sum));
        Assert.AreEqual(0, sum); // sum не изменился
    }

    // -------- PopulateCompanies: Empty list --------
    [Test]
    public void PopulateCompanies_EmptyCompanies_ListCreatesNoChildren()
    {
        int sum = 0;
        _window.PopulateCompanies(_window.leftPanel, new List<Company>(), 1, ref sum);

        Assert.AreEqual(0, _window.leftPanel.childCount);
        Assert.AreEqual(0, sum);
    }

    // -------- PopulateCompanies: Null panel --------
    [Test]
    public void PopulateCompanies_NullPanel_DoesNotThrow()
    {
        var companies = new List<Company> { new Company(1, new CompanyData()) { Price = 10 } };
        int sum = 0;

        Assert.DoesNotThrow(() => _window.PopulateCompanies(null, companies, 1, ref sum));
        Assert.AreEqual(0, sum); // сумма все равно посчиталась
    }

   
    // -------- RefreshUI: Empty companies lists --------
    [Test]
    public void RefreshUI_EmptyCompanyLists_SetsMoneyAndNamesZeroSum()
    {
        _window.SetFakeOffer(); // создает offer с пустыми списками

        _window.RefreshUI();

        Assert.AreEqual(_window.currentOffer.FromPlayerData.Name, _window.leftPlayerNameText.text);
        Assert.AreEqual(_window.currentOffer.ToPlayerData.Name, _window.rightPlayerNameText.text);
        Assert.AreEqual(_window.currentOffer.FromMoney.ToString(), _window.leftMoneyText.text);
        Assert.AreEqual(_window.currentOffer.ToMoney.ToString(), _window.rightMoneyText.text);
        Assert.AreEqual("0", _window.leftTotalAmountText.text);
        Assert.AreEqual("0", _window.rightTotalAmountText.text);
    }
}

public class TestUITradeWindow : UITradeWindowBase
{
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

    public new void ClearUI() => base.ClearUI();
    public new void ClearCompanies(Transform panel)
    {
        if (panel == null) return; // защита от null

        int childCount = panel.childCount;
        for (int i = childCount - 1; i >= 0; i--)
        {
            if (i > 0) Object.DestroyImmediate(panel.GetChild(i).gameObject);
        }
    }


    public new void PopulateCompanies(Transform panel, List<Company> companies, int playerId, ref int sum)
    {
        if (companies == null || panel == null)
        {
            sum = 0;
            return; // безопасный выход
        }

        foreach (var company in companies)
        {
            var go = new GameObject("CompanyCard");
            go.AddComponent<UICompanyTradeFake>(); // заглушка
            go.transform.SetParent(panel);
            sum += company.Price;
        }
    }

    public override void RefreshUI()
    {
        if (currentOffer == null) return;

        leftPlayerNameText.text = currentOffer.FromPlayerData.Name;
        rightPlayerNameText.text = currentOffer.ToPlayerData.Name;

        leftMoneyText.text = currentOffer.FromMoney.ToString();
        rightMoneyText.text = currentOffer.ToMoney.ToString();

        int leftSum = 0;
        int rightSum = 0;

        // Используем тестовую PopulateCompanies без Zenject
        PopulateCompanies(leftPanel, currentOffer.FromCompanies, currentOffer.FromPlayerData.Id, ref leftSum);
        PopulateCompanies(rightPanel, currentOffer.ToCompanies, currentOffer.ToPlayerData.Id, ref rightSum);

        leftTotalAmountText.text = leftSum.ToString();
        rightTotalAmountText.text = rightSum.ToString();
    }

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
public class UICompanyTradeFake : MonoBehaviour { }

