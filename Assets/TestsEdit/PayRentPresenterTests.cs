using NUnit.Framework;
using Moq;
using System;

[TestFixture]
public class PayRentPresenterTests
{
    private PayRentPresenter presenter;

    private Mock<IPayRentWindow> mockWindow;
    private Mock<IPhotonCompanyManager> mockCompanyManager;
    private Mock<IPlayerRepository> mockPlayerRepository;
    private Mock<ILocalPlayerService> mockLocalPlayerService;
    private Mock<IEventBus> mockEventBus;

    private OfferRentEvent offerEvent;
    private RentPaidEvent rentPaidEvent;
    private PlayerData testPlayer;

    // Делегаты для захвата
    private Action<OfferRentEvent> offerHandler;
    private Action<RentPaidEvent> rentPaidHandler;
    private Action payAction;

    [SetUp]
    public void SetUp()
    {
        // Создание моков
        mockWindow = new Mock<IPayRentWindow>();
        mockCompanyManager = new Mock<IPhotonCompanyManager>();
        mockPlayerRepository = new Mock<IPlayerRepository>();
        mockLocalPlayerService = new Mock<ILocalPlayerService>();
        mockEventBus = new Mock<IEventBus>();

        // Захватываем события
        mockEventBus
            .Setup(b => b.Subscribe(It.IsAny<Action<OfferRentEvent>>()))
            .Callback<Action<OfferRentEvent>>(h => offerHandler = h);
        mockEventBus
            .Setup(b => b.Subscribe(It.IsAny<Action<RentPaidEvent>>()))
            .Callback<Action<RentPaidEvent>>(h => rentPaidHandler = h);
        mockWindow
            .Setup(w => w.SetPayAction(It.IsAny<Action>()))
            .Callback<Action>(a => payAction = a);

        // Инициализация презентера
        presenter = new PayRentPresenter();
        presenter.Construct(
            mockWindow.Object,
            mockCompanyManager.Object,
            mockPlayerRepository.Object,
            mockLocalPlayerService.Object,
            mockEventBus.Object
        );

        // Базовые данные
        offerEvent = new OfferRentEvent(5,1, 300);
        rentPaidEvent = new RentPaidEvent(5,1,2,200);
        testPlayer = new PlayerData("Tester", 500, 1, null) { Money = 500 };

        mockPlayerRepository.Setup(r => r.GetPlayerById(1)).Returns(testPlayer);
        mockLocalPlayerService.Setup(s => s.GetLocalPlayerId()).Returns(1);
    }

    [Test]
    public void Initialize_ShouldSubscribeToEvents_AndSetPayAction()
    {
        // Act
        presenter.Initialize();

        // Assert
        mockEventBus.Verify(e => e.Subscribe(It.IsAny<Action<OfferRentEvent>>()), Times.Once);
        mockEventBus.Verify(e => e.Subscribe(It.IsAny<Action<RentPaidEvent>>()), Times.Once);
        mockWindow.Verify(w => w.SetPayAction(It.IsAny<Action>()), Times.Once);
    }

    [Test]
    public void Dispose_ShouldUnsubscribeFromEvents()
    {
        // Arrange
        presenter.Initialize();

        // Act
        presenter.Dispose();

        // Assert
        mockEventBus.Verify(e => e.Unsubscribe(It.IsAny<Action<OfferRentEvent>>()), Times.Once);
        mockEventBus.Verify(e => e.Unsubscribe(It.IsAny<Action<RentPaidEvent>>()), Times.Once);
    }

    [Test]
    public void ShowRentFor_ShouldShowWindow_WhenLocalPlayerIsRenter()
    {
        // Arrange
        presenter.Initialize();

        // Act
        offerHandler?.Invoke(offerEvent);

        // Assert
        mockWindow.Verify(w => w.Show(1, 5, 300, true), Times.Once);
        mockWindow.Verify(w => w.HardHide(), Times.Never);
    }

    [Test]
    public void ShowRentFor_ShouldHideWindow_WhenLocalPlayerIsNotRenter()
    {
        // Arrange
        mockLocalPlayerService.Setup(s => s.GetLocalPlayerId()).Returns(99);
        presenter.Initialize();

        // Act
        offerHandler?.Invoke(offerEvent);

        // Assert
        mockWindow.Verify(w => w.HardHide(), Times.Once);
        mockWindow.Verify(w => w.Show(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()), Times.Never);
    }

    [Test]
    public void OnRentPaid_ShouldHideWindow_WhenMatchesCurrentPlayerAndCell()
    {
        // Arrange
        presenter.Initialize();
        offerHandler?.Invoke(offerEvent);

        // Act
        rentPaidHandler?.Invoke(rentPaidEvent);

        // Assert
        mockWindow.Verify(w => w.Hide(), Times.Once);
    }

    [Test]
    public void OnPayClicked_ShouldRequestPayRent()
    {
        // Arrange
        presenter.Initialize();
        offerHandler?.Invoke(offerEvent);

        // Act
        payAction?.Invoke();

        // Assert
        mockCompanyManager.Verify(m => m.RequestPayRent(5, 1), Times.Once);
    }
    [Test]
    public void ShowRentFor_LocalPlayerCannotPay_ShowsWindowWithFalseCanPay()
    {
        testPlayer.Money = 100; // меньше аренды
        presenter.Initialize();

        offerHandler?.Invoke(new OfferRentEvent(5,1, 300));

        mockWindow.Verify(w => w.Show(1, 5, 300, false), Times.Once);
    }

    [Test]
    public void ShowRentFor_PlayerNotFound_Throws()
    {
        mockPlayerRepository.Setup(r => r.GetPlayerById(1)).Returns((PlayerData)null);
        presenter.Initialize();

        Assert.Throws<InvalidOperationException>(() => offerHandler?.Invoke(offerEvent));
    }

    [Test]
    public void OnRentPaid_DifferentPlayerOrCell_DoesNotHideWindow()
    {
        presenter.Initialize();
        offerHandler?.Invoke(offerEvent);

        // другой игрок
        rentPaidHandler?.Invoke(new RentPaidEvent(99, 5, 1, 300));
        // другая ячейка
        rentPaidHandler?.Invoke(new RentPaidEvent(1, 99, 1, 300));

        mockWindow.Verify(w => w.Hide(), Times.Never);
    }

    
}
