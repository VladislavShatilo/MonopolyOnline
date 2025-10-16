using Moq;
using NUnit.Framework;
using System.Collections.Generic;
using System.Reflection;

[TestFixture]
public class TurnCompanyUIAdapterTests
{
    private Mock<ITurnCompanyUIUseCase> turnUIUseCaseMock;
    private Mock<ICompanyUIService> companyUIServiceMock;
    private Mock<ICompanyRepository> companyRepositoryMock;
    private Mock<IEventBus> eventBusMock;
    private TurnCompanyUIAdapter adapter;

    [SetUp]
    public void SetUp()
    {
        turnUIUseCaseMock = new Mock<ITurnCompanyUIUseCase>();
        companyUIServiceMock = new Mock<ICompanyUIService>();
        companyRepositoryMock = new Mock<ICompanyRepository>();
        eventBusMock = new Mock<IEventBus>();

        adapter = new TurnCompanyUIAdapter();
        adapter.Construct(turnUIUseCaseMock.Object, eventBusMock.Object, companyUIServiceMock.Object, companyRepositoryMock.Object);
        adapter.Initialize();
    }

    [TearDown]
    public void TearDown()
    {
        adapter.Dispose();
    }

    [Test]
    public void Initialize_ShouldSubscribeToTurnStartEvent()
    {
        eventBusMock.Verify(e => e.Subscribe<TurnStartEvent>(It.IsAny<System.Action<TurnStartEvent>>()), Times.Once);
    }

    [Test]
    public void Dispose_ShouldUnsubscribeFromTurnStartEvent()
    {
        adapter.Dispose();
        eventBusMock.Verify(e => e.Unsubscribe<TurnStartEvent>(It.IsAny<System.Action<TurnStartEvent>>()), Times.Once);
    }

