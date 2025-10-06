using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MessageLogView : MonoBehaviour
{
    [SerializeField] private RectTransform contentTransform;
    [SerializeField] private TextMeshProUGUI messagePrefab;
    [SerializeField] private TMP_InputField chatInputField;
    [SerializeField] private Button sendButton;
    [SerializeField] private int maxMessages = 50;

    private readonly Queue<TextMeshProUGUI> messages = new();

    public event System.Action<string> OnSendClicked;

    private void Start()
    {
        sendButton.gameObject.SetActive(false);
        sendButton.onClick.AddListener(() => TrySend(chatInputField.text));
        chatInputField.onValueChanged.AddListener(OnInputChanged);
        chatInputField.onSubmit.AddListener(TrySend);
    }

    private void OnInputChanged(string text)
    {
        sendButton.gameObject.SetActive(!string.IsNullOrWhiteSpace(text));
    }

    private void TrySend(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return;
        OnSendClicked?.Invoke(text);
        chatInputField.text = "";
        chatInputField.ActivateInputField();
    }

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
}
