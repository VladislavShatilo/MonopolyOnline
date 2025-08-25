using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIAuctionWindow : MonoBehaviour
{
    public static UIAuctionWindow Instance { get; private set; }

    [SerializeField] private WindowAnimation windowAnimation;
    [SerializeField] private Button playAuctionButton;
    [SerializeField] private Button cancelAuction;
    [SerializeField] private TextMeshProUGUI auctionPriceText;
    [SerializeField] private TextMeshProUGUI headerText;

    private int playerId;
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }
    private void OnEnable()
    {
        EventBus.Subscribe<AuctionPromptBidEvent>(ShowAuctrionWindow);
    }
    private void OnDisable()
    {
        EventBus.Unsubscribe<AuctionPromptBidEvent>(ShowAuctrionWindow);
    }
    private void Start()
    {
        playAuctionButton.onClick.AddListener(() => PlayAuction());
        cancelAuction.onClick.AddListener(() => Cancel());
    }

    public void ShowAuctrionWindow(AuctionPromptBidEvent e)
    {
        if (e.BidderActorNumber == PhotonNetwork.LocalPlayer.ActorNumber)
        {
            string companyName = CellsManager.Instance.GetCellDataByIndex(e.CompanyId).cellName;
            headerText.text = "На аукционе " + companyName;
            auctionPriceText.text = "Поднять до " + e.MinAllowedBid.ToString("N0", CultureInfo.InvariantCulture);
            windowAnimation.ShowWindow();
            playerId = e.BidderActorNumber;
        }
        else
        {
            HideWindow();
        }
      
    }
    public void HideWindow()
    {
        windowAnimation.HideWindow();
    }
    private void Cancel()
    {
        HideWindow();
        AuctionManager.Instance.PassRequest(playerId);
    }
    private void PlayAuction()
    {
        HideWindow();
        AuctionManager.Instance.PlayActionRequest(playerId);
    }

}
