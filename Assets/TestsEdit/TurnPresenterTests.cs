using Moq;
using NUnit.Framework;
using System;

[TestFixture]
public class TurnPresenterTests
{
    private TurnPresenter turnPresenter;
    private Mock<ILocalPlayerService> localPlayerMock;
    private Mock<IPhotonDiceManager> diceManagerMock;
    private Mock<ITurnWindow> uiTurnWindowMock;
    private Mock<IEventBus> eventBusMock;

    [SetUp]
    public void SetUp()
    {
        localPlayerMock = new Mock<ILocalPlayerService>();
        diceManagerMock = new Mock<IPhotonDiceManager>();
        uiTurnWindowMock = new Mock<ITurnWindow>();
        eventBusMock = new Mock<IEventBus>();

        turnPresenter = new TurnPresenter();
        turnPresenter.Construct(localPlayerMock.Object, uiTurnWindowMock.Object, diceManagerMock.Object, eventBusMock.Object);
        turnPresenter.Initialize();
    }

    [TearDown]
    public void TearDown()
    {
        turnPresenter.Dispose();
    }

    [Test]
    public void ShowTurnFor_ShouldShowWindow_WhenLocalPlayer()
    {
        localPlayerMock.Setup(p => p.GetLocalPlayerId()).Returns(1);

        turnPresenter.ShowTurnFor(1);

        uiTurnWindowMock.Verify(u => u.Show(), Times.Once);
        uiTurnWindowMock.Verify(u => u.Hide(), Times.Never);
    }

    [Test]
    public void ShowTurnFor_ShouldHideWindow_WhenOtherPlayer()
    {
        localPlayerMock.Setup(p => p.GetLocalPlayerId()).Returns(1);

        turnPresenter.ShowTurnFor(2);

        uiTurnWindowMock.Verify(u => u.Hide(), Times.Once);
        uiTurnWindowMock.Verify(u => u.Show(), Times.Never);
    }

    [Test]
    public void HideTurn_ShouldCallHideOnUI()
    {
        turnPresenter.HideTurn();

        uiTurnWindowMock.Verify(u => u.Hide(), Times.Once);
    }

    [Test]
    public void OnTurnStart_ShouldShowWindow_WhenLocalPlayer()
    {
        localPlayerMock.Setup(p => p.GetLocalPlayerId()).Returns(1);

        var e = new TurnStartEvent(1);

        // Вызов приватного метода через reflection
        var method = typeof(TurnPresenter).GetMethod("OnTurnStart",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        method.Invoke(turnPresenter, new object[] { e });

        uiTurnWindowMock.Verify(u => u.Show(), Times.Once);
        uiTurnWindowMock.Verify(u => u.Hide(), Times.Never);

    }

    [Test]
    public void OnTurnStart_ShouldHideWindow_WhenOtherPlayer()
    {
        localPlayerMock.Setup(p => p.GetLocalPlayerId()).Returns(1);

        var e = new TurnStartEvent (2);

        var method = typeof(TurnPresenter).GetMethod("OnTurnStart",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        method.Invoke(turnPresenter, new object[] { e });

        uiTurnWindowMock.Verify(u => u.Hide(), Times.Once);
        uiTurnWindowMock.Verify(u => u.Show(), Times.Never);
    }

    [Test]
    public void OnThrowDiceClicked_ShouldRequestDiceRollAndHideUI()
    {
        localPlayerMock.Setup(p => p.GetLocalPlayerId()).Returns(1);
        uiTurnWindowMock.Setup(u => u.GetSteps1()).Returns(3);
        uiTurnWindowMock.Setup(u => u.GetSteps2()).Returns(4);

        // Получаем Action, установленное через SetThrowDiceAction
        uiTurnWindowMock.Verify(u => u.SetThrowDiceAction(It.IsAny<System.Action>()), Times.Once);
        var throwAction = new System.Action(() =>
        {
            turnPresenter.GetType().GetMethod("OnThrowDiceClicked", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .Invoke(turnPresenter, null);
        });

        // Вызов приватного метода через отражение
        throwAction.Invoke();

        diceManagerMock.Verify(d => d.RequestDiceRoll(1, false, 3, 4), Times.Once);
        uiTurnWindowMock.Verify(u => u.Hide(), Times.Once);
    }
    [Test]
    public void Dispose_ShouldUnsubscribeFromTurnStartEvent()
    {
        turnPresenter.Dispose();
        eventBusMock.Verify(e => e.Unsubscribe<TurnStartEvent>(It.IsAny<Action<TurnStartEvent>>()), Times.Once);
    }
    [Test]
    public void Construct_ShouldThrowArgumentNullException_WhenDependenciesNull()
    {
        Assert.Throws<ArgumentNullException>(() => new TurnPresenter().Construct(null, uiTurnWindowMock.Object, diceManagerMock.Object, eventBusMock.Object));
        Assert.Throws<ArgumentNullException>(() => new TurnPresenter().Construct(localPlayerMock.Object, null, diceManagerMock.Object, eventBusMock.Object));
        Assert.Throws<ArgumentNullException>(() => new TurnPresenter().Construct(localPlayerMock.Object, uiTurnWindowMock.Object, null, eventBusMock.Object));
        Assert.Throws<ArgumentNullException>(() => new TurnPresenter().Construct(localPlayerMock.Object, uiTurnWindowMock.Object, diceManagerMock.Object, null));
    }
    [Test]
    public void OnThrowDiceClicked_ShouldHandleZeroSteps()
    {
        uiTurnWindowMock.Setup(u => u.GetSteps1()).Returns(0);
        uiTurnWindowMock.Setup(u => u.GetSteps2()).Returns(0);

        var method = typeof(TurnPresenter).GetMethod("OnThrowDiceClicked", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        method.Invoke(turnPresenter, null);

        diceManagerMock.Verify(d => d.RequestDiceRoll(localPlayerMock.Object.GetLocalPlayerId(), false, 0, 0), Times.Once);
        uiTurnWindowMock.Verify(u => u.Hide(), Times.Once);
    }

}
