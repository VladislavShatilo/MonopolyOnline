using NUnit.Framework;
using Moq;
using UnityEngine;
using Photon.Pun;
using System.Collections;
using System.Reflection;

[TestFixture]
public class DiceManagerPhotonTests
{
    private DiceManagerPhoton diceManager;
    private Mock<IRollDiceUseCase> mockRollDiceUseCase;
    private Mock<IPhotonNetworkWrapper> mockPhotonNetworkWrapper;
    private Mock<IPhotonViewWrapper> mockPhotonViewWrapper;
    private Mock<PhotonView> mockPhotonView;

    [SetUp]
    public void SetUp()
    {
        // Создаём GameObject, чтобы привязать MonoBehaviour
        var go = new GameObject();
        diceManager = go.AddComponent<DiceManagerPhoton>();

        mockRollDiceUseCase = new Mock<IRollDiceUseCase>();
        mockPhotonNetworkWrapper = new Mock<IPhotonNetworkWrapper>();
        mockPhotonViewWrapper = new Mock<IPhotonViewWrapper>();
        mockPhotonView = new Mock<PhotonView>();

        diceManager.Construct(mockRollDiceUseCase.Object, mockPhotonNetworkWrapper.Object, mockPhotonViewWrapper.Object);
    }

    [Test]
    public void RequestDiceRoll_CallsRPCOnPhotonViewWrapper()
    {
        diceManager.RequestDiceRoll(1, true, 3, 4);

      


        mockPhotonViewWrapper.Verify(p =>
            p.RPC(It.IsAny<PhotonView>(),
                  "RPC_RequestGetDiceResult",
                  RpcTarget.MasterClient,
                  1, true, 3, 4),
            Times.Once);
    }

    [Test]
    public void RPC_RequestGetDiceResult_UsesCheats_WhenAllowed()
    {
        DiceManagerPhoton.AllowCheats = true;
        mockPhotonNetworkWrapper.Setup(p => p.IsMasterClient).Returns(true);

        // вызываем private метод через Reflection
        var method = typeof(DiceManagerPhoton)
            .GetMethod("RPC_RequestGetDiceResult", BindingFlags.NonPublic | BindingFlags.Instance);
        method.Invoke(diceManager, new object[] { 1, true, 5, 6 });

        // проверяем правильный вызов RPC на DiceHandle
        mockPhotonViewWrapper.Verify(p =>
            p.RPC(It.IsAny<PhotonView>(),
                  "RPC_RequestDiceHandle",   // <-- тут исправлено
                  RpcTarget.All,
                  5, 6, 1, true),
            Times.Once);
    }

    [Test]
    public void RPC_RequestGetDiceResult_UsesRollDiceUseCase_WhenNoCheats()
    {
        DiceManagerPhoton.AllowCheats = false;
        mockPhotonNetworkWrapper.Setup(p => p.IsMasterClient).Returns(true);

        mockRollDiceUseCase.Setup(r => r.GetDiceResult(1, true))
            .Returns(new DiceResult(2,3));

        diceManager.GetType()
            .GetMethod("RPC_RequestGetDiceResult", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .Invoke(diceManager, new object[] { 1, true, -1, -1 });

        mockPhotonViewWrapper.Verify(p =>
       p.RPC(It.IsAny<PhotonView>(),
             "RPC_RequestDiceHandle",   // <-- исправлено
             RpcTarget.All,
             2, 3, 1, true),
       Times.Once);
    }

    [Test]
    public void RPC_RequestDiceHandle_StartsCoroutineHandleDice()
    {
        var first = 1;
        var second = 2;
        var playerId = 1;
        var isForJail = false;

        // Мокируем HandleDice как корутину
        mockRollDiceUseCase.Setup(r => r.HandleDice(first, second, playerId, isForJail))
            .Returns(Mock.Of<IEnumerator>());

        diceManager.GetType()
            .GetMethod("RPC_RequestDiceHandle", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .Invoke(diceManager, new object[] { first, second, playerId, isForJail });

        mockRollDiceUseCase.Verify(r => r.HandleDice(first, second, playerId, isForJail), Times.Once);
    }
}
