using Photon.Pun;
using Photon.Realtime;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIPlayerStats : MonoBehaviour
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

    private PlayerData playerData;

    private void OnEnable()
    {
        EventBus.Subscribe<TurnTimerUpdatedEvent>(OnTurnTimerUpdated);
        EventBus.Subscribe<AuctionTimerUpdatedEvent>(OnAuctionTimerUpdated);
        EventBus.Subscribe<TradeTimerUpdatedEvent>(OnTradeTimerUpdated);
        EventBus.Subscribe<OnUpdatePlayerCapitalEvent>(OnCapitalUpdated);
        EventBus.Subscribe<OnTakeLoanEvent>(OnLoanUpdated);
        EventBus.Subscribe<TurnStartEvent>(OnTurnStarted);

        tradeButton.onClick.AddListener(OnTradeButtonClicked);
        takeLoanButton.onClick.AddListener(() => LoanManager.Instance.TakeLoan(playerData.id));
        payLoanButton.onClick.AddListener(() => EventBus.Publish(new PayLoanEvent(playerData.id)));
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe<TurnTimerUpdatedEvent>(OnTurnTimerUpdated);
        EventBus.Unsubscribe<AuctionTimerUpdatedEvent>(OnAuctionTimerUpdated);
        EventBus.Unsubscribe<TradeTimerUpdatedEvent>(OnTradeTimerUpdated);
        EventBus.Unsubscribe<OnUpdatePlayerCapitalEvent>(OnCapitalUpdated);
        EventBus.Unsubscribe<OnTakeLoanEvent>(OnLoanUpdated);
        EventBus.Unsubscribe<TurnStartEvent>(OnTurnStarted);

        tradeButton.onClick.RemoveListener(OnTradeButtonClicked);
    }

    public void SetPlayerStats(PlayerData playerData)
    {
        this.playerData = playerData;
        namePlayerText.text = playerData.Name;
        SetMoney(playerData.Money);
        UpdateCapital(playerData);
        timerGO.SetActive(false);

        leaveButton.gameObject.SetActive(playerData.photonPlayer.IsLocal);
    }

    public void SetMoney(int money) =>
        moneyPlayerText.text = money.ToString("N0", CultureInfo.InvariantCulture);

    private void UpdateCapital(PlayerData player)
    {
        capitalText.text = player.VisibleCapital.ToString("N0", CultureInfo.InvariantCulture);
        liquidText.text = player.LiquidAssets.ToString("N0", CultureInfo.InvariantCulture);
    }

    private void UpdateTimerUI(bool active, float timeLeft, Image highlight)
    {
        timerGO.SetActive(active);
        timerText.gameObject.SetActive(active);
        highlight.enabled = active;

        if (active)
            timerText.text = Mathf.Ceil(timeLeft).ToString();
    }

    private void OnTurnTimerUpdated(TurnTimerUpdatedEvent e)
    {
        if (playerData == null || e.PlayerId != playerData.id) return;
        UpdateTimerUI(e.IsCurrent, e.TimeLeft, highlightTurnImage);
    }

    private void OnAuctionTimerUpdated(AuctionTimerUpdatedEvent e)
    {
        bool isCurrent = playerData != null && e.PlayerId == playerData.id;
        UpdateTimerUI(isCurrent && e.TimeLeft > 0, e.TimeLeft, highlightAuctionImage);
    }

    private void OnTradeTimerUpdated(TradeTimerUpdatedEvent e)
    {
        bool isCurrent = playerData != null && e.PlayerId == playerData.id && e.TimeLeft > 0;
        UpdateTimerUI(isCurrent, e.TimeLeft, highlightAuctionImage);
    }

    private void OnCapitalUpdated(OnUpdatePlayerCapitalEvent e)
    {
        if (e.Player.id == playerData.id)
            UpdateCapital(e.Player);
    }

    private void OnLoanUpdated(OnTakeLoanEvent e)
    {
        if (e.PlayerData.id != playerData.id) return;

        loanContainer.SetActive(e.PlayerData.HasLoan);
        loanTurnsLeftText.text = e.PlayerData.HasLoan ? e.PlayerData.LoanTurnsLeft.ToString() : "";

        takeLoanButton.gameObject.SetActive(!e.PlayerData.HasLoan && e.PlayerData.photonPlayer.IsLocal);
        payLoanButton.gameObject.SetActive(e.PlayerData.HasLoan && e.PlayerData.photonPlayer.IsLocal);
    }

    private void OnTurnStarted(TurnStartEvent e)
    {
        if (playerData == null) return;

        bool isLocalTurn = e.PlayerId == PhotonNetwork.LocalPlayer.ActorNumber;
        bool isThisPlayerLocal = playerData.id == PhotonNetwork.LocalPlayer.ActorNumber;

        tradeButton.gameObject.SetActive(isLocalTurn && !isThisPlayerLocal);
        if (isThisPlayerLocal)
        {
            takeLoanButton.gameObject.SetActive(!playerData.HasLoan);
            payLoanButton.gameObject.SetActive(playerData.HasLoan);
        }
    }

    private void OnTradeButtonClicked()
    {
        if (playerData == null) return;
        UITradeWindow.Instance.ShowWindow();
        EventBus.Publish(new StartTradeRequestEvent(PhotonNetwork.LocalPlayer.ActorNumber, playerData.id));
    }
}

public class OnUpdatePlayerCapitalEvent
{
    public PlayerData Player;


    public OnUpdatePlayerCapitalEvent(PlayerData player)
    {
        Player = player;
    }

}
public class StartTradeRequestEvent
{
    public int FromId;
    public int ToId;
    public StartTradeRequestEvent(int fromId,int toId)
    {
        FromId = fromId;
        ToId= toId;
    }
}