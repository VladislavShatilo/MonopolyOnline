using NUnit.Framework;
using Moq;
using System;
using System.Collections.Generic;
using UnityEngine;

public class CompanyUIServiceTests
{
    private Mock<IBoardService> boardServiceMock;
    private Mock<IEventBus> eventBusMock;
    private CompanyUIService service;

    [SetUp]
    public void Setup()
    {
        boardServiceMock = new Mock<IBoardService>();
        eventBusMock = new Mock<IEventBus>();
        service = new CompanyUIService();
    }

    [Test]
    public void Construct_ShouldThrow_WhenBoardServiceIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => service.Construct(null, eventBusMock.Object));
    }

    [Test]
    public void Construct_ShouldThrow_WhenEventBusIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => service.Construct(boardServiceMock.Object, null));
    }

    [Test]
    public void Construct_ShouldAssignDependencies()
    {
        service.Construct(boardServiceMock.Object, eventBusMock.Object);
        Assert.NotNull(service);
    }

    [Test]
    public void Initialize_ShouldSubscribeToEvent()
    {
        service.Construct(boardServiceMock.Object, eventBusMock.Object);
        service.Initialize();
        eventBusMock.Verify(e => e.Subscribe<HideButtonsTradeEvent>(It.IsAny<Action<HideButtonsTradeEvent>>()), Times.Once);
    }

    [Test]
    public void Dispose_ShouldUnsubscribeFromEvent()
    {
        service.Construct(boardServiceMock.Object, eventBusMock.Object);
        service.Dispose();
        eventBusMock.Verify(e => e.Unsubscribe<HideButtonsTradeEvent>(It.IsAny<Action<HideButtonsTradeEvent>>()), Times.Once);
    }

    [Test]
    public void InitializeUI_ShouldInitializeCellsAndPopups()
    {
        service.Construct(boardServiceMock.Object, eventBusMock.Object);

        var cellData = new List<CellData>
    {
        new CellData { cellType = CellType.Company, companyData = new CompanyData() },
    };
        boardServiceMock.Setup(b => b.GetAllCellData()).Returns(cellData);

        // Создаём один GameObject с обоими компонентами
        var cellGO = new GameObject("CellGO");
        var rect = cellGO.AddComponent<RectTransform>();
        var companyCell = cellGO.AddComponent<TestUICompanyCell>();
        var popup = cellGO.AddComponent<TestCompanyWindowPopup>();

        // Настраиваем boardServiceMock
        boardServiceMock.Setup(b => b.GetCellRectTransform(It.IsAny<int>())).Returns(rect);

        service.InitializeUI();

        // Проверяем, что companyUI добавлен в словарь
        Assert.NotNull(service.GetCompanyUI(0));
    }


    [Test]
    public void GetCompanyUI_ShouldReturnNull_IfNotExists()
    {
        service.Construct(boardServiceMock.Object, eventBusMock.Object);
        var result = service.GetCompanyUI(99);
        Assert.IsNull(result);
    }

    [Test]
    public void HideAllButtonsOnTrade_ShouldCallHideMethods()
    {
        // Arrange
        var fakeCell = new FakeUICompanyCell();

        // Подставляем фейк в приватный словарь companyUIs
        var dictField = typeof(CompanyUIService)
            .GetField("companyUIs", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        dictField.SetValue(service, new Dictionary<int, IUICompanyCellView>
        {
            { 1, fakeCell } // используем ID = 1
        });

        var e = new HideButtonsTradeEvent(1);

        // Act: вызываем приватный метод через reflection
        var method = typeof(CompanyUIService)
            .GetMethod("HideAllButtonsOnTrade", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        method.Invoke(service, new object[] { e });

        // Assert: проверяем, что фейк зафиксировал вызовы
        Assert.IsTrue(fakeCell.HideBranchesCalled, "HideAllBranchButtons не вызвано");
        Assert.IsTrue(fakeCell.HideMortgageCalled, "HideAllMortgageButtons не вызвано");
    }
    // ---------- Заглушки ----------

    private class TestCompanyWindowPopup : MonoBehaviour
    {
        public void Init(int id) { } // имитация метода
        public event Action<int> OnCompanyClicked;
    }

    private class TestUICompanyCell : MonoBehaviour, IUICompanyCellView
    {
        public void Init(int id) { }
        public void HideAllBranchButtons() { }
        public void HideAllMortgageButtons() { }

        public int CompanyId()
        {
            throw new NotImplementedException();
        }

        public void UpdateUI(string name, int price, Color groupColor)
        {
            throw new NotImplementedException();
        }

        public void UpdateOwner(Color ownerColor)
        {
            throw new NotImplementedException();
        }

        public void SetRentText(int rent)
        {
            throw new NotImplementedException();
        }

        public void UpdateBranchStars(int level)
        {
            throw new NotImplementedException();
        }

        public void ShowBuyFirstBranchButton()
        {
            throw new NotImplementedException();
        }

        public void ShowBuySellButtons()
        {
            throw new NotImplementedException();
        }

        public void ShowSellFirstButton()
        {
            throw new NotImplementedException();
        }

        public void ShowMortgageButton()
        {
            throw new NotImplementedException();
        }

        public void ShowBuyoutButton()
        {
            throw new NotImplementedException();
        }

        public void MortgageUI()
        {
            throw new NotImplementedException();
        }

        public void BuyoutUI()
        {
            throw new NotImplementedException();
        }

        public void SetMortgageTurnsText(int turns)
        {
            throw new NotImplementedException();
        }

        public void LoseCompanyUI(Company company)
        {
            throw new NotImplementedException();
        }
    }
    // Заглушка без наследования от MonoBehaviour
    private class FakeUICompanyCell : IUICompanyCellView
    {
        public bool HideBranchesCalled { get; private set; }
        public bool HideMortgageCalled { get; private set; }

        public void HideAllBranchButtons() => HideBranchesCalled = true;
        public void HideAllMortgageButtons() => HideMortgageCalled = true;

        // Пустые реализации остальных методов интерфейса
        public void Init(int id) { }
        public int CompanyId() => 0;
        public void UpdateUI(string name, int price, Color groupColor) { }
        public void UpdateOwner(Color ownerColor) { }
        public void SetRentText(int rent) { }
        public void UpdateBranchStars(int level) { }
        public void ShowBuyFirstBranchButton() { }
        public void ShowBuySellButtons() { }
        public void ShowSellFirstButton() { }
        public void ShowMortgageButton() { }
        public void ShowBuyoutButton() { }
        public void MortgageUI() { }
        public void BuyoutUI() { }
        public void SetMortgageTurnsText(int turns) { }
        public void LoseCompanyUI(Company company) { }
    }
}
