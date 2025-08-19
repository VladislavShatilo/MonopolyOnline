using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIFieldCompanyStats : UIBaseCompanyStats, ICompanyStatsUI<FieldCompanyData>
{

    [Header("Field UI")]
    [SerializeField] private TextMeshProUGUI[] fieldPriceTexts;

    public void SetFieldPrices(int[] values)
    {
        for (int i = 0; i < fieldPriceTexts.Length && i < values.Length; i++)
        {
            fieldPriceTexts[i].text = values[i].ToString("N0", CultureInfo.InvariantCulture);
        }
    }
    public void SetData(FieldCompanyData data)
    {
        SetCompanyName(data.name);
        SetGroupName(data.group.ToString());
        SetTopBarColor(GroupColors.Colors[(int)data.group]);

        SetFieldPrices(data.rentField);
        SetCellPrice(data.price);
        SetPledgePrice(data.pledgePrice);
        SetBuyoutPrice(data.buyoutPrice);
    }

}
