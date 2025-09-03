using Photon.Pun;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIAuctionWindow : UIWindowBase<UIAuctionWindow>
{
    [Header("UI References")]
    [SerializeField] private Button playButton;
    [SerializeField] private Button cantPlayButton;
    [SerializeField] private Button cancelButton;
    [SerializeField] private TextMeshProUGUI playPriceText;
    [SerializeField] private TextMeshProUGUI cantPriceText;
    [SerializeField] private TextMeshProUGUI headerText;

    private int playerId;
    public Button PlayButton
    {
        get => playButton;
        set => playButton = value;
    }

    public Button CantPlayButton
    {
        get => cantPlayButton;
        set => cantPlayButton = value;
    }

    public Button CancelButton
    {
        get => cancelButton;
        set => cancelButton = value;
    }

    public TextMeshProUGUI PlayPriceText
    {
        get => playPriceText;
        set => playPriceText = value;
    }

    public TextMeshProUGUI CantPriceText
    {
        get => cantPriceText;
        set => cantPriceText = value;
    }
    public TextMeshProUGUI HeaderText
    {
        get => headerText;
        set => headerText = value;
    }
    #region Unity Lifecycle
    protected override void SubscribeEvents()
    {
        EventBus.Subscribe<AuctionPromptBidEvent>(OnAuctionPromptBid);
    }

    protected override void UnsubscribeEvents()
    {
        EventBus.Unsubscribe<AuctionPromptBidEvent>(OnAuctionPromptBid);
    }
    protected override void OnEnable()
    {
        base.OnEnable();
        if (playButton != null && cancelButton != null)
        {
            playButton.onClick.AddListener(HandlePlayClicked);
            cancelButton.onClick.AddListener(HandleCancelClicked);
        }
        else
        {
            if (playButton == null)
            {
                Debug.Log(playButton.name + "is null"); 
            }

        }

    }
    protected override void OnDisable()
    {
        base.OnDisable();
        if (playButton != null && cancelButton != null)
        {
            Debug.Log(playButton.name);
            playButton.onClick.RemoveListener(HandlePlayClicked);
            cancelButton.onClick.RemoveListener(HandleCancelClicked);
        }
    }

    #endregion

    #region Event Handling
    private void OnAuctionPromptBid(AuctionPromptBidEvent e)
    {
        if (e.Player.id != PhotonNetwork.LocalPlayer.ActorNumber)
        {
            HideWindow();
            return;
        }

        UpdateUI(e);
        ShowWindow();
    }
    #endregion

    #region UI Logic
    private void UpdateUI(AuctionPromptBidEvent e)
    {
        bool canAfford = e.Player.Money >= e.MinAllowedBid;

        if (playButton != null && cantPlayButton != null)
        {
            playButton.gameObject.SetActive(canAfford);
            cantPlayButton.gameObject.SetActive(!canAfford);
        }
          

        if (headerText != null && playPriceText != null && cantPriceText!=null)
        {
            headerText.text = $"На аукционе {e.CompanyName}";
            playPriceText.text = $"Поднять до {e.MinAllowedBid.ToString("N0", CultureInfo.InvariantCulture)}";
            cantPriceText.text = $"Поднять до {e.MinAllowedBid.ToString("N0", CultureInfo.InvariantCulture)}";
        }
       
        playerId = e.Player.id;
    }
    #endregion

    #region Button Callbacks
    private void HandleCancelClicked()
    {
        Debug.Log("HandleCancelClicked");
        HideWindow();
        EventBus.Publish(new PassAuctionRequestEvent(playerId));
    }

    private void HandlePlayClicked()
    {
        HideWindow(); 
        Debug.Log("HandlePlayClicked");

        EventBus.Publish(new PlayAuctionRequestEvent(playerId));
    }
    #endregion
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