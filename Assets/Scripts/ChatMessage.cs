public class ChatMessage
{
    public int PlayerId { get; }
    public string Text { get; }

    public ChatMessage(int playerId, string text)
    {
        PlayerId = playerId;
        Text = text;
    }
}
