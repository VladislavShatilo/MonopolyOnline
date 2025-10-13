using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using TMPro;
using UnityEngine.UI;

public class MessageLogViewTests
{
    private GameObject gameObject;
    private MessageLogView view;
    private TMP_InputField inputField;
    private Button sendButton;
    private RectTransform content;
    private TextMeshProUGUI messagePrefab;

    [SetUp]
    public void SetUp()
    {
        // Создаём GameObject с нужными компонентами
        gameObject = new GameObject("MessageLogView");
        view = gameObject.AddComponent<MessageLogView>();

        content = new GameObject("Content").AddComponent<RectTransform>();
        messagePrefab = new GameObject("MessagePrefab").AddComponent<TextMeshProUGUI>();
        inputField = new GameObject("Input").AddComponent<TMP_InputField>();
        sendButton = new GameObject("Button").AddComponent<Button>();

        // Присваиваем ссылки через сериализованные поля
        typeof(MessageLogView)
            .GetField("contentTransform", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .SetValue(view, content);
        typeof(MessageLogView)
            .GetField("messagePrefab", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .SetValue(view, messagePrefab);
        typeof(MessageLogView)
            .GetField("chatInputField", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .SetValue(view, inputField);
        typeof(MessageLogView)
            .GetField("sendButton", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .SetValue(view, sendButton);
        typeof(MessageLogView)
            .GetField("maxMessages", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .SetValue(view, 3);
    }

    [UnityTest]
    public IEnumerator Start_ShouldDeactivateSendButton_AndAddListeners()
    {
        yield return null;

        view.SendMessage("Start");

        // 1. Кнопка должна быть выключена
        Assert.IsFalse(sendButton.gameObject.activeSelf, "Send button should be inactive after Start");

        // 2. Проверим, что при вводе текста кнопка становится активной
        inputField.text = "hello";
        inputField.onValueChanged.Invoke("hello");
        Assert.IsTrue(sendButton.gameObject.activeSelf, "Button should activate on non-empty input");

        // 3. Проверим, что по клику вызывается OnSendMessage
        bool messageSent = false;
        view.OnSendClicked += _ => messageSent = true;

        inputField.text = "test";
        sendButton.onClick.Invoke(); // Симулируем клик

        Assert.IsTrue(messageSent, "Click should trigger OnSendClicked event");
    }

    [UnityTest]
    public IEnumerator OnInputChanged_ShouldToggleSendButton()
    {
        view.SendMessage("Start");

        inputField.text = "hello";
        inputField.onValueChanged.Invoke("hello");
        Assert.IsTrue(sendButton.gameObject.activeSelf);

        inputField.text = "";
        inputField.onValueChanged.Invoke("");
        Assert.IsFalse(sendButton.gameObject.activeSelf);

        yield break;
    }

    [UnityTest]
    public IEnumerator OnSendMessage_ShouldInvokeEvent_AndClearInput()
    {
        string receivedText = null;
        view.OnSendClicked += msg => receivedText = msg;

        view.SendMessage("Start");
        inputField.text = "Hi!";
        view.SendMessage("OnSendMessage", "Hi!");

        Assert.AreEqual("Hi!", receivedText);
        Assert.AreEqual("", inputField.text);

        yield break;
    }

    [UnityTest]
    public IEnumerator AddMessage_ShouldInstantiateAndLimitMessages()
    {
        view.SendMessage("Start");

        for (int i = 0; i < 5; i++)
            view.AddMessage($"Message {i}");

        Assert.AreEqual(5, content.childCount, "Should not exceed maxMessages");
        Assert.AreEqual("Message 2", content.GetChild(2).GetComponent<TextMeshProUGUI>().text);

        yield break;
    }
}
