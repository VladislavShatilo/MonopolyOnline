using DG.Tweening;
using Photon.Pun;
using Photon.Realtime;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UITradeWindow : UITradeWindowBase, ITradeWindow
{
    [Header("ButtonsInputs")]
    [SerializeField] private Button offerButton;

    [SerializeField] private Button closeButton;

    [Header("Inputs")]
    [SerializeField] private TMP_InputField leftMoneyInputField;

    [SerializeField] private TMP_InputField rightMoneyInputField;
    [SerializeField] private UIMoneyTrade leftUIMoneyTrade;
    [SerializeField] private UIMoneyTrade rightUIMoneyTrade;

    #region LIFE_CYCLE

    private void OnEnable()
    {
        offerButton.interactable = false;

        leftMoneyInputField.onEndEdit.AddListener(OnLeftMoneyChanged);
        rightMoneyInputField.onEndEdit.AddListener(OnRightMoneyChanged);

        offerButton.onClick.AddListener(OnOfferClicked);
        closeButton.onClick.AddListener(OnCloseClicked);
    }

    private void OnDisable()
    {
        leftMoneyInputField.onEndEdit.RemoveListener(OnLeftMoneyChanged);
        rightMoneyInputField.onEndEdit.RemoveListener(OnRightMoneyChanged);

        offerButton.onClick.AddListener(OnOfferClicked);
        closeButton.onClick.AddListener(OnCloseClicked);
    }

    #endregion LIFE_CYCLE

    #region PUBLIC_METHODS

    public void Show(TradeOffer tradeOffer)
    {
        ValidateOfferButton();
        ClearUI();
        currentOffer = tradeOffer;
        RefreshUI();
        ShowWindow();
    }

    public void Hide()
    {
        OnCloseClicked();
    }

    public void Clear()
    {
        throw new System.NotImplementedException();
    }

    public void SetOfferAction(System.Action onAuction)
    {
        offerButton.onClick.RemoveAllListeners();
        if (onAuction != null)
        {
            offerButton.onClick.AddListener(() => onAuction());
        }
    }

    public void SetCloseAction(System.Action onBuyAction)
    {
        closeButton.onClick.RemoveAllListeners();
        if (onBuyAction != null)
        {
            closeButton.onClick.AddListener(() => onBuyAction());
        }
    }

    public void UpdateTrade(TradeOffer tradeOffer)
    {
        currentOffer = tradeOffer;
        RefreshUI();
        ValidateOfferButton();
    }

    #endregion PUBLIC_METHODS

    #region PRIVATE_METHODS

    private void OnLeftMoneyChanged(string value)
    {
        if (int.TryParse(value, out int amount) && currentOffer != null)
        {
            // TradeManager.Instance.SetMoney(currentOffer.FromPlayerData.Id, amount);
            currentOffer.FromMoney = amount;
            RefreshUI();
            ValidateOfferButton();
        }
    }

    private void OnRightMoneyChanged(string value)
    {
        if (int.TryParse(value, out int amount) && currentOffer != null)
        {
            //TradeManager.Instance.SetMoney(currentOffer.ToPlayerData.Id, amount);
            currentOffer.ToMoney = amount;
            RefreshUI();
            ValidateOfferButton();
        }
    }

    private void ValidateOfferButton()
    {
        if (currentOffer != null)
        {
            Debug.Log("ValidateOfferButton");
            int leftAmount = currentOffer.GetFromTotalValue();
            int rightAmount = currentOffer.GetToTotalValue();
            if (leftAmount == 0 || rightAmount == 0)
            {
                offerButton.interactable = false;
            }
            if (leftAmount > 2 * rightAmount || rightAmount > 2 * leftAmount)
            {
                Debug.Log("leftAmount " + leftAmount + "  " + "rightAmount " + rightAmount);

                offerButton.interactable = false;
            }
            else
            {
                Debug.Log("leftAmount " + leftAmount + "  " + "rightAmount " + rightAmount);

                offerButton.interactable = true;
            }
        }
    }

    #endregion PRIVATE_METHODS

    #region CALLBACKS

    private void OnOfferClicked()
    {
        if (currentOffer == null || !currentOffer.IsValid()) return;
        HideWindow();
    }

    private void OnCloseClicked()
    {
        HideWindow();
        leftMoneyInputField.text = "0";
        rightMoneyInputField.text = "0";
        leftUIMoneyTrade.RefreshUI();
        rightUIMoneyTrade.RefreshUI();
    }

    #endregion CALLBACKS
}