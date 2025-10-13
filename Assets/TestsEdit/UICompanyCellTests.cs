using Moq;
using NUnit.Framework;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
[TestFixture]
public class UICompanyCellTests
{
    private GameObject go;
    private UICompanyCell cell;

    private Mock<IUICompanyCellRepository> repoMock;
    private Mock<IPhotonBranchManager> branchMock;
    private Mock<IPhotonMortgageManager> mortgageMock;

    [SetUp]
    public void SetUp()
    {
        go = new GameObject();
        cell = go.AddComponent<UICompanyCell>();

        repoMock = new Mock<IUICompanyCellRepository>();
        branchMock = new Mock<IPhotonBranchManager>();
        mortgageMock = new Mock<IPhotonMortgageManager>();

        cell.Construct(repoMock.Object, branchMock.Object, mortgageMock.Object);

        // Создаем минимальные UI элементы
        cell.GetType().GetField("companyNameText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .SetValue(cell, new GameObject().AddComponent<TextMeshProUGUI>());
        cell.GetType().GetField("priceText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .SetValue(cell, new GameObject().AddComponent<TextMeshProUGUI>());
        cell.GetType().GetField("BGPriceImage", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .SetValue(cell, new GameObject().AddComponent<Image>());
        cell.GetType().GetField("BGImage", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .SetValue(cell, new GameObject().AddComponent<Image>());

        // Кнопки
        string[] buttonNames = { "buyFirstBranchButton", "buyBranchButton", "sellBranchButton", "sellFirstBranchButton",
                                 "mortgageButton", "buyoutButton" };
        foreach (var name in buttonNames)
        {
            cell.GetType().GetField(name, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .SetValue(cell, new GameObject().AddComponent<Button>());
        }

        // Звезды
        string[] starNames = { "star1Image", "star2Image", "star3Image", "star4Image", "goldStarImage" };
        foreach (var name in starNames)
        {
            cell.GetType().GetField(name, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .SetValue(cell, new GameObject().AddComponent<Image>());
        }

        // Mortgage UI элементы
        cell.GetType().GetField("mortgageStatsGO", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .SetValue(cell, new GameObject());
        cell.GetType().GetField("mortgageFadeImage", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .SetValue(cell, new GameObject().AddComponent<Image>());
        cell.GetType().GetField("mortgageTurnsText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .SetValue(cell, new GameObject().AddComponent<TextMeshProUGUI>());
    }

    [TearDown]
    public void TearDown()
    {
        UnityEngine.Object.DestroyImmediate(go);
    }

    // Репозиторий
    [Test]
    public void Initialize_ShouldRegisterInRepository()
    {
        cell.Initialize();
        var initMethod = cell.GetType().GetMethod("Initialize", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        initMethod?.Invoke(cell, null);

        repoMock.Verify(r => r.Register(cell), Times.Once);
    }

    [Test]
    public void Dispose_ShouldUnregisterFromRepository()
    {
        cell.Dispose();
        repoMock.Verify(r => r.Unregister(cell), Times.Once);
    }

    // Инициализация и ID
    [Test]
    public void Init_ShouldAssignCompanyId()
    {
        cell.Init(42);
        Assert.AreEqual(42, cell.CompanyId());
    }

    // UI методы
    [Test]
    public void UpdateUI_ShouldSetTextsAndColor()
    {
        var nameField = (TextMeshProUGUI)cell.GetType().GetField("companyNameText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(cell);
        var priceField = (TextMeshProUGUI)cell.GetType().GetField("priceText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(cell);
        var bgPrice = (Image)cell.GetType().GetField("BGPriceImage", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(cell);

        cell.UpdateUI("TestCo", 1234, Color.red);

        Assert.AreEqual("TestCo", nameField.text);
        Assert.AreEqual("1,234", priceField.text);
        Assert.AreEqual(Color.red, bgPrice.color);
    }

    [Test]
    public void UpdateOwner_ShouldChangeBGColor()
    {
        var bg = (Image)cell.GetType().GetField("BGImage", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(cell);
        cell.UpdateOwner(Color.green);
        Assert.AreEqual(Color.green, bg.color);
    }

    [Test]
    public void SetRentText_ShouldUpdatePriceText()
    {
        var price = (TextMeshProUGUI)cell.GetType().GetField("priceText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(cell);
        cell.SetRentText(9999);
        Assert.AreEqual("9,999", price.text);
    }

    // Звезды
    [TestCase(0)]
    [TestCase(1)]
    [TestCase(2)]
    [TestCase(3)]
    [TestCase(4)]
    [TestCase(5)]
    public void UpdateBranchStars_ShouldActivateCorrectStars(int level)
    {
        cell.UpdateBranchStars(level);
        var star1 = (Image)cell.GetType().GetField("star1Image", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(cell);
        var star2 = (Image)cell.GetType().GetField("star2Image", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(cell);
        var star3 = (Image)cell.GetType().GetField("star3Image", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(cell);
        var star4 = (Image)cell.GetType().GetField("star4Image", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(cell);
        var gold = (Image)cell.GetType().GetField("goldStarImage", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(cell);

        switch (level)
        {
            case 0:
                Assert.IsFalse(star1.gameObject.activeSelf);
                Assert.IsFalse(star2.gameObject.activeSelf);
                Assert.IsFalse(star3.gameObject.activeSelf);
                Assert.IsFalse(star4.gameObject.activeSelf);
                Assert.IsFalse(gold.gameObject.activeSelf);
                break;
            case 1:
                Assert.IsTrue(star1.gameObject.activeSelf);
                Assert.IsFalse(star2.gameObject.activeSelf);
                Assert.IsFalse(star3.gameObject.activeSelf);
                Assert.IsFalse(star4.gameObject.activeSelf);
                Assert.IsFalse(gold.gameObject.activeSelf);
                break;
            case 2:
                Assert.IsTrue(star1.gameObject.activeSelf);
                Assert.IsTrue(star2.gameObject.activeSelf);
                Assert.IsFalse(star3.gameObject.activeSelf);
                Assert.IsFalse(star4.gameObject.activeSelf);
                Assert.IsFalse(gold.gameObject.activeSelf);
                break;
            case 3:
                Assert.IsTrue(star1.gameObject.activeSelf);
                Assert.IsTrue(star2.gameObject.activeSelf);
                Assert.IsTrue(star3.gameObject.activeSelf);
                Assert.IsFalse(star4.gameObject.activeSelf);
                Assert.IsFalse(gold.gameObject.activeSelf);
                break;
            case 4:
                Assert.IsTrue(star1.gameObject.activeSelf);
                Assert.IsTrue(star2.gameObject.activeSelf);
                Assert.IsTrue(star3.gameObject.activeSelf);
                Assert.IsTrue(star4.gameObject.activeSelf);
                Assert.IsFalse(gold.gameObject.activeSelf);
                break;
            case 5:
                Assert.IsFalse(star1.gameObject.activeSelf);
                Assert.IsFalse(star2.gameObject.activeSelf);
                Assert.IsFalse(star3.gameObject.activeSelf);
                Assert.IsFalse(star4.gameObject.activeSelf);
                Assert.IsTrue(gold.gameObject.activeSelf);
                break;
        }
      
    }

    // Кнопки Branch
    [Test]
    public void ShowBuyFirstBranchButton_ShouldActivateButton()
    {
        cell.ShowBuyFirstBranchButton();
        var btn = (Button)cell.GetType().GetField("buyFirstBranchButton", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(cell);
        Assert.IsTrue(btn.gameObject.activeSelf);
    }

    [Test]
    public void ShowBuySellButtons_ShouldActivateButtons()
    {
        cell.ShowBuySellButtons();
        var btn1 = (Button)cell.GetType().GetField("buyBranchButton", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(cell);
        var btn2 = (Button)cell.GetType().GetField("sellBranchButton", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(cell);
        Assert.IsTrue(btn1.gameObject.activeSelf);
        Assert.IsTrue(btn2.gameObject.activeSelf);
    }

    [Test]
    public void ShowSellFirstButton_ShouldActivateButton()
    {
        cell.ShowSellFirstButton();
        var btn = (Button)cell.GetType().GetField("sellFirstBranchButton", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(cell);
        Assert.IsTrue(btn.gameObject.activeSelf);
    }

    // Mortgage UI
    [Test]
    public void ShowMortgageButton_ShouldActivateMortgageButton()
    {
        cell.ShowMortgageButton();
        var mortgage = (Button)cell.GetType().GetField("mortgageButton", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(cell);
        Assert.IsTrue(mortgage.gameObject.activeSelf);
    }

    [Test]
    public void ShowBuyoutButton_ShouldActivateBuyoutButton()
    {
        cell.ShowBuyoutButton();
        var buyout = (Button)cell.GetType().GetField("buyoutButton", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(cell);
        Assert.IsTrue(buyout.gameObject.activeSelf);
    }

    [Test]
    public void MortgageUI_ShouldActivateMortgageUI()
    {
        cell.MortgageUI();
        var mortgageGO = (GameObject)cell.GetType().GetField("mortgageStatsGO", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(cell);
        Assert.IsTrue(mortgageGO.activeSelf);
    }

    [Test]
    public void BuyoutUI_ShouldActivateBuyoutUI()
    {
        cell.BuyoutUI();
        var mortgageGO = (GameObject)cell.GetType().GetField("mortgageStatsGO", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(cell);
        Assert.IsFalse(mortgageGO.activeSelf);
    }

    // LoseCompanyUI
    [Test]
    public void LoseCompanyUI_ShouldUpdateOwnerAndRent()
    {
        CompanyData companyData = new CompanyData();
        companyData.rent = new int[1];
        companyData.rent[0] = 5000;

        var company = new Company(1, companyData);
        var bg = (Image)cell.GetType().GetField("BGImage", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(cell);
        var price = (TextMeshProUGUI)cell.GetType().GetField("priceText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(cell);

        cell.LoseCompanyUI(company);
        Assert.AreEqual(Color.white, bg.color);
        Assert.AreEqual("5,000", price.text);
    }
}