    [Test]
    public void OnTurnStart_ShouldShowMortgageButton_WhenActionIsMortgage()
    {
        var companyUI = new Mock<IUICompanyCellView>();
        companyUIServiceMock.Setup(c => c.GetCompanyUI(1)).Returns(companyUI.Object);
        turnUIUseCaseMock.Setup(u => u.GetAvailableActions(1))
            .Returns(new List<CompanyUIAction> { new(1, CompanyActionType.Mortgage) });

        // Вызов через событие
        var turnStartEvent = new TurnStartEvent(1);
        var method = typeof(TurnCompanyUIAdapter).GetMethod("OnTurnStart", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        method.Invoke(adapter, new object[] { turnStartEvent });

        companyUI.Verify(u => u.ShowMortgageButton(), Times.Once);
    }

    [Test]
    public void OnTurnStart_ShouldShowBuyoutButton_WhenActionIsBuyout()
    {
        var companyUI = new Mock<IUICompanyCellView>();
        companyUIServiceMock.Setup(c => c.GetCompanyUI(2)).Returns(companyUI.Object);
        turnUIUseCaseMock.Setup(u => u.GetAvailableActions(2))
            .Returns(new List<CompanyUIAction> { new(2, CompanyActionType.Buyout) });

        var turnStartEvent = new TurnStartEvent(2);
        var method = typeof(TurnCompanyUIAdapter).GetMethod("OnTurnStart", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        method.Invoke(adapter, new object[] { turnStartEvent });

        companyUI.Verify(u => u.ShowBuyoutButton(), Times.Once);
    }

    [Test]
    public void OnTurnStart_ShouldShowBuyFirstBranchButton_WhenRentLevelIs0()
    {
        var companyUI = new Mock<IUICompanyCellView>();
        companyUIServiceMock.Setup(c => c.GetCompanyUI(3)).Returns(companyUI.Object); // <-- важно
        var company = new Company(3, new CompanyData());
        company.RentLevel = 0;
        companyRepositoryMock.Setup(r => r.GetCompanyById(3)).Returns(company);

        turnUIUseCaseMock.Setup(u => u.GetAvailableActions(3))
            .Returns(new List<CompanyUIAction> { new(3, CompanyActionType.ManageBranches) });

        var turnStartEvent = new TurnStartEvent(3);
        var method = typeof(TurnCompanyUIAdapter).GetMethod("OnTurnStart",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        method.Invoke(adapter, new object[] { turnStartEvent });

        companyUI.Verify(u => u.ShowBuyFirstBranchButton(), Times.Once);
    }

    [Test]
    public void OnTurnStart_ShouldShowSellFirstButton_WhenRentLevelIs5()
    {
        var companyUI = new Mock<IUICompanyCellView>();
        companyUIServiceMock.Setup(c => c.GetCompanyUI(4)).Returns(companyUI.Object);
        var company = new Company(4, new CompanyData());
        company.RentLevel = 5;
        companyRepositoryMock.Setup(r => r.GetCompanyById(4)).Returns(company);
        turnUIUseCaseMock.Setup(u => u.GetAvailableActions(4))
            .Returns(new List<CompanyUIAction> { new(4, CompanyActionType.ManageBranches) });

        var turnStartEvent = new TurnStartEvent(4);
        var method = typeof(TurnCompanyUIAdapter).GetMethod("OnTurnStart", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        method.Invoke(adapter, new object[] { turnStartEvent });

        companyUI.Verify(u => u.ShowSellFirstButton(), Times.Once);
    }

    [Test]
    public void OnTurnStart_ShouldShowBuySellButtons_WhenRentLevelOther()
    {
        var companyUI = new Mock<IUICompanyCellView>();
        companyUIServiceMock.Setup(c => c.GetCompanyUI(5)).Returns(companyUI.Object);
        var company = new Company(5, new CompanyData());
        company.RentLevel = 3;
        companyRepositoryMock.Setup(r => r.GetCompanyById(5)).Returns(company);
        turnUIUseCaseMock.Setup(u => u.GetAvailableActions(5))
            .Returns(new List<CompanyUIAction> { new(5, CompanyActionType.ManageBranches) });

        var turnStartEvent = new TurnStartEvent(5);
        var method = typeof(TurnCompanyUIAdapter).GetMethod("OnTurnStart", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        method.Invoke(adapter, new object[] { turnStartEvent });

        companyUI.Verify(u => u.ShowBuySellButtons(), Times.Once);
    }

    [Test]
    public void OnTurnStart_ShouldHideButtons_WhenActionIsNone()
    {
        var companyUI = new Mock<IUICompanyCellView>();
        companyUIServiceMock.Setup(c => c.GetCompanyUI(6)).Returns(companyUI.Object); // <-- добавлено
        turnUIUseCaseMock.Setup(u => u.GetAvailableActions(6))
            .Returns(new List<CompanyUIAction> { new(6, CompanyActionType.None) });

        var turnStartEvent = new TurnStartEvent(6);
        var method = typeof(TurnCompanyUIAdapter).GetMethod("OnTurnStart", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        method.Invoke(adapter, new object[] { turnStartEvent });

        companyUI.Verify(u => u.HideAllBranchButtons(), Times.Once);
        companyUI.Verify(u => u.HideAllMortgageButtons(), Times.Once);
    }
    [Test]
    public void OnTurnStart_ShouldDoNothing_WhenNoActions()
    {
        turnUIUseCaseMock.Setup(u => u.GetAvailableActions(1))
            .Returns(new List<CompanyUIAction>());

        var turnStartEvent = new TurnStartEvent(1);
        var method = typeof(TurnCompanyUIAdapter).GetMethod("OnTurnStart", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        // Проверяем, что не выбрасывается исключений
        Assert.DoesNotThrow(() => method.Invoke(adapter, new object[] { turnStartEvent }));
    }

    [Test]
    public void OnTurnStart_ShouldThrow_WhenCompanyUIIsNull()
    {
        turnUIUseCaseMock.Setup(u => u.GetAvailableActions(1))
            .Returns(new List<CompanyUIAction> { new(1, CompanyActionType.Mortgage) });

        companyUIServiceMock.Setup(c => c.GetCompanyUI(1)).Returns((IUICompanyCellView)null);

        var turnStartEvent = new TurnStartEvent(1);
        var method = typeof(TurnCompanyUIAdapter).GetMethod("OnTurnStart", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        Assert.Throws<TargetInvocationException>(() => method.Invoke(adapter, new object[] { turnStartEvent }));
    }

    [Test]
    public void OnTurnStart_ShouldThrow_WhenCompanyIsNull_ForManageBranches()
    {
        turnUIUseCaseMock.Setup(u => u.GetAvailableActions(1))
            .Returns(new List<CompanyUIAction> { new(1, CompanyActionType.ManageBranches) });

        var uiMock = new Mock<IUICompanyCellView>();
        companyUIServiceMock.Setup(c => c.GetCompanyUI(1)).Returns(uiMock.Object);
        companyRepositoryMock.Setup(r => r.GetCompanyById(1)).Returns((Company)null);

        var turnStartEvent = new TurnStartEvent(1);
        var method = typeof(TurnCompanyUIAdapter).GetMethod("OnTurnStart", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        Assert.Throws<TargetInvocationException>(() => method.Invoke(adapter, new object[] { turnStartEvent }));
    }

    [Test]
    public void OnTurnStart_ShouldHideButtons_WhenUnknownActionType()
    {
        var uiMock = new Mock<IUICompanyCellView>();
        turnUIUseCaseMock.Setup(u => u.GetAvailableActions(1))
            .Returns(new List<CompanyUIAction> { new(1, (CompanyActionType)999) }); // неизвестный тип
        companyUIServiceMock.Setup(c => c.GetCompanyUI(1)).Returns(uiMock.Object);

        var turnStartEvent = new TurnStartEvent(1);
        var method = typeof(TurnCompanyUIAdapter).GetMethod("OnTurnStart", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        method.Invoke(adapter, new object[] { turnStartEvent });

        uiMock.Verify(u => u.HideAllBranchButtons(), Times.Once);
        uiMock.Verify(u => u.HideAllMortgageButtons(), Times.Once);
    }
}
