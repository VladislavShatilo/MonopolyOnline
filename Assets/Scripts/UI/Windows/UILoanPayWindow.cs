using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UILoanPayWindow : UIWindowBase, ILoanPayWindow
{
    [Header("Buttons")]
    [SerializeField] private Button payLoanButton;
    [SerializeField] private Button cantPayLoanButton;

    [Header("Texts")]
    [SerializeField] private TextMeshProUGUI payLoanText;
    [SerializeField] private TextMeshProUGUI cantPayLoanText;

    private int currentPlayerId;

    public void Show(int playerId, int loanAmount, bool canAfford)
    {
        currentPlayerId = playerId;

        payLoanButton.gameObject.SetActive(canAfford);
        cantPayLoanButton.gameObject.SetActive(!canAfford);

        string text = $"Заплатите банку {loanAmount.ToString("N0", CultureInfo.InvariantCulture)}";
        payLoanText.text = text;
        cantPayLoanText.text = text;

        ShowWindow();
    }

    public void Hide() => HideWindow();

    public void SetPayLoanAction(Action<int> onPayLoan)
    {
        payLoanButton.onClick.RemoveAllListeners();
        if (onPayLoan != null)
        {
            payLoanButton.onClick.AddListener(() => onPayLoan(currentPlayerId));
        }
    }
}
public class PayLoanEvent 
{
    public int PlayerId;
    public PayLoanEvent(int playerId)
    {
        PlayerId = playerId;
    }
}