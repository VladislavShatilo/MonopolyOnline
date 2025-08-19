using System.Collections;
using System.Collections.Generic;
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

    public void SetNamePlayerText(string namePlayer)
    {
        namePlayerText.text = namePlayer;
    }

    public void SetMoneyPlayerText(int moneyPlayer) 
    {
        moneyPlayerText.text=moneyPlayer.ToString("N0", CultureInfo.InvariantCulture)+"k";
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
        if(timerText!= null)
        {
            timerText.gameObject.SetActive(active);

        }
        highlightImage.enabled = active;
    }
    public void UpdateTurnTimer(float secondsLeft)
    {
        if (isTurnActive)
            timerText.text = Mathf.Ceil(secondsLeft).ToString();
    }
    public PlayerData GetPlayerData() => playerData;
  

}
