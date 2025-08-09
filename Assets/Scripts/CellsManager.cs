using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CellsManager : MonoBehaviour
{
    [SerializeField] private BoardConfig boardConfig;
    [SerializeField] private UIBuyWindow uiBuyWindow;
    public void CellHandle(GameObject cellGO, int currentCellID)
    {
        Debug.Log(currentCellID);
        switch (boardConfig.cells[currentCellID].cellType)
        {
            case CellType.Company:
            {
                    Debug.Log(currentCellID);

                    uiBuyWindow.SetBuyText(boardConfig.cells[currentCellID].companyData.price[0]);
                    uiBuyWindow.ShowBuyWindow();
                    break;
            }
        }
    }
}
