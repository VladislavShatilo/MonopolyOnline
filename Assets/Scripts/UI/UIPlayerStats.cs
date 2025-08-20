using Photon.Pun;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIPlayerStats : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI namePlayerText;
    [SerializeField] private TextMeshProUGUI moneyPlayerText;
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private Image highlightImage;
    private bool isTurnActive = false;
    private PlayerData playerData;

    private void OnEnable()
    {
       // EventBus.Subscribe<MoneyAddedEvent>(OnMoneyChanged);
      //  EventBus.Subscribe<MoneyRemovedEvent>(OnMoneyChanged);
       // EventBus.Subscribe<MoneyTransferredEvent>(OnMoneyChanged);

        PhotonNetwork.NetworkingClient.EventReceived += OnPhotonEventReceived;
    }

    private void OnDisable()
    {
       // EventBus.Unsubscribe<MoneyAddedEvent>(OnMoneyChanged);
        //EventBus.Unsubscribe<MoneyRemovedEvent>(OnMoneyChanged);
        //EventBus.Unsubscribe<MoneyTransferredEvent>(OnMoneyChanged);

        PhotonNetwork.NetworkingClient.EventReceived -= OnPhotonEventReceived;
    }

    private void OnPhotonEventReceived(ExitGames.Client.Photon.EventData photonEvent)
    {
        // ѕри изменении CustomProperties вызываем обновление UI
        if (playerData == null) return;

        int money = playerData.photonPlayer.CustomProperties.TryGetValue("Money", out var val) ? (int)val : playerData.Money;
        SetMoneyPlayerText(money);
    }

    //private void OnMoneyChanged(object e)
    //{
    //    switch (e)
    //    {
    //        case MoneyAddedEvent added when added.Player.id == playerData.id:
    //            SetMoneyPlayerText(added.Player.Money);
    //            break;
    //        case MoneyRemovedEvent removed when removed.Player.id == playerData.id:
    //            SetMoneyPlayerText(removed.Player.Money);
    //            break;
    //        case MoneyTransferredEvent transfer:
    //            if (transfer.From.id == playerData.id)
    //                SetMoneyPlayerText(transfer.From.Money);
    //            else if (transfer.To.id == playerData.id)
    //                SetMoneyPlayerText(transfer.To.Money);
    //            break;
    //    }
    //}
    private void OnTurnTimerUpdated(TurnTimerUpdatedEvent e)
    {
        // обновл€ем только если это событие моего игрока
        if (playerData == null || e.PlayerId != playerData.id) return;

        SetTurnActive(e.IsCurrent);
        UpdateTurnTimer(e.TimeLeft);
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
        highlightImage.enabled = false;
    }

    public void SetTurnActive(bool active)
    {
        isTurnActive = active;
        if (timerText != null)
            timerText.gameObject.SetActive(active);

        highlightImage.enabled = active;
    }

    public void UpdateTurnTimer(float secondsLeft)
    {
        if (isTurnActive)
            timerText.text = Mathf.Ceil(secondsLeft).ToString();
    }
}
