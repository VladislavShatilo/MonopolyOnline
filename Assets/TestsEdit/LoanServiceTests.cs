using Moq;
using NUnit.Framework;
using System;
using UnityEngine;

public class LoanServiceTests
{
    private LoanService loanService;
    private Mock<IPlayerRepository> playerRepoMock;
    private Mock<IPhotonLoanManager> photonLoanManagerMock;
    private Mock<IPhotonNetworkWrapper> photonWrapperMock;
    private Mock<IBankService> bankServiceMock;
    private Mock<IEventBus> eventBusMock;
    private GameSettings gameSettings;
    private PlayerData player = new PlayerData("123", 124, 1, null);

    // Простой класс для теста вместо полноценного PlayerData


    [SetUp]
    public void Setup()
    {
        playerRepoMock = new Mock<IPlayerRepository>();
        photonLoanManagerMock = new Mock<IPhotonLoanManager>();
        photonWrapperMock = new Mock<IPhotonNetworkWrapper>();
        bankServiceMock = new Mock<IBankService>();
        eventBusMock = new Mock<IEventBus>();

        gameSettings = ScriptableObject.CreateInstance<GameSettings>();
        gameSettings.loanAmount = 1000;
        gameSettings.loanAmountBack = 1200;

        playerRepoMock.Setup(r => r.GetPlayerById(It.IsAny<int>())).Returns(() => player);

        loanService = new LoanService();
        loanService.Construct(playerRepoMock.Object, photonLoanManagerMock.Object, bankServiceMock.Object,
                              eventBusMock.Object, gameSettings, photonWrapperMock.Object);
    }

    [Test]
    public void TakeLoanConfirmed_ShouldSetPlayerLoan_CallBank_AddEvents()
    {
        photonWrapperMock.Setup(p => p.IsMasterClient).Returns(true);

        loanService.TakeLoanConfirmed(1);

        Assert.IsTrue(player.HasLoan);
        Assert.AreEqual(1, player.LoanTurnsLeft);

        bankServiceMock.Verify(b => b.AddMoney(1, gameSettings.loanAmount), Times.Once);
        eventBusMock.Verify(e => e.Publish(It.IsAny<OnTakeLoanEvent>()), Times.Once);
        eventBusMock.Verify(e => e.Publish(It.IsAny<OnUpdatePlayerMoneyEvent>()), Times.Once);
    }

    [Test]
    public void PayLoanConfirmed_ShouldResetPlayerLoan_CallBank_RemoveEvents()
    {
        photonWrapperMock.Setup(p => p.IsMasterClient).Returns(true);
        player.HasLoan = true;
        player.LoanTurnsLeft = 1;

        loanService.PayLoanConfirmed(1);

        Assert.IsFalse(player.HasLoan);
        Assert.AreEqual(0, player.LoanTurnsLeft);

        bankServiceMock.Verify(b => b.RemoveMoney(1, gameSettings.loanAmountBack), Times.Once);
        eventBusMock.Verify(e => e.Publish(It.IsAny<OnTakeLoanEvent>()), Times.Once);
        eventBusMock.Verify(e => e.Publish(It.IsAny<OnUpdatePlayerMoneyEvent>()), Times.Once);
    }

    [Test]
    public void OnPlayerTurnStart_ShouldDecrementTurns_AndShowWindowIfZero()
    {
        player.HasLoan = true;
        player.LoanTurnsLeft = 1;

        var method = typeof(LoanService).GetMethod("OnPlayerTurnStart", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        method.Invoke(loanService, new object[] { new OnStartTurnLoanEvent(1) });

        Assert.AreEqual(0, player.LoanTurnsLeft);
        photonLoanManagerMock.Verify(p => p.ShowLoanWindow(1, gameSettings.loanAmountBack), Times.Once);
    }

    [Test]
    public void OnPlayerTurnStart_ShouldPublishEventIfTurnsRemain()
    {
        player.HasLoan = true;
        player.LoanTurnsLeft = 2;

        var method = typeof(LoanService).GetMethod("OnPlayerTurnStart", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        method.Invoke(loanService, new object[] { new OnStartTurnLoanEvent(1) });

        Assert.AreEqual(1, player.LoanTurnsLeft);
        eventBusMock.Verify(e => e.Publish(It.IsAny<OnTakeLoanEvent>()), Times.Once);
        photonLoanManagerMock.Verify(p => p.ShowLoanWindow(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
    }

    [Test]
    public void Initialize_ShouldSubscribeEvent()
    {
       loanService.Initialize();
        eventBusMock.Verify(e => e.Subscribe<OnStartTurnLoanEvent>(It.IsAny<Action<OnStartTurnLoanEvent>>()), Times.Once);
    }

    [Test]
    public void Dispose_ShouldUnsubscribeEvent()
    {
        loanService.Dispose();
        eventBusMock.Verify(e => e.Unsubscribe<OnStartTurnLoanEvent>(It.IsAny<Action<OnStartTurnLoanEvent>>()), Times.Once);
    }
}
