using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    [SerializeField] private UIPlayerStats playerStatsPrefab;
    [SerializeField] protected Transform playersStatsContainer;
    private void OnEnable()
    {
        EventBus.Subscribe<PlayerJoinedEvent>(SetPlayerStats);

    }

    private void OnDisable()
    {
        EventBus.Unsubscribe<PlayerJoinedEvent>(SetPlayerStats);

    }
    private void SetPlayerStats(PlayerJoinedEvent e)
    {
        var uiStats = Instantiate(playerStatsPrefab, playersStatsContainer);
        uiStats.SetPlayerStats(e.Player);
        Bank.Instance.OnBalanceChanged += (changedPlayer, money) =>
        {
            if (changedPlayer.Id == e.Player.Id)
                uiStats.SetMoney(money);
        };
    }
}
