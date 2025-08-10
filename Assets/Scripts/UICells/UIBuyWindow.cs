using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIBuyWindow : MonoBehaviour
{
    [SerializeField] private WindowAnimation windowAnimation;
    [SerializeField] private Button buyButton;
    [SerializeField] private Button auctionButton;
    [SerializeField] private TextMeshProUGUI buyButtonText;
 
    
    public void ShowBuyWindow()=>windowAnimation.ShowWindow();
    public void HideBuyWindow() => windowAnimation.HideWindow();

    public void SetBuyText(int price)
    {
        buyButtonText.text ="Купить за "+ price.ToString("N0", CultureInfo.InvariantCulture) +"k";
    }
    public Button BuyButton()=>buyButton;
 
}
