using System;
using System.Collections.Generic;
using Moq;
using NUnit.Framework;
using UnityEngine;

[TestFixture]
public class UICompanyCellPresenterTests
{
    private Mock<IPlayerRepository> mockPlayerRepo;
    private Mock<ICompanyRepository> mockCompanyRepo;
    private Mock<IUICompanyCellRepository> mockUIRepo;
    private Mock<ICompanyService> mockCompanyService;
    private Mock<IEventBus> mockEventBus;
    private UICompanyCellPresenter presenter;

    private Action<CompanyBoughtEvent> subscribedAction;

    [SetUp]
    public void Setup()
    {
        mockPlayerRepo = new Mock<IPlayerRepository>();
        mockCompanyRepo = new Mock<ICompanyRepository>();
        mockUIRepo = new Mock<IUICompanyCellRepository>();
        mockCompanyService = new Mock<ICompanyService>();
        mockEventBus = new Mock<IEventBus>();

        // Перехватываем делегат, который презентер подписывает на событие
        mockEventBus.Setup(e => e.Subscribe(It.IsAny<Action<CompanyBoughtEvent>>()))
            .Callback<Action<CompanyBoughtEvent>>(a => subscribedAction = a);

        presenter = new UICompanyCellPresenter();
        presenter.Construct(
            mockPlayerRepo.Object,
            mockCompanyRepo.Object,
            mockUIRepo.Object,
            mockCompanyService.Object,
            mockEventBus.Object
        );

        presenter.Initialize();
    }

    [TearDown]
    public void TearDown()
    {
        presenter.Dispose();
    }

    [Test]
    public void CompanyBoughtUpdate_ShouldDoNothing_WhenCompanyNotOwnedByPlayer()
    {
        var companyGroup = new CompanyGroup();
        var company = new Company(1, new CompanyData())
        {
            OwnerId = 999,
            Group = companyGroup
        };

        mockCompanyRepo.Setup(r => r.GetCompanyById(1)).Returns(company);
        mockCompanyRepo.Setup(r => r.GetByGroup(companyGroup)).Returns(new[] { company });

        // Вызов события с PlayerId != OwnerId
        subscribedAction?.Invoke(new CompanyBoughtEvent(1, 42));

        // UI не должен вызываться
        mockUIRepo.Verify(u => u.GetByCompanyId(It.IsAny<int>()), Times.Never);
    }

    [Test]
    public void CompanyBoughtUpdate_ShouldHandleNullViewGracefully()
    {
        var companyGroup = new CompanyGroup();
        var company = new Company(1, new CompanyData())
        {
            OwnerId = 42,
            Group = companyGroup
        };
        mockCompanyRepo.Setup(r => r.GetCompanyById(1)).Returns(company);
        mockCompanyRepo.Setup(r => r.GetByGroup(companyGroup)).Returns(new[] { company });

        mockUIRepo.Setup(u => u.GetByCompanyId(1)).Returns((IUICompanyCellView)null);

        subscribedAction?.Invoke(new CompanyBoughtEvent(1, 42));

        // Проверяем, что ошибок не возникло и UI не вызван
        mockUIRepo.Verify(u => u.GetByCompanyId(1), Times.Once);
    }

    [Test]
    public void CompanyBoughtUpdate_ShouldUpdateUI_WhenCompanyOwnedByPlayer()
    {
        var companyGroup = new CompanyGroup();

        var company = new Company(1, new CompanyData())
        {
            OwnerId = 42,
            Group = companyGroup
        };
        var player = new PlayerData("p1", 5000, 42, null);
        var mockView = new Mock<IUICompanyCellView>();

        mockCompanyRepo.Setup(r => r.GetCompanyById(1)).Returns(company);
        mockCompanyRepo.Setup(r => r.GetByGroup(companyGroup)).Returns(new[] { company });
        mockPlayerRepo.Setup(p => p.GetPlayerById(42)).Returns(player);
        mockUIRepo.Setup(u => u.GetByCompanyId(1)).Returns(mockView.Object);
        mockCompanyService.Setup(s => s.CalculateRent(company, 1)).Returns(100);

        subscribedAction?.Invoke(new CompanyBoughtEvent(1, 42));

        mockView.Verify(v => v.UpdateOwner(It.IsAny<Color>()), Times.Once);
        mockView.Verify(v => v.SetRentText(100), Times.Once);
    }
}