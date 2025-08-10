using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;

public class UICornerCell : UICellBase
{
    [SerializeField] private Image logoImage;
    public override void UpdateUI(CellData cellData, Player owner)
    {
        var corner = cellData.cornerData;
        logoImage.sprite = corner.logoSprite;
    }
}
