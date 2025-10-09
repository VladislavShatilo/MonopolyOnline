using NUnit.Framework;
using Moq;
using System.Collections.Generic;
using UnityEngine;

public class CompanyUIServiceTests
{
    private CompanyUIService service;
    private Mock<IBoardService> boardServiceMock;
    private Mock<IEventBus> eventBusMock;

    [SetUp]
    public void Setup()
    {
        boardServiceMock = new Mock<IBoardService>();
        eventBusMock = new Mock<IEventBus>();

        service = new CompanyUIService();
        service.Construct(boardServiceMock.Object, eventBusMock.Object);
    }

    [Test]
    public void Initialize_ShouldSubscribeToEvent()
    {
        service.Initialize();
        eventBusMock.Verify(e => e.Subscribe<HideButtonsTradeEvent>(It.IsAny<System.Action<HideButtonsTradeEvent>>()), Times.Once);
    }

    [Test]
    public void Dispose_ShouldUnsubscribeFromEvent()
    {
        service.Initialize(); // подписка
        service.Dispose();
        eventBusMock.Verify(e => e.Unsubscribe<HideButtonsTradeEvent>(It.IsAny<System.Action<HideButtonsTradeEvent>>()), Times.Once);
    }

    [Test]
    public void InitializeUI_ShouldCreateAndInitCompanyCellsAndPopups()
    {
        // Подготовка моков
        var mockCells = new List<CellData>
        {
            new CellData { cellType = CellType.Company, companyData = new CompanyData() },
            new CellData { cellType = CellType.FieldCompany, fieldCompanyData = new FieldCompanyData() }
        };

        var mockTransforms = new List<RectTransform> { new GameObject().AddComponent<RectTransform>(), new GameObject().AddComponent<RectTransform>() };
        boardServiceMock.Setup(b => b.GetAllCellData()).Returns(mockCells);
        boardServiceMock.Setup(b => b.GetCellRectTransform(It.IsAny<int>())).Returns<int>(i => mockTransforms[i]);

        // Добавим компоненты к трансформам
        var popup = mockTransforms[0].gameObject.AddComponent<CompanyWindowPopup>();
        var companyUI = mockTransforms[1].gameObject.AddComponent<UICompanyCell>();

        service.InitializeUI();

        // Проверка, что UI и popup сохранились в словарях
        Assert.IsNotNull(service.GetCompanyUI(1));
    }

    [Test]
    public void HideAllButtonsOnTrade_ShouldCallHideMethods()
    {
        var mockCompanyUI = new Mock<IUICompanyCellView>();
        service.InitializeUI(); // иначе словарь пустой
        // Вставляем напрямую для теста
        service.GetType().GetField("companyUIs", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .SetValue(service, new Dictionary<int, UICompanyCell> { { 0, mockCompanyUI.Object as UICompanyCell } });

        var e = new HideButtonsTradeEvent(0);
        // Вызов приватного метода через reflection
        var method = service.GetType().GetMethod("HideAllButtonsOnTrade", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        method.Invoke(service, new object[] { e });

        mockCompanyUI.Verify(c => c.HideAllBranchButtons(), Times.Once);
        mockCompanyUI.Verify(c => c.HideAllMortgageButtons(), Times.Once);
    }
}
