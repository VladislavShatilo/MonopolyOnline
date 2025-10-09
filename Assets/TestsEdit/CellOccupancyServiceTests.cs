using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CellOccupancyServiceTests
{
    private CellOccupancyService service;
    private Mock<IBoardService> boardServiceMock;
    private Mock<IEventBus> eventBusMock;
    private PlayerMove testPlayer;

    [SetUp]
    public void Setup()
    {
        service = new CellOccupancyService();

        boardServiceMock = new Mock<IBoardService>();
        boardServiceMock.Setup(b => b.GetCellRectTransform(It.IsAny<int>()))
            .Returns(() => new GameObject().AddComponent<RectTransform>());

        eventBusMock = new Mock<IEventBus>();

        service.Construct(boardServiceMock.Object, eventBusMock.Object);

        // Заглушка игрока
        testPlayer = new GameObject().AddComponent<PlayerMove>();
        testPlayer.SetTargetPosition(Vector3.zero); // чтобы был метод
    }

    [Test]
    public void InitializePlayer_SubscribesToEvents()
    {
        service.InitializePlayer();

        eventBusMock.Verify(e => e.Subscribe<PlayerOccupancyRegisterEvent>(It.IsAny<System.Action<PlayerOccupancyRegisterEvent>>()), Times.Once);
        eventBusMock.Verify(e => e.Subscribe<PlayerOccupancyUnregisterEvent>(It.IsAny<System.Action<PlayerOccupancyUnregisterEvent>>()), Times.Once);
    }

    [Test]
    public void Dispose_UnsubscribesFromEvents()
    {
        service.InitializePlayer();
        ((IDisposable)service).Dispose();

        eventBusMock.Verify(e => e.Unsubscribe<PlayerOccupancyRegisterEvent>(It.IsAny<System.Action<PlayerOccupancyRegisterEvent>>()), Times.Once);
        eventBusMock.Verify(e => e.Unsubscribe<PlayerOccupancyUnregisterEvent>(It.IsAny<System.Action<PlayerOccupancyUnregisterEvent>>()), Times.Once);
    }

    [Test]
    public void RegisterPlayerOnCell_AddsPlayerToCell()
    {
        // Имитация события
        var e = new PlayerOccupancyRegisterEvent(1, testPlayer);


        // Вызываем приватный метод через reflection
        typeof(CellOccupancyService)
            .GetMethod("RegisterPlayerOnCell", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .Invoke(service, new object[] { e });

        // Проверяем, что игрок добавлен
        var playersField = typeof(CellOccupancyService)
            .GetField("cellPlayers", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .GetValue(service) as Dictionary<int, List<PlayerMove>>;

        Assert.IsTrue(playersField.ContainsKey(1));
        Assert.Contains(testPlayer, playersField[1]);
    }

    [Test]
    public void UnregisterPlayerFromCell_RemovesPlayerFromCell()
    {
        // Добавим игрока вручную
        var playersField = typeof(CellOccupancyService)
            .GetField("cellPlayers", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .GetValue(service) as Dictionary<int, List<PlayerMove>>;

        playersField[1] = new List<PlayerMove> { testPlayer };

        var e = new PlayerOccupancyUnregisterEvent(1, testPlayer);


        typeof(CellOccupancyService)
            .GetMethod("UnregisterPlayerFromCell", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .Invoke(service, new object[] { e });

        Assert.IsFalse(playersField.ContainsKey(1));
    }

}
