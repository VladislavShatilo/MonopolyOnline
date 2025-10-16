using NUnit.Framework;
using Moq;
using System;

public class DicePresenterTests
{
    private DicePresenter presenter;
    private Mock<IDiceManager3D> diceManagerMock;
    private Mock<IEventBus> eventBusMock;

    [SetUp]
    public void Setup()
    {
        diceManagerMock = new Mock<IDiceManager3D>();
        eventBusMock = new Mock<IEventBus>();

        presenter = new DicePresenter();
        presenter.Construct(diceManagerMock.Object, eventBusMock.Object);
    }

    #region Construct Tests

    [Test]
    public void Construct_ShouldThrow_WhenDiceManagerIsNull()
    {
        var ex = Assert.Throws<ArgumentNullException>(() =>
        {
            var p = new DicePresenter();
            p.Construct(null, eventBusMock.Object);
        });
        Assert.That(ex.ParamName, Is.EqualTo("diceManager3D"));
    }

    [Test]
    public void Construct_ShouldThrow_WhenEventBusIsNull()
    {
        var ex = Assert.Throws<ArgumentNullException>(() =>
        {
            var p = new DicePresenter();
            p.Construct(diceManagerMock.Object, null);
        });
        Assert.That(ex.ParamName, Is.EqualTo("eventBus"));
    }

    #endregion

    #region Initialize & Dispose Tests

    [Test]
    public void Initialize_ShouldSubscribeToDiceRolledEvent()
    {
        presenter.Initialize();
        eventBusMock.Verify(e => e.Subscribe<DiceRolledEvent>(It.IsAny<Action<DiceRolledEvent>>()), Times.Once);
    }

    [Test]
    public void Dispose_ShouldUnsubscribeFromDiceRolledEvent()
    {
        presenter.Dispose();
        eventBusMock.Verify(e => e.Unsubscribe<DiceRolledEvent>(It.IsAny<Action<DiceRolledEvent>>()), Times.Once);
    }

    [Test]
    public void Initialize_CalledTwice_ShouldSubscribeTwice()
    {
        presenter.Initialize();
        presenter.Initialize();
        eventBusMock.Verify(e => e.Subscribe<DiceRolledEvent>(It.IsAny<Action<DiceRolledEvent>>()), Times.Exactly(2));
    }

    [Test]
    public void Dispose_CalledTwice_ShouldUnsubscribeTwice()
    {
        presenter.Dispose();
        presenter.Dispose();
        eventBusMock.Verify(e => e.Unsubscribe<DiceRolledEvent>(It.IsAny<Action<DiceRolledEvent>>()), Times.Exactly(2));
    }

    #endregion

    #region OnDiceRolled Tests

    [Test]
    public void OnDiceRolled_ShouldCallShowDiceOnManager()
    {
        var diceResult = new DiceResult(3, 5);
        var diceEvent = new DiceRolledEvent(diceResult, 1, false);

        var method = typeof(DicePresenter).GetMethod("OnDiceRolled",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        method.Invoke(presenter, new object[] { diceEvent });

        diceManagerMock.Verify(d => d.ShowDice(3, 5), Times.Once);
    }

    [Test]
    public void OnDiceRolled_ShouldBeTriggeredByEvent()
    {
        Action<DiceRolledEvent> capturedCallback = null;
        eventBusMock.Setup(e => e.Subscribe<DiceRolledEvent>(It.IsAny<Action<DiceRolledEvent>>()))
            .Callback<Action<DiceRolledEvent>>(cb => capturedCallback = cb);

        presenter.Initialize();

        var diceResult = new DiceResult(6, 2);
        var diceEvent = new DiceRolledEvent(diceResult, 1, false);

        // Симулируем событие
        capturedCallback?.Invoke(diceEvent);

        diceManagerMock.Verify(d => d.ShowDice(6, 2), Times.Once);
    }

    #endregion
}
