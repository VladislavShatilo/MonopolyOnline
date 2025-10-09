using NUnit.Framework;

[TestFixture]
public class ChatMessageTests
{
    [Test]
    public void Constructor_ShouldAssignProperties()
    {
        int playerId = 5;
        string text = "Hello, world!";

        var message = new ChatMessage(playerId, text);

        Assert.AreEqual(playerId, message.PlayerId);
        Assert.AreEqual(text, message.Text);
    }
}
