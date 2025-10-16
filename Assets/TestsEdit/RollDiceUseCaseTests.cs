using NUnit.Framework;
using Moq;
using System.Collections;
using UnityEngine.TestTools;
using System;

[TestFixture]
public class RollDiceUseCaseTests
{
    private Mock<IDiceService> diceServiceMock;
    private Mock<IPhotonJailManager> photonJailManagerMock;
    private Mock<IPlayerRepository> playerRepositoryMock;
    private Mock<ILocalPlayerService> localPlayerServiceMock;
    private Mock<IPhotonTurnManager> photonTurnManagerMock;
    private Mock<IEventBus> eventBusMock;
    private RollDiceUseCase useCase;
    private PlayerData player;

    [SetUp]
    public void SetUp()
    {
        diceServiceMock = new Mock<IDiceService>();
        photonJailManagerMock = new Mock<IPhotonJailManager>();
        playerRepositoryMock = new Mock<IPlayerRepository>();
        localPlayerServiceMock = new Mock<ILocalPlayerService>();
        photonTurnManagerMock = new Mock<IPhotonTurnManager>();
        eventBusMock = new Mock<IEventBus>();

        useCase = new RollDiceUseCase();
        useCase.Construct(diceServiceMock.Object, photonJailManagerMock.Object, playerRepositoryMock.Object,
            localPlayerServiceMock.Object, eventBusMock.Object, photonTurnManagerMock.Object);

        player = new PlayerData("Test", 500, 1, null);
        playerRepositoryMock.Setup(r => r.GetPlayerById(It.IsAny<int>())).Returns(player);
    }

    [Test]
    public void GetDiceResult_Should_CallDiceServiceAndReturnResult()
    {
        var expected = new DiceResult(3, 4);
        diceServiceMock.Setup(d => d.Roll()).Returns(expected);

        var result = useCase.GetDiceResult();

        Assert.AreEqual(3, result.First);
        Assert.AreEqual(4, result.Second);
        diceServiceMock.Verify(d => d.Roll(), Times.Once);
    }

    [UnityTest]
    public IEnumerator HandleDice_Should_PublishDiceRolledEvent()
    {
        yield return useCase.HandleDice(2, 5, 1, false);

        eventBusMock.Verify(e => e.Publish(It.Is<DiceRolledEvent>(ev => ev.PlayerId == 1 && ev.DiceResult.First == 2 && ev.DiceResult.Second == 5)), Times.Once);
    }

    [UnityTest]
    public IEnumerator HandleDice_Should_CallJailManager_When_IsForJail()
    {
        localPlayerServiceMock.Setup(l => l.GetLocalPlayerId()).Returns(1);

        yield return useCase.HandleDice(4, 4, 1, true);

        photonJailManagerMock.Verify(j => j.CheckDice(1, 4, 4), Times.Once);
        eventBusMock.Verify(e => e.Publish(It.Is<DiceFadeEvent>(ev => ev.CellId == 10 && ev.IsMovementStart == false)), Times.Once);
    }

    [UnityTest]
    public IEnumerator HandlePlayerMove_Should_PublishOnPlayerMoveEvent_Forward()
    {
        localPlayerServiceMock.Setup(l => l.GetLocalPlayerId()).Returns(1);
        player.NextMoveBackward = false;

        yield return useCase.HandleDice(3, 3, 1, false);

        eventBusMock.Verify(e => e.Publish(It.Is<OnPlayerMoveEvent>(ev => ev.PlayerId == 1 && ev.Steps == 6 && ev.Forward == true)), Times.Once);
    }

    [UnityTest]
    public IEnumerator HandlePlayerMove_Should_RegisterDouble_When_IsDoubleAndNotFromJail()
    {
        localPlayerServiceMock.Setup(l => l.GetLocalPlayerId()).Returns(1);
        player.NextMoveBackward = false;

        yield return useCase.HandleDice(5, 5, 1, false);

        photonTurnManagerMock.Verify(p => p.RegisterDouble(1), Times.Once);
    }
       [UnityTest]
    public IEnumerator HandlePlayerMove_Should_PublishOnPlayerMoveEvent_Backward()
    {
        // Arrange
        localPlayerServiceMock.Setup(l => l.GetLocalPlayerId()).Returns(1);
        player.NextMoveBackward = true;

        // Act
        yield return useCase.HandleDice(2, 3, 1, false);

        // Assert
        eventBusMock.Verify(e => e.Publish(It.Is<OnPlayerMoveEvent>(ev => ev.PlayerId == 1 && ev.Steps == 5 && ev.Forward == false)), Times.Once);
        Assert.IsFalse(player.NextMoveBackward); // должен сброситьс€
    }

    [UnityTest]
    public IEnumerator HandleDice_Should_NotRegisterDouble_When_FromJail()
    {
        // Arrange
        localPlayerServiceMock.Setup(l => l.GetLocalPlayerId()).Returns(1);

        // Act
        yield return useCase.HandleDice(6, 6, 1, true); // дубль из тюрьмы

        // Assert
        photonTurnManagerMock.Verify(p => p.RegisterDouble(It.IsAny<int>()), Times.Never);
    }

    [UnityTest]
    public IEnumerator HandlePlayerMove_Should_DoNothing_ForNonLocalPlayer()
    {
        // Arrange
        localPlayerServiceMock.Setup(l => l.GetLocalPlayerId()).Returns(2); // другой игрок
        player.NextMoveBackward = false;

        // Act
        yield return useCase.HandleDice(3, 4, 1, false);

        // Assert
        eventBusMock.Verify(e => e.Publish(It.IsAny<OnPlayerMoveEvent>()), Times.Never);
        photonTurnManagerMock.Verify(p => p.RegisterDouble(It.IsAny<int>()), Times.Never);
    }
    [Test]
    public void HandlePlayerMove_Should_Throw_When_PlayerNotFound()
    {
        // Arrange
        playerRepositoryMock.Setup(r => r.GetPlayerById(1)).Returns((PlayerData)null);

        var diceResult = new DiceResult(1, 2);

        // Act & Assert
        var ex = Assert.Throws<System.Reflection.TargetInvocationException>(() =>
        {
            var method = typeof(RollDiceUseCase)
                .GetMethod("HandlePlayerMove", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            method.Invoke(useCase, new object[] { diceResult, 1, false });
        });

        // ѕровер€ем, что внутренн€€ причина Ч NullReferenceException
        Assert.IsInstanceOf<NullReferenceException>(ex.InnerException);
    }
}
