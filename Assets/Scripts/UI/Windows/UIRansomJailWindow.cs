using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIRansomJailWindow : UIWindowBase
{
    [Header("Settings")]
    [SerializeField] private int ransomAmount = 500;

    [Header("Buttons")]
    [SerializeField] private Button ransomButton;
    [SerializeField] private Button cantRansomButton;

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

    protected  void OnEnable()
    {
        if (ransomButton != null)
        {
            ransomButton.onClick.AddListener(() => HandleRansomClicked());
        }

    }
    protected  void OnDisable()
    {
        if (ransomButton != null)
        {
            ransomButton.onClick.RemoveListener(() => HandleRansomClicked());
        }

    }
  
    public void ShowWindow(PlayerData player)
    {
        this.player = player;
        this.playerID = player.Id;
        UpdateUI();
        windowAnimation.ShowWindow();
    }
    private void UpdateUI()
    {
        bool canAfford = player.Money >= ransomAmount;
        if (ransomButton != null && cantRansomButton != null)
        {
            ransomButton.gameObject.SetActive(canAfford);
            cantRansomButton.gameObject.SetActive(!canAfford);
        }
           
        if (ransomText != null && cantRansomText != null)
        {
            ransomText.text = "Заплатите " + ransomAmount.ToString("N0", CultureInfo.InvariantCulture);
            cantRansomText.text = "Заплатите " + ransomAmount.ToString("N0", CultureInfo.InvariantCulture);
        }
    }
    private void HandleRansomClicked()
    {

        EventBus.Publish(new ReleaseFromJailEvent(playerID, true));
        HideWindow();
    }
}
