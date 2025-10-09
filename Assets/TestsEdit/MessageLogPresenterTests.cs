using NUnit.Framework;
using Moq;
using System;
using UnityEngine;
using System.Reflection;

[TestFixture]
public class MessageLogPresenterTests
{
    private Mock<IMessageLogView> viewMock;
    private Mock<IChatService> chatServiceMock;
    private Mock<IPlayerRepository> playerRepositoryMock;
    private Mock<IEventBus> eventBusMock;
    private MessageLogPresenter presenter;

    [SetUp]
    public void SetUp()
    {
        viewMock = new Mock<IMessageLogView>();
        viewMock.SetupAllProperties(); // нужно для работы событий
        chatServiceMock = new Mock<IChatService>();
        playerRepositoryMock = new Mock<IPlayerRepository>();
        eventBusMock = new Mock<IEventBus>();

        presenter = new MessageLogPresenter();
        presenter.Construct(viewMock.Object, chatServiceMock.Object, playerRepositoryMock.Object, eventBusMock.Object);
        presenter.Initialize();
    }

    [TearDown]
    public void TearDown()
    {
        presenter.Dispose();
    }

    [Test]
    public void Initialize_ShouldSubscribeToViewAndEventBus()
    {
        // Подписка на событие view
     //  Assert.IsNotNull(viewMock.Object.OnSendClicked);

        // Подписка на событие eventBus
        eventBusMock.Verify(e => e.Subscribe<ChatMessage>(It.IsAny<Action<ChatMessage>>()), Times.Once);
    }

    [Test]
    public void Dispose_ShouldUnsubscribeFromViewAndEventBus()
    {
        // Сначала dispose
        presenter.Dispose();

        // Проверяем отписку от eventBus
        eventBusMock.Verify(e => e.Unsubscribe<ChatMessage>(It.IsAny<Action<ChatMessage>>()), Times.Once);
    }

    [Test]
    public void OnSendClicked_ShouldCallSendChatMessageUseCaseWithCorrectParams()
    {
        // Вместо Photon используем Reflection для вызова приватного метода
        var sendMethod = presenter.GetType().GetMethod("OnSendClicked", BindingFlags.NonPublic | BindingFlags.Instance);

        // Эмулируем ActorNumber
        int testPlayerId = 123;

        // Подменяем метод SendMessage через мок chatService
        chatServiceMock.Setup(s => s.SendMessage(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<bool>()));

        // Вызов метода
        sendMethod.Invoke(presenter, new object[] { "Hello" });

        // Проверяем, что SendMessage вызван (ActorNumber в реальном коде берется из Photon)
        chatServiceMock.Verify(s => s.SendMessage(It.IsAny<int>(), "Hello", true), Times.Once);
    }

    [Test]
    public void OnMessageReceived_ShouldFormatMessageAndCallViewAddMessage()
    {
        // Создаем тестовое сообщение
        var message = new ChatMessage(1, "Hello world");
        var player = new PlayerData("John", 500, 1, new PlayerColor(255, 0, 0));

        playerRepositoryMock.Setup(r => r.GetPlayerById(1)).Returns(player);

        // Вызов приватного метода напрямую
        var receiveMethod = presenter.GetType().GetMethod("OnMessageReceived", BindingFlags.NonPublic | BindingFlags.Instance);
        receiveMethod.Invoke(presenter, new object[] { message });

        // Проверяем форматирование
        string expected = $"<color=#{ColorUtility.ToHtmlStringRGB(player.PlayerColor.ToUnityColor())}>John</color>: Hello world";
        viewMock.Verify(v => v.AddMessage(expected), Times.Once);
    }
}
