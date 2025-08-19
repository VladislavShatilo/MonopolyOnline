using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIBuyWindow : MonoBehaviour
{
    public static UIBuyWindow Instance { get; private set; }

    [SerializeField] private WindowAnimation windowAnimation;
    [SerializeField] private Button buyButton;
    [SerializeField] private Button auctionButton;
    [SerializeField] private TextMeshProUGUI buyButtonText;

    private int currentCellIndex;

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
        buyButton.onClick.AddListener(() => OnBuyClicked());
        auctionButton.onClick.AddListener(() => AuctionWindow());
    }
    public void ShowBuyWindow(int cellIndex, CompanyBaseData companyBaseData)
    {
        currentCellIndex = cellIndex;
        buyButtonText.text = "Купить за " + companyBaseData.price.ToString("N0", CultureInfo.InvariantCulture) + "k";
        windowAnimation.ShowWindow();
    }
    public void HideWindow()
    { 
        windowAnimation.HideWindow();
    }
    private void AuctionWindow()
    {
        windowAnimation.HideWindow();
        TurnManager.Instance.RequestEndTurn();
    }
    private void OnBuyClicked()
    {
        CompanyManager.Instance.TryBuyCompany (currentCellIndex);

    }

    public Button BuyButton()=>buyButton;
 
}
