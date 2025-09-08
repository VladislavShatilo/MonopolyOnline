using DG.Tweening;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UITradeReviewWindow : UITradeWindowBase
{
    public static UITradeReviewWindow Instance { get; private set; }

    [Header("Buttons")]
    [SerializeField] private Button acceptButton;
    [SerializeField] private Button cancelButton;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        acceptButton.onClick.AddListener(TradeManager.Instance.AcceptTrade);
        cancelButton.onClick.AddListener(TradeManager.Instance.DeclineTrade);
    }

    private void OnEnable()
    {
        EventBus.Subscribe<TradeCancelledEvent>(_ => ClearUI());
        EventBus.Subscribe<TradeProposalReceivedEvent>(OnTradeProposalReceived);
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe<TradeCancelledEvent>(_ => ClearUI());
        EventBus.Unsubscribe<TradeProposalReceivedEvent>(OnTradeProposalReceived);
    }

    private void OnTradeProposalReceived(TradeProposalReceivedEvent e)
    {
        if (PhotonNetwork.LocalPlayer.ActorNumber == e.FromPlayerId) return;

        currentOffer = e.Offer;
        ShowWindow();

        bool isRecipient = PhotonNetwork.LocalPlayer.ActorNumber == currentOffer.ToPlayerData.Id;
        acceptButton.gameObject.SetActive(isRecipient);
        cancelButton.gameObject.SetActive(isRecipient);

        RefreshUI();
    }
}
