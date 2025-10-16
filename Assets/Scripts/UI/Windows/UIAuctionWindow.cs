using Photon.Pun;
using System;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIAuctionWindow : UIWindowBase, IAuctionWindow
{
    [Header("UI References")]
    [SerializeField] private Button playButton;

    [SerializeField] private Button cantPlayButton;
    [SerializeField] private Button cancelButton;
    [SerializeField] private TextMeshProUGUI playPriceText;
    [SerializeField] private TextMeshProUGUI cantPriceText;
    [SerializeField] private TextMeshProUGUI headerText;

    private int playerId;
    private Action<int> onPlay;
    private Action<int> onPass;

    #region LIFE_CYCLE
    public void ValidateUI()
    {
        if (playButton == null) throw new ArgumentNullException(nameof(playButton));
        if (cantPlayButton == null) throw new ArgumentNullException(nameof(cantPlayButton));
        if (cancelButton == null) throw new ArgumentNullException(nameof(cancelButton));
        if (playPriceText == null) throw new ArgumentNullException(nameof(playPriceText));
        if (cantPriceText == null) throw new ArgumentNullException(nameof(cantPriceText));
        if (headerText == null) throw new ArgumentNullException(nameof(headerText));
    }

    private void Start() => ValidateUI();
    protected void OnEnable()
    {
        playButton.onClick.AddListener(HandlePlayClicked);
        cancelButton.onClick.AddListener(HandlePassClicked);
    }

    protected void OnDisable()
    {
        playButton.onClick.RemoveListener(HandlePlayClicked);
        cancelButton.onClick.RemoveListener(HandlePassClicked);
    }

    #endregion LIFE_CYCLE

    #region PUBLIC_METHODS

    public void Show(int playerId, string companyName, int minAllowedBid, int money)
    {
        this.playerId = playerId;

        bool canAfford = money >= minAllowedBid;

        playButton.gameObject.SetActive(canAfford);
        cantPlayButton.gameObject.SetActive(!canAfford);

        headerText.text = $"На аукционе {companyName}";
        playPriceText.text = $"Поднять до {minAllowedBid.ToString("N0", CultureInfo.InvariantCulture)}";
        cantPriceText.text = $"Поднять до {minAllowedBid.ToString("N0", CultureInfo.InvariantCulture)}";

        ShowWindow();
    }

    public void Hide() => HideWindow();

    public void SetPlayAction(Action<int> onPlay) => this.onPlay = onPlay;

    public void SetPassAction(Action<int> onPass) => this.onPass = onPass;

    public Button PlayButton { get => playButton; set => playButton = value; }
    public Button CantPlayButton { get => cantPlayButton; set => cantPlayButton = value; }
    public Button CancelButton { get => cancelButton; set => cancelButton = value; }
    public TextMeshProUGUI PlayPriceText { get => playPriceText; set => playPriceText = value; }
    public TextMeshProUGUI CantPriceText { get => cantPriceText; set => cantPriceText = value; }
    public TextMeshProUGUI HeaderText { get => headerText; set => headerText = value; }

    #endregion PUBLIC_METHODS

    #region CALLBACKS

    protected void HandlePlayClicked()
    {
        HideWindow();
        onPlay?.Invoke(playerId);
    }

    protected void HandlePassClicked()
    {
        HideWindow();
        onPass?.Invoke(playerId);
    }

    #endregion CALLBACKS
}