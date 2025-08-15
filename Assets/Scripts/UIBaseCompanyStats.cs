using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public abstract class UIBaseCompanyStats : MonoBehaviour
{
    [Header("Top Bar")]
    [SerializeField] protected Image topBarImage1;
    [SerializeField] protected Image topBarImage2;
    [SerializeField] protected TextMeshProUGUI companyNameText;
    [SerializeField] protected TextMeshProUGUI groupNameText;

    [Header("Buying UI")]
    [SerializeField] protected TextMeshProUGUI cellPriceText;
    [SerializeField] protected TextMeshProUGUI pledgePriceText;
    [SerializeField] protected TextMeshProUGUI buyoutPriceText;

    public virtual void SetTopBarColor(Color color)
    {
        topBarImage1.color = color;
        topBarImage2.color = color;
    }

    public void SetCompanyName(string value) => companyNameText.text = value;
    public void SetGroupName(string value) => groupNameText.text = value;
    public void SetCellPrice(string value) => cellPriceText.text = value;
    public void SetPledgePrice(string value) => pledgePriceText.text = value;
    public void SetBuyoutPrice(string value) => buyoutPriceText.text = value;
}
