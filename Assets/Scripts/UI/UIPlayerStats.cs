using Photon.Pun;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIPlayerStats : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI namePlayerText;
    [SerializeField] private TextMeshProUGUI moneyPlayerText;
    [SerializeField] private GameObject timerGO;
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private Image highlightTurnImage;
    [SerializeField] private Image highlightAuctionImage;

    private bool isTurnActive = false;
    private PlayerData playerData;
    private bool isActive = false;

    private void OnEnable()
    {
        EventBus.Subscribe<TurnTimerUpdatedEvent>(OnTimerUpdated);
        EventBus.Subscribe<AuctionTimerUpdatedEvent>(OnTimerUpdated);

        PhotonNetwork.NetworkingClient.EventReceived += OnPhotonEventReceived;
    }

    private void OnDisable()
    {
       EventBus.Unsubscribe<TurnTimerUpdatedEvent>(OnTimerUpdated);
        EventBus.Unsubscribe<AuctionTimerUpdatedEvent>(OnTimerUpdated);

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
    public void SetMoneyPlayerText(int moneyPlayer)
    {
        moneyPlayerText.text = moneyPlayer.ToString("N0", CultureInfo.InvariantCulture) + "k";
    }

    public void SetPlayerStats(PlayerData playerData)
    {
        this.playerData = playerData;
        namePlayerText.text = playerData.Name;
        moneyPlayerText.text = playerData.Money.ToString("N0", CultureInfo.InvariantCulture) + "k";
        timerText.gameObject.SetActive(false);
        highlightTurnImage.enabled = false;
        highlightAuctionImage.enabled = false;
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
}
