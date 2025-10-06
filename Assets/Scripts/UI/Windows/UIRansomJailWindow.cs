using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIRansomJailWindow : UIWindowBase,IRansomJailWindow
{
    [Header("Button")]
    [SerializeField] private Button ransomButton;
    [SerializeField] private Button cantRansomButton;

    [Header("Text")]
    [SerializeField] private TextMeshProUGUI ransomText;
    [SerializeField] private TextMeshProUGUI cantRansomText;

    private int currentPlayerId;

    #region PUBLIC_METHODS

    public void Show(int playerId, int fine, bool canAfford)
    {
        currentPlayerId = playerId;

        ransomButton.gameObject.SetActive(canAfford);
        cantRansomButton.gameObject.SetActive(!canAfford);
        ransomText.text = $"Заплатите {fine.ToString("N0", CultureInfo.InvariantCulture)}";
        cantRansomText.text = $"Заплатите {fine.ToString("N0", CultureInfo.InvariantCulture)}";

        ShowWindow();
    }

    public void Hide() => HideWindow();

    public void HardHide() => HardHideWindow();

    public void SetRansomAction(System.Action<int> onRansom)
    {
        ransomButton.onClick.RemoveAllListeners();
        if (onRansom != null)
            ransomButton.onClick.AddListener(() => onRansom(currentPlayerId));
    }

    #endregion PUBLIC_METHODS

}
