using NUnit.Framework;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

public class UIAuctionWindowTests 
{
    private UIAuctionWindow window;
    private PlayerData testPlayer;
    private GameObject windowGO;
    private bool playEventReceived;
    private int playReceivedPlayerId;
    private bool cancelEventReceived;
    private int cancelReceivedPlayerId;
   
    [SetUp]
    public void Setup()
    {
        windowGO = new GameObject("UIAuctionWindow");
        windowGO.SetActive(false); // объект неактивен

        window = windowGO.AddComponent<UIAuctionWindow>();

        // Сначала создаём кнопки и тексты
        var playAuctionBtn = new GameObject("PlayBtn", typeof(Button), typeof(RectTransform));
        var cantPlayBtn = new GameObject("CantPlayBtn", typeof(Button), typeof(RectTransform));
        var cancelBtn = new GameObject("CancelBtn", typeof(Button), typeof(RectTransform));

        var playText = new GameObject("PlayText", typeof(TextMeshProUGUI));
        var cantPlayText = new GameObject("CantPlayText", typeof(TextMeshProUGUI));
            var headerText = new GameObject("HeaderText", typeof(TextMeshProUGUI));
         
        // Присваиваем **до SetActive(true)**!
        window.PlayButton = playAuctionBtn.GetComponent<Button>();
        window.CantPlayButton = cantPlayBtn.GetComponent<Button>();
        window.CancelButton = cancelBtn.GetComponent<Button>();

        window.PlayPriceText = playText.GetComponent<TextMeshProUGUI>();
        window.CantPriceText = cantPlayText.GetComponent<TextMeshProUGUI>();
        window.HeaderText = headerText.GetComponent<TextMeshProUGUI>();


        var animGO = new GameObject("WindowAnimation");
        var anim = animGO.AddComponent<WindowAnimation>();
        animGO.transform.SetParent(windowGO.transform);

        var rectTransform = new GameObject("RectTrans", typeof(RectTransform));
        rectTransform.GetComponent<RectTransform>().position = Vector3.zero;
        anim.WindowRectTransform = rectTransform.GetComponent<RectTransform>();

        window.WindowAnimation = anim;
        EventBus.ClearAll();

        testPlayer = new PlayerData("TestPlayer", 0, 0, Color.red, null);
        windowGO.SetActive(true);
    }
    [TearDown]
    public void Teardown()
    {
        EventBus.ClearAll();
        Object.Destroy(windowGO);
    
    }
    [UnityTest]
    public IEnumerator OnAuctionPromptBid_ForOtherPlayer_HidesWindow()
    {
        var testPlayer1 = new PlayerData("TestPlayer1", 500, PhotonNetwork.LocalPlayer.ActorNumber, Color.red, null);
        var testPlayer2 = new PlayerData("TestPlayer2",600, PhotonNetwork.LocalPlayer.ActorNumber+1, Color.blue, null);

       // EventBus.Publish(new AuctionPromptBidEvent(testPlayer2, "Mers",500));

        yield return null;
       
        Assert.AreEqual(new Vector3(0,160,0), window.WindowAnimation.WindowRectTransform.position);

        window.HideWindow();

      //  EventBus.Publish(new AuctionPromptBidEvent(testPlayer1, "Mers", 500));

        yield return new WaitForSeconds(0.5f);

        Assert.AreEqual(Vector3.zero, window.WindowAnimation.WindowRectTransform.position);
    }
    [UnityTest]
    public IEnumerator OnAuctionPromptBid_ForLocalPlayer_ShowsWindowAndUpdatesUI_WhenCanAfford()
    {
        var testPlayer1 = new PlayerData("TestPlayer1", 5000, PhotonNetwork.LocalPlayer.ActorNumber, Color.red, null);

      //  EventBus.Publish(new AuctionPromptBidEvent(testPlayer1, "Mers", 600));


        yield return new WaitForSeconds(0.5f);

        Assert.AreEqual(Vector3.zero, window.WindowAnimation.WindowRectTransform.position);
        Assert.AreEqual("На аукционе Mers", window.HeaderText.text);
        Assert.AreEqual("Поднять до 600", window.PlayPriceText.text);
        Assert.IsTrue(window.PlayButton.gameObject.activeSelf);
        Assert.IsFalse(window.CantPlayButton.gameObject.activeSelf);
    }
    [UnityTest]
    public IEnumerator OnAuctionPromptBid_ForLocalPlayer_ShowsCantPlay_WhenNotEnoughMoney()
    {
        var testPlayer2 = new PlayerData("TestPlayer2", 100, PhotonNetwork.LocalPlayer.ActorNumber, Color.red, null);

        //EventBus.Publish(new AuctionPromptBidEvent(testPlayer2, "Honda", 600));

        yield return new WaitForSeconds(0.5f);

        Assert.AreEqual(Vector3.zero, window.WindowAnimation.WindowRectTransform.position);
        //Assert.AreEqual("На аукционе Honda", window.HeaderText.text);
        Assert.AreEqual("Поднять до 600", window.CantPriceText.text);
        Assert.IsFalse(window.PlayButton.gameObject.activeSelf);
        Assert.IsTrue(window.CantPlayButton.gameObject.activeSelf);
    }
    [UnityTest]
    public IEnumerator PlayButton_PublishesPlayAuctionRequestEvent()
    {
        yield return null;

        EventBus.Subscribe<PlayAuctionRequestEvent>(e =>
        {
            playEventReceived = true;
            playReceivedPlayerId = e.PlayerId;
        });

        typeof(UIAuctionWindow).GetField("playerId",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .SetValue(window, 42);

        window.PlayButton.onClick.Invoke();
        yield return new WaitForSeconds(0.1f);


        Assert.IsTrue(playEventReceived);
        Assert.AreEqual(42, playReceivedPlayerId);

        yield return new WaitForSeconds(0.5f);

        Assert.AreEqual(new Vector3(0,160,0), window.WindowAnimation.WindowRectTransform.position);

    }

    [UnityTest]
    public IEnumerator CancelButton_PublishesPassAuctionRequestEvent()
    {
        var testPlayer3 = new PlayerData("TestPlayer3", 100, 3, Color.red, null);

       // EventBus.Publish(new AuctionPromptBidEvent(testPlayer3, "Honda", 1000));

        EventBus.Subscribe<PassAuctionRequestEvent>(e =>
        {
            cancelEventReceived = true;
            cancelReceivedPlayerId = e.PlayerId;
        });

        typeof(UIAuctionWindow).GetField("playerId",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .SetValue(window, 77);

        window.CancelButton.onClick.Invoke();

        yield return null;

        Assert.IsTrue(cancelEventReceived);
        Assert.AreEqual(77, cancelReceivedPlayerId);

        yield return new WaitForSeconds(0.5f);

        Assert.AreEqual(new Vector3(0, 160, 0), window.WindowAnimation.WindowRectTransform.position);

    }

    [Test]
    public void OnEnable_AddsListeners_AndOnDisable_RemovesThem()
    {
        window.enabled = false;
        window.enabled = true;

        int playCalls = window.PlayButton.onClick.GetPersistentEventCount();
        int cancelCalls = window.CancelButton.onClick.GetPersistentEventCount();

        Assert.AreEqual(0, playCalls);
        Assert.AreEqual(0, cancelCalls);
    }
}
