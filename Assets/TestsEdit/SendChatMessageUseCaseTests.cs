using NUnit.Framework;
using Moq;

[TestFixture]
public class SendChatMessageUseCaseTests
{
    private Mock<IChatService> chatServiceMock;
    private SendChatMessageUseCase useCase;

    [SetUp]
    public void SetUp()
    {
        chatServiceMock = new Mock<IChatService>();
        useCase = new SendChatMessageUseCase(chatServiceMock.Object);
    }

    [Test]
    public void Execute_Should_CallSendMessage_When_TextIsNotEmpty()
    {
        // Arrange
        int playerId = 1;
        string text = "Hello World";

        // Act
        useCase.Execute(playerId, text);

        // Assert
        chatServiceMock.Verify(c => c.SendMessage(playerId, text, true), Times.Once);
    }

    [TestCase("")]
    [TestCase(" ")]
    [TestCase(null)]
    public void Execute_Should_NotCallSendMessage_When_TextIsEmptyOrNull(string text)
    {
        // Arrange
        int playerId = 1;

        // Act
        useCase.Execute(playerId, text);

        // Assert
        chatServiceMock.Verify(c => c.SendMessage(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<bool>()), Times.Never);
    }
    [Test]
    public void Execute_Should_NotThrow_When_ChatServiceIsNull()
    {
        // Arrange
        SendChatMessageUseCase useCaseWithNullService = new SendChatMessageUseCase(null);
        int playerId = 1;
        string text = "Hello";

        // Act & Assert
        Assert.DoesNotThrow(() => useCaseWithNullService.Execute(playerId, text));
    }

    [Test]
    public void Execute_Should_DoNothing_When_ChatServiceIsNull_AndTextIsEmpty()
    {
        // Arrange
        SendChatMessageUseCase useCaseWithNullService = new SendChatMessageUseCase(null);
        int playerId = 1;
        string text = "";

        // Act & Assert
        Assert.DoesNotThrow(() => useCaseWithNullService.Execute(playerId, text));
    }
}
