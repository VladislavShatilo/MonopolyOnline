using NUnit.Framework;
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

    [Test]
    public void GetAllCells_ShouldReturnAllCells()
    {
        var cells = boardRepository.GetAllCells();

        Assert.AreEqual(2, cells.Count);
        Assert.AreEqual(cell1, cells[0]);
        Assert.AreEqual(cell2, cells[1]);
    }

    [Test]
    public void GetCell_ValidId_ShouldReturnCorrectCell()
    {
        var cell = boardRepository.GetCell(0);
        Assert.AreEqual(cell1, cell);

        cell = boardRepository.GetCell(1);
        Assert.AreEqual(cell2, cell);
    }

    [Test]
    public void GetCell_InvalidId_ShouldReturnNull()
    {
        Assert.IsNull(boardRepository.GetCell(-1));
        Assert.IsNull(boardRepository.GetCell(2)); // index out of range
    }

    [Test]
    public void GetAllCells_ReturnedList_ShouldBeReadOnly()
    {
        var cells = boardRepository.GetAllCells();

        Assert.IsInstanceOf<IReadOnlyList<CellData>>(cells);
    }
}
