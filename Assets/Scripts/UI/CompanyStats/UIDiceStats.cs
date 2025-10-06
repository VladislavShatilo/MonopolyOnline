using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIDiceStats : UIBaseCompanyStats,ICompanyStatsUI<DiceCompanyData>
{
    [Header("Field UI")]
    [SerializeField] private TextMeshProUGUI[] diceFieldMultiTexts;

    #region PUBLIC_METHODS

    public void SetDiceFieldMultiTexts(int[] values)
    {
        for (int i = 0; i < diceFieldMultiTexts.Length && i < values.Length; i++)
        {
            diceFieldMultiTexts[i].text = values[i].ToString("N0", CultureInfo.InvariantCulture);
        }
    }
    public void SetData(DiceCompanyData data)
    {
        SetCompanyName(data.name);
        SetGroupName(data.group.ToString());
        SetTopBarColor((int)data.group);

        SetDiceFieldMultiTexts(data.rentMultiplier);
        SetCellPrice(data.price);
        SetPledgePrice(data.pledgePrice);
        SetBuyoutPrice(data.buyoutPrice);
    }

    #endregion PUBLIC_METHODS


}