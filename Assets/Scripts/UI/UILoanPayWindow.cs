using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UILoanPayWindow : MonoBehaviour
{
    public static UILoanPayWindow Instance { get; private set; }

    [SerializeField] private WindowAnimation windowAnimation;
    [SerializeField] private Button payLoanButton;

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
        payLoanButton.onClick.AddListener(() => OnPayLoanClicked());

    }
    public void ShowLoanWindow()
    {
        windowAnimation.ShowWindow();
    }
    public void HideWindow()
    {
        windowAnimation.HideWindow();
    }
    private void OnPayLoanClicked()
    {
        LoanManager.Instance.PayLoan(PhotonNetwork.LocalPlayer.ActorNumber);
        HideWindow();

    }
}
