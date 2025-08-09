using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Xml.Serialization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UICompanyCell : MonoBehaviour
{
    [SerializeField] private UIBuyWindow uiBuyWindow;
    [SerializeField] private TextMeshProUGUI companyNameText;
    [SerializeField] private TextMeshProUGUI priceText;
    [SerializeField] private Image BGImage;
    [SerializeField] private Image BGPriceImage;
    [SerializeField] private Color[] groupColors;

    public void SetupCompany(string companyName,int price,int groupId)
    {
        companyNameText.text = companyName;
        priceText.text = price.ToString("N0", CultureInfo.InvariantCulture) + "k";
        BGPriceImage.color = groupColors[groupId];

    }
    //private string DesignText(int number)
    //{
    //    string res = "";
    //    if (number > 1000)
    //    { 
    //        res = (number%1000).ToString()+","+(number- (number % 1000)*1000).ToString();
    //    }
    //    else
    //    {
    //        res = number.ToString();
    //    }
    //    res += "k";
    //    return res;
    //}
    public void SetBGColor(Color color)
    {
        BGImage.color = color;
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
