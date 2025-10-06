using System;
using UnityEngine;
using Zenject;

public class MessageLogPresenter : IInitializable, IDisposable
{
    private MessageLogView view;
    private IChatService chatService;
    private IEventBus eventBus;
    private IPlayerRepository playerRepository;
    private SendChatMessageUseCase sendChatMessageUseCase;

    #region LIFE_CYCLE

    [Inject]
    public void Construct(MessageLogView view, IChatService chatService, IPlayerRepository playerRepository, IEventBus eventBus)
    {
        this.view = view;
        this.chatService = chatService;
        this.playerRepository = playerRepository;
        this.eventBus = eventBus;
    }

    void IInitializable.Initialize()
    {
        view.OnSendClicked += OnSendClicked;
        sendChatMessageUseCase = new SendChatMessageUseCase(chatService);
        eventBus.Subscribe<ChatMessage>(OnMessageReceived);
    }

    void IDisposable.Dispose()
    {
        view.OnSendClicked -= OnSendClicked;
        eventBus.Unsubscribe<ChatMessage>(OnMessageReceived);
    }

    #endregion LIFE_CYCLE

    #region CALLBACKS

    private void OnSendClicked(string text)
    {
        int playerId = Photon.Pun.PhotonNetwork.LocalPlayer.ActorNumber;
        sendChatMessageUseCase.Execute(playerId, text);
    }

    private void OnMessageReceived(ChatMessage message)
    {
        var player = playerRepository.GetPlayerById(message.PlayerId);
        string coloredName = $"<color=#{ColorUtility.ToHtmlStringRGB(player.PlayerColor.ToUnityColor())}>{player.Name}</color>";
        string formatted = $"{coloredName}: {message.Text}";
        view.AddMessage(formatted);
    }

    #endregion CALLBACKS
}