using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIFieldCompanyStats : UIBaseCompanyStats
{

    [Header("Field UI")]
    [SerializeField] private TextMeshProUGUI[] fieldPriceTexts;

    public void SetFieldPrices(int[] values)
    {
        for (int i = 0; i < fieldPriceTexts.Length && i < values.Length; i++)
        {
            fieldPriceTexts[i].text = values[i].ToString();
        }
    }

}
