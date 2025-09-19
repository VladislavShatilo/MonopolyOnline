using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIJailWindow : UIWindowBase, IJailWindow
{
    [Header("Buttons")]
    [SerializeField] private Button ransomButton;
    [SerializeField] private Button cantRansomButton;
    [SerializeField] private Button throwDiceButton;

    [Header("Texts")]
    [SerializeField] private TextMeshProUGUI ransomText;
    [SerializeField] private TextMeshProUGUI cantRansomText;

    private int currentPlayerId;

    public void Show(int playerId, int ransomMoney, bool canAfford)
    {
        currentPlayerId = playerId;

        ransomButton.gameObject.SetActive(canAfford);
        cantRansomButton.gameObject.SetActive(!canAfford);

        string ransomString = $"Заплатите {ransomMoney.ToString("N0", CultureInfo.InvariantCulture)}";
        ransomText.text = ransomString;
        cantRansomText.text = ransomString;

        ShowWindow();
    }

    public void Hide() => HideWindow();
    public void HardHide() => HardHideWindow();
    public void SetThrowDiceAction(System.Action<int> onThrowDice)
    {
        throwDiceButton.onClick.RemoveAllListeners();
        if (onThrowDice != null)
        {
            throwDiceButton.onClick.AddListener(() => onThrowDice(currentPlayerId));
        }
    }

    public void SetRansomAction(System.Action<int> onRansom)
    {
        ransomButton.onClick.RemoveAllListeners();
        if (onRansom != null)
        {
            ransomButton.onClick.AddListener(() => onRansom(currentPlayerId));
        }
    }
}
public class ReleaseFromJailEvent
{
    public int PlayerID;
    public bool IsPaidExit;

    public ReleaseFromJailEvent(int playerId, bool isPaidExit)
    {
        PlayerID= playerId;
        IsPaidExit = isPaidExit;

    }
}