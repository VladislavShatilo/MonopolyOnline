using Moq;
using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class BoardServiceTests
{
    private BoardService service;
    private Mock<IBoardRepository> mockRepository;
    private Transform parent;

    [SetUp]
    public void SetUp()
    {
        mockRepository = new Mock<IBoardRepository>();

        // Создаем GameObject как родителя и несколько дочерних
        parent = new GameObject("BoardParent").transform;
        for (int i = 0; i < 3; i++)
        {
            var cell = new GameObject($"Cell{i}");
            cell.AddComponent<RectTransform>();
            cell.transform.SetParent(parent);
        }

        service = new BoardService();
        service.Construct(mockRepository.Object, parent);
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(parent.gameObject);
    }

    [Test]
    public void InitializeBoard_ShouldCacheAllChildren()
    {
        // Act
        service.InitializeBoard();

        // Assert
        Assert.AreEqual(3, service.CellsCount);
    }

    [Test]
    public void GetCellRectTransform_ShouldReturnCorrectTransform_WhenIndexIsValid()
    {
        service.InitializeBoard();

        var rect = service.GetCellRectTransform(1);

        Assert.NotNull(rect);
        Assert.AreEqual("Cell1", rect.gameObject.name);
    }

    [Test]
    public void GetCellRectTransform_ShouldReturnNull_WhenIndexIsInvalid()
    {
        service.InitializeBoard();

        Assert.IsNull(service.GetCellRectTransform(-1));
        Assert.IsNull(service.GetCellRectTransform(10));
    }

    [Test]
    public void GetCellGameObject_ShouldReturnCorrectGameObject_WhenIndexIsValid()
    {
        service.InitializeBoard();

        var go = service.GetCellGameObject(2);

        Assert.NotNull(go);
        Assert.AreEqual("Cell2", go.name);
    }

    [Test]
    public void GetCellGameObject_ShouldReturnNull_WhenIndexIsInvalid()
    {
        service.InitializeBoard();

        Assert.IsNull(service.GetCellGameObject(5));
    }

    [Test]
    public void GetCellData_ShouldReturnDataFromRepository()
    {
        var expectedCell = new CellData();
        expectedCell.index = 42;

        mockRepository.Setup(r => r.GetCell(42)).Returns(expectedCell);

        var result = service.GetCellData(42);

        Assert.AreEqual(expectedCell, result);
        mockRepository.Verify(r => r.GetCell(42), Times.Once);
    }

    [Test]
    public void GetAllCellData_ShouldReturnAllFromRepository()
    {
        var cellData1 = new CellData();
        cellData1.index = 1;

        var cellData2 = new CellData();
        cellData2.index = 2;
        var allCells = new List<CellData>
        {
          cellData1,
          cellData2
        };

        mockRepository.Setup(r => r.GetAllCells()).Returns(allCells);

        var result = service.GetAllCellData();

        Assert.AreEqual(2, result.Count);
        Assert.AreEqual(1, result[0].index);
    }

    [Test]
    public void CellsCount_ShouldBeZero_BeforeInitialization()
    {
        Assert.AreEqual(0, service.CellsCount);
    }
}