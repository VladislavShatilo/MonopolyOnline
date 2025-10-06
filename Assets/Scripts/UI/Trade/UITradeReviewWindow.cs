using DG.Tweening;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UITradeReviewWindow : UITradeWindowBase, ITradeReviewWindow
{
    [SerializeField] private Button acceptButton;
    [SerializeField] private Button cancelButton;

    #region PUBLIC_METHODS

    public void Show(bool isRecipient, TradeOffer tradeOffer)
    {
        acceptButton.gameObject.SetActive(isRecipient);
        cancelButton.gameObject.SetActive(isRecipient);
        currentOffer = tradeOffer;
        ShowWindow();
        RefreshUI();
    }

    public void SetAcceptAction(System.Action onAccept)
    {
        acceptButton.onClick.RemoveAllListeners();
        if (onAccept != null)
        {
            acceptButton.onClick.AddListener(() => onAccept());
        }
    }

    public void SetCancelAction(System.Action onCancelAction)
    {
        cancelButton.onClick.RemoveAllListeners();
        if (onCancelAction != null)
        {
            cancelButton.onClick.AddListener(() => onCancelAction());
        }
    }

    public void Hide()
    {
        ClearUI();
        HideWindow();
    }

    #endregion PUBLIC_METHODS
}