using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

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

    private IGroupColors groupColors;

    #region LIFE_CYCLE

    [Inject]
    public void Constuct(IGroupColors groupColors)
    {
        this.groupColors = groupColors ?? throw new ArgumentNullException(nameof(groupColors));
        if (topBarImage1 == null)
            throw new ArgumentNullException(nameof(topBarImage1));
        if (topBarImage2 == null)
            throw new ArgumentNullException(nameof(topBarImage2));
        if (companyNameText == null)
            throw new ArgumentNullException(nameof(companyNameText));
        if (groupNameText == null)
            throw new ArgumentNullException(nameof(groupNameText));
        if (cellPriceText == null)
            throw new ArgumentNullException(nameof(cellPriceText));
        if (pledgePriceText == null)
            throw new ArgumentNullException(nameof(pledgePriceText));
        if (buyoutPriceText == null)
            throw new ArgumentNullException(nameof(buyoutPriceText));
    }

    #endregion LIFE_CYCLE

    #region PUBLIC_METHODS

    public virtual void SetTopBarColor(int groupNumber)
    {
        Color groupColor = groupColors.Colors[groupNumber];
        if (topBarImage1 != null) topBarImage1.color = groupColor;
        if (topBarImage2 != null) topBarImage2.color = groupColor;
    }

    public void SetCompanyName(string value) => SetText(companyNameText, value);

    public void SetGroupName(string value) => SetText(groupNameText, value);

    public void SetCellPrice(int value) => SetText(cellPriceText, FormatNumber(value));

    public void SetPledgePrice(int value) => SetText(pledgePriceText, FormatNumber(value));

    public void SetBuyoutPrice(int value) => SetText(buyoutPriceText, FormatNumber(value));

    #endregion PUBLIC_METHODS

    #region Helpers

    protected void SetText(TextMeshProUGUI textField, string value)
    {
        if (textField != null)
            textField.text = value;
    }

    protected string FormatNumber(int value) => value.ToString("N0", numberFormat);

    private readonly NumberFormatInfo numberFormat = CultureInfo.InvariantCulture.NumberFormat;

    #endregion Helpers
}