using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIPayRent : MonoBehaviour
{
    public static UIPayRent Instance { get; private set; }

    [SerializeField] private WindowAnimation windowAnimation;
    [SerializeField] private Button payRentButton;
    [SerializeField] private TextMeshProUGUI payButtonText;

    private int currentCellIndex;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }
    private void Start()
    {
        payRentButton.onClick.AddListener(() => OnRentClicked());
       
    }
    public void ShowRentWindow(int cellIndex, CompanyData companyData)
    {
        currentCellIndex = cellIndex;
        payButtonText.text = "Заплатите " + companyData.rent[0].ToString("N0", CultureInfo.InvariantCulture) + "k";
        windowAnimation.ShowWindow();
    }
    public void HideWindow()
    {
        windowAnimation.HideWindow();
        TurnManager.Instance.RequestEndTurn();
    }
    private void OnRentClicked()
    {
        CompanyManager.Instance.TryPayRent(currentCellIndex);
        

    }
}
