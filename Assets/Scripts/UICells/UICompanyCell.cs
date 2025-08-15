using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Xml.Serialization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UICompanyCell : UICellBase
{

    [SerializeField] private TextMeshProUGUI companyNameText;
    [SerializeField] private TextMeshProUGUI priceText;
    [SerializeField] private Image BGImage;
    [SerializeField] private Image BGPriceImage;

    public override void UpdateUI(CellData cellData, PlayerData owner)
    {
        switch (cellData.cellType)
        {
            case CellType.Company:
                var company = cellData.companyData;
                companyNameText.text = company.name;
                priceText.text = company.price.ToString("N0", CultureInfo.InvariantCulture) + "k";
                BGPriceImage.color = GroupColors.Colors[(int)company.group];
                break;
            case CellType.FieldCompany:
                var fieldCompany = cellData.fieldCompanyData;
                companyNameText.text = fieldCompany.name;
                priceText.text = fieldCompany.price.ToString("N0", CultureInfo.InvariantCulture) + "k";
                BGPriceImage.color = GroupColors.Colors[(int)fieldCompany.group];
                break;
            case CellType.DiceCompany:
                var diceCompany = cellData.diceCompanyData;
                companyNameText.text = diceCompany.name;
                priceText.text = diceCompany.price.ToString("N0", CultureInfo.InvariantCulture) + "k";
                BGPriceImage.color = GroupColors.Colors[(int)diceCompany.group];
                break;

        }

        if (owner != null)
        {
            BGImage.color = owner.playerColor;
        }
        else
        {
            BGImage.color = Color.white; // или стандартный цвет
        }
    }
   
    public void RotateLogoText(int angle)
    {
        companyNameText.rectTransform.eulerAngles = new Vector3(0,0, angle);
    }
    public void RotatePriceText()
    {
        priceText.rectTransform.eulerAngles = new Vector3(0, 0, 180);
    }


}
