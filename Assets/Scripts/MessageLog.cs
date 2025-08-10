using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MessageLog : MonoBehaviour
{
    public static MessageLog Instance { get; private set; }

    [SerializeField] private RectTransform contentTransform;
    [SerializeField] private TextMeshProUGUI messagePrefab;
    [SerializeField] private int maxMessages = 50;

    private readonly Queue<TextMeshProUGUI> messages = new Queue<TextMeshProUGUI>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void AddMessage(string message)
    {
        var newMsg = Instantiate(messagePrefab, contentTransform);
        newMsg.text = message;
        messages.Enqueue(newMsg);

        if (messages.Count > maxMessages)
        {
            var oldMsg = messages.Dequeue();
            Destroy(oldMsg.gameObject);
        }
    }
}
