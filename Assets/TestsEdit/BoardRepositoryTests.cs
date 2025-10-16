using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;

[TestFixture]
public class BoardRepositoryTests
{
    private BoardRepository boardRepository;
    private BoardConfig config;
    private CellData cell1;
    private CellData cell2;

    [SetUp]
    public void Setup()
    {
        cell1 = new CellData();
        cell2 = new CellData();
        config = new BoardConfig { cells = new List<CellData> { cell1, cell2 } };

        boardRepository = new BoardRepository();
        boardRepository.Construct(config);
    }

    // ===== GetAllCells =====
    [Test]
    public void GetAllCells_ShouldReturnAllCells()
    {
        var cells = boardRepository.GetAllCells();

        Assert.AreEqual(2, cells.Count);
        Assert.AreEqual(cell1, cells[0]);
        Assert.AreEqual(cell2, cells[1]);
    }

    [Test]
    public void GetAllCells_ReturnedList_ShouldBeReadOnly()
    {
        var cells = boardRepository.GetAllCells();
        Assert.IsInstanceOf<IReadOnlyList<CellData>>(cells);
    }

    // ===== GetCell =====
    [Test]
    public void GetCell_ValidId_ShouldReturnCorrectCell()
    {
        Assert.AreEqual(cell1, boardRepository.GetCell(0));
        Assert.AreEqual(cell2, boardRepository.GetCell(1));
    }

    [Test]
    public void GetCell_InvalidId_ShouldReturnNull()
    {
        Assert.IsNull(boardRepository.GetCell(-1));
        Assert.IsNull(boardRepository.GetCell(2)); // index out of range
    }

    // ===== Construct =====
    [Test]
    public void Construct_WithEmptyList_ShouldCreateEmptyRepository()
    {
        var emptyConfig = new BoardConfig { cells = new List<CellData>() };
        var repo = new BoardRepository();
        repo.Construct(emptyConfig);

        var cells = repo.GetAllCells();
        Assert.IsNotNull(cells);
        Assert.AreEqual(0, cells.Count);
    }

    [Test]
    public void Construct_WithNullList_ShouldThrowArgumentNullException()
    {
        var configWithNullList = new BoardConfig { cells = null };
        var repo = new BoardRepository();

        Assert.Throws<ArgumentNullException>(() => repo.Construct(configWithNullList));
    }

    [Test]
    public void Construct_WithNullConfig_ShouldThrowArgumentNullException()
    {
        var repo = new BoardRepository();
        Assert.Throws<ArgumentNullException>(() => repo.Construct(null));
    }

    [Test]
    public void ModifyingOriginalList_ShouldNotAffectRepository()
    {
        var cell1 = new CellData();
        var cell2 = new CellData();
        var originalList = new List<CellData> { cell1, cell2 };
        var configCopy = new BoardConfig { cells = originalList };

        var repo = new BoardRepository();
        repo.Construct(configCopy);

        // »змен€ем оригинальный список после конструктора
        originalList.Clear();

        var cells = repo.GetAllCells();
        Assert.AreEqual(2, cells.Count);
        Assert.AreEqual(cell1, cells[0]);
        Assert.AreEqual(cell2, cells[1]);
    }
}
