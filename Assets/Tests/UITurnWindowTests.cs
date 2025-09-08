using NUnit.Framework;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

public class UITurnWindowTests 
{
    private UITurnWindow window;
    private GameObject windowGO;
    private Button throwDiceButton;
    private bool rollEventReceived;
    private int rollEventPlayerId;

    [SetUp]
    public void Setup()
    {
        // Создаём объект окна
        windowGO = new GameObject("UITurnWindow");
        windowGO.SetActive(false);

        window = windowGO.AddComponent<UITurnWindow>();

        // Создаём кнопку
        var buttonGO = new GameObject("ThrowDiceButton", typeof(Button));
        throwDiceButton = buttonGO.GetComponent<Button>();
        window.ThrowDiceButton = throwDiceButton;

        // Имитируем локального игрока

        var animGO = new GameObject("WindowAnimation");
        var anim = animGO.AddComponent<WindowAnimation>();
        animGO.transform.SetParent(windowGO.transform);

        var rectTransform = new GameObject("RectTrans", typeof(RectTransform));
        rectTransform.GetComponent<RectTransform>().position = Vector3.zero;
        anim.WindowRectTransform = rectTransform.GetComponent<RectTransform>();
        window.WindowAnimation = anim;

        EventBus.ClearAll();
        windowGO.SetActive(true);
        var color = new PlayerColor(1, 0, 0);

        var testPlayer = new PlayerData("Test", 1000, 1, color, null);


    }

    [TearDown]
    public void Teardown()
    {
        EventBus.ClearAll();
        Object.Destroy(windowGO);
        Object.Destroy(throwDiceButton.gameObject);
    }

    [UnityTest]
    public IEnumerator OnEnable_AddsButtonListener()
    {
        yield return null;

        bool clicked = false;
        throwDiceButton.onClick.AddListener(() => clicked = true);

        throwDiceButton.onClick.Invoke();

        Assert.IsTrue(clicked, "ThrowDiceButton listener was not added");
    }

    [UnityTest]
    public IEnumerator OnDisable_RemovesButtonListener()
    {
        window.enabled = true;
        window.enabled = false;

        bool clicked = false;
        throwDiceButton.onClick.AddListener(() => clicked = true);

        throwDiceButton.onClick.Invoke();
        yield return null;

        Assert.IsTrue(clicked, "ThrowDiceButton should be functional after adding new listener post OnDisable");
    }

    [UnityTest]
    public IEnumerator OnThrowButtonClick_PublishesRollEvent()
    {

        yield return null;

        EventBus.Subscribe<RollDiceButtonEvent>(e =>
        {
            rollEventReceived = true;
            rollEventPlayerId = e.PlayerId;
        });

        typeof(UITurnWindow).GetField("localPlayerId",
         System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
         .SetValue(window, 52);

        window.ThrowDiceButton.onClick.Invoke();


        yield return null;

        Assert.IsTrue(rollEventReceived, "RollDiceButtonEvent was not published");
        Assert.AreEqual(52, rollEventPlayerId, "PlayerId in RollDiceButtonEvent is incorrect");

        yield return new WaitForSeconds(0.5f);

        Assert.AreEqual(new Vector3(0, 160, 0), window.WindowAnimation.WindowRectTransform.position);


    }

    [UnityTest]
    public IEnumerator TurnChangeWindow_ShowsWindow_ForLocalPlayer()
    {
        var color = new PlayerColor(1, 0, 0);

        var testPlayer1 = new PlayerData("TestPlayer1", 500, PhotonNetwork.LocalPlayer.ActorNumber, color, null);
        EventBus.Publish(new TurnStartEvent(testPlayer1.Id));
        yield return new WaitForSeconds(0.5f);

        Assert.AreEqual(Vector3.zero, window.WindowAnimation.WindowRectTransform.position);

       

    }

    [UnityTest]
    public IEnumerator TurnChangeWindow_DoesNotShowWindow_ForOtherPlayer()
    {
        var color = new PlayerColor(1, 0, 0);

        var testPlayer2 = new PlayerData("TestPlayer2", 600, 15,color, null);

        EventBus.Publish(new TurnStartEvent(testPlayer2.Id));
        yield return new WaitForSeconds(0.5f);

        Assert.AreEqual(new Vector3(0, 160, 0), window.WindowAnimation.WindowRectTransform.position);
    }
}
