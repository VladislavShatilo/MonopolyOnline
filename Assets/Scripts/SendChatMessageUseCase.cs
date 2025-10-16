public class SendChatMessageUseCase
{
    private readonly IChatService chatService;

    #region LIFE_CYCLE

    public SendChatMessageUseCase(IChatService chatService)
    {
        this.chatService = chatService;
    }

    #endregion LIFE_CYCLE

    #region PUBLIC_METHODS

    public void Execute(int playerId, string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return;
        if(chatService != null)
        {
            chatService.SendMessage(playerId, text, true);
        }
    }

    #endregion PUBLIC_METHODS
}