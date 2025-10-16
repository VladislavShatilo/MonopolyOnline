using System;
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

    #region PUBLIC_METHODS

    public override void UpdateUI(CellData cellData, PlayerData owner)
    {
        if (spendImage == null)
            throw new ArgumentNullException(nameof(spendImage));
        var spend = cellData.spendData;
        spendImage.sprite = spend.spendSprite;
    }
    public void RotateIcon()
    {
        if (spendImage == null)
            throw new ArgumentNullException(nameof(spendImage));
        spendImage.rectTransform.eulerAngles = new Vector3(0, 0, 270);
    }

    #endregion PUBLIC_METHODS

}
