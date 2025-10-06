    using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class UICellBase : MonoBehaviour
{
    #region PUBLIC_METHODS

    public abstract void UpdateUI(CellData cellData, PlayerData owner);

    #endregion PUBLIC_METHODS

}
