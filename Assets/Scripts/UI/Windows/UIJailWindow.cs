using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIJailWindow : UIWindowBase<UIJailWindow>
{
    [Header ("Setting")]
    [SerializeField] private int ransomMoney = 500;

    [Header("Buttons")]
    [SerializeField] private Button ransomButton;
    [SerializeField] private Button cantRansomButton;
    [SerializeField] private Button throwDiceButton;

    [Header("Texts")]
    [SerializeField] private TextMeshProUGUI ransomText;
    [SerializeField] private TextMeshProUGUI cantRansomText;

    private int playerID;
    private PlayerData player;

    public Button RansomButton
    {
        get => ransomButton;
        set => ransomButton = value;
    }

    public Button ThrowDiceButton
    {
        get => throwDiceButton;
        set => throwDiceButton = value;
    }
    public Button CantRansomButton
    {
        get => cantRansomButton;
        set => cantRansomButton = value;
    }
    public TextMeshProUGUI RansomText
    {
        get => ransomText;
        set => ransomText = value;
    }
    public TextMeshProUGUI CantRansomText
    {
        get => cantRansomText;
        set => cantRansomText = value;
    }
    protected override void OnEnable()
    {
        if(ransomButton != null && throwDiceButton != null)
        {
            throwDiceButton.onClick.AddListener(OnThrowDiceClicked);
            ransomButton.onClick.AddListener(OnRansomClicked);
        }
       
    }

    protected override void OnDisable()
    {
        if (ransomButton != null && throwDiceButton != null)
        {
            throwDiceButton.onClick.RemoveListener(OnThrowDiceClicked);
            ransomButton.onClick.RemoveListener(OnRansomClicked);
        }
    }

    public void ShowWindow(PlayerData player)
    {
        this.playerID = player.id;
        this.player = player;
        UpdateUI();
        windowAnimation.ShowWindow();
    }
    private void UpdateUI()
    {
        bool canAfford = player.Money >= ransomMoney;

        ransomButton.gameObject.SetActive(canAfford);
        cantRansomButton.gameObject.SetActive(!canAfford);

        if(ransomText != null && cantRansomText != null)
        {
            ransomText.text = "Заплатите " + ransomMoney.ToString("N0", CultureInfo.InvariantCulture);
            cantRansomText.text = "Заплатите " + ransomMoney.ToString("N0", CultureInfo.InvariantCulture);
        }
      
    }
    private void OnThrowDiceClicked()
    {
        HideWindow();
        EventBus.Publish(new RollDiceJailButtonEvent(playerID));
        
      
    }
    private void OnRansomClicked()
    {
        EventBus.Publish(new ReleaseFromJailEvent(playerID, true));
        HideWindow();
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