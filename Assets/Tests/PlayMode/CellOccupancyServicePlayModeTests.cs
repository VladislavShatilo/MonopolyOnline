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
    private PlayerMove player1;
    private PlayerMove player2;

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
        player1 = new GameObject().AddComponent<PlayerMove>();
        player2 = new GameObject().AddComponent<PlayerMove>();

        // вручную добавляем игрока
        var cellPlayers = typeof(CellOccupancyService)
            .GetField("cellPlayers", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .GetValue(service) as Dictionary<int, List<PlayerMove>>;

        cellPlayers[1] = new List<PlayerMove> { player };
    }

    [UnityTest]
    public IEnumerator UpdatePositions_ShouldMovePlayerToTarget()
    {
        service.UpdatePositions(1);

        yield return null; // ждём один кадр, чтобы Unity обновила Transform

        // Проверяем, что позиция игрока изменилась
        Assert.AreNotEqual(Vector3.zero, player.transform.position);
    }
    [UnityTest]
    public IEnumerator UpdatePositions_ShouldSetTargetPositions_ForMultiplePlayers_CellIndex0()
    {
        // Setup
        var cellPlayers = typeof(CellOccupancyService)
            .GetField("cellPlayers", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .GetValue(service) as Dictionary<int, List<PlayerMove>>;
        cellPlayers[0] = new List<PlayerMove> { player1, player2 };

        // Act
        service.UpdatePositions(0);

        yield return null; // Ждём один кадр, чтобы позиции применились

        // Assert
        var pos1 = player1.transform.position;
        var pos2 = player2.transform.position;

        Assert.AreNotEqual(pos1, pos2);
    }
    [UnityTest]
    public IEnumerator UpdatePositions_ShouldSetTargetPositions_For1Player_PlayMode()
    {
        var cellPlayers = typeof(CellOccupancyService)
            .GetField("cellPlayers", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .GetValue(service) as Dictionary<int, List<PlayerMove>>;

        cellPlayers[5] = new List<PlayerMove> { player1 };

        service.UpdatePositions(5);

        yield return null; // ждём кадр, чтобы позиции применились

        Assert.AreNotEqual(Vector3.zero, player1.transform.position);
    }
}
