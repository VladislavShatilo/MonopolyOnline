using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class UICellBase : MonoBehaviour
{
    public abstract void UpdateUI(CellData cellData, Player owner);
}
