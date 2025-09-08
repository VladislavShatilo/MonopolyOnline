using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UILoanPayWindow : UIWindowBase
{
    [Header("Settings")]
    [SerializeField] private int loanAmount = 5500;

    [Header("Buttons")]
    [SerializeField] private Button payLoanButton;
    [SerializeField] private Button cantPayLoanButton;

    [Header("Texts")]
    [SerializeField] private TextMeshProUGUI payLoanText;
    [SerializeField] private TextMeshProUGUI cantPayLoanText;

    private PlayerData player;
    public Button PayLoanButton
    {
        get => payLoanButton;
        set => payLoanButton = value;
    }
    public Button CantPayLoanButton
    {
        get => cantPayLoanButton;
        set => cantPayLoanButton = value;
    }
    public TextMeshProUGUI PayLoanText
    {
        get => payLoanText;
        set => payLoanText = value;
    }
    public TextMeshProUGUI CantPayLoanText
    {
        get => cantPayLoanText;
        set => cantPayLoanText = value;
    }

    protected void OnEnable()
    {
        if (payLoanButton != null)
        {
            payLoanButton.onClick.AddListener(() => OnPayLoanClicked());
        }

    }
    protected  void OnDisable()
    {
        if (payLoanButton != null)
        {
            payLoanButton.onClick.RemoveListener(() => OnPayLoanClicked());
        }

    }
    public void ShowLoanWindow(PlayerData player)
    {
        this.player = player;
        UpdateUI();
        ShowWindow();
    }

    private void UpdateUI()
    {
        bool canAfford = player.Money >= loanAmount;
        if (payLoanButton != null && cantPayLoanButton != null)
        {
            payLoanButton.gameObject.SetActive(canAfford);
            cantPayLoanButton.gameObject.SetActive(!canAfford);
        }
           

        if (payLoanText != null && cantPayLoanText != null)
        {
            payLoanText.text = "Заплатите банку " + loanAmount.ToString("N0", CultureInfo.InvariantCulture);
            cantPayLoanText.text = "Заплатите банку " + loanAmount.ToString("N0", CultureInfo.InvariantCulture);
        }

    }
    private void OnPayLoanClicked()
    {
        EventBus.Publish(new PayLoanEvent(PhotonNetwork.LocalPlayer.ActorNumber));
        HideWindow();

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