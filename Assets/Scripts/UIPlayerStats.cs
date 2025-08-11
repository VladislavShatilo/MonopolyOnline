using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using TMPro;
using UnityEngine;

public class UIPlayerStats : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI namePlayerText;
    [SerializeField] private TextMeshProUGUI moneyPlayerText;

    public void SetNamePlayerText(string namePlayer)
    {
        namePlayerText.text = namePlayer;
    }

    public void SetMoneyPlayerText(int moneyPlayer) 
    {
        moneyPlayerText.text=moneyPlayer.ToString("N0", CultureInfo.InvariantCulture)+"k";
    }

    public void SetPlayerStats(PlayerData player)
    {
        namePlayerText.text = player.Name;
        moneyPlayerText.text = player.Money.ToString("N0", CultureInfo.InvariantCulture) + "k";

    }

}
