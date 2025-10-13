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

    #region PUBLIC_METHODS

    public void SetPayAction(Action payAction)
    {
        payRentButton.onClick.RemoveAllListeners();
        if (payAction != null)
        {
            payRentButton.onClick.AddListener(() => payAction());
        }

    }

    public void Show(int playerId, int cellIndex, float rent, bool canPay)
    {
        payButtonText.text = $"Заплатите {rent.ToString("N0", CultureInfo.InvariantCulture)}";
        cantPayRentText.text = $"Заплатите {rent.ToString("N0", CultureInfo.InvariantCulture)}";

        payRentButton.gameObject.SetActive(canPay);
        cantPayRentButton.gameObject.SetActive(!canPay);

        ShowWindow();
    }

    public void Hide() => HideWindow();
    public void HardHide() => HardHideWindow();

    public Button PayRentButton { get => payRentButton; set => payRentButton = value; }
    public Button CantPayRentButton { get => cantPayRentButton; set => cantPayRentButton = value; }
    public TextMeshProUGUI PayButtonText { get => payButtonText; set => payButtonText = value; }
    public TextMeshProUGUI CantPayRentText { get => cantPayRentText; set => cantPayRentText = value; }

    #endregion PUBLIC_METHODS


}
