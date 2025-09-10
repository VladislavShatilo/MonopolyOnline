using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhotonBankNotifier : IBankNotifier
{
    public void NotifyBalanceChanged(PlayerData player)
    {
        // Локальное событие (UI / логика)
        EventBus.Publish(new OnUpdatePlayerCapitalEvent(player));

        //// Если игрок — Photon игрок, обновляем кастомные свойства
        //if (player.photonPlayer != null)
        //{
        //    var props = new ExitGames.Client.Photon.Hashtable { { "Money", player.Money } };
        //    player.photonPlayer.SetCustomProperties(props);
        //}
    }
}
