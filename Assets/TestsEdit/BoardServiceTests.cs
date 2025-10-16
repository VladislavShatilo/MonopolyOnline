
using Moq;
using NUnit.Framework;
using System;
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
        UnityEngine.Object.DestroyImmediate(parent.gameObject);
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
    [Test]
    public void InitializeBoard_WithEmptyParent_ShouldHaveZeroCells()
    {
        // Arrange
        var emptyParent = new GameObject("EmptyParent").transform;
        var emptyService = new BoardService();
        emptyService.Construct(mockRepository.Object, emptyParent);

        // Act
        emptyService.InitializeBoard();

        // Assert
        Assert.AreEqual(0, emptyService.CellsCount);

        UnityEngine.Object.DestroyImmediate(emptyParent.gameObject);
    }

    [Test]
    public void InitializeBoard_CalledTwice_ShouldNotDuplicateCells()
    {
        // Act
        service.InitializeBoard();
        service.InitializeBoard();

        // Assert
        Assert.AreEqual(3, service.CellsCount);
    }

    [Test]
    public void GetCellRectTransform_WhenChildHasNoRectTransform_ShouldReturnNull()
    {
        // Arrange
        var go = new GameObject("NoRectTransform");
        go.transform.SetParent(parent);
        var testService = new BoardService();
        testService.Construct(mockRepository.Object, parent);
        testService.InitializeBoard();

        // Act
        var rect = testService.GetCellRectTransform(3); // index нового объекта без RectTransform

        // Assert
        Assert.IsNull(rect);

        UnityEngine.Object.DestroyImmediate(go);
    }

    [Test]
    public void GetCellRectTransform_AtBoundaryIndices_ShouldReturnCorrectTransform()
    {
        service.InitializeBoard();

        // Act & Assert
        var first = service.GetCellRectTransform(0);
        var last = service.GetCellRectTransform(service.CellsCount - 1);

        Assert.NotNull(first);
        Assert.NotNull(last);
        Assert.AreEqual("Cell0", first.gameObject.name);
        Assert.AreEqual("Cell2", last.gameObject.name);
    }
    [Test]
    public void Construct_WithNullRepository_ShouldThrow()
    {
        var service = new BoardService();
        Assert.Throws<ArgumentNullException>(() => service.Construct(null, parent));
    }

    [Test]
    public void Construct_WithNullParent_ShouldThrow()
    {
        var service = new BoardService();
        Assert.Throws<ArgumentNullException>(() => service.Construct(mockRepository.Object, null));
    }

    [Test]
    public void InitializeBoard_WithNullChild_ShouldIgnoreNull()
    {

        var child1 = new GameObject("Child1").transform;
        var child3 = new GameObject("Child3").transform;

        child1.SetParent(parent);
        child3.SetParent(parent);

        var service = new BoardService();
        service.Construct(mockRepository.Object, parent);

        // Act
        service.InitializeBoard();

        // Assert
        Assert.AreEqual(5, service.CellsCount); // учитываются только реальные трансформы
    }

    [Test]
    public void GetCellRectTransform_ForChildWithoutRectTransform_ShouldReturnNull()
    {
        var go = new GameObject("NoRect");
        go.transform.SetParent(parent);

        var service = new BoardService();
        service.Construct(mockRepository.Object, parent);
        service.InitializeBoard();

        var rect = service.GetCellRectTransform(service.CellsCount - 1); // последний объект без RectTransform
        Assert.IsNull(rect);

        UnityEngine.Object.DestroyImmediate(go);
    }

}