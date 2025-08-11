using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UI;

public class UISpendCell : UICellBase
{
    [SerializeField] private Image spendImage;
    public override void UpdateUI(CellData cellData, PlayerData owner)
    {
        var spend = cellData.spendData;
        spendImage.sprite = spend.spendSprite;
    }   
    public void RotateIcon()
    {
        spendImage.rectTransform.eulerAngles = new Vector3(0,0,270);
    }
}
