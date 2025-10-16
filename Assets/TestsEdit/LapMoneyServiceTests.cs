using System;
using Moq;
using NUnit.Framework;

public class LapMoneyServiceTests
{
    private LapMoneyService lapMoneyService;
    private Mock<IEventBus> eventBus;
    private Mock<IBankService> bankService;
    private Mock<IChatService> chatService;
    private GameSettings settings;

    [SetUp]
    public void SetUp()
    {
        eventBus = new Mock<IEventBus>();
        bankService = new Mock<IBankService>();
        chatService = new Mock<IChatService>();
        settings = new GameSettings { lapMoney = 200 };

        lapMoneyService = new LapMoneyService();
        lapMoneyService.Construct(eventBus.Object, bankService.Object, settings, chatService.Object);
        lapMoneyService.Initialize();
    }

    [TearDown]
    public void TearDown()
    {
        lapMoneyService.Dispose();
    }

    [Test]
    public void Initialize_ShouldSubscribeToLapMoneyEvent()
    {
        eventBus.Verify(e => e.Subscribe<LapMoneyEvent>(It.IsAny<Action<LapMoneyEvent>>()), Times.Once);
    }

    [Test]
    public void Dispose_ShouldUnsubscribeFromLapMoneyEvent()
    {
        lapMoneyService.Dispose();
        eventBus.Verify(e => e.Unsubscribe<LapMoneyEvent>(It.IsAny<Action<LapMoneyEvent>>()), Times.Once);
    }

    [Test]
    public void GiveMoneyLap_PlayerPassedStart_AddsMoneyAndSendsMessage()
    {
        var lapEvent = new LapMoneyEvent(1, 5, 2, true);
     

        // Вызов приватного метода через Reflection
        lapMoneyService.GetType()
            .GetMethod("GiveMoneyLap", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .Invoke(lapMoneyService, new object[] { lapEvent });

        bankService.Verify(b => b.AddMoney(1, settings.lapMoney), Times.Once);
        chatService.Verify(c => c.SendMessage(1, $"Игрок 1 прошёл круг и получил {settings.lapMoney}$!", false), Times.Once);
    }

    [Test]
    public void GiveMoneyLap_PlayerDidNotPassStart_DoesNothing()
    {
        var lapEvent = new LapMoneyEvent(1, 2, 5, true);
      

        lapMoneyService.GetType()
            .GetMethod("GiveMoneyLap", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .Invoke(lapMoneyService, new object[] { lapEvent });

        bankService.Verify(b => b.AddMoney(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
        chatService.Verify(c => c.SendMessage(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<bool>()), Times.Never);
    }

    [Test]
    public void GiveMoneyLap_PlayerMovedBackward_DoesNothing()
    {
        var lapEvent = new LapMoneyEvent(1, 5, 2, false);
       
        lapMoneyService.GetType()
            .GetMethod("GiveMoneyLap", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .Invoke(lapMoneyService, new object[] { lapEvent });

        bankService.Verify(b => b.AddMoney(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
        chatService.Verify(c => c.SendMessage(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<bool>()), Times.Never);
    }
    [Test]
    public void Construct_NullDependencies_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => new LapMoneyService().Construct(null, bankService.Object, settings, chatService.Object));
        Assert.Throws<ArgumentNullException>(() => new LapMoneyService().Construct(eventBus.Object, null, settings, chatService.Object));
        Assert.Throws<ArgumentNullException>(() => new LapMoneyService().Construct(eventBus.Object, bankService.Object, null, chatService.Object));
        Assert.Throws<ArgumentNullException>(() => new LapMoneyService().Construct(eventBus.Object, bankService.Object, settings, null));
    }
}
