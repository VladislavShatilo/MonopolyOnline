using DG.Tweening;
using Photon.Pun;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIBuyWindow : UIWindowBase,IBuyWindow
{
    [Header("Buttons")]
    [SerializeField] private Button buyButton;
    [SerializeField] private Button cantBuyButton;
    [SerializeField] private Button auctionButton;

    [Header("Texts")]
    [SerializeField] private TextMeshProUGUI buyButtonText;
    [SerializeField] private TextMeshProUGUI cantBuyButtonText;

    private int currentCellIndex;

    #region PUBLIC_METHODS

    public void Show(int playerId, int cellIndex, int price, bool canAfford)
    {
        currentCellIndex = cellIndex;

        buyButton.gameObject.SetActive(canAfford);
        cantBuyButton.gameObject.SetActive(!canAfford);

        buyButtonText.text = $"Купить за {price.ToString("N0", CultureInfo.InvariantCulture)}";
        cantBuyButtonText.text = $"Купить за {price.ToString("N0", CultureInfo.InvariantCulture)}";

        ShowWindow();
    }

    public void Hide() => HideWindow();

    public void SetAuctionAction(System.Action<int> onAuction)
    {
        auctionButton.onClick.RemoveAllListeners();
        if (onAuction != null)
        {
            auctionButton.onClick.AddListener(() => onAuction(currentCellIndex));
        }
    }
    public void SetBuyAction(System.Action<int> onBuyAction)
    {
        buyButton.onClick.RemoveAllListeners();
        if (onBuyAction != null)
        {
            buyButton.onClick.AddListener(() => onBuyAction(currentCellIndex));
        }
    }

  
    public Button BuyButton { get => buyButton; set => buyButton = value; }
    public Button CantBuyButton { get => cantBuyButton; set => cantBuyButton = value; }
    public Button AuctionButton { get => auctionButton; set => auctionButton = value; }
    public TextMeshProUGUI BuyButtonText { get => buyButtonText; set => buyButtonText = value; }
    public TextMeshProUGUI CantBuyButtonText { get => cantBuyButtonText; set => cantBuyButtonText = value; }
    #endregion PUBLIC_METHODS


}


