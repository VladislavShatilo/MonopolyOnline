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
    [SerializeField] private Color[] groupColors;

    public override void UpdateUI(CellData cellData, PlayerData owner)
    {
        var company = cellData.companyData;
        companyNameText.text = company.name;
        priceText.text = company.price[0].ToString("N0", CultureInfo.InvariantCulture) + "k";
        BGPriceImage.color = groupColors[(int)company.group];

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
