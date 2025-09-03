using DG.Tweening;
using Photon.Pun;
using Photon.Realtime;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UITradeWindow : UITradeWindowBase
{
    public static UITradeWindow Instance { get; private set; }

    [Header("Buttons & Inputs")]
    [SerializeField] private Button offerButton;
    [SerializeField] private Button closeButton;
    [SerializeField] private TMP_InputField leftMoneyInputField;
    [SerializeField] private TMP_InputField rightMoneyInputField;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        offerButton.onClick.AddListener(OnOffer);
        closeButton.onClick.AddListener(OnClose);
    }

    private void OnEnable()
    {
        leftMoneyInputField.onEndEdit.AddListener(OnLeftMoneyChanged);
        rightMoneyInputField.onEndEdit.AddListener(OnRightMoneyChanged);

        EventBus.Subscribe<TradeStartedEvent>(OnTradeStarted);
        EventBus.Subscribe<TradeUpdatedEvent>(OnTradeUpdated);
        EventBus.Subscribe<TradeCancelledEvent>(_ => ClearUI());
    }

    private void OnDisable()
    {
        leftMoneyInputField.onEndEdit.RemoveListener(OnLeftMoneyChanged);
        rightMoneyInputField.onEndEdit.RemoveListener(OnRightMoneyChanged);

        EventBus.Unsubscribe<TradeStartedEvent>(OnTradeStarted);
        EventBus.Unsubscribe<TradeUpdatedEvent>(OnTradeUpdated);
        EventBus.Unsubscribe<TradeCancelledEvent>(_ => ClearUI());
    }

    private void OnTradeStarted(TradeStartedEvent e)
    {
        if (PhotonNetwork.LocalPlayer.ActorNumber != e.FromPlayerId) return;
        ClearUI();
        currentOffer = e.Offer;
        RefreshUI();
        ShowWindow();
    }

    private void OnTradeUpdated(TradeUpdatedEvent e)
    {
        currentOffer = e.Offer;
        RefreshUI();
    }

    private void OnLeftMoneyChanged(string value)
    {
        if (int.TryParse(value, out int amount) && currentOffer != null)
        {
            TradeManager.Instance.SetMoney(currentOffer.FromPlayerId, amount);
            currentOffer.FromMoney = amount;
            RefreshUI();
        }
    }

    private void OnRightMoneyChanged(string value)
    {
        if (int.TryParse(value, out int amount) && currentOffer != null)
        {
            TradeManager.Instance.SetMoney(currentOffer.ToPlayerId, amount);
            currentOffer.ToMoney = amount;
            RefreshUI();
        }
    }

    private void OnOffer()
    {
        if (currentOffer == null || !currentOffer.IsValid()) return;
        TradeManager.Instance.OfferTrade();
        HideWindow();
    }

    private void OnClose()
    {
        TradeManager.Instance.CancelTrade();
        HideWindow();
    }
}
