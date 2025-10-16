using NUnit.Framework;
using Moq;
using System;

public class PlayerMoveUseCaseTests
{
    private Mock<IPlayerRepository> mockPlayerRepository;
    private Mock<IBoardService> mockBoardService;
    private Mock<IEventBus> mockEventBus;
    private Mock<ICellHighlighterService> mockCellHighlighterService;

    private PlayerMoveUseCase playerMoveUseCase;
    private PlayerData testPlayer;

    [SetUp]
    public void SetUp()
    {
        mockPlayerRepository = new Mock<IPlayerRepository>();
        mockBoardService = new Mock<IBoardService>();
        mockEventBus = new Mock<IEventBus>();
        mockCellHighlighterService = new Mock<ICellHighlighterService>();

        testPlayer = new PlayerData("Alice", 1000, 1, new PlayerColor(255,0,0));
        testPlayer.CurrentCellId = 5;

        mockPlayerRepository.Setup(r => r.GetPlayerById(1)).Returns(testPlayer);
        mockBoardService.Setup(b => b.CellsCount).Returns(10);

        playerMoveUseCase = new PlayerMoveUseCase();
        playerMoveUseCase.Construct(
            mockPlayerRepository.Object,
            mockBoardService.Object,
            mockEventBus.Object,
            mockCellHighlighterService.Object
        );
    }

    #region LIFE_CYCLE

    [Test]
    public void Initialize_ShouldSubscribeToDiceFadeEvent()
    {
        // Act
        playerMoveUseCase.Initialize();

        // Assert
        mockEventBus.Verify(bus => bus.Subscribe<DiceFadeEvent>(It.IsAny<Action<DiceFadeEvent>>()), Times.Once);
    }

    [Test]
    public void Dispose_ShouldUnsubscribeFromDiceFadeEvent()
    {
        // Arrange
        playerMoveUseCase.Initialize();

        // Act
        playerMoveUseCase.Dispose();

        // Assert
        mockEventBus.Verify(bus => bus.Unsubscribe<DiceFadeEvent>(It.IsAny<Action<DiceFadeEvent>>()), Times.Once);
    }

    #endregion

    #region MovePlayer

    [Test]
    public void MovePlayer_ShouldMoveForwardAndWrapAroundBoard()
    {
        // Arrange
        int steps = 7; // 5 + 7 = 12 => 12 % 10 = 2
        int expectedTargetCell = 2;

        // Act
        playerMoveUseCase.MovePlayer(1, steps, true);

        // Assert
        Assert.AreEqual(expectedTargetCell, testPlayer.CurrentCellId);
        Assert.AreEqual(steps, testPlayer.LastDiceSum);
        mockEventBus.Verify(bus =>
            bus.Publish(It.Is<MovePlayerEvent>(e =>
                e.PlayerId == 1 &&
                e.CurrentCellIndex == 5 &&
                e.TargetIndex == expectedTargetCell &&
                e.IsForward == true
            )), Times.Once);
    }

    [Test]
    public void MovePlayer_ShouldMoveBackwardAndWrapAroundBoard()
    {
        // Arrange
        int steps = 7; // 5 - 7 + 10 = 8
        int expectedTargetCell = 8;

        // Act
        playerMoveUseCase.MovePlayer(1, steps, false);

        // Assert
        Assert.AreEqual(expectedTargetCell, testPlayer.CurrentCellId);
        Assert.AreEqual(steps, testPlayer.LastDiceSum);
        mockEventBus.Verify(bus =>
            bus.Publish(It.Is<MovePlayerEvent>(e =>
                e.PlayerId == 1 &&
                e.TargetIndex == expectedTargetCell &&
                e.IsForward == false
            )), Times.Once);
    }

    #endregion

    #region TeleportPlayer

    [Test]
    public void TeleportPlayer_ShouldCallMovePlayerWithCorrectSteps_WhenTargetAhead()
    {
        // Arrange
        testPlayer.CurrentCellId = 3;
        mockBoardService.Setup(b => b.CellsCount).Returns(10);

        // Act
        playerMoveUseCase.TeleportPlayer(1, randomIndex: 8, currentCellIndex: 3);

        // Assert
        mockPlayerRepository.Verify(r => r.GetPlayerById(1), Times.AtLeastOnce);
        mockEventBus.Verify(bus => bus.Publish(It.IsAny<MovePlayerEvent>()), Times.Once);
        Assert.AreEqual(8, testPlayer.CurrentCellId);
    }

    [Test]
    public void TeleportPlayer_ShouldHandleWrapAround_WhenTargetBehind()
    {
        // Arrange
        testPlayer.CurrentCellId = 8; // позади
        mockBoardService.Setup(b => b.CellsCount).Returns(10);

        // Act
        playerMoveUseCase.TeleportPlayer(1, randomIndex: 2, currentCellIndex: 8);

        // Assert
        // шаги = 2 + 10 - 8 = 4
        Assert.AreEqual((8 + 4) % 10, testPlayer.CurrentCellId);
    }

    #endregion

    #region HighlightCell

    [Test]
    public void HighlightCell_ShouldShowHighlight_WhenMovementStarts()
    {
        // Arrange
        var diceEvent = new DiceFadeEvent(isMovementStart: true, cellId: 4);

        // Act
        var method = typeof(PlayerMoveUseCase).GetMethod("HighlightCell",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        method.Invoke(playerMoveUseCase, new object[] { diceEvent });

        // Assert
        mockCellHighlighterService.Verify(s => s.ShowHighlight(4), Times.Once);
        mockCellHighlighterService.Verify(s => s.HideHighlight(), Times.Never);
    }

    [Test]
    public void HighlightCell_ShouldHideHighlight_WhenMovementEnds()
    {
        // Arrange
        var diceEvent = new DiceFadeEvent(isMovementStart: false, cellId: 4);

        // Act
        var method = typeof(PlayerMoveUseCase).GetMethod("HighlightCell",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        method.Invoke(playerMoveUseCase, new object[] { diceEvent });

        // Assert
        mockCellHighlighterService.Verify(s => s.HideHighlight(), Times.Once);
        mockCellHighlighterService.Verify(s => s.ShowHighlight(It.IsAny<int>()), Times.Never);
    }
    [Test]
    public void MovePlayer_ShouldThrow_WhenPlayerNotFound()
    {
        mockPlayerRepository.Setup(r => r.GetPlayerById(99)).Returns((PlayerData)null);

        Assert.Throws<NullReferenceException>(() => playerMoveUseCase.MovePlayer(99, 3, true));
    }
    [Test]
    public void TeleportPlayer_ShouldHandleSameCellIndex()
    {
        testPlayer.CurrentCellId = 4;
        playerMoveUseCase.TeleportPlayer(1, randomIndex: 4, currentCellIndex: 4);

        Assert.AreEqual(4, testPlayer.CurrentCellId);
        mockEventBus.Verify(bus => bus.Publish(It.Is<MovePlayerEvent>(e => e.PlayerId == 1 && e.TargetIndex == 4)), Times.Once);
    }
    [Test]
    public void MovePlayer_ShouldPublishEvent_WhenStepsZero()
    {
        playerMoveUseCase.MovePlayer(1, 0, true);

        Assert.AreEqual(5, testPlayer.CurrentCellId);
        mockEventBus.Verify(bus => bus.Publish(It.IsAny<MovePlayerEvent>()), Times.Once);
    }

    #endregion
}
