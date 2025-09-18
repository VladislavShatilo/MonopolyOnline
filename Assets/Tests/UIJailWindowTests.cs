using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

public class UIJailWindowTests
{
    private UIJailWindow window;
    private GameObject windowGO;
    private WindowAnimation mockAnimation;
    private bool rollEventReceived;
    private int rollReceivedPlayerId;
    private bool ransomEventReceived;
    private int ransomReceivedPlayerId;

    private PlayerData testPlayer;

    [SetUp]
    public void Setup()
    {
        // Создаём объект окна
        windowGO = new GameObject("UIJailWindow");
        windowGO.SetActive(false);
        window = windowGO.AddComponent<UIJailWindow>();

        // Кнопки
        var throwDiceGO = new GameObject("ThrowDiceButton", typeof(Button));
        var ransomGO = new GameObject("RansomButton", typeof(Button));
        var cantRansomGO = new GameObject("CantRansomButton", typeof(Button));
        window.ThrowDiceButton = throwDiceGO.GetComponent<Button>();
        window.RansomButton = ransomGO.GetComponent<Button>();
        window.CantRansomButton = cantRansomGO.GetComponent<Button>();

        // Тексты
        var ransomTextGO = new GameObject("RansomText", typeof(TextMeshProUGUI));
        var cantRansomTextGO = new GameObject("CantRansomText", typeof(TextMeshProUGUI));
        window.RansomText = ransomTextGO.GetComponent<TextMeshProUGUI>();
        window.CantRansomText = cantRansomTextGO.GetComponent<TextMeshProUGUI>();

        // Заглушка WindowAnimation
        var animGO = new GameObject("WindowAnimation");
        mockAnimation = animGO.AddComponent<WindowAnimation>();
        window.WindowAnimation = mockAnimation;

        var rectGO = new GameObject("Rect", typeof(RectTransform));
        rectGO.transform.SetParent(animGO.transform);
        mockAnimation.WindowRectTransform = rectGO.GetComponent<RectTransform>();

        //EventBus.ClearAll();

        windowGO.SetActive(true); // OnEnable отработает

        // Тестовый игрок
        var color = new PlayerColor(1, 0, 0);

        testPlayer = new PlayerData("Test", 1000, 1, color, null);
    }

    [TearDown]
    public void Teardown()
    {
        //EventBus.ClearAll();
        Object.Destroy(windowGO);
        Object.Destroy(mockAnimation.gameObject);
    }

    [UnityTest]
    public IEnumerator OnEnable_AddsButtonListeners()
    {
        yield return null;

        bool throwDiceClicked = false;
        window.ThrowDiceButton.onClick.AddListener(() => throwDiceClicked = true);
        bool ransomClicked = false;
        window.RansomButton.onClick.AddListener(() => ransomClicked = true);

        window.ThrowDiceButton.onClick.Invoke();
        window.RansomButton.onClick.Invoke();

        Assert.IsTrue(throwDiceClicked, "ThrowDiceButton listener not added");
        Assert.IsTrue(ransomClicked, "RansomButton listener not added");
    }

    [UnityTest]
    public IEnumerator ShowWindow_UpdatesUIAndSetsPlayerID()
    {
        testPlayer.Id = 42;
        window.ShowWindow(testPlayer);
        yield return null;

        int playerID = (int)typeof(UIJailWindow)
            .GetField("playerID", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .GetValue(window);
        yield return new WaitForSeconds(0.5f);

        Assert.AreEqual(new Vector3(0, 0, 0), window.WindowAnimation.WindowRectTransform.position);

        Assert.AreEqual(42, playerID);
        Assert.AreEqual("Заплатите 500", window.RansomText.text);
        Assert.AreEqual("Заплатите 500", window.CantRansomText.text);
        Assert.IsTrue(window.RansomButton.gameObject.activeSelf);
        Assert.IsFalse(window.CantRansomButton.gameObject.activeSelf);
    }
    [UnityTest]
    public IEnumerator ShowWindow_UpdatesUIAndSetsPlayerIDNoMoney()
    {
        testPlayer.Money = 0;
        testPlayer.Id = 42;

        window.ShowWindow(testPlayer);
        yield return null;

        int playerID = (int)typeof(UIJailWindow)
            .GetField("playerID", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .GetValue(window);
        yield return new WaitForSeconds(0.5f);

        Assert.AreEqual(new Vector3(0, 0, 0), window.WindowAnimation.WindowRectTransform.position);

        Assert.AreEqual(42, playerID);
        Assert.AreEqual("Заплатите 500", window.RansomText.text);
        Assert.AreEqual("Заплатите 500", window.CantRansomText.text);
        Assert.IsFalse(window.RansomButton.gameObject.activeSelf);
        Assert.IsTrue(window.CantRansomButton.gameObject.activeSelf);
    }
    [UnityTest]
    public IEnumerator ThrowDiceButton_PublishesEventAndHidesWindow()
    {
        testPlayer.Id = 7;

        window.ShowWindow(testPlayer);

        //EventBus.Subscribe<RollDiceJailButtonEvent>(e =>
        //{
        //    rollEventReceived = true;
        //    rollReceivedPlayerId = e.PlayerId;
        //});

        window.ThrowDiceButton.onClick.Invoke();
        yield return null;
   
        Assert.IsTrue(rollEventReceived, "RollDiceJailButtonEvent was not published");
        Assert.AreEqual(7, rollReceivedPlayerId);
        Assert.AreNotEqual(Vector3.zero, window.WindowAnimation.WindowRectTransform.position, "Window should be hidden");
        yield return new WaitForSeconds(0.5f);

        Assert.AreEqual(new Vector3(0, 160, 0), window.WindowAnimation.WindowRectTransform.position);

    }

    [UnityTest]
    public IEnumerator RansomButton_PublishesEventAndHidesWindow()
    {
        testPlayer.Id = 13;

        //EventBus.Subscribe<ReleaseFromJailEvent>(e =>
        //{
        //    ransomEventReceived = true;
        //    ransomReceivedPlayerId = e.PlayerID;
        //});

        window.RansomButton.onClick.Invoke();
        yield return null;

        Assert.IsTrue(ransomEventReceived, "ReleaseFromJailEvent was not published");
        Assert.AreEqual(13, ransomReceivedPlayerId);
        Assert.AreNotEqual(Vector3.zero, window.WindowAnimation.WindowRectTransform.position, "Window should be hidden");
        yield return new WaitForSeconds(0.5f);

        Assert.AreEqual(new Vector3(0, 160, 0), window.WindowAnimation.WindowRectTransform.position);

    }
}
