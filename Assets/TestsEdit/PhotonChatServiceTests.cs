using NUnit.Framework;
using Moq;
using Photon.Pun;
using UnityEngine;

public class PhotonChatServiceTests
{
    private PhotonChatService chatService;
    private Mock<IPhotonViewWrapper> viewWrapperMock;
    private Mock<IPhotonNetworkWrapper> networkMock;
    private Mock<IEventBus> eventBusMock;
    private PhotonView photonView;

    [SetUp]
    public void SetUp()
    {
        var go = new GameObject();
        chatService = go.AddComponent<PhotonChatService>();
        photonView = go.AddComponent<PhotonView>();

        viewWrapperMock = new Mock<IPhotonViewWrapper>();
        networkMock = new Mock<IPhotonNetworkWrapper>();
        eventBusMock = new Mock<IEventBus>();

        chatService.Construct(eventBusMock.Object, networkMock.Object, viewWrapperMock.Object);
    }

    [Test]
    public void SendMessage_ShouldAlwaysCallRPC_WhenIsChatTrue()
    {
        chatService.SendMessage(1, "Hello", true);

        viewWrapperMock.Verify(v => v.RPC(chatService.photonView,"RPC_ReceiveMessage",
            RpcTarget.All, 1, "Hello"), Times.Once);
    }

    [Test]
    public void SendMessage_ShouldCallRPC_WhenNotChat_AndIsMasterClient()
    {
        networkMock.Setup(n => n.IsMasterClient).Returns(true);

        chatService.SendMessage(2, "Server message", false);

        viewWrapperMock.Verify(v => v.RPC(chatService.photonView, "RPC_ReceiveMessage",
            RpcTarget.All, 2, "Server message"), Times.Once);
    }

    [Test]
    public void SendMessage_ShouldNotCallRPC_WhenNotChat_AndNotMasterClient()
    {
        networkMock.Setup(n => n.IsMasterClient).Returns(false);

        chatService.SendMessage(2, "Server message", false);

        viewWrapperMock.Verify(v => v.RPC(It.IsAny<PhotonView>(), It.IsAny<string>(), It.IsAny<RpcTarget>(), It.IsAny<object[]>()), Times.Never);
    }

    [Test]
    public void RPC_ReceiveMessage_ShouldPublishEvent()
    {
        var method = typeof(PhotonChatService)
            .GetMethod("RPC_ReceiveMessage", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        method.Invoke(chatService, new object[] { 5, "Test message" });

        eventBusMock.Verify(e => e.Publish(It.Is<ChatMessage>(msg => msg.PlayerId == 5 && msg.Text == "Test message")), Times.Once);
    }
}
