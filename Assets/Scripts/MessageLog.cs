using Photon.Pun;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class MessageLog : MonoBehaviourPun
{
    public static MessageLog Instance { get; private set; }

    [Header("UI")]
    [SerializeField] private RectTransform contentTransform;
    [SerializeField] private TextMeshProUGUI messagePrefab;
    [SerializeField] private TMP_InputField chatInputField;
    [SerializeField] private Button sendButton;
    [SerializeField] private int maxMessages = 50;
    [Inject] private IPlayerRepository playerRepository;

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

    private void Start()
    {
        sendButton.gameObject.SetActive(false);
        sendButton.onClick.AddListener(OnSendClicked);
        chatInputField.onValueChanged.AddListener(OnInputChanged);
        chatInputField.onSubmit.AddListener(OnInputSubmitted); // Enter
    }

    private void OnInputChanged(string text)
    {
        sendButton.gameObject.SetActive(!string.IsNullOrWhiteSpace(text));
    }

    private void OnInputSubmitted(string text)
    {
        SendChatMessage(text);
        chatInputField.text = "";
        chatInputField.ActivateInputField();
    }

    private void OnSendClicked()
    {
        SendChatMessage(chatInputField.text);
        chatInputField.text = "";
        chatInputField.ActivateInputField();
    }

    private void SendChatMessage(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return;

        int localPlayerId = PhotonNetwork.LocalPlayer.ActorNumber;
        photonView.RPC(nameof(RPC_ReceiveChatMessage), RpcTarget.All, localPlayerId, text);
    }

    [PunRPC]
    private void RPC_ReceiveChatMessage(int playerId, string text)
    {
        AddMessage(text, playerId);
    }

    public void AddMessage(string message, int playerId)
    {
        PlayerData player = playerRepository.GetPlayerById(playerId);
        var newMsg = Instantiate(messagePrefab, contentTransform);
        string coloredName = $"<color=#{ColorUtility.ToHtmlStringRGB(player.PlayerColor.ToUnityColor())}>{player.Name}</color>";
        newMsg.text = $"{coloredName}: {message}";

        LayoutRebuilder.ForceRebuildLayoutImmediate(contentTransform);
        messages.Enqueue(newMsg);

        if (messages.Count > maxMessages)
        {
            var oldMsg = messages.Dequeue();
            Destroy(oldMsg.gameObject);
        }
    }
}
