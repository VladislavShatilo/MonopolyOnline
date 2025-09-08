using DG.Tweening;
using Photon.Pun;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIBuyWindow : UIWindowBase
{
    [Header("Buttons")]
    [SerializeField] private Button buyButton;
    [SerializeField] private Button cantBuyButton;
    [SerializeField] private Button auctionButton;

    [Header("Texts")]
    [SerializeField] private TextMeshProUGUI buyButtonText;
    [SerializeField] private TextMeshProUGUI cantBuyButtonText;

    private int currentCellIndex;
    private int companyPrice;
    private PlayerData currentPlayer;


    public Button BuyButton
    {
        get => buyButton;
        set
        {
            buyButton = value;
           
        }
    }

    public Button CantBuyButton
    {
        get => cantBuyButton;
        set => cantBuyButton = value;
    }

    public Button AuctionButton
    {
        get => auctionButton;
        set => auctionButton = value;
    }

    public TextMeshProUGUI BuyButtonText
    {
        get => buyButtonText;
        set => buyButtonText = value;
    }

    public TextMeshProUGUI CantBuyButtonText
    {
        get => cantBuyButtonText;
        set => cantBuyButtonText = value;
    }
    protected void OnEnable()
    {

        if (buyButton != null)
        {
            buyButton.onClick.AddListener(HandleBuyClicked);
        }
        if (auctionButton != null)
        {
            auctionButton.onClick.AddListener(HandleAuctionClicked);
        }
  
    }
    protected  void OnDisable()
    {
        if (buyButton != null)
        {
            buyButton.onClick.RemoveListener(HandleBuyClicked);

        }
        if (auctionButton != null)
        {
            auctionButton.onClick.RemoveListener(HandleAuctionClicked);
        }
    }
    public void ShowBuyWindow(PlayerData player,int cellIndex, int price)
    {
        currentPlayer = player;
        currentCellIndex = cellIndex;
        companyPrice = price;

        UpdateButtons(price);

        windowAnimation.ShowWindow();
    }

    private void UpdateButtons(int price)
    {
        bool canAfford = currentPlayer.Money >= companyPrice;

        buyButton.gameObject.SetActive(canAfford);
        SetButtonText(buyButtonText, companyPrice);

       cantBuyButton.gameObject.SetActive(!canAfford);
        SetButtonText(cantBuyButtonText, companyPrice);
    }
    private void SetButtonText(TextMeshProUGUI textElement, int price)
    {
        textElement.text = $"Купить за {FormatPrice(price)}";
    }
    private string FormatPrice(int price) =>
        price.ToString("N0", CultureInfo.InvariantCulture);

    private void HandleBuyClicked()
    {
        HideWindow();
        EventBus.Publish(new TryBuyCompanyEvent(currentCellIndex));
    }

    private void HandleAuctionClicked()
    {
        HideWindow();
        EventBus.Publish(new StartAuctionEvent(currentPlayer, currentCellIndex, companyPrice));
    }
}
public class TryBuyCompanyEvent
{
    public int CellIndex;
    public TryBuyCompanyEvent(int cellIndex)
    {
        CellIndex = cellIndex;
    }

}
public class StartAuctionEvent
{
    public PlayerData Player;
    public int CellIndex;
    public int Price;

    public StartAuctionEvent(PlayerData player, int cellIndex, int price)
    {
        Player = player;
        CellIndex = cellIndex;
        Price = price;
    }

}