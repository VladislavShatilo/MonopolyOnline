using Moq;
using NUnit.Framework;
using UnityEngine;

public class GameBootstrapperTests
{
    private GameBootstrapper bootstrapper;
    private Mock<GameManager> mockGameManager;
    private Mock<IBoardService> mockBoardService;
    private Mock<ICompanyUIService> mockCompanyUIService;
    private Mock<ICellOccupancyService> mockCellOccupancyService;
    private FakePlayerStatsService fakePlayerStatsService;

    [SetUp]
    public void SetUp()
    {
        var go = new GameObject();
        bootstrapper = go.AddComponent<GameBootstrapper>();

        mockGameManager = new Mock<GameManager>();
        mockBoardService = new Mock<IBoardService>();
        mockCompanyUIService = new Mock<ICompanyUIService>();
        mockCellOccupancyService = new Mock<ICellOccupancyService>();
        fakePlayerStatsService = new FakePlayerStatsService();

        bootstrapper.Construct(
            mockGameManager.Object,
            mockBoardService.Object,
            mockCompanyUIService.Object,
            mockCellOccupancyService.Object,
            fakePlayerStatsService
        );
    }

    [Test]
    public void Awake_ShouldInitializeAllServices()
    {
        bootstrapper.SendMessage("Awake");

        mockBoardService.Verify(b => b.InitializeBoard(), Times.Once);
        mockCompanyUIService.Verify(u => u.InitializeUI(), Times.Once);
        mockCellOccupancyService.Verify(c => c.InitializePlayer(), Times.Once);
        mockGameManager.Verify(g => g.Initialize(), Times.Once);

        Assert.IsTrue(fakePlayerStatsService.Initialized);
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(bootstrapper.gameObject);
    }

    private class FakePlayerStatsService : PlayerStatsService
    {
        public bool Initialized { get; private set; }

        public new void Initialize()
        {
            Initialized = true;
        }
    }
}
