using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class UICompanyStats : UIBaseCompanyStats, ICompanyStatsUI<CompanyData>
{
    [Header("Star UI")]
    [SerializeField] private TextMeshProUGUI[] rentPriceTexts;

    [Header("Branch Price")]
    [SerializeField] private TextMeshProUGUI branchPriceText;

    public void SetRentPrices(int[] values)
    {
        if (rentPriceTexts == null) return;
        for (int i = 0; i < rentPriceTexts.Length && i < values.Length; i++)
            rentPriceTexts[i].text = values[i].ToString("N0", CultureInfo.InvariantCulture);
    }

    public void SetBranchPrice(int value) => branchPriceText.text = value.ToString("N0", CultureInfo.InvariantCulture);

    public void SetData(CompanyData data)
    {
        SetCompanyName(data.name);
        SetGroupName(data.group.ToString());
       // SetTopBarColor(GroupColors.Colors[(int)data.group]);

        SetRentPrices(data.rent);
        SetCellPrice(data.price);
        SetPledgePrice(data.pledgePrice);
        SetBuyoutPrice(data.buyoutPrice);
        SetBranchPrice(data.branchPrice);
    }
}