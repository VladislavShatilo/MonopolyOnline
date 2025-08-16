using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UICompanyStats : UIBaseCompanyStats
{
    [Header("Star UI")]
    [SerializeField] private TextMeshProUGUI[] rentPriceTexts; // Массив вместо отдельных полей

    public void SetRentPrices(int[] values)
    {
        for (int i = 0; i < rentPriceTexts.Length && i < values.Length; i++)
        {
            rentPriceTexts[i].text = values[i].ToString("N0", CultureInfo.InvariantCulture);
        }
    }

    public void SetBranchPrice(string value) => branchPriceText.text = value;

    [Header("Branch Price")]
    [SerializeField] private TextMeshProUGUI branchPriceText;
}
