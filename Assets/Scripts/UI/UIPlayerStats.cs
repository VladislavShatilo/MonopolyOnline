using Photon.Pun;
using Photon.Realtime;
using System;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIPlayerStats : MonoBehaviour, IPlayerStatsView
{
    [Header("UI")]
    [SerializeField] private TextMeshProUGUI namePlayerText;
    [SerializeField] private TextMeshProUGUI moneyPlayerText;
    [SerializeField] private TextMeshProUGUI capitalText;
    [SerializeField] private TextMeshProUGUI liquidText;

    [Header("Timer UI")]
    [SerializeField] private GameObject timerGO;
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private Image highlightTurnImage;
    [SerializeField] private Image highlightAuctionImage;

    [Header("Loan UI")]
    [SerializeField] private GameObject loanContainer;
    [SerializeField] private TextMeshProUGUI loanTurnsLeftText;
    [SerializeField] private Button takeLoanButton;
    [SerializeField] private Button payLoanButton;

    [Header("Buttons")]
    [SerializeField] private Button tradeButton;
    [SerializeField] private Button leaveButton;

    #region LIFE_CYCLE

    private void Start()
    {
        if (namePlayerText == null)
            throw new ArgumentNullException(nameof(namePlayerText));
        if (moneyPlayerText == null)
            throw new ArgumentNullException(nameof(moneyPlayerText));
        if (capitalText == null)
            throw new ArgumentNullException(nameof(capitalText));

        if (liquidText == null)
            throw new ArgumentNullException(nameof(liquidText));
        if (timerGO == null)
            throw new ArgumentNullException(nameof(timerGO));
        if (timerText == null)
            throw new ArgumentNullException(nameof(timerText));
        if (highlightTurnImage == null)
            throw new ArgumentNullException(nameof(highlightTurnImage));
        if (highlightAuctionImage == null)
            throw new ArgumentNullException(nameof(highlightAuctionImage));
        if (loanContainer == null)
            throw new ArgumentNullException(nameof(loanContainer));
        if (loanTurnsLeftText == null)
            throw new ArgumentNullException(nameof(loanTurnsLeftText));
        if (takeLoanButton == null)
            throw new ArgumentNullException(nameof(takeLoanButton));
        if (payLoanButton == null)
            throw new ArgumentNullException(nameof(payLoanButton));
        if (tradeButton == null)
            throw new ArgumentNullException(nameof(tradeButton));
        if (leaveButton == null)
            throw new ArgumentNullException(nameof(leaveButton));

    }

    #endregion LIFE_CYCLE


    #region PUBLIC_METHODS

    public void SetName(string name) =>
       namePlayerText.text = name;

    public void SetMoney(int money) =>
        moneyPlayerText.text = money.ToString("N0", CultureInfo.InvariantCulture);

    public void SetCapital(int visibleCapital, int liquidAssets)
    {
        capitalText.text = visibleCapital.ToString("N0", CultureInfo.InvariantCulture);
        liquidText.text = liquidAssets.ToString("N0", CultureInfo.InvariantCulture);
    }

    public void SetTimer(bool active, float timeLeft, bool highlightTurn, bool highlightAuction)
    {
        timerGO.SetActive(active);
        timerText.gameObject.SetActive(active);
        highlightTurnImage.gameObject.SetActive(active && highlightTurn);
        highlightAuctionImage.gameObject.SetActive(active && highlightAuction);

        if (active)
            timerText.text = Mathf.Ceil(timeLeft).ToString();
    }

    public void SetLoan(bool hasLoan, int turnsLeft, bool isLocal)
    {
        loanContainer.SetActive(hasLoan);
        loanTurnsLeftText.text = hasLoan ? turnsLeft.ToString() : "";

        takeLoanButton.gameObject.SetActive(!hasLoan && isLocal);
        payLoanButton.gameObject.SetActive(hasLoan && isLocal);
    }
    public void SetTimerGOVisible(bool visible) =>
     timerGO.SetActive(visible);
    public void SetLoanContainerVisible(bool visible) =>
       loanContainer.SetActive(visible);
    public void SetTurnHighlightVisible(bool visible) =>
        highlightTurnImage.gameObject.SetActive(visible);
    public void SetAuctionHighlightVisible(bool visible) =>
        highlightAuctionImage.gameObject.SetActive(visible);
    public void SetTradeButtonVisible(bool visible) =>
        tradeButton.gameObject.SetActive(visible);

    public void SetLoanButtonsVisible(bool canTakeLoan, bool canPayLoan)
    {
        takeLoanButton.gameObject.SetActive(canTakeLoan);
        payLoanButton.gameObject.SetActive(canPayLoan);
    }

    public void SetLeaveButtonVisible(bool visible) =>
        leaveButton.gameObject.SetActive(visible);

    public void BindTradeAction(System.Action onTrade)
    {
        tradeButton.onClick.RemoveAllListeners();
        tradeButton.onClick.AddListener(() => onTrade?.Invoke());
    }

    public void BindTakeLoanAction(System.Action onTakeLoan)
    {
        takeLoanButton.onClick.RemoveAllListeners();
        takeLoanButton.onClick.AddListener(() => onTakeLoan?.Invoke());
    }

    public void BindPayLoanAction(System.Action onPayLoan)
    {
        payLoanButton.onClick.RemoveAllListeners();
        payLoanButton.onClick.AddListener(() => onPayLoan?.Invoke());
    }

    #endregion PUBLIC_METHODS


}
