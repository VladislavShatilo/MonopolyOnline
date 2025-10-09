using System;
using Moq;
using NUnit.Framework;

public class LoanPayPresenterTests
{
    private LoanPayPresenter presenter;
    private Mock<ILoanPayWindow> loanPayWindow;
    private Mock<ILocalPlayerService> localPlayerService;
    private Mock<IEventBus> eventBus;
    private Mock<IBankService> bankService;
    private Mock<IPhotonLoanManager> photonLoanManager;

    [SetUp]
    public void SetUp()
    {
        loanPayWindow = new Mock<ILoanPayWindow>();
        localPlayerService = new Mock<ILocalPlayerService>();
        eventBus = new Mock<IEventBus>();
        bankService = new Mock<IBankService>();
        photonLoanManager = new Mock<IPhotonLoanManager>();

        presenter = new LoanPayPresenter();
        presenter.Construct(loanPayWindow.Object, localPlayerService.Object, eventBus.Object, bankService.Object, photonLoanManager.Object);
        presenter.Initialize();
    }

    [TearDown]
    public void TearDown()
    {
        presenter.Dispose();
    }

    [Test]
    public void Initialize_ShouldSubscribeToOfferLoanPayEvent()
    {
        eventBus.Verify(e => e.Subscribe<OfferLoanPayEvent>(It.IsAny<Action<OfferLoanPayEvent>>()), Times.Once);
        loanPayWindow.Verify(w => w.SetPayLoanAction(It.IsAny<Action<int>>()), Times.Once);
    }

    [Test]
    public void Dispose_ShouldUnsubscribeFromOfferLoanPayEvent()
    {
        presenter.Dispose();
        eventBus.Verify(e => e.Unsubscribe<OfferLoanPayEvent>(It.IsAny<Action<OfferLoanPayEvent>>()), Times.Once);
    }

    [Test]
    public void ShowLoanWindow_LocalPlayer_CanAfford_ShowsWindow()
    {
        int localId = 1;
        localPlayerService.Setup(s => s.GetLocalPlayerId()).Returns(localId);
        bankService.Setup(b => b.HasEnoughMoney(localId, 500)).Returns(true);

        var e = new OfferLoanPayEvent(localId, 500);

        // Вызов приватного метода через Reflection
        presenter.GetType()
            .GetMethod("ShowLoanWindow", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .Invoke(presenter, new object[] { e });

        loanPayWindow.Verify(w => w.Show(localId, 500, true), Times.Once);
    }

    [Test]
    public void ShowLoanWindow_LocalPlayer_CannotAfford_ShowsWindowWithFalse()
    {
        int localId = 1;
        localPlayerService.Setup(s => s.GetLocalPlayerId()).Returns(localId);
        bankService.Setup(b => b.HasEnoughMoney(localId, 500)).Returns(false);

        var e = new OfferLoanPayEvent(localId, 500);

        presenter.GetType()
            .GetMethod("ShowLoanWindow", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .Invoke(presenter, new object[] { e });

        loanPayWindow.Verify(w => w.Show(localId, 500, false), Times.Once);
    }

    [Test]
    public void ShowLoanWindow_NotLocalPlayer_HidesWindow()
    {
        int localId = 1;
        localPlayerService.Setup(s => s.GetLocalPlayerId()).Returns(localId);

        var e = new OfferLoanPayEvent(2, 500);

        presenter.GetType()
            .GetMethod("ShowLoanWindow", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .Invoke(presenter, new object[] { e });

        loanPayWindow.Verify(w => w.Hide(), Times.Once);
    }

    [Test]
    public void OnPayLoan_CallsPayLoanRequestAndHidesWindow()
    {
        int playerId = 1;

        presenter.GetType()
            .GetMethod("OnPayLoan", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .Invoke(presenter, new object[] { playerId });

        photonLoanManager.Verify(p => p.PayLoanRequest(playerId), Times.Once);
        loanPayWindow.Verify(w => w.Hide(), Times.Once);
    }
}

