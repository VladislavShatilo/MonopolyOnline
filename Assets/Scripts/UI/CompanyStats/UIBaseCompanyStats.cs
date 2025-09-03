using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public abstract class UIBaseCompanyStats : MonoBehaviour
{
    [Header("Top Bar")]
    [SerializeField] private Image topBarImage1;
    [SerializeField] private Image topBarImage2;
    [SerializeField] private TextMeshProUGUI companyNameText;
    [SerializeField] private TextMeshProUGUI groupNameText;

    [Header("Buying UI")]
    [SerializeField] private TextMeshProUGUI cellPriceText;
    [SerializeField] private TextMeshProUGUI pledgePriceText;
    [SerializeField] private TextMeshProUGUI buyoutPriceText;

    private readonly NumberFormatInfo numberFormat = CultureInfo.InvariantCulture.NumberFormat;

    #region Public API
    public virtual void SetTopBarColor(Color color)
    {
        if (topBarImage1 != null) topBarImage1.color = color;
        if (topBarImage2 != null) topBarImage2.color = color;
    }
    public void SetCompanyName(string value) => SetText(companyNameText, value);
    public void SetGroupName(string value) => SetText(groupNameText, value);
    public void SetCellPrice(int value) => SetText(cellPriceText, FormatNumber(value));
    public void SetPledgePrice(int value) => SetText(pledgePriceText, FormatNumber(value));
    public void SetBuyoutPrice(int value) => SetText(buyoutPriceText, FormatNumber(value));
    #endregion

    #region Helpers
    protected void SetText(TextMeshProUGUI textField, string value)
    {
        if (textField != null)
            textField.text = value;
    }

    protected string FormatNumber(int value) => value.ToString("N0", numberFormat);
    #endregion
}
