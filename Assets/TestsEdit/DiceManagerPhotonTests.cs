using Moq;
using NUnit.Framework;
using Photon.Pun;
using System;
using System.Collections;
using System.Reflection;
using UnityEngine;
using Object = UnityEngine.Object;

[TestFixture]
public class DiceManagerPhotonTests
{
    private DiceManagerPhoton diceManager;
    private Mock<IRollDiceUseCase> mockRollDiceUseCase;
    private Mock<IPhotonNetworkWrapper> mockPhotonNetworkWrapper;
    private Mock<IPhotonViewWrapper> mockPhotonViewWrapper;
    private GameObject go;
    private PhotonView photonView;

    [SetUp]
    public void SetUp()
    {
        // Создаем GameObject и добавляем необходимые компоненты
        go = new GameObject("DiceManagerPhoton");
        diceManager = go.AddComponent<DiceManagerPhoton>();

        // Добавляем настоящий PhotonView
        photonView = go.AddComponent<PhotonView>();

        // Создаем моки
        mockRollDiceUseCase = new Mock<IRollDiceUseCase>();
        mockPhotonNetworkWrapper = new Mock<IPhotonNetworkWrapper>();
        mockPhotonViewWrapper = new Mock<IPhotonViewWrapper>();

        // Конструктор
        diceManager.Construct(mockRollDiceUseCase.Object, mockPhotonNetworkWrapper.Object, mockPhotonViewWrapper.Object);
    }

    [TearDown]
    public void TearDown()
    {
        if (go != null)
            Object.DestroyImmediate(go);
    }

    // ---------------- RequestDiceRoll ----------------

    [Test]
    public void RequestDiceRoll_CallsRPCOnPhotonViewWrapper()
    {
        diceManager.RequestDiceRoll(1, true, 3, 4);

        mockPhotonViewWrapper.Verify(p =>
            p.RPC(photonView,
                  "RPC_RequestGetDiceResult",
                  RpcTarget.MasterClient,
                  1, true, 3, 4),
            Times.Once);
    }

    [Test]
    public void RequestDiceRoll_ThrowsException_WhenPhotonViewNull()
    {
        // Создаем новый объект без PhotonView
        var goWithoutPV = new GameObject("NoPhotonView");
        var dm = goWithoutPV.AddComponent<DiceManagerPhoton>();
        dm.Construct(mockRollDiceUseCase.Object, mockPhotonNetworkWrapper.Object, mockPhotonViewWrapper.Object);

        Assert.Throws<NullReferenceException>(() =>
            dm.RequestDiceRoll(1, false));

        Object.DestroyImmediate(goWithoutPV);
    }

    // ---------------- RPC_RequestGetDiceResult ----------------



    [Test]
    public void RPC_RequestGetDiceResult_DoesNothing_WhenNotMasterClient()
    {
        mockPhotonNetworkWrapper.Setup(p => p.IsMasterClient).Returns(false);

        typeof(DiceManagerPhoton)
            .GetMethod("RPC_RequestGetDiceResult", BindingFlags.NonPublic | BindingFlags.Instance)
            .Invoke(diceManager, new object[] { 1, true, 5, 6 });

        mockPhotonViewWrapper.Verify(p => p.RPC(It.IsAny<PhotonView>(),
                                                It.IsAny<string>(),
                                                It.IsAny<RpcTarget>(),
                                                It.IsAny<object[]>()),
                                     Times.Never);
    }

    [Test]
    public void RPC_RequestGetDiceResult_ThrowsException_WhenPhotonViewNull()
    {
        mockPhotonNetworkWrapper.Setup(p => p.IsMasterClient).Returns(true);

        // Создаем новый объект без PhotonView
        var goWithoutPV = new GameObject("NoPhotonView");
        var dm = goWithoutPV.AddComponent<DiceManagerPhoton>();
        dm.Construct(mockRollDiceUseCase.Object, mockPhotonNetworkWrapper.Object, mockPhotonViewWrapper.Object);

        var method = typeof(DiceManagerPhoton)
            .GetMethod("RPC_RequestGetDiceResult", BindingFlags.NonPublic | BindingFlags.Instance);

        Assert.Throws<TargetInvocationException>(() =>
            method.Invoke(dm, new object[] { 1, true, 1, 1 }));

        Object.DestroyImmediate(goWithoutPV);
    }

    // ---------------- RPC_RequestDiceHandle ----------------

    [Test]
    public void RPC_RequestDiceHandle_StartsCoroutineHandleDice()
    {
        var first = 1;
        var second = 2;
        var playerId = 1;
        var isForJail = false;

        mockRollDiceUseCase.Setup(r => r.HandleDice(first, second, playerId, isForJail))
            .Returns(Mock.Of<IEnumerator>());

        typeof(DiceManagerPhoton)
            .GetMethod("RPC_RequestDiceHandle", BindingFlags.NonPublic | BindingFlags.Instance)
            .Invoke(diceManager, new object[] { first, second, playerId, isForJail });

        mockRollDiceUseCase.Verify(r => r.HandleDice(first, second, playerId, isForJail), Times.Once);
    }

    // ---------------- Update ----------------

   
}
