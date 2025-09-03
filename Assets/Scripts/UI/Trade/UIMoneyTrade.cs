using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIMoneyTrade : MonoBehaviour
{
    [SerializeField] private Button openMoneyButton;
    [SerializeField] private Image penImage;
    [SerializeField] private TMP_InputField moneyInputField;
    [SerializeField] private TextMeshProUGUI moneyTextValue;
    [SerializeField] private TextMeshProUGUI moneyText;



    private int currentMoney = 0;

    private void Start()
    {
        openMoneyButton.onClick.AddListener(OpenMoneyInput);
        RefreshUI();
    }

    private void OpenMoneyInput()
    {
        moneyInputField.gameObject.SetActive(true);
        moneyTextValue.gameObject.SetActive(false);
        moneyText.gameObject.SetActive(false);
        penImage.gameObject.SetActive(false);

        moneyInputField.onEndEdit.AddListener(OnMoneyChanged);
        moneyInputField.text = currentMoney.ToString();
    }

    private void OnMoneyChanged(string value)
    {
        if (int.TryParse(value, out int money))
            currentMoney = money;
        else
            currentMoney = 0;

        moneyTextValue.text = currentMoney.ToString("N0", CultureInfo.InvariantCulture);

        penImage.gameObject.SetActive(true);
        moneyTextValue.gameObject.SetActive(true);
        moneyInputField.gameObject.SetActive(false);
        moneyText.gameObject.SetActive(true);

        moneyInputField.onEndEdit.RemoveListener(OnMoneyChanged);

        // Обновляем общий трейд через TradeManager
        TradeManager.Instance.UpdateMoneyFromUI(currentMoney, this);
    }

    private void RefreshUI()
    {
        moneyTextValue.text = currentMoney.ToString("N0", CultureInfo.InvariantCulture);
        moneyInputField.gameObject.SetActive(false);
        moneyTextValue.gameObject.SetActive(true);
        penImage.gameObject.SetActive(true);
    }
}
