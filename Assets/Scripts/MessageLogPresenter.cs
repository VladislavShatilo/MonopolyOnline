using System;
using UnityEngine;
using Zenject;

public class MessageLogPresenter : IInitializable, IDisposable
{
    private IMessageLogView messageLogView;
    private IChatService chatService;
    private IEventBus eventBus;
    private IPlayerRepository playerRepository;
    private ILocalPlayerService localPlayerService;
    private SendChatMessageUseCase sendChatMessageUseCase;

    #region LIFE_CYCLE

    [Inject]
    public void Construct(IMessageLogView messageLogView, IChatService chatService, IPlayerRepository playerRepository, IEventBus eventBus, ILocalPlayerService localPlayerService)
    {
        this.messageLogView = messageLogView ?? throw new ArgumentNullException(nameof(messageLogView));
        this.chatService = chatService ?? throw new ArgumentNullException(nameof(chatService));
        this.playerRepository = playerRepository ?? throw new ArgumentNullException(nameof(playerRepository));
        this.eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
        this.localPlayerService = localPlayerService ?? throw new ArgumentNullException(nameof(localPlayerService));
    }

    public void Initialize()
    {
        messageLogView.OnSendClicked += OnSendClicked;
        sendChatMessageUseCase = new SendChatMessageUseCase(chatService);
        eventBus.Subscribe<ChatMessage>(OnMessageReceived);
    }

    public void Dispose()
    {
        messageLogView.OnSendClicked -= OnSendClicked;
        eventBus.Unsubscribe<ChatMessage>(OnMessageReceived);
    }

    #endregion LIFE_CYCLE

    #region CALLBACKS

    private void OnSendClicked(string text)
    {
        int playerId = localPlayerService.GetLocalPlayerId();
        if (sendChatMessageUseCase != null)
        {
            sendChatMessageUseCase.Execute(playerId, text);

        }
    }

    private void OnMessageReceived(ChatMessage message)
    {
        var player = playerRepository.GetPlayerById(message.PlayerId) ?? throw new InvalidOperationException(nameof(messageLogView));
        string coloredName = $"<color=#{ColorUtility.ToHtmlStringRGB(player.PlayerColor.ToUnityColor())}>{player.Name}</color>";
        string formatted = $"{coloredName}: {message.Text}";
        messageLogView.AddMessage(formatted);
    }

    #endregion CALLBACKS
}