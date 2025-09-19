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


    public void Show(int playerId, int cellIndex, int price, bool canAfford)
    {
        currentCellIndex = cellIndex;

        buyButton.gameObject.SetActive(canAfford);
        cantBuyButton.gameObject.SetActive(!canAfford);

        buyButtonText.text = $"Купить за {price:N0}";
        cantBuyButtonText.text = $"Купить за {price:N0}";

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
    
}
public class TryBuyCompanyEvent
{
    public int CellIndex;
    public TryBuyCompanyEvent(int cellIndex)
    {
        CellIndex = cellIndex;
    }

}

