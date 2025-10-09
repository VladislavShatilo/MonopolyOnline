using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Moq;
using Photon.Pun;

public class DiceManagerPhotonTests
{
    private GameObject _gameObject;
    private DiceManagerPhoton _diceManager;

    private Mock<IRollDiceUseCase> _mockRollDice;
    private Mock<IPhotonNetworkWrapper> _mockPhotonWrapper;

    [SetUp]
    public void SetUp()
    {
        _gameObject = new GameObject();
        _diceManager = _gameObject.AddComponent<DiceManagerPhoton>();

        _mockRollDice = new Mock<IRollDiceUseCase>();
        _mockPhotonWrapper = new Mock<IPhotonNetworkWrapper>();

        _diceManager.Construct(_mockRollDice.Object, _mockPhotonWrapper.Object);

        // Присвоим photonView, чтобы RPC не падал
        _diceManager.gameObject.AddComponent<PhotonView>();
    }

    [TearDown]
    public void TearDown()
    {
        GameObject.Destroy(_gameObject);
    }

    [UnityTest]
    public IEnumerator RequestDiceRoll_CallsRPCWithCorrectParams()
    {
        // Подменим PhotonView.RPC через мок
        bool rpcCalled = false;
       
        _diceManager.RequestDiceRoll(1, true);
        yield return null;

        Assert.IsTrue(rpcCalled);
    }

    [UnityTest]
    public IEnumerator RPC_RequestGetDiceResult_UsesCheat_WhenAllowed()
    {
        DiceManagerPhoton.AllowCheats = true;
        int playerId = 1;

        var called = false;
        _mockRollDice.Setup(r => r.HandleDice(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
            .Returns(SetupCoroutine())
            .Callback(() => called = true);

        // Симуляция вызова RPC с читом
        _diceManager.GetType()
            .GetMethod("RPC_RequestGetDiceResult", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .Invoke(_diceManager, new object[] { playerId, false, 4, 5 });

        yield return null;

        Assert.IsTrue(called);
    }

    private IEnumerator SetupCoroutine()
    {
        yield return null;
    }

    [UnityTest]
    public IEnumerator RPC_RequestDiceHandle_StartsCoroutine()
    {
        bool coroutineStarted = false;
        _mockRollDice.Setup(r => r.HandleDice(4, 5, 1, false))
            .Returns(CoroutineMock(() => coroutineStarted = true));

        _diceManager.GetType()
            .GetMethod("RPC_RequestDiceHandle", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .Invoke(_diceManager, new object[] { 4, 5, 1, false });

        yield return null;

        Assert.IsTrue(coroutineStarted);
    }

    private IEnumerator CoroutineMock(System.Action callback)
    {
        callback?.Invoke();
        yield return null;
    }
}
