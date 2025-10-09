using System;
using Moq;
using NUnit.Framework;
using UnityEngine;

public class JailPresenterTests
{
    private JailPresenter presenter;
    private Mock<IJailWindow> jailWindow;
    private Mock<IRansomJailWindow> ransomWindow;
    private Mock<IPlayerRepository> playerRepo;
    private Mock<ILocalPlayerService> localPlayer;
    private Mock<IEventBus> eventBus;
    private Mock<IPhotonDiceManager> diceManager;
    private Mock<IJailService> jailService;
    private Mock<IBankService> bankService;
    private GameSettings settings;

    [SetUp]
    public void SetUp()
    {
        jailWindow = new Mock<IJailWindow>();
        ransomWindow = new Mock<IRansomJailWindow>();
        playerRepo = new Mock<IPlayerRepository>();
        localPlayer = new Mock<ILocalPlayerService>();
        eventBus = new Mock<IEventBus>();
        diceManager = new Mock<IPhotonDiceManager>();
        jailService = new Mock<IJailService>();
        bankService = new Mock<IBankService>();
        settings = new GameSettings { jailRansom = 500 };

        localPlayer.Setup(l => l.GetLocalPlayerId()).Returns(1);

        presenter = new JailPresenter();
        presenter.Construct(
            jailWindow.Object,
            localPlayer.Object,
            eventBus.Object,
            playerRepo.Object,
            ransomWindow.Object,
            diceManager.Object,
            jailService.Object,
            bankService.Object,
            settings
        );
        presenter.Initialize();
    }

    [TearDown]
    public void TearDown()
    {
        presenter.Dispose();
    }

    [Test]
    public void Initialize_ShouldSubscribeAndSetActions()
    {
        eventBus.Verify(e => e.Subscribe<StartTurnJailEvent>(It.IsAny<Action<StartTurnJailEvent>>()), Times.Once);
        jailWindow.Verify(j => j.SetThrowDiceAction(It.IsAny<Action<int>>()), Times.Once);
        jailWindow.Verify(j => j.SetRansomAction(It.IsAny<Action<int>>()), Times.Once);
        ransomWindow.Verify(r => r.SetRansomAction(It.IsAny<Action<int>>()), Times.Once);
    }

    [Test]
    public void Dispose_ShouldUnsubscribeEvent()
    {
        presenter.Dispose();
        eventBus.Verify(e => e.Unsubscribe<StartTurnJailEvent>(It.IsAny<Action<StartTurnJailEvent>>()), Times.Once);
    }

    [Test]
    public void OnStartTurn_PlayerInJail_LocalPlayer_ShowsJailWindow()
    {
        var player = new PlayerData("player1", 1000, 0, null);
        player.JailTurnsLeft = 1;
        playerRepo.Setup(r => r.GetPlayerById(1)).Returns(player);

        // вызов приватного метода через Reflection
        presenter.GetType()
            .GetMethod("OnStartTurn", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .Invoke(presenter, new object[] { new StartTurnJailEvent ( 1) });

        jailWindow.Verify(j => j.Show(1, settings.jailRansom, true), Times.Once);
    }

    [Test]
    public void OnStartTurn_PlayerNotLocal_HidesJailWindow()
    {
        var player = new PlayerData("player1", 1000, 0, null);
        player.JailTurnsLeft = 1;
        playerRepo.Setup(r => r.GetPlayerById(2)).Returns(player);

        presenter.GetType()
            .GetMethod("OnStartTurn", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .Invoke(presenter, new object[] { new StartTurnJailEvent(2) });

        jailWindow.Verify(j => j.HardHide(), Times.Once);
    }

    [Test]
    public void OnStartTurn_PlayerNotInJail_LocalPlayer_ShowsRansomWindow()
    {
        var player = new PlayerData("player1", 1000, 0, null);
        player.JailTurnsLeft = 0;

        playerRepo.Setup(r => r.GetPlayerById(1)).Returns(player);

        presenter.GetType()
            .GetMethod("OnStartTurn", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .Invoke(presenter, new object[] { new StartTurnJailEvent(1) });

        ransomWindow.Verify(r => r.Show(1, settings.jailRansom, true), Times.Once);
    }

    [Test]
    public void OnThrowDiceClicked_ShouldRequestDiceAndHideWindow()
    {
        presenter.GetType()
            .GetMethod("OnThrowDiceClicked", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .Invoke(presenter, new object[] { 1 });

        diceManager.Verify(d => d.RequestDiceRoll(1, true,-1,-1), Times.Once);
        jailWindow.Verify(j => j.Hide(), Times.Once);
    }

    [Test]
    public void OnRansomClicked_ShouldReleasePlayer_RemoveMoney_RequestDiceAndHideWindows()
    {
        presenter.GetType()
            .GetMethod("OnRansomClicked", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .Invoke(presenter, new object[] { 1 });

        jailService.Verify(j => j.ReleasePlayer(1, true), Times.Once);
        bankService.Verify(b => b.RemoveMoney(1, settings.jailRansom), Times.Once);
        diceManager.Verify(d => d.RequestDiceRoll(1, false,-1,-1), Times.Once);
        jailWindow.Verify(j => j.Hide(), Times.Once);
        ransomWindow.Verify(r => r.Hide(), Times.Once);
    }
}
