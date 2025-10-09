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

    [Test]
    public void Initialize_ShouldSubscribeToDiceRolledEvent()
    {
        // Act
        presenter.Initialize();

        // Assert
        eventBusMock.Verify(e => e.Subscribe<DiceRolledEvent>(It.IsAny<Action<DiceRolledEvent>>()), Times.Once);
    }

    [Test]
    public void Dispose_ShouldUnsubscribeFromDiceRolledEvent()
    {
        // Act
        presenter.Dispose();

        // Assert
        eventBusMock.Verify(e => e.Unsubscribe<DiceRolledEvent>(It.IsAny<Action<DiceRolledEvent>>()), Times.Once);
    }

    [Test]
    public void OnDiceRolled_ShouldCallShowDiceOnManager()
    {
        // Arrange
        var diceResult = new DiceResult(3,5);
        var diceEvent = new DiceRolledEvent(diceResult,1,false);

        // Act
        // Через отражение вызовем приватный метод OnDiceRolled
        var method = typeof(DicePresenter).GetMethod("OnDiceRolled", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        method.Invoke(presenter, new object[] { diceEvent });

        // Assert
        diceManagerMock.Verify(d => d.ShowDice(3, 5), Times.Once);
    }
}
