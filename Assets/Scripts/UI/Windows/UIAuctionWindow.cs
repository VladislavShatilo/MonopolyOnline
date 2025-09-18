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

    private void HandlePlayClicked()
    {
        HideWindow();
        onPlay?.Invoke(playerId);
    }

    private void HandlePassClicked()
    {
        HideWindow();
        onPass?.Invoke(playerId);
    }
}
public class PassAuctionRequestEvent
{
    public int PlayerId;
    public PassAuctionRequestEvent(int playerId)
    {
        PlayerId = playerId; 
    }
}
public class PlayAuctionRequestEvent
{
    public int PlayerId;
    public PlayAuctionRequestEvent(int playerId)
    {
        Debug.Log("PlayAuctionRequestEvent");

        PlayerId = playerId;
    }
}