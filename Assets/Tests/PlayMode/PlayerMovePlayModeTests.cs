using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Collections;
using Moq;

public class PlayerMovePlayModeTests
{
    private GameObject playerGO;
    private PlayerMove playerMove;
    private Mock<IBoardService> boardServiceMock;
    private Mock<IEventBus> eventBusMock;

    [SetUp]
    public void Setup()
    {
        playerGO = new GameObject();
        playerGO.AddComponent<Photon.Pun.PhotonView>(); // нужен для photonView.OwnerActorNr
        playerMove = playerGO.AddComponent<PlayerMove>();

        boardServiceMock = new Mock<IBoardService>();
        eventBusMock = new Mock<IEventBus>();

        boardServiceMock.Setup(b => b.CellsCount).Returns(20);
        boardServiceMock.Setup(b => b.GetCellRectTransform(It.IsAny<int>()))
            .Returns<int>(i =>
            {
                var go = new GameObject($"Cell{i}");
                var tr = go.AddComponent<RectTransform>();
                tr.position = new Vector3(i, 0, 0);
                return tr;
            });

        playerMove.Initialize(boardServiceMock.Object, eventBusMock.Object);
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(playerGO);
    }

    [UnityTest]
    public IEnumerator MovePlayerEvent_ShouldMovePlayerAndPublishEvents()
    {
        // Настроим OwnerActorNr
        var photonView = playerGO.GetComponent<Photon.Pun.PhotonView>();
        photonView.ViewID = 1;
        photonView.OwnerActorNr = 1;

        // Вызов события
        playerMove.gameObject.SendMessage("OnPlayerMove", new MovePlayerEvent(1, 0, 3, true, 3));

        // Ждём выполнение корутины движения
        yield return new WaitForSeconds(2f);

        // Проверяем, что игрок дошёл до нужной позиции
        Assert.AreEqual(new Vector3(3, 0, 0), playerMove.transform.position);

        // Проверяем, что события публиковались
        eventBusMock.Verify(e => e.Publish(It.IsAny<DiceFadeEvent>()), Times.AtLeastOnce);
        eventBusMock.Verify(e => e.Publish(It.IsAny<PlayerOccupancyRegisterEvent>()), Times.AtLeastOnce);
        eventBusMock.Verify(e => e.Publish(It.IsAny<LapMoneyEvent>()), Times.Once);
        eventBusMock.Verify(e => e.Publish(It.IsAny<HandleCellEvent>()), Times.Once);
    }

    [UnityTest]
    public IEnumerator MoveToJailEvent_ShouldMovePlayerToJail()
    {
        var photonView = playerGO.GetComponent<Photon.Pun.PhotonView>();
        photonView.OwnerActorNr = 1;

        playerMove.gameObject.SendMessage("MoveToJail", new MoveToJailEvent(1));

        yield return new WaitForSeconds(0.3f);

        // Проверка, что игрок дошёл до клетки тюрьмы
        Vector3 jailPos = boardServiceMock.Object.GetCellRectTransform(10).position;
        Assert.AreEqual(jailPos, playerMove.transform.position);
    }
}
