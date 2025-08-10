using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CellsManager : MonoBehaviour
{
    [SerializeField] private BoardConfig boardConfig; // Ссылка на ScriptableObject с данными
    [SerializeField] private Transform parentTransform; // Родитель для клеток на сцене
    private List<Transform> boardCellsTransforms = new List<Transform>();

    private Dictionary<int, UICellBase> cellUIMap = new Dictionary<int, UICellBase>();
    private void Start()
    {
        foreach (Transform child in parentTransform)
        {
            boardCellsTransforms.Add(child);
        }
        for(int i = 0; i < boardCellsTransforms.Count; i++)
        {
            cellUIMap[i] = boardCellsTransforms[i].GetComponent<UICellBase>();
        }
    }
    public void RefreshCellUI(int cellIndex, Player owner)
    {
        if (cellUIMap.TryGetValue(cellIndex, out var ui))
        {
            var cellData = boardConfig.cells[cellIndex];
            ui.UpdateUI(cellData, owner);
        }
    }
}