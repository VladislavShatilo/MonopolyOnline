using Moq;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TestTools;

public class CellOccupancyServicePlayModeTests
{
    private CellOccupancyService service;
    private Mock<IBoardService> boardServiceMock;
    private PlayerMove player;

    [SetUp]
    public void Setup()
    {
        service = new CellOccupancyService();

        boardServiceMock = new Mock<IBoardService>();
        var cellGO = new GameObject("Cell");
        var rect = cellGO.AddComponent<RectTransform>();
        rect.position = new Vector3(100, 200, 0);
        boardServiceMock.Setup(b => b.GetCellRectTransform(It.IsAny<int>())).Returns(rect);

        var eventBus = new Mock<IEventBus>();
        service.Construct(boardServiceMock.Object, eventBus.Object);

        player = new GameObject("Player").AddComponent<PlayerMove>();

        // вручную добавл€ем игрока
        var cellPlayers = typeof(CellOccupancyService)
            .GetField("cellPlayers", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .GetValue(service) as Dictionary<int, List<PlayerMove>>;

        cellPlayers[1] = new List<PlayerMove> { player };
    }

    [UnityTest]
    public IEnumerator UpdatePositions_ShouldMovePlayerToTarget()
    {
        service.UpdatePositions(1);

        yield return null; // ждЄм один кадр, чтобы Unity обновила Transform

        // ѕровер€ем, что позици€ игрока изменилась
        Assert.AreNotEqual(Vector3.zero, player.transform.position);
    }
}
