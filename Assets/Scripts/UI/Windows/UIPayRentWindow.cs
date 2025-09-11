using Photon.Pun;
using Photon.Realtime;
using System;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIPayRentWindow : UIWindowBase, IPayRentWindow
{
    [SerializeField] private Button payRentButton;
    [SerializeField] private TextMeshProUGUI payButtonText;
    [SerializeField] private Button cantPayRentButton;
    [SerializeField] private TextMeshProUGUI cantPayRentText;

    private Action<int> payAction;

    public void SetPayAction(Action payAction)
    {
        payRentButton.onClick.RemoveAllListeners();
        if (payAction != null)
        {
            payRentButton.onClick.AddListener(() => payAction());
        }
    
    }
        

    private void OnEnable() => payRentButton.onClick.AddListener(OnPayClicked);
    private void OnDisable() => payRentButton.onClick.RemoveListener(OnPayClicked);

    public void Show(int playerId, int cellIndex, float rent, bool canPay)
    {
        payButtonText.text = $"Заплатите {rent:N0}";
        cantPayRentText.text = $"Заплатите {rent:N0}";

        payRentButton.gameObject.SetActive(canPay);
        cantPayRentButton.gameObject.SetActive(!canPay);

        windowAnimation.ShowWindow();
    }

    public void Hide() => windowAnimation.HideWindow();
    public void HardHide() => windowAnimation.HardHideWindow();

    private void OnPayClicked() => payAction?.Invoke(0); // playerId можно передавать через presenter
}
