using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIDiceStats : UIBaseCompanyStats
{
    [Header("Field UI")]
    [SerializeField] private TextMeshProUGUI[] diceFieldMultiTexts;

    public void SetDiceFieldMultiTexts(int[] values)
    {
        for (int i = 0; i < diceFieldMultiTexts.Length && i < values.Length; i++)
        {
            diceFieldMultiTexts[i].text = values[i].ToString("N0", CultureInfo.InvariantCulture);
        }
    }
}