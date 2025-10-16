using NUnit.Framework;
using Moq;
using System;
using UnityEngine;

public class GameBootstrapperTests
{
    private GameObject go;
    private GameBootstrapper bootstrapper;

    private Mock<GameManager> gameManagerMock;
    private Mock<IBoardService> boardServiceMock;
    private Mock<ICompanyUIService> companyUIMock;
    private Mock<ICellOccupancyService> cellOccupancyMock;
    private Mock<PlayerStatsService> playerStatsMock;

    [SetUp]
    public void Setup()
    {
        go = new GameObject();
        bootstrapper = go.AddComponent<GameBootstrapper>();

        gameManagerMock = new Mock<GameManager>();
        boardServiceMock = new Mock<IBoardService>();
        companyUIMock = new Mock<ICompanyUIService>();
        cellOccupancyMock = new Mock<ICellOccupancyService>();
        playerStatsMock = new Mock<PlayerStatsService>();

        bootstrapper.Construct(gameManagerMock.Object, boardServiceMock.Object, companyUIMock.Object,
            cellOccupancyMock.Object, playerStatsMock.Object);
    }

    #region Construct Tests

    [Test]
    public void Construct_ShouldThrow_WhenGameManagerIsNull()
    {
        var ex = Assert.Throws<ArgumentNullException>(() =>
        {
            var goTest = new GameObject();
            var comp = goTest.AddComponent<GameBootstrapper>();
            comp.Construct(null, boardServiceMock.Object, companyUIMock.Object, cellOccupancyMock.Object, playerStatsMock.Object);
        });
        Assert.That(ex.ParamName, Is.EqualTo("gameManager"));
    }

    [Test]
    public void Construct_ShouldThrow_WhenBoardServiceIsNull()
    {
        var ex = Assert.Throws<ArgumentNullException>(() =>
        {
            var goTest = new GameObject();
            var comp = goTest.AddComponent<GameBootstrapper>();
            comp.Construct(gameManagerMock.Object, null, companyUIMock.Object, cellOccupancyMock.Object, playerStatsMock.Object);
        });
        Assert.That(ex.ParamName, Is.EqualTo("boardService"));
    }

    // Аналогично для остальных зависимостей...
    [Test]
    public void Construct_ShouldThrow_WhenCompanyUIServiceIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => bootstrapper.Construct(
            gameManagerMock.Object, boardServiceMock.Object, null, cellOccupancyMock.Object, playerStatsMock.Object));
    }

    [Test]
    public void Construct_ShouldThrow_WhenCellOccupancyServiceIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => bootstrapper.Construct(
            gameManagerMock.Object, boardServiceMock.Object, companyUIMock.Object, null, playerStatsMock.Object));
    }

    [Test]
    public void Construct_ShouldThrow_WhenPlayerStatsServiceIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => bootstrapper.Construct(
            gameManagerMock.Object, boardServiceMock.Object, companyUIMock.Object, cellOccupancyMock.Object, null));
    }

    #endregion

    #region Awake Tests

    //[Test]
    //public void Awake_ShouldCallInitializeOnAllServices()
    //{
    //    bootstrapper.Awake();

    //    boardServiceMock.Verify(b => b.InitializeBoard(), Times.Once);
    //    companyUIMock.Verify(c => c.InitializeUI(), Times.Once);
    //    cellOccupancyMock.Verify(c => c.InitializePlayer(), Times.Once);
    //    playerStatsMock.Verify(p => p.Initialize(), Times.Once);
    //    gameManagerMock.Verify(g => g.Initialize(), Times.Once);
    //}

    #endregion
}
