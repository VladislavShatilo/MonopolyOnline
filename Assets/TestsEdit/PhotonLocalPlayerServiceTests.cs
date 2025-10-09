//using NUnit.Framework;
//using Photon.Pun;
//using Photon.Realtime;
//using System.Collections;
//using UnityEngine;
//using UnityEngine.TestTools;

//public class PhotonLocalPlayerServiceTests
//{
//    private PhotonLocalPlayerService localPlayerService;
//    private Player testPlayer;

//    [SetUp]
//    public void SetUp()
//    {
//        // Создаем тестового игрока
//        testPlayer = new Player("TestPlayer", 42); // 42 — это ActorNumber
//        PhotonNetwork.SetOfflineMode(true); // Без этого LocalPlayer не будет установлен в Editor
//        PhotonNetwork.AddCallbackTarget(this); // чтобы инициализация Photon прошла
//        PhotonNetwork.LocalPlayer = testPlayer; // вручную задаем локального игрока

//        localPlayerService = new PhotonLocalPlayerService();
//    }

//    [TearDown]
//    public void TearDown()
//    {
//        PhotonNetwork.LocalPlayer = null;
//        PhotonNetwork.RemoveCallbackTarget(this);
//    }

//    [Test]
//    public void GetLocalPlayerId_ShouldReturn_ActorNumber_OfLocalPlayer()
//    {
//        // Act
//        var result = localPlayerService.GetLocalPlayerId();

//        // Assert
//        Assert.AreEqual(42, result);
//    }

//    [Test]
//    public void GetLocalPlayerId_ShouldThrow_WhenLocalPlayerIsNull()
//    {
//        // Arrange
//        PhotonNetwork.LocalPlayer = null;

//        // Act & Assert
//        Assert.Throws<System.NullReferenceException>(() => localPlayerService.GetLocalPlayerId());
//    }
//}
