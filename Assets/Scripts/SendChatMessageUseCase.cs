public class SendChatMessageUseCase
{
    private readonly IChatService chatService;

    public SendChatMessageUseCase(IChatService chatService)
    {
        this.chatService = chatService;
    }

    public void Execute(int playerId, string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return;
        chatService.SendMessage(playerId, text, true);
    }
}