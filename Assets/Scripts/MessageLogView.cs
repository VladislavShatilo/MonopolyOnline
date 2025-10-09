using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MessageLogView : MonoBehaviour, IMessageLogView
{
    #region EVENTS

    public event System.Action<string> OnSendClicked;

    #endregion EVENTS

    [SerializeField] private RectTransform contentTransform;
    [SerializeField] private TextMeshProUGUI messagePrefab;
    [SerializeField] private TMP_InputField chatInputField;
    [SerializeField] private Button sendButton;
    [SerializeField] private int maxMessages = 50;

    private readonly Queue<TextMeshProUGUI> messages = new();

    #region LIFE_CYCLE

    private void Start()
    {
        sendButton.gameObject.SetActive(false);
        sendButton.onClick.AddListener(() => OnSendMessage(chatInputField.text));
        chatInputField.onValueChanged.AddListener(OnInputChanged);
        chatInputField.onSubmit.AddListener(OnSendMessage);
    }

    #endregion LIFE_CYCLE

    #region PUBLIC_METHODS

    public void AddMessage(string formattedText)
    {
        var newMsg = Instantiate(messagePrefab, contentTransform);
        newMsg.text = formattedText;
        messages.Enqueue(newMsg);

        if (messages.Count > maxMessages)
        {
            var oldMsg = messages.Dequeue();
            Destroy(oldMsg.gameObject);
        }
    }

    #endregion PUBLIC_METHODS

    #region CALLBACKS

    private void OnInputChanged(string text)
    {
        sendButton.gameObject.SetActive(!string.IsNullOrWhiteSpace(text));
    }

    private void OnSendMessage(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return;
        OnSendClicked?.Invoke(text);
        chatInputField.text = "";
        chatInputField.ActivateInputField();
    }

    #endregion CALLBACKS
}