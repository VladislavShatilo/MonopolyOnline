using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Collections;
using Moq;
using TMPro;
using UnityEngine.UI;
using Photon.Pun;

public class PlayerSkinPlayModeTests
{
    private GameObject go;
    private PlayerSkin playerSkin;
    private Mock<IEventBus> eventBusMock;
    private Mock<IPhotonViewWrapper> photonViewWrapperMock;

    [SetUp]
    public void Setup()
    {
        go = new GameObject();
        playerSkin = go.AddComponent<PlayerSkin>();

        var imageGO = new GameObject();
        imageGO.transform.parent = go.transform;
        var image = imageGO.AddComponent<Image>();

        var textGO = new GameObject();
        textGO.transform.parent = go.transform;
        var text = textGO.AddComponent<TextMeshProUGUI>();

        // Устанавливаем SerializeField вручную
        typeof(PlayerSkin).GetField("playerSkinImage", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .SetValue(playerSkin, image);
        typeof(PlayerSkin).GetField("turnJailText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .SetValue(playerSkin, text);

        eventBusMock = new Mock<IEventBus>();
        photonViewWrapperMock = new Mock<IPhotonViewWrapper>();

        playerSkin.Initialize(eventBusMock.Object, photonViewWrapperMock.Object);

        // Создаём PhotonView для теста OwnerActorNr
        var photonView = go.AddComponent<PhotonView>();
        photonView.ViewID = 1;
        photonView.OwnerActorNr = 1;
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(go);
    }

    [UnityTest]
    public IEnumerator SetColorDirect_ShouldChangeImageColor()
    {
        Color target = Color.red;
        playerSkin.SetColorDirect(target);

        yield return null;

        var image = typeof(PlayerSkin).GetField("playerSkinImage", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .GetValue(playerSkin) as Image;

        Assert.AreEqual(target, image.color);
    }

    [UnityTest]
    public IEnumerator RPC_SetTurnJain_ShouldShowTextCorrectly()
    {
        var text = typeof(PlayerSkin).GetField("turnJailText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .GetValue(playerSkin) as TextMeshProUGUI;

        // Проверим скрытие текста
        var method = playerSkin.GetType().GetMethod("RPC_SetTurnJain", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        method.Invoke(playerSkin, new object[] { 0 });

        yield return null;
        Assert.IsFalse(text.gameObject.activeSelf);

        // Проверим показ текста
        method.Invoke(playerSkin, new object[] { 3 });
        yield return null;
        Assert.IsTrue(text.gameObject.activeSelf);
        Assert.AreEqual("3", text.text);
    }

    [UnityTest]
    public IEnumerator SetTurnJainEvent_ShouldCallRPC()
    {
        // Имитируем событие
        var photonView = go.GetComponent<PhotonView>();
        var evt = new SetTurnsJailEvent(photonView.OwnerActorNr,2);

        var method = playerSkin.GetType().GetMethod("SetTurnJain", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        method.Invoke(playerSkin, new object[] { evt });

        yield return null;

        photonViewWrapperMock.Verify(p => p.RPC(photonView, "RPC_SetTurnJain", RpcTarget.AllBuffered, 2), Times.Once);
    }
}
