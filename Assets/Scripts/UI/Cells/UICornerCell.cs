using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;

public class UICornerCell : UICellBase
{
    [SerializeField] private Image logoImage;

    #region PUBLIC_METHODS

    public override void UpdateUI(CellData cellData, PlayerData owner)
    {
        if (logoImage == null)
            throw new ArgumentNullException(nameof(logoImage));
        var corner = cellData.cornerData;
        logoImage.sprite = corner.logoSprite;
    }

    #endregion PUBLIC_METHODS

}
