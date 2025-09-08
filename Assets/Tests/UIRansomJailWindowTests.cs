using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

public class UIRansomJailWindowTests
{
    private UIRansomJailWindow window;
    private GameObject windowGO;
    private WindowAnimation mockAnimation;
    private bool ransomEventReceived;
    private int ransomReceivedPlayerId;

    private PlayerData testPlayer;

    [SetUp]
    public void Setup()
    {
        // Создаём объект окна
        windowGO = new GameObject("UIRansomJailWindow");
        windowGO.SetActive(false);
        window = windowGO.AddComponent<UIRansomJailWindow>();

        // Кнопки
        var ransomGO = new GameObject("RansomButton", typeof(Button));
        var cantRansomGO = new GameObject("CantRansomButton", typeof(Button));
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

        EventBus.ClearAll();
        windowGO.SetActive(true);

        // Тестовый игрок
        var color = new PlayerColor(1, 0, 0);

        testPlayer = new PlayerData("Test", 1000, 1, color, null);
    }

    [TearDown]
    public void Teardown()
    {
        EventBus.ClearAll();
        Object.Destroy(windowGO);
        Object.Destroy(mockAnimation.gameObject);
    }

    [UnityTest]
    public IEnumerator ShowWindow_UpdatesUIAndButtons_WhenCanAfford()
    {
        window.ShowWindow(testPlayer);
        yield return null;
        yield return new WaitForSeconds(0.5f);

        Assert.AreEqual(new Vector3(0, 0, 0), window.WindowAnimation.WindowRectTransform.position);

        Assert.IsTrue(window.RansomButton.gameObject.activeSelf, "RansomButton should be active");
        Assert.IsFalse(window.CantRansomButton.gameObject.activeSelf, "CantRansomButton should be inactive");
        Assert.AreEqual("Заплатите 500", window.RansomText.text);
        Assert.AreEqual("Заплатите 500", window.CantRansomText.text);
    }
    [UnityTest]
    public IEnumerator ShowWindow_UpdatesUIAndButtons_WhenCantAfford()
    {
        testPlayer.Money = 0;
        window.ShowWindow(testPlayer);
        yield return null;
        yield return new WaitForSeconds(0.5f);

        Assert.AreEqual(new Vector3(0, 0, 0), window.WindowAnimation.WindowRectTransform.position);

        Assert.IsFalse(window.RansomButton.gameObject.activeSelf, "RansomButton should be active");
        Assert.IsTrue(window.CantRansomButton.gameObject.activeSelf, "CantRansomButton should be inactive");
        Assert.AreEqual("Заплатите 500", window.CantRansomText.text);
        Assert.AreEqual("Заплатите 500", window.CantRansomText.text);
    }
    [UnityTest]
    public IEnumerator RansomButton_PublishesEventAndHidesWindow()
    {
        window.ShowWindow(testPlayer);

        EventBus.Subscribe<ReleaseFromJailEvent>(e =>
        {
            ransomEventReceived = true;
            ransomReceivedPlayerId = e.PlayerID;
        }); 

        window.RansomButton.onClick.Invoke();
        yield return null;
        yield return new WaitForSeconds(0.5f);

        Assert.AreEqual(new Vector3(0, 160, 0), window.WindowAnimation.WindowRectTransform.position);

        Assert.IsTrue(ransomEventReceived, "ReleaseFromJailEvent was not published");
        Assert.AreEqual(testPlayer.Id, ransomReceivedPlayerId, "PlayerID in event incorrect");
        Assert.AreNotEqual(Vector3.zero, window.WindowAnimation.WindowRectTransform.position, "Window should be hidden");
    }

    [UnityTest]
    public IEnumerator OnEnable_AddsButtonListener()
    {
        yield return null;

        bool clicked = false;

        window.RansomButton.onClick.AddListener(() => clicked = true);

        window.RansomButton.onClick.Invoke();
        yield return null;

        Assert.IsTrue(clicked, "RansomButton listener was not added");
    }

    [UnityTest]
    public IEnumerator OnDisable_RemovesButtonListener()
    {
        window.enabled = true;
        window.enabled = false;
        yield return null;

        bool clicked = false;
        window.RansomButton.onClick.AddListener(() => clicked = true);
        window.RansomButton.onClick.Invoke();
        Assert.IsTrue(clicked, "RansomButton listener should be removed after OnDisable");
    }
}
