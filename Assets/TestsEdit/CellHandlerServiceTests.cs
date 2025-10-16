using NUnit.Framework;
using Moq;
using System;
using UnityEngine;

public class CellHandlerServiceTests
{
    private CellHandlerService service;
    private Mock<IBoardService> mockBoardService;
    private Mock<ICompanyService> mockCompanyService;
    private Mock<IPhotonChanceManager> mockChanceManager;
    private Mock<IPhotonJailManager> mockJailManager;
    private Mock<IPhotonTurnManager> mockTurnManager;
    private Mock<IEventBus> mockEventBus;
    private Mock<IPhotonNetworkWrapper> mockPhotonNetwork;

    [SetUp]
    public void SetUp()
    {
        mockBoardService = new Mock<IBoardService>();
        mockCompanyService = new Mock<ICompanyService>();
        mockChanceManager = new Mock<IPhotonChanceManager>();
        mockJailManager = new Mock<IPhotonJailManager>();
        mockTurnManager = new Mock<IPhotonTurnManager>();
        mockEventBus = new Mock<IEventBus>();
        mockPhotonNetwork = new Mock<IPhotonNetworkWrapper>();

        service = new CellHandlerService();
        service.Construct(
            mockBoardService.Object,
            mockCompanyService.Object,
            mockChanceManager.Object,
            mockJailManager.Object,
            mockTurnManager.Object,
            mockEventBus.Object,
            mockPhotonNetwork.Object
        );
    }

    [Test]
    public void Initialize_ShouldSubscribeToHandleCellEvent()
    {
        // Act
        service.Initialize();

        // Assert
        mockEventBus.Verify(e => e.Subscribe(It.IsAny<Action<HandleCellEvent>>()), Times.Once);
    }

    [Test]
    public void Dispose_ShouldUnsubscribeFromHandleCellEvent()
    {
        // Act
        service.Dispose();

        // Assert
        mockEventBus.Verify(e => e.Unsubscribe(It.IsAny<Action<HandleCellEvent>>()), Times.Once);
    }

    [Test]
    public void OnHandleCell_ShouldDoNothing_WhenCellIndexOutOfRange()
    {
        mockBoardService.Setup(b => b.CellsCount).Returns(5);

        var e = new HandleCellEvent(10, 1);

        service.OnHandleCell(e);

        mockBoardService.Verify(b => b.GetCellData(It.IsAny<int>()), Times.Never);
        mockCompanyService.Verify(c => c.HandleCell(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
        mockChanceManager.Verify(c => c.GiveRandomBuff(It.IsAny<int>()), Times.Never);
        mockTurnManager.Verify(t => t.RequestEndTurn(), Times.Never);
    }

    [TestCase(CellType.Company)]
    [TestCase(CellType.FieldCompany)]
    [TestCase(CellType.DiceCompany)]
    public void OnHandleCell_ShouldHandleCompanyCells(CellType type)
    {
        mockBoardService.Setup(b => b.CellsCount).Returns(10);
        mockBoardService.Setup(b => b.GetCellData(2)).Returns(new CellData { cellType = type });

        var e = new HandleCellEvent(2, 42);

        service.OnHandleCell(e);

        mockCompanyService.Verify(c => c.HandleCell(2, 42), Times.Once);
    }

    [TestCase(CellType.Question)]
    [TestCase(CellType.Spend)]
    public void OnHandleCell_ShouldHandleChanceCells(CellType type)
    {
        mockBoardService.Setup(b => b.CellsCount).Returns(10);
        mockBoardService.Setup(b => b.GetCellData(1)).Returns(new CellData { cellType = type });

        var e = new HandleCellEvent(1, 5);

        service.OnHandleCell(e);

        mockChanceManager.Verify(c => c.GiveRandomBuff(5), Times.Once);
    }

    [TestCase(CornerType.Start)]
    [TestCase(CornerType.ChillJail)]
    public void OnHandleCell_ShouldEndTurn_ForStartAndChillJailCorners_WhenMasterClient(CornerType cornerType)
    {
        mockBoardService.Setup(b => b.CellsCount).Returns(10);
        mockBoardService.Setup(b => b.GetCellData(3)).Returns(new CellData
        {
            cellType = CellType.Corner,
            cornerData = new CornerData { type = cornerType }
        });

        mockPhotonNetwork.Setup(p => p.IsMasterClient).Returns(true);

        var e = new HandleCellEvent(3, 7);

        service.OnHandleCell(e);

        mockTurnManager.Verify(t => t.RequestEndTurn(), Times.Once);
    }

    [Test]
    public void OnHandleCell_ShouldNotEndTurn_WhenNotMasterClient()
    {
        mockBoardService.Setup(b => b.CellsCount).Returns(10);
        mockBoardService.Setup(b => b.GetCellData(3)).Returns(new CellData
        {
            cellType = CellType.Corner,
            cornerData = new CornerData { type = CornerType.Start }
        });

        mockPhotonNetwork.Setup(p => p.IsMasterClient).Returns(false);

        var e = new HandleCellEvent(3, 7);

        service.OnHandleCell(e);

        mockTurnManager.Verify(t => t.RequestEndTurn(), Times.Never);
    }

    [Test]
    public void OnHandleCell_ShouldSendToJail_WhenCornerTypeIsPolice()
    {
        mockBoardService.Setup(b => b.CellsCount).Returns(10);
        mockBoardService.Setup(b => b.GetCellData(4)).Returns(new CellData
        {
            cellType = CellType.Corner,
            cornerData = new CornerData { type = CornerType.Police }
        });

        var e = new HandleCellEvent(4, 8);

        service.OnHandleCell(e);

        mockJailManager.Verify(j => j.SendToJail(8), Times.Once);
    }

    [Test]
    public void OnHandleCell_ShouldEndTurn_WhenCornerTypeIsCasinoAndMasterClient()
    {
        mockBoardService.Setup(b => b.CellsCount).Returns(10);
        mockBoardService.Setup(b => b.GetCellData(5)).Returns(new CellData
        {
            cellType = CellType.Corner,
            cornerData = new CornerData { type = CornerType.Caisno }
        });

        mockPhotonNetwork.Setup(p => p.IsMasterClient).Returns(true);

        var e = new HandleCellEvent(5, 10);

        service.OnHandleCell(e);

        mockTurnManager.Verify(t => t.RequestEndTurn(), Times.Once);
    }

    [Test]
    public void OnHandleCell_ShouldNotEndTurn_WhenCornerTypeIsCasinoAndNotMasterClient()
    {
        mockBoardService.Setup(b => b.CellsCount).Returns(10);
        mockBoardService.Setup(b => b.GetCellData(5)).Returns(new CellData
        {
            cellType = CellType.Corner,
            cornerData = new CornerData { type = CornerType.Caisno }
        });

        mockPhotonNetwork.Setup(p => p.IsMasterClient).Returns(false);

        var e = new HandleCellEvent(5, 10);

        service.OnHandleCell(e);

        mockTurnManager.Verify(t => t.RequestEndTurn(), Times.Never);
    }

    [Test]
    public void OnHandleCell_ShouldThrow_WhenCellDataIsNull()
    {
        mockBoardService.Setup(b => b.CellsCount).Returns(10);
        mockBoardService.Setup(b => b.GetCellData(3)).Returns((CellData)null);

        var e = new HandleCellEvent(3, 7);

        Assert.Throws<NullReferenceException>(() => service.OnHandleCell(e));
    }


}
