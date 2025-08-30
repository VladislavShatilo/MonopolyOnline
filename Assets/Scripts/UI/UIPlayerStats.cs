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
    [SerializeField] private GameObject timerGO;
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private Image highlightTurnImage;
    [SerializeField] private Image highlightAuctionImage;
    [SerializeField] private TextMeshProUGUI loanTurnsLeftText;
    [SerializeField] private GameObject loanContainer;

    [Header("Buttons")]
    [SerializeField] private Button tradeButton;
    [SerializeField] private Button takeLoanButton;
    [SerializeField] private Button leaveButton;
    [SerializeField] private Button payLoanButton;


    private bool isTurnActive = false;
    private PlayerData playerData;
    private bool isActive = false;

    private void Start()
    {
        tradeButton.onClick.AddListener(OnTradeButtonClicked);
        takeLoanButton.onClick.AddListener(() => LoanManager.Instance.TakeLoan(playerData.id));
        payLoanButton.onClick.AddListener(() => LoanManager.Instance.PayLoan(playerData.id));
        loanContainer.gameObject.SetActive(false);

    }

    private void OnEnable()
    {
        EventBus.Subscribe<TurnTimerUpdatedEvent>(OnTimerUpdated);
        EventBus.Subscribe<AuctionTimerUpdatedEvent>(OnTimerUpdated);
        EventBus.Subscribe<OnTakeLoanEvent>(UpdateLoanUI);
        EventBus.Subscribe<TradeTimerUpdatedEvent>(OnTimerUpdated);

        EventBus.Subscribe<TurnStartEvent>(OnTurnStarted);
        PhotonNetwork.NetworkingClient.EventReceived += OnPhotonEventReceived;
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe<TurnTimerUpdatedEvent>(OnTimerUpdated);
        EventBus.Unsubscribe<AuctionTimerUpdatedEvent>(OnTimerUpdated);
        EventBus.Unsubscribe<TradeTimerUpdatedEvent>(OnTimerUpdated);
        EventBus.Unsubscribe<OnTakeLoanEvent>(UpdateLoanUI);

        EventBus.Unsubscribe<TurnStartEvent>(OnTurnStarted);

        PhotonNetwork.NetworkingClient.EventReceived -= OnPhotonEventReceived;
    }

    private void OnPhotonEventReceived(ExitGames.Client.Photon.EventData photonEvent)
    {
        // При изменении CustomProperties вызываем обновление UI
        if (playerData == null) return;

        int money = playerData.photonPlayer.CustomProperties.TryGetValue("Money", out var val) ? (int)val : playerData.Money;
        SetMoneyPlayerText(money);
    }

    private void OnTimerUpdated(TurnTimerUpdatedEvent e)
    {
        if (playerData == null || e.PlayerId != playerData.id) return;

        isActive = e.IsCurrent;
        timerText.gameObject.SetActive(isActive);
        timerGO.gameObject.SetActive(isActive);
        highlightTurnImage.enabled = isActive;

        if (isActive)
            timerText.text = Mathf.Ceil(e.TimeLeft).ToString();
    }

    private void OnTimerUpdated(AuctionTimerUpdatedEvent e)
    {
        if (playerData == null) return;

        bool isCurrentBidder = e.PlayerId == playerData.id;

        // Показываем таймер и подсветку только у текущего игрока
        timerText.gameObject.SetActive(isCurrentBidder);
        timerGO.SetActive(isCurrentBidder);
        highlightAuctionImage.enabled = isCurrentBidder;

        if (isCurrentBidder)
            timerText.text = Mathf.Ceil(e.TimeLeft).ToString();

        // Если аукцион закончился, сбрасываем все
        if (e.TimeLeft <= 0f)
        {
            timerText.gameObject.SetActive(false);
            timerGO.SetActive(false);
            highlightAuctionImage.enabled = false;
        }
    }

    private void OnTimerUpdated(TradeTimerUpdatedEvent e)
    {
        if (playerData == null) return;
        if (e.TimeLeft > 0f)
        {
            bool isCurrentTrader = e.PlayerId == playerData.id;

            timerText.gameObject.SetActive(isCurrentTrader);
            timerGO.SetActive(isCurrentTrader);
            highlightAuctionImage.enabled = isCurrentTrader;
            timerText.text = Mathf.Ceil(e.TimeLeft).ToString();
        }
        else
        {
            // Когда время вышло, у ВСЕХ игроков скрываем всё
            timerText.gameObject.SetActive(false);
            timerGO.SetActive(false);
            highlightAuctionImage.enabled = false;
        }
    }
    public void UpdateLoanUI(OnTakeLoanEvent e)
    {
        // Если обновление не для этого игрока — выходим
        if (e.PlayerData.id != playerData.id) return;

        if (e.PlayerData.HasLoan)
        {
            loanContainer.gameObject.SetActive(true);

            // Кнопку оплаты показываем только локальному игроку
            if (e.PlayerData.photonPlayer.IsLocal)
            {
                takeLoanButton.gameObject.SetActive(false);
                payLoanButton.gameObject.SetActive(true);
            }
            else
            {
                takeLoanButton.gameObject.SetActive(false);
                payLoanButton.gameObject.SetActive(false);
            }

            loanTurnsLeftText.text = e.PlayerData.LoanTurnsLeft.ToString();
        }
        else
        {
            if (e.PlayerData.photonPlayer.IsLocal)
                takeLoanButton.gameObject.SetActive(true);

            payLoanButton.gameObject.SetActive(false);
            loanTurnsLeftText.text = "";
            loanContainer.gameObject.SetActive(false);
        }
    }
  

    public void SetMoneyPlayerText(int moneyPlayer)
    {
        moneyPlayerText.text = moneyPlayer.ToString("N0", CultureInfo.InvariantCulture);
    }

    public void SetPlayerStats(PlayerData playerData)
    {
        this.playerData = playerData;
        namePlayerText.text = playerData.Name;
        moneyPlayerText.text = playerData.Money.ToString("N0", CultureInfo.InvariantCulture);
        timerText.gameObject.SetActive(false);
        highlightTurnImage.enabled = false;
        highlightAuctionImage.enabled = false;
        leaveButton.gameObject.SetActive(playerData.id == PhotonNetwork.LocalPlayer.ActorNumber);
        
            timerGO.SetActive(false);
    }

    public void SetTurnActive(bool active)
    {
        isTurnActive = active;
        if (timerText != null)
            timerText.gameObject.SetActive(active);

        highlightTurnImage.enabled = active;
    }

    public void UpdateTurnTimer(float secondsLeft)
    {
        if (isTurnActive)
            timerText.text = Mathf.Ceil(secondsLeft).ToString();
    }

    private void OnTurnStarted(TurnStartEvent e)
    {
        if (playerData == null) return;

        int currentTurnPlayerId = e.PlayerId;
        int thisPlayerId = playerData.id;
        int localPlayerId = PhotonNetwork.LocalPlayer.ActorNumber;

        // 1. Если это НЕ мой ход  скрыть все кнопки
        if (currentTurnPlayerId != localPlayerId)
        {
            tradeButton.gameObject.SetActive(false);
         
                takeLoanButton.gameObject.SetActive(false);
                payLoanButton.gameObject.SetActive(false);

            return;
        }
     
        // 2. Если это МОЙ ход  показывать кнопки только на других игроков
        if (thisPlayerId == localPlayerId)
        {
            tradeButton.gameObject.SetActive(false); // сам себе не предлагаю
            if (!playerData.HasLoan)
            {
                takeLoanButton.gameObject.SetActive(true);
                payLoanButton.gameObject.SetActive(false);

            }

        }
        else
        {
            tradeButton.gameObject.SetActive(true); // всем остальным показываю
            if (!playerData.HasLoan)
            {
                takeLoanButton.gameObject.SetActive(false);
                payLoanButton.gameObject.SetActive(false);

            }
        }
    }

    private void OnTradeButtonClicked()
    {
        if (playerData == null) return;
        UITradeWindow.Instance.ShowWindow();
        int fromPlayerId = PhotonNetwork.LocalPlayer.ActorNumber;
        int toPlayerId = playerData.id;

        TradeManager.Instance.StartTradeRequest(fromPlayerId, toPlayerId);
    }
}